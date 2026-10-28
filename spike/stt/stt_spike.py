"""STT spike (HANDOFF 8.2, M0): run one lecture recording through Soniox, Deepgram and OpenAI.

Not part of the app. Uses only the Python standard library (3.10+); ffmpeg is optional.

Usage:
    python spike/stt/stt_spike.py lecture.m4a [--reference reference.txt] [--providers soniox deepgram openai]

API keys are read from environment variables only:
    SONIOX_API_KEY, DEEPGRAM_API_KEY, OPENAI_API_KEY
A provider without a key is skipped.

If ffmpeg is on PATH, the recording is first converted to the pipeline format
(16 kHz, mono, PCM16 WAV): every provider then gets the same input, and 10 minutes
fit the 25 MB OpenAI upload limit.

Results go to spike/stt/out/<recording name>/: one <provider>.txt per provider and summary.md.
With --reference (a hand-checked transcript of the same audio) the summary also shows WER.
Neither the recording nor the results are committed (see .gitignore).
"""

from __future__ import annotations

import argparse
import json
import os
import re
import shutil
import subprocess
import sys
import tempfile
import time
import unicodedata
import urllib.error
import urllib.request
import uuid
from dataclasses import dataclass
from pathlib import Path

LANGUAGE = "uk"
OPENAI_MAX_BYTES = 25 * 1024 * 1024
SONIOX_POLL_SECONDS = 2
SONIOX_TIMEOUT_SECONDS = 15 * 60


@dataclass
class Result:
    provider: str
    model: str
    text: str
    seconds: float


# ---------- HTTP helpers ----------

def http(method: str, url: str, headers: dict[str, str], body: bytes | None = None, timeout: int = 600) -> bytes:
    request = urllib.request.Request(url, data=body, headers=headers, method=method)
    try:
        with urllib.request.urlopen(request, timeout=timeout) as response:
            return response.read()
    except urllib.error.HTTPError as error:
        detail = error.read().decode("utf-8", errors="replace")
        raise RuntimeError(f"{method} {url} -> HTTP {error.code}: {detail}") from error


def http_json(method: str, url: str, headers: dict[str, str], payload: object | None = None) -> dict:
    body = None
    if payload is not None:
        body = json.dumps(payload).encode("utf-8")
        headers = {**headers, "Content-Type": "application/json"}
    raw = http(method, url, headers, body)
    return json.loads(raw) if raw else {}


def multipart(fields: dict[str, str], file_field: str, file_path: Path) -> tuple[bytes, str]:
    boundary = uuid.uuid4().hex
    parts: list[bytes] = []
    for name, value in fields.items():
        parts.append(
            f'--{boundary}\r\nContent-Disposition: form-data; name="{name}"\r\n\r\n{value}\r\n'.encode("utf-8"))
    parts.append(
        f'--{boundary}\r\nContent-Disposition: form-data; name="{file_field}"; filename="{file_path.name}"\r\n'
        f"Content-Type: application/octet-stream\r\n\r\n".encode("utf-8"))
    parts.append(file_path.read_bytes())
    parts.append(f"\r\n--{boundary}--\r\n".encode("utf-8"))
    return b"".join(parts), f"multipart/form-data; boundary={boundary}"


# ---------- Providers ----------

def transcribe_soniox(audio: Path, key: str, model: str) -> str:
    """Async file API: upload -> create transcription -> poll -> fetch transcript -> clean up."""
    base = "https://api.soniox.com/v1"
    auth = {"Authorization": f"Bearer {key}"}

    body, content_type = multipart({}, "file", audio)
    file_id = json.loads(http("POST", f"{base}/files", {**auth, "Content-Type": content_type}, body))["id"]
    transcription_id = None
    try:
        created = http_json("POST", f"{base}/transcriptions", auth,
                            {"model": model, "file_id": file_id, "language_hints": [LANGUAGE]})
        transcription_id = created["id"]

        deadline = time.monotonic() + SONIOX_TIMEOUT_SECONDS
        while True:
            status = http_json("GET", f"{base}/transcriptions/{transcription_id}", auth)
            if status.get("status") == "completed":
                break
            if status.get("status") == "error":
                raise RuntimeError(f"Soniox transcription failed: {status.get('error_message')}")
            if time.monotonic() > deadline:
                raise RuntimeError("Soniox transcription timed out")
            time.sleep(SONIOX_POLL_SECONDS)

        transcript = http_json("GET", f"{base}/transcriptions/{transcription_id}/transcript", auth)
        if "text" in transcript:
            return transcript["text"]
        return "".join(token["text"] for token in transcript.get("tokens", []))
    finally:
        if transcription_id:
            _try_delete(f"{base}/transcriptions/{transcription_id}", auth)
        _try_delete(f"{base}/files/{file_id}", auth)


def _try_delete(url: str, headers: dict[str, str]) -> None:
    try:
        http("DELETE", url, headers)
    except Exception as error:  # cleanup only; the transcript is already fetched or the run already failed
        print(f"  warning: cleanup failed: {error}", file=sys.stderr)


def transcribe_deepgram(audio: Path, key: str, model: str) -> str:
    url = (f"https://api.deepgram.com/v1/listen?model={model}&language={LANGUAGE}"
           "&smart_format=true&punctuate=true")
    headers = {"Authorization": f"Token {key}", "Content-Type": "application/octet-stream"}
    response = json.loads(http("POST", url, headers, audio.read_bytes()))
    return response["results"]["channels"][0]["alternatives"][0]["transcript"]


def transcribe_openai(audio: Path, key: str, model: str) -> str:
    size = audio.stat().st_size
    if size > OPENAI_MAX_BYTES:
        raise RuntimeError(f"file is {size / 2**20:.1f} MB, OpenAI accepts up to 25 MB; "
                           "install ffmpeg or convert to 16 kHz mono WAV/MP3 first")
    body, content_type = multipart({"model": model, "language": LANGUAGE, "response_format": "json"}, "file", audio)
    headers = {"Authorization": f"Bearer {key}", "Content-Type": content_type}
    return json.loads(http("POST", "https://api.openai.com/v1/audio/transcriptions", headers, body))["text"]


PROVIDERS = {
    # name: (function, env var, default model)
    "soniox": (transcribe_soniox, "SONIOX_API_KEY", "stt-async-v3"),
    "deepgram": (transcribe_deepgram, "DEEPGRAM_API_KEY", "nova-3"),
    "openai": (transcribe_openai, "OPENAI_API_KEY", "gpt-4o-transcribe"),
}


# ---------- Audio ----------

def prepare_audio(source: Path, work_dir: Path) -> Path:
    if shutil.which("ffmpeg") is None:
        print("ffmpeg not found: sending the original file to every provider")
        return source
    target = work_dir / f"{source.stem}.16k-mono.wav"
    subprocess.run(["ffmpeg", "-y", "-loglevel", "error", "-i", str(source),
                    "-ac", "1", "-ar", "16000", "-c:a", "pcm_s16le", str(target)], check=True)
    return target


# ---------- WER ----------

_APOSTROPHES = str.maketrans({"’": "'", "ʼ": "'", "`": "'", "´": "'"})


def normalize(text: str) -> list[str]:
    """Lower-case, unify apostrophes, drop punctuation; numbers are kept as written."""
    text = unicodedata.normalize("NFC", text).lower().translate(_APOSTROPHES)
    text = re.sub(r"[^\w'\s-]", " ", text)
    text = re.sub(r"(?<!\w)['-]|['-](?!\w)", " ", text)
    return text.split()


def word_error_rate(reference: str, hypothesis: str) -> float:
    ref, hyp = normalize(reference), normalize(hypothesis)
    if not ref:
        raise ValueError("reference transcript is empty")
    previous = list(range(len(hyp) + 1))
    for i, ref_word in enumerate(ref, 1):
        current = [i] + [0] * len(hyp)
        for j, hyp_word in enumerate(hyp, 1):
            current[j] = min(previous[j] + 1, current[j - 1] + 1, previous[j - 1] + (ref_word != hyp_word))
        previous = current
    return previous[-1] / len(ref)


# ---------- Main ----------

def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("audio", type=Path, help="lecture recording (wav, mp3, m4a, ...)")
    parser.add_argument("--reference", type=Path, help="hand-checked transcript for WER")
    parser.add_argument("--providers", nargs="+", choices=PROVIDERS, default=list(PROVIDERS))
    for name, (_, _, model) in PROVIDERS.items():
        parser.add_argument(f"--{name}-model", default=model, help=f"default: {model}")
    args = parser.parse_args()

    if not args.audio.is_file():
        parser.error(f"file not found: {args.audio}")
    reference = args.reference.read_text(encoding="utf-8") if args.reference else None

    out_dir = Path(__file__).parent / "out" / args.audio.stem
    out_dir.mkdir(parents=True, exist_ok=True)

    results: list[Result] = []
    errors: dict[str, str] = {}
    with tempfile.TemporaryDirectory() as work_dir:
        audio = prepare_audio(args.audio, Path(work_dir))
        for name in args.providers:
            function, env_var, _ = PROVIDERS[name]
            model = getattr(args, f"{name}_model")
            key = os.environ.get(env_var)
            if not key:
                print(f"{name}: skipped ({env_var} is not set)")
                continue
            print(f"{name} ({model}): transcribing...")
            started = time.perf_counter()
            try:
                text = function(audio, key, model)
            except Exception as error:
                errors[name] = str(error)
                print(f"  failed: {error}", file=sys.stderr)
                continue
            result = Result(name, model, text, time.perf_counter() - started)
            results.append(result)
            (out_dir / f"{name}.txt").write_text(text, encoding="utf-8")
            print(f"  done in {result.seconds:.1f} s, {len(normalize(text))} words")

    lines = [f"# STT spike: {args.audio.name}", "",
             "| Provider | Model | Wall time, s | Words | WER |", "|---|---|---|---|---|"]
    for r in results:
        wer = f"{word_error_rate(reference, r.text):.1%}" if reference else "—"
        lines.append(f"| {r.provider} | {r.model} | {r.seconds:.1f} | {len(normalize(r.text))} | {wer} |")
    for name, message in errors.items():
        lines.append(f"| {name} | — | failed | — | {message[:120]} |")
    lines += ["", "Wall time is a whole-file (batch) request, not the streaming latency the app will see."]
    (out_dir / "summary.md").write_text("\n".join(lines) + "\n", encoding="utf-8")
    print(f"\nResults: {out_dir}")
    return 0 if results else 1


if __name__ == "__main__":
    sys.exit(main())
