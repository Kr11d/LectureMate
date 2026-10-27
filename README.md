# LectureMate — «Помічник на лекції»

Десктопний застосунок для Windows (C# / .NET 10 / WPF), який допомагає студенту на онлайн-лекції:

- слухає системний звук і показує **живий транскрипт** (українською або англійською);
- розпізнає **питання викладача** і за 2–3 с показує коротку відповідь-підказку у спливаючому вікні;
- враховує **контекст**: PDF, слайди, нотатки та скриншот екрана;
- після лекції генерує **конспект**, Q&A та **картки для повторення**; зберігає історію сесій.

Семестровий проєкт з дисципліни «Основи ООП» (2025–2026), команда з двох студентів. Закриває лабораторні №1–3.

## Документи

| Файл | Зміст |
|---|---|
| [HANDOFF.md](HANDOFF.md) | Повний контекст проєкту: вимоги, архітектура, план |
| [ELEMENTS.md](ELEMENTS.md) | Реєстр елементів ООП і GUI (лаби 1, 2) |
| [LIBRARIES.md](LIBRARIES.md) | Зовнішні бібліотеки та їхні можливості (лаба 3) |
| [RETROSPECTIVE.md](RETROSPECTIVE.md) | Ретроспектива використання бібліотек (лаба 3) |
| [AI_USAGE.md](AI_USAGE.md) | Журнал використання ШІ (академічна доброчесність) |

## Структура рішення (план)

```
src/
  LectureMate.Core/            домен, абстракції (без WPF і SDK провайдерів)
  LectureMate.Infrastructure/  NAudio, STT, LLM, PdfPig, EF Core, QuestPDF, DPAPI, Demo/
  LectureMate.App/             WPF, MVVM, ресурси .resx — єдиний .exe
tests/
  LectureMate.Core.Tests/
  LectureMate.Infrastructure.Tests/
benchmarks/LectureMate.Benchmarks/
docs/                          DocFX
samples/                       тестові WAV, сценарії, PDF
```

Залежності: `App → Infrastructure → Core`, `App → Core`.

## Збірка

Потрібно: Windows 10/11, [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet build
dotnet test
```

Режим «Демо» працює без мережі та API-ключів.

## Робота з git

- `main` захищена; робота в гілках `feature/<коротко>`, злиття через PR з рев'ю іншого учасника;
- коміти англійською у стилі [Conventional Commits](https://www.conventionalcommits.org/): `feat(audio): add loopback source`.
