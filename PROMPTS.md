# Промпти для ШІ-агента на кожен день

Як користуватися:
1. Зранку: `git switch main && git pull`, відкрити **нову** сесію Claude Code у корені репозиторію.
2. Вставити промпт свого дня (A або B). Загальні правила агент бере з `CLAUDE.md` автоматично, тому в промптах їх не повторюємо.
3. Якщо задача дня велика, агент може зупинитися посередині: тоді напиши «продовжуй із пункту N».
4. Якщо агент пропонує змінити дизайн з HANDOFF, рішення приймаєте ви, а не агент (HANDOFF §17.11).
5. Після злиття PR прочитай, що згенерував агент: на захисті треба пояснити кожен рядок.

Розклад і точки синхронізації: [PLAN.md](PLAN.md).

---

## Фаза 1. Core і лаба 1

### Д1 · Ср 28.10 · A — домен, винятки, CI

```text
Я студент A (звук, мова, сесія). Прочитай HANDOFF.md повністю, особливо §6, §7, §8.11, §14.6.
Сьогодні фундамент Core і CI. Зроби ДВА окремі PR.

PR 1 — гілка feature/core-domain (B на нього чекає, тому зроби першим):
1. src/LectureMate.Core/Domain/: усі типи з §7, КРІМ LectureSummary, QaPair, Term, Flashcard (їх робить B):
   ITimestamped, AudioChunk, TranscriptSegment, QuestionCandidate + QuestionSource,
   QaItem + QaStatus, LatencySample + LatencyStage, SessionState.
2. Domain/AudioFormats: константи 16 кГц, моно, 16 біт; методи BytesToDuration(int bytes) і
   DurationToBytes(TimeSpan) з вирівнюванням на розмір семпла.
3. src/LectureMate.Core/Errors/: abstract LectureMateException і 4 нащадки з §8.11.
   LlmProviderException: StatusCode (int?), IsRetryable (429 і 5xx — так; 401/403/400 — ні;
   null — визначається параметром конструктора, напр. для таймауту).
4. Тести в LectureMate.Core.Tests: AudioFormats (обидва напрямки, непарні байти),
   IsRetryable як [Theory] для 400/401/403/429/500/503/null.

PR 2 — гілка chore/ci:
5. .github/workflows/ci.yml: windows-latest, actions/setup-dotnet з 10.0.x, restore,
   build -c Release, test (з фільтром Category!=Live), dotnet format --verify-no-changes.
   Тригери: push у main і pull_request.

Готово, коли: обидва PR відкриті, CI зелений, чекліст CLAUDE.md виконано.
```

### Д1 · Ср 28.10 · B — контракти LLM, спайк LLM

```text
Я студент B (ШІ, контекст, дані). Прочитай HANDOFF.md повністю, особливо §4.1, §7, §8.6, §12.

Частина 1 — гілка feature/core-llm-contracts:
1. src/LectureMate.Core/Llm/: MessagePart, TextPart, ImagePart, LlmRequest, ILlmClient (§8.6),
   AnswerMode, AnswerUpdate, AnswerUpdateKind. Лише типи, без реалізацій.
2. src/LectureMate.Core/Summary/: LectureSummary, QaPair, Term, Flashcard (§7).
3. XML-документація для всього public. Тести тут не потрібні (немає логіки).
Якщо A ще не злив feature/core-domain, не створюй дублікати його типів: зачекай або зроби rebase.

Частина 2 — спайк LLM (HANDOFF §16, M0), гілка spike/llm-latency:
4. tools/spikes/LlmLatency/ — окремий консольний проєкт, НЕ додавай його в LectureMate.sln.
   Ключі лише зі змінних середовища OPENAI_API_KEY і ANTHROPIC_API_KEY.
5. Сценарій: system-промпт з §12 українською, кешований префікс ~15–20 тис. токенів
   (будь-який український навчальний текст), питання українською. По 10 запусків для
   OpenAI і Anthropic (швидкі моделі; перевір актуальні назви моделей у документації),
   streaming. Міряй час до першого токена і загальний час, друкуй p50/p90.
   Окремо: перший запит (холодний кеш) і наступні (кеш префікса). Для Anthropic постав
   cache_control на межі префікса.
6. Сьогодні — каркас і перші заміри; завтра — висновок у RETROSPECTIVE.md.
Не коміть ключі, логи з текстами відповідей і великі файли.
```

### Д2 · Чт 29.10 · A — спайк STT, узагальнені типи

```text
Я студент A. Прочитай HANDOFF.md §8.2 (спайк M0), §8.8, §4.2.

Частина 1 — спайк STT, гілка spike/stt-quality:
1. tools/spikes/SttQuality/ — консольний проєкт поза .sln. Вхід: WAV із записом лекції
   (шлях аргументом, файл НЕ комітимо). Прогнати через Soniox, Deepgram і OpenAI (актуальна
   модель транскрипції; звір API з документацією). Мова: українська.
2. Зберегти транскрипти в tools/spikes/SttQuality/out/ (додай у .gitignore).
3. Утиліта WER: еталон (я вручну напишу текст перших 2 хв у reference.txt) проти кожного
   транскрипту; нормалізація (регістр, пунктуація, апостроф ’/'/ʼ). Левенштейн на словах.
4. Для кожного провайдера зафіксуй: WER, чи є потоковий режим з partial для української,
   ціну за годину, чи можна передати словник термінів.
5. Запиши таблицю й рішення в RETROSPECTIVE.md (розділ «Спайк M0»). Якщо переміг не Soniox —
   онови назви класів у HANDOFF §8.2 і §11 окремим комітом.

Частина 2 — гілка feature/core-generics:
6. Core/Common/TimeWindowBuffer<T> where T : ITimestamped: Add, Snapshot() (останні N за
   часом відносно найпізнішого End), обрізання старих елементів при Add, Clear, Count.
   Потокобезпечний (lock). Вікно задається в конструкторі.
7. Core/Common/Retry.ExecuteAsync<T> + RetryOptions(MaxAttempts, InitialDelay, Multiplier, MaxDelay)
   з експоненційним backoff. Для тестованості затримки — System.TimeProvider (BCL).
   Перевага — ще перевантаження без результату (Task) поверх generic-версії.
8. Тести: вікно обрізає старе, порядок, межі; Retry — успіх з 1 спроби, повтор на retryable,
   негайна відмова на non-retryable, вичерпання спроб кидає останній виняток,
   скасування через CancellationToken, послідовність затримок 1-2-4-8.
```

### Д2 · Чт 29.10 · B — ієрархія контексту, рішення по LLM

```text
Я студент B. Прочитай HANDOFF.md §8.4, §8.5, §12 (порядок частин запиту).

Гілка feature/core-context:
1. Core/Context/ContextItem (abstract, §8.4) + TextNoteContext, PdfDocumentContext, ImageContext.
   - TextNoteContext: Text; EstimateTokens = символи / 4 (округлення вгору).
   - PdfDocumentContext: IReadOnlyList<string> Pages (текст уже витягнутий), FromPage/ToPage
     (1-based, валідація меж). ToMessageParts віддає TextPart з маркерами «[стор. N]».
   - ImageContext: Bytes, MediaType, Width, Height; EstimateTokens = width*height/750 (формула
     Anthropic, достатньо грубо). ToMessageParts віддає ImagePart.
2. Core/Context/IPdfTextExtractor: Task<IReadOnlyList<string>> ExtractPagesAsync(Stream, CancellationToken);
   помилки — ContextLoadException.
3. Core/Context/SessionContext.Create(items, tokenBudget): бере лише IsEnabled, робить незмінний
   знімок, детермінований порядок (AddedAt, потім Id), EstimatedTokens, IsWithinBudget,
   ToCachedParts(). Важливо: два виклики ToCachedParts дають однаковий вміст (кешування префікса).
4. Тести: оцінки токенів, діапазон сторінок і помилки меж, вимкнені матеріали не входять,
   межа бюджету (рівно бюджет = в межах), порядок не залежить від порядку на вході,
   зміна IsEnabled після Create не впливає на знімок.

Спайк (гілка spike/llm-latency):
5. Добери заміри, порахуй p50/p90 для холодного і теплого кешу, запиши таблицю й рішення
   (OpenAI чи Anthropic, яка модель) у RETROSPECTIVE.md. Обґрунтування: затримка першого
   токена українською, якість короткої відповіді на око, ціна, кешування.
```

### Д3 · Пт 30.10 · A — детектор питань, LatencyTracker, інтерфейси звуку і STT

```text
Я студент A. Прочитай HANDOFF.md §8.1, §8.2, §8.3, §8.9 (LatencyTracker), §4.1.

Гілка feature/core-detector:
1. Core/Detection/DetectorRule, RuleKind (§8.3).
2. Core/Detection/HeuristicQuestionDetector: конструктор (rules, questionMarkWeight, threshold);
   Analyze(TranscriptSegment) повертає QuestionCandidate або null.
   - тільки IsFinal сегменти; правила лише тієї ж мови, що й сегмент;
   - нормалізація: нижній регістр, пробіли, апострофи ’ ' ʼ → один символ;
   - InterrogativeWord — лише перше слово; TriggerPhrase — будь-де, по межах слів
     («щодо» НЕ спрацьовує на «що»); AntiTrigger — від'ємна вага; знак «?» в кінці — окрема вага;
   - Score = сума ваг; кандидат, якщо Score >= threshold; Source = Heuristic.
   Без алокацій у гарячому циклі, де це просто (бюджет < 5 мс, а реально мікросекунди).
3. Core/Detection/DefaultDetectorRules: стартовий набір uk/en з §8.3 з розумними вагами.
4. Core/Diagnostics/LatencyTracker: Record(LatencyStage, start, end) на TimeWindowBuffer<LatencySample>
   (вікно 5 хв), GetPercentile(stage, 0.5/0.9), Last(stage). Це другий клієнт TimeWindowBuffer<T>.
5. Core/Audio/IAudioSource (§8.1). Core/Transcription/ITranscriber, TranscriptionOptions,
   TranscriberStatus (Connected, Reconnecting, Failed) (§8.2). Лише інтерфейси.
6. Тести: [Theory] ≥ 20 фраз uk/en з очікуваним результатом, зокрема негативні (твердження,
   «чи не так?», «щодо цього»), partial ігнорується, чужа мова, межа порогу. LatencyTracker:
   p50/p90 на відомих даних, старі семпли випадають з вікна.
```

### Д3 · Пт 30.10 · B — базовий LLM-клієнт, фейк, інтерфейси сховищ

```text
Я студент B. Прочитай HANDOFF.md §8.6, §8.8, §8.12, §13 (рядки про LLM), §14.3.

Гілка feature/core-llm-base:
1. Core/Llm/LlmClientBase (Template Method):
   - public StreamAsync: валідація запиту; Retry (з Core/Common, зроблений A) на
     LlmProviderException.IsRetryable — ДО ПЕРШОГО токена, максимум 2 повтори; після першого
     токена помилка летить далі без повтору;
   - таймаут до першого токена (за замовчуванням 8 с, у конструкторі) → LlmProviderException
     з ознакою таймауту, яку UI зможе відрізнити;
   - логування через ILogger: провайдер, довжина й хеш промпту, час першого токена; НЕ текст;
   - protected abstract StreamCoreAsync і CompleteJsonCoreAsync(request, jsonSchema, ct).
   - public CompleteJsonAsync<T>: JSON-схема з T через System.Text.Json.Schema.JsonSchemaExporter,
     десеріалізація; невалідний JSON → LlmProviderException (non-retryable).
2. Infrastructure/Demo/FakeLlmClient : LlmClientBase: детерміновані відповіді з налаштовною
   затримкою першого токена й між токенами; якщо в Prompt є маркерна фраза зі сценарію —
   відповідає «NO_QUESTION»; можна налаштувати кидання винятку (для тестів);
   CompleteJsonAsync повертає правдоподібний LectureSummary.
3. Core/Courses/Course (Id, Name, Language, SttTerms), Core/Lectures/Lecture (Id, CourseId,
   StartedAt, EndedAt, сегменти, QaItems, Summary?) — мінімально, без зайвих полів.
4. Core/Storage/ICourseStore і ILectureStore — лише інтерфейси: курси CRUD, матеріали CRUD,
   правила детектора CRUD; лекції: CreateAsync, AppendSegmentsAsync(пакет), SaveQaItemAsync,
   SaveSummaryAsync, ListAsync(фільтр: текст, період), GetAsync, DeleteAsync.
   Напиши в описі PR сигнатури ILectureStore: A використає їх у Д6.
5. Тести (через тестового нащадка LlmClientBase): retry до першого токена, без retry після,
   таймаут, скасування, невалідний JSON, non-retryable 401 без повторів.
```

### Д4 · Сб 31.10 · A — NAudio, джерела звуку

```text
Я студент A. Прочитай HANDOFF.md §6.3, §8.1, §11 (NAudio).

Гілка feature/audio-sources:
1. Додай NAudio в LectureMate.Infrastructure (погоджено в §11). Рядок у LIBRARIES.md
   (версія, можливості, де в коді) і перший запис у RETROSPECTIVE.md.
2. Infrastructure/Audio/AudioSourceBase : IAudioSource (Template Method):
   - стан і перевірки (повторний Start → InvalidOperationException, Stop без Start — без помилки);
   - protected метод публікації сирого буфера з довільним WaveFormat: ресемплінг до 16 кГц моно
     PCM16 (WdlResamplingSampleProvider, змішування в моно), Timestamp від початку;
   - рівень RMS 0..1 → LevelChanged (не частіше ~20 разів/с);
   - виклик подій безпечний: виняток у підписника не валить захоплення;
   - колбек NAudio лише копіює буфер, без важкої роботи (§6.3).
3. Infrastructure/Demo/WaveFileAudioSource: читає WAV у реальному темпі шматками по 100 мс;
   параметр Speed (1.0 — реальний час, 0 — максимально швидко для тестів); кінець файлу — Stop.
4. Тести в Infrastructure.Tests: згенеруй у temp WAV 48 кГц стерео з синусоїдою 1 с →
   на виході сумарно ~1 с аудіо у форматі AudioFormats; тиша дає рівень 0;
   повторний StartAsync кидає виняток; StopAsync зупиняє події.
Окремо (руками): я запишу samples/demo-lecture.wav — 1–2 хв, 3–4 питання, одне риторичне.
Підкажи мені текст такої міні-лекції українською (тема — основи ООП), щоб я його начитав.
```

### Д4 · Сб 31.10 · B — PromptFactory

```text
Я студент B. Прочитай HANDOFF.md §8.7, §12, §4.1 (рядок про побудову промпту), §14.1.

Гілка feature/core-prompts:
1. Core/Prompts/PromptFactory і вхідні записи:
   AnswerPromptInput(CourseName, ResponseLanguage, AnswerStyle, AnswerMode, SessionContext,
   RecentTranscript (IReadOnlyList<TranscriptSegment>), Question (QuestionCandidate),
   Screenshot (ImageContext?)), ElaboratePromptInput, SummaryPromptInput(повний транскрипт, Q&A).
2. Тексти з §12 у Core/Resources/Prompts.resx (uk) і Prompts.en.resx (переклад). Мова промпту
   обирається за мовою відповіді, а не за культурою інтерфейсу.
3. Порядок: System → CachedContext = SessionContext.ToCachedParts() → Prompt:
   «Останні 3 хв лекції:» (рядки «[mm:ss] текст») → «Питання: …» → скриншот останнім.
   Auto додає інструкцію NO_QUESTION; Manual — інструкцію «останні 30 секунд».
   Elaborate: замість «1–3 речення» → «розгорнуто, до 2 абзаців».
   MaxTokens: відповідь 200, детальніше 600, конспект 4000 (константи).
4. Тести: CachedContext байт-у-байт однаковий для двох запитів з різними питаннями
   (порівняння серіалізованих частин); Auto містить маркер, Manual — ні; скриншот — останній
   елемент Prompt; en-відповідь дає англійські тексти; Elaborate відрізняється лише інструкцією.
5. Якщо PromptFactory дуже часто створює рядки — це нормально, оптимізувати будемо за
   бенчмарком у Д18.
```

### Д5 · Нд 01.11 · A — транскрибери

```text
Я студент A. Прочитай HANDOFF.md §4.2, §6.2, §8.2, §13 (рядок про обрив WebSocket).

Гілка feature/transcribers:
1. Infrastructure/Transcription/TranscriberBase : ITranscriber (Template Method):
   - StartAsync запускає фоновий цикл: ConnectCoreAsync через Retry (затримки 1, 2, 4, 8, 16 с,
     5 спроб) → паралельно «читати канал → SendCoreAsync» і «ReceiveCoreAsync → SegmentReceived»;
   - обрив з'єднання (TranscriptionException) → StatusChanged(Reconnecting), канал НЕ читаємо
     (він накопичує звук), перепідключаємось; після 5 невдач → StatusChanged(Failed);
   - зсув часу: мітки сегментів нового з'єднання + час від початку сесії до старту з'єднання;
   - якщо обрив довший за ємність каналу (~10 с), видати фінальний сегмент-позначку пропуску
     (текст — константа, UI покаже його з ресурсів);
   - StopAsync: коректне завершення, без винятків при подвійному виклику.
   Абстрактні: ConnectCoreAsync, SendCoreAsync(AudioChunk), ReceiveCoreAsync (IAsyncEnumerable),
   DisconnectCoreAsync.
2. Infrastructure/Demo/ScriptedTranscriber: читає JSON-сценарій [{start, end, text, lang}];
   синхронізується з аудіо: віддає сегмент, коли Timestamp прочитаних фрагментів пройшов його End;
   перед фінальним віддає 1–2 partial (наростаючі слова).
3. samples/demo-lecture.json — сценарій під samples/demo-lecture.wav (я його записав;
   якщо файлу ще нема — зроби сценарій за текстом із Д4, мітки я підправлю).
4. Тести: тестовий нащадок TranscriberBase, що падає N разів → кількість спроб, події статусу,
   Failed після 5; зсув часу після перепідключення; ScriptedTranscriber віддає сегменти по порядку
   і partial перед final. Затримки — через TimeProvider, тести не повинні спати секундами.
```

### Д5 · Нд 01.11 · B — AnswerService

```text
Я студент B. Прочитай HANDOFF.md §4.1, §6.3, §8.6 (маркер NO_QUESTION), §13.

Гілка feature/answer-service:
1. Core/Answers/AnswerService: залежності — ILlmClient, PromptFactory, ILectureStore,
   LatencyTracker, ILogger; сесійні дані (курс, мова, стиль, SessionContext, доступ до
   останніх сегментів) — через конструктор або метод ініціалізації, обери простіше.
2. AskAsync(question, mode, screenshot, ct):
   - скасовує попередню незавершену відповідь (linked CancellationTokenSource);
   - створює QaItem (Answering), подія Started;
   - Auto: буферизує початок потоку, поки він збігається з префіксом «NO_QUESTION» (без
     урахування пробілів на початку). Щойно розійшлося — віддає весь буфер Token-ами й далі
     без затримки. Повний збіг → Discarded, нічого не показуємо;
   - Manual: маркер не перевіряє;
   - кінець → Completed; LlmProviderException/таймаут → Failed з причиною, яку UI покаже;
   - затримки FirstToken і Total → LatencyTracker і в QaItem.Latencies; QaItem зберігається
     через ILectureStore.SaveQaItemAsync.
   - Що робити зі скасованою відповіддю (статус, частковий текст) — обери й обґрунтуй у PR.
3. ElaborateAsync(qaId, ct): той самий контекст, запит Elaborate, окремий QaItem або оновлення
   існуючого — обери простіше й опиши.
4. Тести (FakeLlmClient або NSubstitute): повний маркер → Discarded; «NO» потім розходження →
   увесь текст показано; Manual ігнорує маркер; нове питання скасовує попереднє; 401 → Failed
   без повторів; таймаут → Failed; затримки записано; QaItem збережено.
```

### Д6 · Пн 02.11 · A — LectureSession і фабрика провайдерів

```text
Я студент A. Прочитай HANDOFF.md §6.2, §6.3, §8.9, §4.2, §13. Подивись API AnswerService (B, Д5)
і ILectureStore (B, Д3).

Гілка feature/lecture-session:
1. Core/Session/SessionSetup (курс, IAudioSource, ITranscriber, TranscriptionOptions,
   SessionContext, AnswerService, налаштування детектора, автодетекція on/off).
2. Core/Session/LectureSession (State):
   - явна таблиця дозволених переходів SessionState; недозволений → InvalidOperationException;
   - Start: Channel<AudioChunk> обмежений (~10 с аудіо), DropOldest; ChunkAvailable → TryWrite;
     старт транскрибера; підписки на події;
   - фінальні сегменти: TimeWindowBuffer (3 хв) → пакетний запис ILectureStore.AppendSegmentsAsync
     (кожні 5 с або 20 сегментів) → детектор (якщо автодетекція) → QuestionDetected →
     AnswerService.AskAsync(Auto);
   - AskNowAsync(screenshot): кандидат Manual з останніх 30 с;
   - Pause/Resume/Stop; Stop дописує незбережені сегменти;
   - аудіо Faulted → Paused (§13); транскрибер Failed → Faulted;
   - затримки SttFinal і Detection → LatencyTracker.
3. Core/Providers/IProviderFactory, AudioSourceSettings; Infrastructure/Providers/ProviderFactory:
   гілка «Демо» (WaveFileAudioSource + ScriptedTranscriber + FakeLlmClient); реальна гілка поки
   кидає NotSupportedException з поясненням (зробимо в Д13–Д14).
4. Тести: переходи станів (дозволені й ні), pause/resume, Stop зберігає залишок сегментів,
   збій аудіо → Paused, збій STT → Faulted, виявлене питання запускає відповідь (FakeLlmClient),
   AskNowAsync бере лише останні 30 с.
```

### Д6 · Пн 02.11 · B — конспект і експорт

```text
Я студент B. Прочитай HANDOFF.md §8.10, §12 (конспект), §13 (рядок «Транскрипт задовгий»).

Гілка feature/summary-export:
1. Core/Summary/SummaryService(ILlmClient, PromptFactory, ILectureStore, ILogger):
   GenerateAsync(lectureId, ct) — завантажує лекцію, перевіряє довжину транскрипту проти ліміту
   (константа/налаштування; оцінка символи/4), один запит CompleteJsonAsync<LectureSummary>,
   зберігає через SaveSummaryAsync. Задовгий транскрипт — зрозуміла помилка з ієрархії §8.11
   без нового типу (якщо можливо), транскрипт і Q&A не губляться.
2. Core/Export/ExporterBase (Template Method): FormatName, FileExtension; ExportAsync перевіряє
   аргументи й скасування, UTF-8, загортає IO-помилки; WriteAsync абстрактний.
3. Core/Export/MarkdownExporter: заголовок, тези, Q&A, терміни, картки.
4. Core/Export/FlashcardCsvExporter: формат для імпорту в Anki (2 колонки front/back), коректне
   екранування лапок, ком, переносів рядків.
   PdfExporter (QuestPDF) — в Д14, в Infrastructure.
5. Тести: Markdown — порівняння з очікуваним текстом; CSV — складні рядки; ExporterBase —
   null-аргументи й скасування; SummaryService — успіх зберігає конспект, задовгий транскрипт,
   помилка LLM не губить дані.
```

### Д7 · Вт 03.11 · A — інтеграційний тест, налаштування, аудит лаби 1

```text
Я студент A. Прочитай HANDOFF.md §2.1, §9, §14.1, §15.1, §8.12 (ISettingsStore).

Гілка feature/pipeline-integration:
1. Інтеграційний тест у Infrastructure.Tests без мережі: WaveFileAudioSource(Speed=0) +
   ScriptedTranscriber(samples/demo-lecture.json) + HeuristicQuestionDetector + AnswerService
   (FakeLlmClient) + ILectureStore (NSubstitute) через LectureSession. Перевір: усі фінальні
   сегменти збережено, питання виявлено, риторичне → Discarded, інші → Completed.
   Виправ баги, які він знайде (якщо баг у зоні B — опиши в PR і скажи мені).
2. Core/Settings/AppSettings (record: режим Demo/Real, id пристрою, джерело, мова інтерфейсу,
   мова відповіді, стиль, бюджет токенів, поріг детектора, вага «?», автодетекція),
   ISettingsStore; Infrastructure/Settings/JsonSettingsStore у %LOCALAPPDATA%\LectureMate\settings.json.
   Пошкоджений файл → значення за замовчуванням + лог попередження. Тести з temp-каталогом.
3. Повний аудит ELEMENTS.md: пройди весь код і перелічи класи/записи/інтерфейси, поля,
   нетривіальні методи, ієрархії, випадки поліморфізму з клієнтським кодом (формат §15.1).
   Підсумкова таблиця проти мінімумів §2.1. Якщо чогось бракує — скажи, НЕ додавай класи
   «для кількості».
```

### Д7 · Вт 03.11 · B — EF Core

```text
Я студент B. Прочитай HANDOFF.md §7 (що зберігається), §8.12, §13 (рядок про БД), §14.1.

Гілка feature/ef-storage:
1. Додай Microsoft.EntityFrameworkCore.Sqlite (+ Design у тому проєкті, де потрібні міграції);
   рядок у LIBRARIES.md, запис у RETROSPECTIVE.md.
2. Infrastructure/Storage/AppDbContext: Courses, ContextItems (TPH з дискримінатором для 3
   нащадків), DetectorRules, Lectures, сегменти, QaItems (Latencies — JSON), Summary — JSON-колонка.
   Мапінг доменних типів напряму через fluent-конфігурацію (окремі *Record класи лише якщо
   прямий мапінг неможливий; поясни в PR). БД: %LOCALAPPDATA%\LectureMate\lecturemate.db.
3. EfCourseStore, EfLectureStore (реалізації інтерфейсів з Д3). AppendSegmentsAsync — пакетний
   запис однією транзакцією. ListAsync — пошук за текстом транскрипту/назвою курсу і періодом.
4. Початкова міграція; при першому запуску — Migrate + сід DefaultDetectorRules.
5. Файли матеріалів копіюються в %LOCALAPPDATA%\LectureMate\materials (шлях зберігається
   відносно цієї папки).
6. Тести на SQLite in-memory (відкрите з'єднання на тест): CRUD курсу, round-trip усіх трьох
   ContextItem, пакетні сегменти, пошук і фільтр за датами, видалення лекції з усіма даними,
   пошкоджений файл БД → зрозумілий виняток.
Увечері разом з A: перевірка чекпойнта лаби 1 за ELEMENTS.md.
```

---

## Фаза 2. GUI і лаба 2

### Д8 · Ср 04.11 · A — каркас застосунку

```text
Я студент A. Прочитай HANDOFF.md §6.3, §10 (вступ), §14.3, §14.5, §17. B сьогодні чекає на мій
PR до обіду — роби мінімальний каркас першим, полірування потім.

Гілка feature/app-shell:
1. Пакети (погоджені в §11): CommunityToolkit.Mvvm, Microsoft.Extensions.Hosting,
   Serilog + Serilog.Sinks.File + Serilog.Extensions.Hosting. Рядки в LIBRARIES.md.
2. App.xaml.cs: Generic Host; DI через Infrastructure/DependencyInjection.AddLectureMateInfrastructure()
   і реєстрацію ViewModel/вікон у App. StartupUri прибрати, MainWindow з DI.
3. Serilog: %LOCALAPPDATA%\LectureMate\logs, щоденна ротація, структуровані поля.
   Глобальні обробники: DispatcherUnhandledException, TaskScheduler.UnobservedTaskException,
   AppDomain.UnhandledException → лог + зрозуміле повідомлення з ресурсів.
4. i18n: Resources/Strings.resx (uk) + Strings.en.resx, публічний генератор, у XAML {x:Static};
   культура з AppSettings при старті. Ключі з префіксом вікна (Main_, Settings_ …).
5. Невеликий сервіс відкриття вікон/діалогів для ViewModel (щоб VM не створювали вікна й
   залишались тестованими) — поясни в PR, чому він потрібен.
6. MainWindow: меню (Матеріали / Історія / Налаштування / Довідка) і порожня розмітка;
   заглушки SettingsWindow і HistoryWindow. Вікна B НЕ створюй — він зробить їх сам.
7. Запуск у режимі «Демо» не падає; міграція БД виконується при старті.
```

### Д8 · Ср 04.11 · B — секрети, PDF, імпорт матеріалів

```text
Я студент B. Прочитай HANDOFF.md §4.3, §8.4, §8.12, §13 (рядок про PDF).

Гілка feature/secrets-pdf-import (не залежить від каркаса A, можна починати зранку):
1. Core/Storage/ISecretStore (Get/Set/Delete за назвою);
   Infrastructure/Security/DpapiSecretStore: ProtectedData (CurrentUser) +
   файл у %LOCALAPPDATA%\LectureMate\secrets. Пакет System.Security.Cryptography.ProtectedData
   (§11). Тести: round-trip, відсутній ключ → null, файл на диску не містить відкритого тексту.
2. Infrastructure/Pdf/PdfPigTextExtractor : IPdfTextExtractor (UglyToad.PdfPig): текст
   посторінково; зашифрований/пошкоджений → ContextLoadException. Тести: тестовий PDF згенеруй
   у тесті через PdfDocumentBuilder з PdfPig (з кирилицею, якщо вдасться), пошкоджені байти.
3. Infrastructure/Context/ContextItemImporter: за розширенням файлу створює PdfDocumentContext /
   ImageContext / TextNoteContext, копіює файл у папку materials, витягує текст PDF один раз.
   Зображення: обмеження довгої сторони (напр. 1568 px) — якщо масштабування потребує WPF,
   винеси його в App і скажи мені.
4. Після злиття каркаса A: зареєструй свої сервіси в DependencyInjection окремим блоком.
5. LIBRARIES.md і RETROSPECTIVE.md для PdfPig і ProtectedData.
```

### Д9 · Чт 05.11 · A — головне вікно

```text
Я студент A. Прочитай HANDOFF.md §3 (S1–S3), §6.3, §10 п.1, §13, §4.3 (нагадування про згоду).

Гілка feature/main-window:
1. MainViewModel (CommunityToolkit.Mvvm): курси (ICourseStore), джерело (системний звук /
   мікрофон / демо-файл), команди Почати / Пауза / Стоп / Відповісти зараз / Скриншот + питання
   (поки заглушка до Д15), автодетекція. Створення сесії через IProviderFactory.
   Блокування старту, якщо SessionContext.IsWithinBudget == false, з поясненням.
2. Живий транскрипт: ObservableCollection; partial оновлює останній рядок, final фіксує;
   ListBox віртуалізований; автопрокрутка, якщо користувач унизу.
3. ListView питань зі статусом (відповідь / відхилено / помилка) — локалізовані назви статусів.
4. ProgressBar рівня звуку; StatusBar: стан, остання затримка, p50 (LatencyTracker), мова.
5. Усі оновлення з Core → Dispatcher.InvokeAsync; жодного .Result/.Wait().
6. Справжні обробники подій (лаба 2): Loaded, Closing (підтвердження зупинки сесії),
   SelectionChanged курсу, ScrollChanged/автопрокрутка, MouseDoubleClick по питанню →
   подія у VM «відкрити відповідь» (її підхопить вікно B — домовся з B про назву).
7. Перший старт сесії: нагадування про згоду викладача і передачу даних провайдерам (§4.3),
   прапорець «не показувати знову» в налаштуваннях.
8. Тести MainViewModel (без WPF): старт/стоп, блок бюджету, статуси питань.
```

### Д9 · Чт 05.11 · B — спливаюче вікно відповіді

```text
Я студент B. Прочитай HANDOFF.md §3 (S2, S4), §4.1 (показ < 50 мс), §10 п.2, §13.

Гілка feature/answer-overlay:
1. AnswerOverlayViewModel: підписка на AnswerService.AnswerUpdated; питання, текст відповіді
   (дозапис токенів без перебудови всього тексту), джерело (евристика / вручну / скриншот),
   стан; команди Копіювати, Детальніше (ElaborateAsync), Закріпити, Закрити, Повторити (Failed).
   Discarded — вікно не показується.
2. AnswerOverlayWindow: Topmost, ShowInTaskbar=False, без рамки, ShowActivated=False і
   WS_EX_NOACTIVATE (не красти фокус у Zoom); позиція — правий нижній кут робочої області.
3. Обробники: MouseLeftButtonDown → DragMove; MouseEnter/MouseLeave — пауза таймера
   автоприховування; Tick DispatcherTimer; ValueChanged Slider прозорості.
4. Повідомлення про помилки зрозумілою мовою з ресурсів: таймаут, 401 (кнопка «Відкрити
   налаштування»), мережа.
5. Інтеграція з MainWindow: подія «відкрити відповідь» від A (домовтеся про назву) і показ
   при кожній новій відповіді. Метод Hide/Show для скриншота (A використає в Д15).
6. Тести ViewModel: дозапис токенів, Discarded не показує, Failed вмикає «Повторити».
```

### Д10 · Пт 06.11 · A — вікно налаштувань

```text
Я студент A. Прочитай HANDOFF.md §10 п.6, §8.3, §4.3, §14.5.

Гілка feature/settings-window:
1. SettingsWindow з TabControl і SettingsViewModel (ISettingsStore, ISecretStore, ICourseStore).
2. Провайдери: режим Демо/Реальні; PasswordBox для ключів (PasswordChanged у code-behind →
   VM, бо PasswordBox не біндиться; ключ одразу в ISecretStore, у VM не зберігається);
   «Перевірити з'єднання» (короткий запит через ILlmClient; поки в Демо — фейк); бюджет токенів.
   Блок «які дані куди надсилаються» (§4.3).
3. Звук: список пристроїв (поки заглушка — реальний перелік у Д13), індикатор рівня,
   «Тест 5 с».
4. Детектор: DataGrid правил (фраза, тип, вага, мова) з Додати/Редагувати/Видалити через
   ICourseStore; TextBox тестової фрази → TextChanged → живий Score і «питання / ні»;
   Slider чутливості (поріг).
5. Загальне: мова інтерфейсу (після перезапуску — показати підказку), мова відповіді,
   стиль (коротко/детально), підказка з гарячими клавішами.
6. Збереження/скасування змін; валідація (вага — число, фраза не порожня).
7. Тести SettingsViewModel: CRUD правил, живий тест фрази, збереження.
```

### Д10 · Пт 06.11 · B — вікно матеріалів

```text
Я студент B. Прочитай HANDOFF.md §3 (S5), §8.4, §10 п.3, §13 (рядки про PDF і бюджет).

Гілка feature/materials-window:
1. MaterialsViewModel (ICourseStore, ContextItemImporter, ISettingsStore для бюджету).
2. MaterialsWindow: TreeView «курс → матеріали» (HierarchicalDataTemplate); додати /
   перейменувати / видалити курс (підтвердження видалення); кнопки Додати PDF / Зображення /
   Нотатку (OpenFileDialog); Drop файлів (обробники DragOver і Drop).
3. CheckBox «використовувати в сесії»; панель перегляду: текст PDF з діапазоном сторінок,
   зображення, нотатка з редагуванням і збереженням.
4. Лічильник токенів увімкненого набору проти бюджету (SessionContext.Create), червоний,
   якщо перевищено, з підказкою, що звузити.
5. Поле «терміни для розпізнавання» (Course.SttTerms).
6. Матеріал з помилкою (ContextLoadException) позначається в дереві й не ламає решту.
7. Тести MaterialsViewModel: додавання/видалення, перерахунок токенів, помилковий PDF.
```

### Д11 · Сб 07.11 · A — історія сесій

```text
Я студент A. Прочитай HANDOFF.md §3 (S7), §10 п.4, §4.1 (затримки в історії).

Гілка feature/history-window:
1. HistoryViewModel (ILectureStore): DataGrid сесій (дата, курс, тривалість, кількість питань).
2. Пошук: TextBox з TextChanged і затримкою ~300 мс (не запит на кожну літеру); DatePicker'и
   періоду (SelectedDateChanged).
3. TabControl деталей: транскрипт; Q&A з затримками по етапах (STT, детекція, промпт,
   перший токен, разом).
4. Видалити (з підтвердженням), Експорт (список ExporterBase з DI — поліморфний вибір формату,
   SaveFileDialog), Відкрити конспект → SummaryWindow (B) з lectureId — домовся з B про API.
5. Пошкоджена БД (§13): історія показує повідомлення, решта застосунку працює.
6. Тести HistoryViewModel: фільтр, видалення, порожній результат.
```

### Д11 · Сб 07.11 · B — конспект і картки

```text
Я студент B. Прочитай HANDOFF.md §3 (S6), §8.10, §10 п.5.

Гілка feature/summary-window:
1. SummaryViewModel (SummaryService, ILectureStore, IEnumerable<ExporterBase>).
2. SummaryWindow: редаговані назва, тези, терміни; ListView карток з CRUD
   (додати / змінити / видалити); Перегенерувати (з підтвердженням, якщо є ручні правки);
   ComboBox формату експорту (поліморфізм ExporterBase — клієнтський код для лаби 1) + Експорт.
3. Збереження правок у ILectureStore.
4. Генерація: індикатор прогресу, скасування, зрозуміла помилка (задовгий транскрипт, LLM).
5. Інтеграція: після «Стоп» у MainWindow запропонувати згенерувати конспект і відкрити вікно;
   відкриття з HistoryWindow за lectureId (домовся з A).
6. Тести SummaryViewModel: CRUD карток, вибір формату, помилка генерації.
```

### Д12 · Нд 08.11 · A — гарячі клавіші, наскрізний прогін, GUI в ELEMENTS

```text
Я студент A. Прочитай HANDOFF.md §2.2, §3, §10, §15.1.

Гілка feature/hotkeys-polish:
1. NHotkey.Wpf (§11; LIBRARIES.md + RETROSPECTIVE.md): Ctrl+Shift+Space → AskNowAsync,
   Ctrl+Shift+S → поки та сама дія без скриншота (скриншот у Д15). Якщо клавіша зайнята іншою
   програмою — зрозуміле повідомлення, застосунок працює далі.
2. Пройди сценарії S1, S2, S3, S5, S6, S7 у режимі «Демо» (запусти застосунок і дай мені
   покроковий чекліст ручної перевірки). Виправ знайдені баги в моїй зоні; для зони B —
   список для неї/нього.
3. ELEMENTS.md, GUI-частина: усі вікна, контроли, контейнери, обробники подій (окремо
   справжні події й ICommand), підсумок проти мінімумів §2.2.
4. Перевір, що CI зелений і застосунок стартує з чистої папки %LOCALAPPDATA%.
```

### Д12 · Нд 08.11 · B — i18n і наскрізний прогін

```text
Я студент B. Прочитай HANDOFF.md §14.5, §17.5.

Гілка feature/i18n-audit:
1. Знайди всі «зашиті» тексти в XAML і C# (Content=, Text=, Header=, Title=, ToolTip=,
   MessageBox, рядки винятків, які бачить користувач) і перенеси в Strings.resx.
2. Повний англійський переклад Strings.en.resx; перевір, що ключі в обох файлах збігаються
   (напиши тест, який порівнює набори ключів).
3. Формати дат/чисел за культурою; назви статусів і типів правил через ресурси.
4. Запуск з мовою en (після перезапуску) — пройди всі 6 вікон.
5. Наскрізний прогін моїх вікон: Materials → старт сесії → Overlay → Стоп → Summary → експорт
   MD і CSV. Виправ баги; список для A — окремо.
Увечері разом: чекпойнт лаби 2, скриншоти всіх вікон для README.
```

---

## Фаза 3. Реальні провайдери і лаба 3

### Д13 · Пн 09.11 · A — реальне захоплення звуку

```text
Я студент A. Прочитай HANDOFF.md §6.3, §8.1, §13 (рядок про аудіопристрій), §4.2.

Гілка feature/wasapi:
1. Infrastructure/Audio/WasapiAudioSource : AudioSourceBase: WasapiLoopbackCapture для звуку ПК,
   WasapiCapture для мікрофона — за обраним пристроєм (MMDeviceEnumerator, id пристрою).
   Колбек DataAvailable лише копіює буфер і віддає в базовий клас.
2. Пристрій зник/недоступний → AudioCaptureException через подію Faulted → сесія в Paused
   з пропозицією обрати інший пристрій.
3. Перелік пристроїв для SettingsWindow: невеликий інтерфейс у Core (щоб App/Core не залежали
   від NAudio) + реалізація на MMDeviceEnumerator — поясни в PR. «Тест 5 с» показує рівень.
4. ProviderFactory: реальна гілка для звуку (транскрибер і LLM — Д14/B сьогодні).
5. Тести, що не потребують пристрою: мапінг помилок NAudio → AudioCaptureException,
   перелік з NSubstitute. Ручна перевірка: YouTube-відео → рівень рухається.
6. Онови LIBRARIES.md (нові можливості NAudio) і RETROSPECTIVE.md.
```

### Д13 · Пн 09.11 · B — реальний LLM-клієнт

```text
Я студент B. Прочитай HANDOFF.md §8.6, §12, §4.1, §13, і висновок спайку в RETROSPECTIVE.md.

Гілка feature/real-llm:
1. Infrastructure/Llm/<OpenAi|Anthropic>LlmClient : LlmClientBase з офіційним SDK (обраний на
   спайку). Звір актуальне API SDK і назви моделей з документацією.
   - streaming токенів; System окремо; CachedContext — незмінний префікс
     (Anthropic: cache_control на останній частині префікса; OpenAI: автоматичне кешування,
     просто тримати префікс першим і незмінним); Prompt після нього; ImagePart — vision;
   - помилки SDK → LlmProviderException зі StatusCode (401, 429, 5xx, мережа);
   - модель з налаштувань, ключ з ISecretStore (ніколи не логувати).
2. ProviderFactory: створення реального LLM-клієнта (лише цей шматок; файл належить A —
   узгодь злиття).
3. «Перевірити з'єднання» в SettingsWindow працює з реальним ключем.
4. Тести: мапінг помилок (без мережі, якщо SDK дозволяє підмінити HTTP), Live-тест
   [Trait("Category","Live")] з коротким запитом українською, пропускається без ключа.
5. LIBRARIES.md (можливості: streaming, vision, кешування, структуровані відповіді) і
   RETROSPECTIVE.md.
```

### Д14 · Вт 10.11 · A — реальний транскрибер

```text
Я студент A. Прочитай HANDOFF.md §8.2, §11 (примітка про STT), §4.2, і висновок спайку в
RETROSPECTIVE.md.

Гілка feature/real-stt:
1. Infrastructure/Transcription/<Soniox|Deepgram>Transcriber : TranscriberBase (обраний на спайку).
   Soniox — ClientWebSocket напряму; Deepgram — офіційний SDK (погодити пакет). Звір протокол
   з актуальною документацією.
   - конфігураційне повідомлення: PCM16 16 кГц моно, мова uk або en, терміни курсу як контекст;
   - partial і final токени → TranscriptSegment; endpointing провайдера;
   - помилки протоколу/автентифікації → TranscriptionException (401 — non-retryable);
   - ключ з ISecretStore.
2. ProviderFactory: реальна гілка транскрибера; режим «Реальні» тепер працює наскрізно.
3. Тести: парсинг повідомлень провайдера (збережені JSON-приклади з документації), мапінг
   помилок; Live-тест на samples/demo-lecture.wav, пропускається без ключа.
4. Ручна перевірка: українська лекція з YouTube через системний звук — транскрипт з'являється
   ≤ ~1 с. Запиши враження в RETROSPECTIVE.md.
```

### Д14 · Вт 10.11 · B — структурований конспект і PDF

```text
Я студент B. Прочитай HANDOFF.md §8.10, §12 (конспект), §11 (QuestPDF).

Гілка feature/pdf-export:
1. Реальний CompleteJsonAsync<T> у LLM-клієнті через структуровані відповіді SDK (JSON-схема
   з LlmClientBase). Перевір на реальній лекції з Д13: SummaryService дає валідний конспект.
2. Infrastructure/Export/PdfExporter : ExporterBase на QuestPDF: Community License в коді
   (§11), шрифт з кирилицею (системний Segoe UI/Arial або вбудований шрифт з OFL-ліцензією —
   обери й опиши), розділи як у Markdown.
3. Зареєструй PdfExporter у DI — він автоматично з'явиться в ComboBox форматів (перевір,
   що клієнтський код не змінювався: це демонстрація поліморфізму).
4. Тести: PDF не порожній; прочитай його назад через PdfPig і перевір, що українські тези є
   в тексті.
5. LIBRARIES.md і RETROSPECTIVE.md для QuestPDF.
```

### Д15 · Ср 11.11 · A — скриншоти

```text
Я студент A. Прочитай HANDOFF.md §3 (S4), §8.5, §3.2 (чого НЕ робимо).

Гілка feature/screenshot:
1. IScreenCapture і CaptureMode у Core (§8.5); WindowsScreenCapture у App (бо потрібен WPF для
   кодування PNG): Win32 GDI через P/Invoke; ForegroundWindow (межі через DwmGetWindowAttribute)
   і FullScreen (монітор з активним вікном); DPI-awareness для кількох моніторів.
2. Результат → ImageContext (PNG), масштаб довгої сторони до ліміту провайдера.
3. Перед знімком сховати AnswerOverlayWindow (метод від B), дочекатися оновлення екрана
   (~50–100 мс або DwmFlush), зняти, показати знову. Overlay не потрапляє на знімок.
4. Ctrl+Shift+S і кнопка «Скриншот + питання» → LectureSession.AskNowAsync(screenshot).
   Скриншот не зберігається серед матеріалів і не входить у кешований префікс.
5. Тести: масштабування, вибір режиму (з фейковим IScreenCapture у тесті VM). Ручна перевірка
   зі слайдом у PowerPoint/PDF.
SetWindowDisplayAffinity та інші режими «невидимості» НЕ використовувати.
```

### Д15 · Ср 11.11 · B — обробка помилок (зона B)

```text
Я студент B. Прочитай HANDOFF.md §13 повністю і §4.2.

Гілка feature/error-handling-b:
Для кожного рядка §13 з моєї зони перевір поведінку в коді й UI та наявність ≥ 1 unit-тесту:
1. Невірний API-ключ (401): повідомлення + «Відкрити налаштування», без повторів.
2. 429/5xx: до 2 повторів лише до першого токена, далі повідомлення + «Повторити».
3. Таймаут > 8 с до першого токена: скасування, «Не вдалося отримати відповідь» + «Повторити».
4. NO_QUESTION: відповідь не показується, статус «відхилено» у списку.
5. PDF не читається: матеріал позначений помилкою, решта працює.
6. Бюджет токенів перевищено: старт заблоковано з поясненням.
7. Транскрипт задовгий для конспекту: зрозуміле повідомлення, дані на місці.
8. Пошкоджена/заблокована БД: лог + повідомлення, живий режим працює.
Склади таблицю «рядок §13 → де в коді → назва тесту» і додай її в опис PR.
Помилки ніколи не ковтаються мовчки: знайди catch без логування і виправ.
```

### Д16 · Чт 12.11 · A — обробка помилок (зона A), затримки, перший реальний прогін

```text
Я студент A. Прочитай HANDOFF.md §4.1, §4.2, §13, §14.3.

Гілка feature/latency-errors-a:
1. Рядки §13 моєї зони (аудіопристрій, обрив STT з 5 спробами та позначкою пропуску) —
   перевір поведінку й тести; таблиця «рядок → код → тест» в описі PR.
2. LatencyTracker покриває всі етапи (SttFinal, Detection, PromptBuild, FirstToken, Total);
   StatusBar показує останнє значення і p50; затримки кожного QaItem видно в історії.
3. Serilog: поля SessionId, Stage, LatencyMs; перевір grep'ом, що в логах немає ключів і
   повних промптів.
4. Перший реальний прогін: 15–20 хв української лекції з YouTube у режимі «Реальні».
   Збери p50/p90 по етапах з логів і порівняй з бюджетом §4.1 — таблицю в RETROSPECTIVE.md.
   Що не вкладається — список гіпотез і виправлень.
```

### Д16 · Чт 12.11 · B — кешування, тюнінг промптів, LIBRARIES.md

```text
Я студент B. Прочитай HANDOFF.md §4.1, §8.4 (чому набір фіксується), §12, §2.3.

Гілка feature/prompt-tuning:
1. Логуй з відповіді провайдера кількість кешованих токенів (usage). Перевір на реальній сесії,
   що другий і наступні запити мають cache hit. Якщо ні — знайди, що змінює префікс
   (порядок, мітки часу, пробіли), і виправ; додай регресійний тест.
2. Заміри першого токена з кешем і без — у RETROSPECTIVE.md.
3. Тюнінг промптів §12 на транскрипті реальної лекції від A: 10 питань, оцінка коротких
   відповідей, частка хибних NO_QUESTION. Зміни в Prompts.resx — з поясненням у PR.
4. LIBRARIES.md: перевір кожен рядок (версія, конкретні можливості, файли в коді) для всіх
   бібліотек обох студентів. Позначка, які бібліотеки — для лаби 3, а які ні (§2.3).
```

### Д17 · Пт 13.11 · A і B — реальна лекція, чекпойнт лаби 3

```text
(однаковий для обох, кожен у своїй гілці fix/<коротко>)
Я студент <A|B>. Сьогодні чекпойнт лаби 3. Прочитай HANDOFF.md §2.3, §3, §4.1.

1. Ми провели наскрізний прогін на реальній лекції. Ось список проблем:
   <вставити список з нотаток: що, де, як відтворити>
   Для кожної проблеми з моєї зони: відтвори (тест, якщо можливо), виправ, додай регресійний тест.
   Проблеми зони партнера — лише опиши, не виправляй.
2. Онови RETROSPECTIVE.md для бібліотек моєї зони: задача, що спрацювало, проблеми і як
   розв'язали, чи обрали б знову (шаблон у файлі).
3. Перевір, що режим «Демо» досі працює без мережі (вимкни Wi-Fi або перевір інтеграційним тестом).
```

---

## Фаза 4. Якість, реліз, здача

### Д18 · Сб 14.11 · A — бенчмарки

```text
Я студент A. Прочитай HANDOFF.md §14.2, §4.1.

Гілка feature/benchmarks:
1. benchmarks/LectureMate.Benchmarks (BenchmarkDotNet, MemoryDiagnoser):
   - ресемплінг 48 кГц стерео → 16 кГц моно (буфер 100 мс);
   - HeuristicQuestionDetector.Analyze (короткий/довгий сегмент, з правилами uk+en);
   - PromptFactory.CreateAnswerRequest з SessionContext на 50 сторінок PDF.
   Program.cs — BenchmarkSwitcher.
2. Запусти в Release, результати в docs/benchmarks.md з висновками проти бюджету §4.1.
   Якщо щось перевищує бюджет або сильно алокує — оптимізуй і покажи «до/після».
3. CI: бенчмарки лише збираються, не запускаються.
```

### Д18 · Сб 14.11 · B — документація DocFX

```text
Я студент B. Прочитай HANDOFF.md §14.4, §6.

Гілка feature/docfx:
1. docs/docfx.json: API з Core, Infrastructure, App; docs/index.md (що це за проєкт),
   docs/articles/: архітектура (конвеєр §6.2, патерни §6.4), як запустити, режим «Демо»,
   посилання на ELEMENTS/LIBRARIES/RETROSPECTIVE.
2. .github/workflows/docs.yml: збірка DocFX і публікація на GitHub Pages з main.
3. Знайди public-члени без XML-документації або з порожніми/формальними коментарями і допиши
   змістовні (summary, param, returns, exception).
4. Посилання на сайт документації в README.md.
Якщо DocFX не встигаємо — пріоритет: пункт 3, потім 1, потім 2.
```

### Д19 · Нд 15.11 · A і B — взаємне рев'ю і покриття

```text
(для кожного; <зона партнера> — список класів партнера з HANDOFF §15)
Я студент <A|B>. На захисті я маю пояснити весь код, зокрема код партнера.
Гілка chore/review-<a|b>.

1. Проведи мене по коду <зона партнера>: для кожного класу — навіщо він, який патерн, як
   взаємодіє з іншими, де обробляються помилки, які тести його покривають. Коротко, з
   посиланнями на файли й рядки.
2. Склади 15 контрольних питань, які може поставити викладач по цьому коду (ООП, SOLID,
   поліморфізм, асинхронність, бібліотеки), і дай стислі правильні відповіді.
3. Запусти тести з coverlet (dotnet test --collect:"XPlat Code Coverage"), покажи найменш
   покриті класи моєї зони; допиши тести там, де покриття < 60% і є логіка.
4. Знайди дублювання коду й порушення HANDOFF §17 (логіка в code-behind, зашиті рядки,
   SDK у Core/VM) по всьому рішенню; виправ у своїй зоні, решту — списком.
```

### Д20 · Пн 16.11 · A — реліз і README

```text
Я студент A. Прочитай HANDOFF.md §2.4 (одна програма), README.md.

Гілка chore/release:
1. dotnet publish LectureMate.App: win-x64, self-contained, PublishSingleFile — один .exe.
   Перевір, що він запускається на чистій машині/користувачі в режимі «Демо» і знаходить
   samples (вбудувати демо-WAV і сценарій як ресурси або поряд — обери й опиши).
2. README.md: що вміє (зі скриншотами всіх 6 вікон), як запустити exe, як зібрати, як увімкнути
   реальних провайдерів (ключі через налаштування), гарячі клавіші, посилання на документи.
3. docs/demo-script.md: сценарій демонстрації на захисті на 5–7 хв (Демо-режим → реальний
   режим → історія → конспект → експорт) і що робити, якщо мережа впаде (Демо).
4. Code freeze о 20:00: після цього лише виправлення критичних багів.
```

### Д20 · Пн 16.11 · B — фінальний аудит документів

```text
Я студент B. Прочитай HANDOFF.md §2 повністю, §15.1, §17.8.

Гілка chore/final-docs:
1. ELEMENTS.md: повний перерахунок по фінальному коду (класи, поля, нетривіальні методи,
   ієрархії, поліморфізм з клієнтським кодом, вікна, контроли, контейнери, обробники).
   Видали рядки про елементи, яких більше немає. Підсумкові таблиці проти мінімумів лаб 1 і 2.
2. AI_USAGE.md: переконайся, що є запис за кожен день роботи з агентом, з роллю і тим,
   що змінено вручну.
3. RETROSPECTIVE.md: фінальний висновок по кожній бібліотеці лаби 3 і загальний висновок.
4. HANDOFF.md §18: впиши відповіді викладача на відкриті питання; §16 — фактичні дати віх.
5. Чекліст здачі в PLAN.md: усі вимоги §2.1–§2.4 з посиланням, де саме вони закриті.
```

### Д21 · Вт 17.11 · A і B — здача

```text
(можна разом в одній сесії)
Сьогодні здача, нових функцій не додаємо.
1. Перевір із чистого клону: git clone → dotnet build → dotnet test → запуск exe у Демо.
2. Тег v1.0.0 і GitHub Release з exe та коротким описом (gh release create).
3. Пройди чекліст здачі з PLAN.md і скажи, чого бракує.
4. Репетиція захисту: постав мені по черзі 10 питань по коду партнера і оціни відповіді.
```
