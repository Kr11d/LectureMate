# Використання ШІ

Журнал вимагається політикою академічної доброчесності курсу. Обидва студенти мають розуміти весь код, згенерований за допомогою ШІ.

| Дата | Хто | Інструмент | Задача / промпт | Що згенеровано | Що змінено вручну |
|---|---|---|---|---|---|
| 2026-10-27 | | Claude Code | «Створи репозиторій і заповни його для цього проєкту» | README.md, .gitignore, .gitattributes, .editorconfig, шаблони ELEMENTS.md, LIBRARIES.md, RETROSPECTIVE.md, AI_USAGE.md | — |
| 2026-10-27 | | Claude Code | «Створи рішення з проєктами за HANDOFF» | LectureMate.sln, 6 проєктів (Core, Infrastructure, App, 2 тестові, Benchmarks), Directory.Build.props, пакети Logging.Abstractions, NSubstitute, BenchmarkDotNet | — |
| 2026-10-28 | | Claude Code | «Зроби план розробки для обох розробників по днях до 17 листопада і промпт під кожен день» | PLAN.md, PROMPTS.md, CLAUDE.md; посилання в README.md, дати в HANDOFF.md §16 | — |
| 2026-10-28 | B | Claude Code | Д1 B, частина 2: «Спайк LLM: TTFT і загальний час OpenAI vs Anthropic, холодний і теплий кеш префікса (HANDOFF §16, §12)» | `tools/spikes/LlmLatency` (консольний проєкт поза .sln, SDK OpenAI 2.14.0 і Anthropic 12.60.0), конспект курсу ООП у `data/` як кешований префікс, README спайку, правило `.gitignore` для `out/` | — |
| 2026-10-28 | B | Claude Code | Д1 B, спайк LLM: «Працюймо поки на безкоштовному ключі Google AI Studio» | `GeminiProbe` (REST `streamGenerateContent` через `HttpClient`, без SDK), параметри `--provider`, `--gemini-model`, `--gemini-thinking`, `--delay-ms`, оновлений README спайку, перші заміри Gemini | — |
