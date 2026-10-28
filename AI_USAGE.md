# Використання ШІ

Журнал вимагається політикою академічної доброчесності курсу. Обидва студенти мають розуміти весь код, згенерований за допомогою ШІ.

| Дата | Хто | Інструмент | Задача / промпт | Що згенеровано | Що змінено вручну |
|---|---|---|---|---|---|
| 2026-10-27 | | Claude Code | «Створи репозиторій і заповни його для цього проєкту» | README.md, .gitignore, .gitattributes, .editorconfig, шаблони ELEMENTS.md, LIBRARIES.md, RETROSPECTIVE.md, AI_USAGE.md | — |
| 2026-10-27 | | Claude Code | «Створи рішення з проєктами за HANDOFF» | LectureMate.sln, 6 проєктів (Core, Infrastructure, App, 2 тестові, Benchmarks), Directory.Build.props, пакети Logging.Abstractions, NSubstitute, BenchmarkDotNet | — |
| 2026-10-28 | | Claude Code | «Зроби план розробки для обох розробників по днях до 17 листопада і промпт під кожен день» | PLAN.md, PROMPTS.md, CLAUDE.md; посилання в README.md, дати в HANDOFF.md §16 | — |
| 2026-10-28 | A | Claude Code | Д1, PR 1: доменні типи §7 (без типів конспекту), `AudioFormats`, ієрархія винятків §8.11, тести | `Core/Domain/*`, `Core/Errors/*`, тести `AudioFormats`, `LlmProviderException`, `LectureMateException` | — |
