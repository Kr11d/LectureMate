# Зовнішні бібліотеки (лаба 3)

Для кожної бібліотеки вказано, **які саме можливості** використано. Нова залежність додається лише за погодженням команди й одразу описується тут.

| Бібліотека (NuGet) | Версія | Можливості, які використовуємо | Де в коді | Відповідальний |
|---|---|---|---|---|
| NAudio | — | WASAPI loopback, мікрофон, ресемплінг (WDL), читання WAV | — | A |
| NHotkey.Wpf | — | Глобальні гарячі клавіші | — | A |
| Serilog (+ Sinks.File, Extensions.Hosting) | — | Структуроване логування, ротація файлів | — | A |
| OpenAI *або* Anthropic SDK | — | Streaming, vision, структуровані відповіді, prompt caching | — | B |
| UglyToad.PdfPig | — | Текст із PDF посторінково | — | B |
| QuestPDF | — | Експорт конспекту в PDF (Community License) | — | B |
| Microsoft.EntityFrameworkCore.Sqlite | — | Локальна БД, міграції | — | B |
| System.Security.Cryptography.ProtectedData | — | DPAPI для API-ключів | — | B |
| CommunityToolkit.Mvvm | — | MVVM (спільна інфраструктура) | — | спільне |
| Microsoft.Extensions.Hosting / DI | — | DI, хостинг (спільна інфраструктура) | — | спільне |

Не для лаби 3: xUnit, NSubstitute (тести), BenchmarkDotNet, DocFX.

Не використовуємо: FluentAssertions (комерційна ліцензія з v8), Moq.

Колонки «Версія» та «Де в коді» заповнюються під час підключення бібліотеки.
