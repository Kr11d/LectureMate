# Реєстр елементів (лаби 1 і 2)

Оновлюється після кожної зміни коду. Формат описано в `HANDOFF.md`, розділ 15.1.

| # | Тип | Назва | Файл | Примітка |
|---|-----|-------|------|----------|
| 1 | запис (абстр.) | MessagePart | src/LectureMate.Core/Llm/MessagePart.cs | база ієрархії MessagePart |
| 2 | запис | TextPart | src/LectureMate.Core/Llm/TextPart.cs | ієрархія MessagePart; 1 властивість |
| 3 | запис | ImagePart | src/LectureMate.Core/Llm/ImagePart.cs | ієрархія MessagePart; 2 властивості |
| 4 | запис | LlmRequest | src/LectureMate.Core/Llm/LlmRequest.cs | 4 властивості |
| 5 | інтерфейс | ILlmClient | src/LectureMate.Core/Llm/ILlmClient.cs | поліморфізм зарахуємо, коли з'являться реалізації (Д3) і клієнти `AnswerService` (Д5), `SummaryService` (Д6) |
| 6 | enum | AnswerMode | src/LectureMate.Core/Llm/AnswerMode.cs | |
| 7 | запис | AnswerUpdate | src/LectureMate.Core/Llm/AnswerUpdate.cs | 3 властивості |
| 8 | enum | AnswerUpdateKind | src/LectureMate.Core/Llm/AnswerUpdateKind.cs | |
| 9 | запис | LectureSummary | src/LectureMate.Core/Summary/LectureSummary.cs | 5 властивостей |
| 10 | запис | QaPair | src/LectureMate.Core/Summary/QaPair.cs | 2 властивості |
| 11 | запис | Term | src/LectureMate.Core/Summary/Term.cs | 2 властивості |
| 12 | запис | Flashcard | src/LectureMate.Core/Summary/Flashcard.cs | 2 властивості |
| 13 | ієрархія | MessagePart → TextPart, ImagePart | src/LectureMate.Core/Llm/ | 3 типи |

## Підсумок на команду

| Елемент | Є | Мінімум |
|---|---|---|
| Класи (та подібні типи) | 12 (9 записів, 1 інтерфейс, 2 enum) | 9 |
| Поля | 21* | 15 |
| Нетривіальні методи | 0 | 25 |
| Ієрархії успадкування (з них ≥ 3 класів) | 1 (1) | 2 (1) |
| Поліморфізм: статичний / динамічний / усього | 0 / 0 / 0 | 1 / 1 / 3 |
| Вікна / форми | 0 | 4 |
| Елементи керування | 0 | 20 |
| Контейнери (список, таблиця, дерево) | 0 | 1 |
| Обробники подій | 0 | 10 |

\* Властивості позиційних записів: для кожної компілятор створює приховане поле. Якщо викладач рахує лише явно оголошені поля, тут 0.
