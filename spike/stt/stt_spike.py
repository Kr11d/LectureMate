"""STT spike (HANDOFF 8.2, M0): check Soniox quality on a real Ukrainian lecture recording.

Not part of the app. Uses only the Python standard library (3.10+); ffmpeg is optional.

Usage:
    python spike/stt/stt_spike.py lecture.m4a [--reference reference.txt] [--model stt-async-v3]

The API key is read from the SONIOX_API_KEY environment variable only.

If ffmpeg is on PATH, the recording is first converted to the pipeline format
(16 kHz, mono, PCM16 WAV), the same audio the app will send.

Results go to spike/stt/out/<recording name>/: soniox.txt and summary.md.
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
from pathlib import Path

BASE_URL = "https://api.soniox.com/v1"
LANGUAGE = "uk"
DEFAULT_MODEL = "stt-async-v3"
POLL_SECONDS = 2
TIMEOUT_SECONDS = 15 * 60


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


def multipart_file(field: str, file_path: Path) -> tuple[bytes, str]:
    boundary = uuid.uuid4().hex
    body = b"".join([
        f'--{boundary}\r\nContent-Disposition: form-data; name="{field}"; filename="{file_path.name}"\r\n'
        f"Content-Type: application/octet-stream\r\n\r\n".encode("utf-8"),
        file_path.read_bytes(),
        f"\r\n--{boundary}--\r\n".encode("utf-8"),
    ])
    return body, f"multipart/form-data; boundary={boundary}"


# ---------- Soniox ----------

def transcribe(audio: Path, key: str, model: str) -> str:
    """Async file API: upload -> create transcription -> poll -> fetch transcript -> clean up."""
    auth = {"Authorization": f"Bearer {key}"}

    body, content_type = multipart_file("file", audio)
    file_id = json.loads(http("POST", f"{BASE_URL}/files", {**auth, "Content-Type": content_type}, body))["id"]
    transcription_id = None
    try:
        created = http_json("POST", f"{BASE_URL}/transcriptions", auth,
                            {"model": model, "file_id": file_id, "language_hints": [LANGUAGE]})
        transcription_id = created["id"]

        deadline = time.monotonic() + TIMEOUT_SECONDS
        while True:
            status = http_json("GET", f"{BASE_URL}/transcriptions/{transcription_id}", auth)
            if status.get("status") == "completed":
                break
            if status.get("status") == "error":
                raise RuntimeError(f"transcription failed: {status.get('error_message')}")
            if time.monotonic() > deadline:
                raise RuntimeError("transcription timed out")
            time.sleep(POLL_SECONDS)

        transcript = http_json("GET", f"{BASE_URL}/transcriptions/{transcription_id}/transcript", auth)
        if "text" in transcript:
            return transcript["text"]
        return "".join(token["text"] for token in transcript.get("tokens", []))
    finally:
        if transcription_id:
            _try_delete(f"{BASE_URL}/transcriptions/{transcription_id}", auth)
        _try_delete(f"{BASE_URL}/files/{file_id}", auth)


def _try_delete(url: str, headers: dict[str, str]) -> None:
    try:
        http("DELETE", url, headers)
    except Exception as error:  # cleanup only; the transcript is already fetched or the run already failed
        print(f"warning: cleanup failed: {error}", file=sys.stderr)


# ---------- Audio ----------

def prepare_audio(source: Path, work_dir: Path) -> Path:
    if shutil.which("ffmpeg") is None:
        print("ffmpeg not found: sending the original file")
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
    parser.add_argument("--model", default=DEFAULT_MODEL, help=f"Soniox async model (default: {DEFAULT_MODEL})")
    args = parser.parse_args()

    if not args.audio.is_file():
        parser.error(f"file not found: {args.audio}")
    key = os.environ.get("SONIOX_API_KEY")
    if not key:
        parser.error("set the SONIOX_API_KEY environment variable")
    reference = args.reference.read_text(encoding="utf-8") if args.reference else None

    out_dir = Path(__file__).parent / "out" / args.audio.stem
    out_dir.mkdir(parents=True, exist_ok=True)

    with tempfile.TemporaryDirectory() as work_dir:
        audio = prepare_audio(args.audio, Path(work_dir))
        print(f"soniox ({args.model}): transcribing...")
        started = time.perf_counter()
        try:
            text = transcribe(audio, key, args.model)
        except Exception as error:
            print(f"failed: {error}", file=sys.stderr)
            return 1
        seconds = time.perf_counter() - started

    (out_dir / "soniox.txt").write_text(text, encoding="utf-8")
    words = len(normalize(text))
    wer = f"{word_error_rate(reference, text):.1%}" if reference else "—"
    summary = (f"# STT spike: {args.audio.name}\n\n"
               "| Model | Wall time, s | Words | WER |\n|---|---|---|---|\n"
               f"| {args.model} | {seconds:.1f} | {words} | {wer} |\n\n"
               "Wall time is a whole-file (batch) request, not the streaming latency the app will see.\n")
    (out_dir / "summary.md").write_text(summary, encoding="utf-8")
    print(f"done in {seconds:.1f} s, {words} words\nResults: {out_dir}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
