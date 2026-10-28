# Реєстр елементів (лаби 1 і 2)

Оновлюється після кожної зміни коду. Формат описано в `HANDOFF.md`, розділ 15.1.

| # | Тип | Назва | Файл | Примітка |
|---|-----|-------|------|----------|
| 1 | інтерфейс | ITimestamped | src/LectureMate.Core/Domain/ITimestamped.cs | обмеження для TimeWindowBuffer<T> (статичний поліморфізм, Д2) |
| 2 | запис | AudioChunk | src/LectureMate.Core/Domain/AudioChunk.cs | 2 поля: Data, Timestamp |
| 3 | запис | TranscriptSegment | src/LectureMate.Core/Domain/TranscriptSegment.cs | 5 полів; реалізує ITimestamped |
| 4 | запис | QuestionCandidate | src/LectureMate.Core/Domain/QuestionCandidate.cs | 5 полів; реалізує ITimestamped |
| 5 | перелік | QuestionSource | src/LectureMate.Core/Domain/QuestionSource.cs |  |
| 6 | запис | QaItem | src/LectureMate.Core/Domain/QaItem.cs | 6 полів |
| 7 | перелік | QaStatus | src/LectureMate.Core/Domain/QaStatus.cs |  |
| 8 | запис | LatencySample | src/LectureMate.Core/Domain/LatencySample.cs | 3 поля; реалізує ITimestamped |
| 9 | перелік | LatencyStage | src/LectureMate.Core/Domain/LatencyStage.cs |  |
| 10 | перелік | SessionState | src/LectureMate.Core/Domain/SessionState.cs |  |
| 11 | клас (static) | AudioFormats | src/LectureMate.Core/Domain/AudioFormats.cs | 5 констант формату |
| 12 | метод (нетрив.) | AudioFormats.BytesToDuration | src/LectureMate.Core/Domain/AudioFormats.cs | вирівнювання на семпл, перевірка аргументу |
| 13 | метод (нетрив.) | AudioFormats.DurationToBytes | src/LectureMate.Core/Domain/AudioFormats.cs | вирівнювання на семпл, перевірка переповнення |
| 14 | клас (абстр.) | LectureMateException | src/LectureMate.Core/Errors/LectureMateException.cs | корінь ієрархії винятків (5 класів) |
| 15 | клас | AudioCaptureException | src/LectureMate.Core/Errors/AudioCaptureException.cs | ієрархія LectureMateException |
| 16 | клас | TranscriptionException | src/LectureMate.Core/Errors/TranscriptionException.cs | ієрархія LectureMateException |
| 17 | клас | LlmProviderException | src/LectureMate.Core/Errors/LlmProviderException.cs | ієрархія LectureMateException; 2 поля: StatusCode, IsRetryable |
| 18 | клас | ContextLoadException | src/LectureMate.Core/Errors/ContextLoadException.cs | ієрархія LectureMateException |
| 19 | метод (нетрив.) | LlmProviderException.IsRetryableStatusCode | src/LectureMate.Core/Errors/LlmProviderException.cs | 429 і 5xx — повторювані |
| 20 | ієрархія | LectureMateException → 4 нащадки | src/LectureMate.Core/Errors/ | 5 класів (≥ 3) |

## Підсумок на команду

Поля: властивості записів і властивості зі станом (автовластивості з полем, яке генерує компілятор); константи не рахуються.

| Елемент | Є | Мінімум |
|---|---|---|
| Класи (та подібні типи) | 16 (11 класів і записів, 1 інтерфейс, 4 переліки) | 9 |
| Поля | 23 | 15 |
| Нетривіальні методи | 3 | 25 |
| Ієрархії успадкування (з них ≥ 3 класів) | 1 (1) | 2 (1) |
| Поліморфізм: статичний / динамічний / усього | 0 / 0 / 0 | 1 / 1 / 3 |
| Вікна / форми | 0 | 4 |
| Елементи керування | 0 | 20 |
| Контейнери (список, таблиця, дерево) | 0 | 1 |
| Обробники подій | 0 | 10 |
