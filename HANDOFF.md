# HANDOFF: LectureMate — «Помічник на лекції»

> Робоча назва: **LectureMate** (можна змінити).
> Версія документа: **v2 (27.10.2026)**. Що змінилося порівняно з v1 і чому, описано в розділі 20.
> Мова документа: українська (основна). Англійська версія: `HANDOFF.en.md`. Якщо версії розходяться, правильною вважається українська.
> Призначення: повний контекст проєкту для ШІ-агента (Claude Code, Cursor, Copilot тощо) і для обох учасників команди.

---

## 0. Як ШІ-агенту працювати з цим файлом

1. Прочитай документ повністю, перш ніж писати код.
2. Розділ 2 містить **жорсткі вимоги курсу**. Розділи 9–11 показують, як проєкт їх закриває: не скорочуй їх нижче мінімумів розділу 2 без погодження з командою.
3. Якщо вимога неоднозначна або суперечить іншій, **зупинись і спитай**. Не вигадуй власне тлумачення (цього вимагають і загальні правила курсу).
4. **Простота важливіша за «більше патернів».** Нова абстракція (інтерфейс, базовий клас, патерн) з'являється лише тоді, коли є друга реалізація, потреба підмінити залежність у тестах або потреба ізолювати `Core` від бібліотеки. Мінімуми лаб уже закриті з запасом (розділ 9), тому додавати класи «для кількості» не треба.
5. Після кожної зміни онови `ELEMENTS.md` (реєстр елементів для лаб) і допиши запис у `AI_USAGE.md` (розділ 17).
6. Код, коментарі XML-документації та назви — **англійською**. Тексти інтерфейсу — **тільки через ресурси** (`.resx`), українська за замовчуванням, англійська як друга мова.

---

## 1. Коротко про проєкт

Десктопний застосунок для Windows, який допомагає студенту на онлайн-лекції (Zoom, Microsoft Teams, Google Meet або будь-який інший звук на ПК):

- **слухає** системний звук і в реальному часі показує **живий транскрипт** лекції (українською або англійською);
- **розпізнає питання** викладача (автоматично або за гарячою клавішею) і за 2–3 секунди показує **коротку відповідь-підказку** у спливаючому вікні поверх інших вікон;
- під час відповіді враховує **контекст**: прикріплені PDF, слайди, нотатки, а також **скриншот екрана**, який застосунок робить сам;
- після лекції генерує **конспект**: ключові тези, питання з відповідями, **картки для повторення**; зберігає **історію сесій** з пошуком та експортом.

Нішева перевага: **якісна підтримка української мови**. Популярні аналоги її або не мають, або мають лише частково (деталі в документі з обґрунтуванням).

---

## 2. Навчальний контекст і вимоги курсу

Проєкт робиться на дисципліні «Основи ООП» (2025–2026) **командою з двох студентів** як семестровий навчальний проєкт. Він має одночасно закрити **лабораторні №1, №2 і №3**. Мінімуми рахуються **на команду**: окремо для кожного студента їх не перевіряємо.

### 2.1. Лаба 1 — базові ідеї ООП (мінімум)
- ≥ 9 класів (або подібних типів);
- ≥ 15 полів (сумарно);
- ≥ 25 **нетривіальних** методів. Не рахуються прості getter/setter і конструктори, які лише присвоюють поля;
- ≥ 2 ієрархії успадкування, щонайменше одна з них має ≥ 3 класи;
- ≥ 3 незалежні випадки поліморфізму: щонайменше один **статичний** (generics) і один **динамічний** (віртуальні методи / інтерфейси). Для кожного має бути **клієнтський код**, що працює через поліморфний інтерфейс;
- правильна інкапсуляція (доцільні модифікатори доступу), абстрактні класи та інтерфейси там, де це доречно;
- треба вказати **точну кількість** кожного типу елементів з переліком (`ELEMENTS.md`).

### 2.2. Лаба 2 — графічний інтерфейс (мінімум)
- ≥ 4 вікна/форми;
- ≥ 20 елементів керування сумарно;
- ≥ 1 елемент-контейнер (список, таблиця, дерево);
- ≥ 10 обробників подій;
- «класичний» GUI: перегляд, додавання, редагування даних. Логіка **відділена** від інтерфейсу.

### 2.3. Лаба 3 — зовнішні бібліотеки
- використання сторонніх (не стандартних) бібліотек для частини функціональності;
- Qt і фреймворки unit-тестів **не зараховуються** як «зовнішня бібліотека» для цієї лаби;
- у репозиторії має бути чітко вказано, які бібліотеки використано і **які саме їхні можливості** (файл `LIBRARIES.md`);
- **звіт-ретроспектива** про досвід використання бібліотек (`RETROSPECTIVE.md`; записи ведемо в процесі, а не в кінці).

### 2.4. Загальні вимоги курсу (впливають на весь код)
- правильний ОО-дизайн, принципи SOLID, без дублювання коду; патерни дають додаткові бали, але лише там, де вони розв'язують реальну задачу;
- клієнт може обирати реалізацію, і зміна реалізації **не змінює код клієнта** (тому інтерфейси + DI на межах із зовнішнім світом: звук, STT, LLM, файли, БД);
- **одна програма** (один виконуваний файл); дозволено розбиття на бібліотеки + exe. Проєкти тестів і бенчмарків є допоміжними;
- обробка помилок і нестандартних ситуацій, **покрита тестами**;
- один стиль коду (див. 14.6), автоформатування;
- git з першого дня, часті коміти зі змістовними повідомленнями, гілки + pull requests;
- unit-тести (обов'язкові для претендентів на «відмінно»), документація з коментарів (DocFX), мікробенчмарки, логування, **i18n**;
- **академічна доброчесність**: використання ШІ дозволене, але його треба **явно задокументувати** (`AI_USAGE.md`), і обидва студенти мають розуміти весь код.

---

## 3. Користувацькі сценарії (MVP)

| ID | Сценарій | Критерій готовності |
|---|---|---|
| S1 | Користувач обирає курс і джерело звуку (системний звук / мікрофон / демо-файл), натискає «Почати». Застосунок показує живий транскрипт | Проміжні (partial) результати з'являються не пізніше ніж за ~1 с після мовлення; фінальні сегменти зберігаються |
| S2 | Викладач ставить питання. Евристика його помічає, і зʼявляється спливаюче вікно з короткою відповіддю. Якщо модель вирішує, що це не питання до студентів, вікно не з'являється, а в списку питань запис позначається як відхилений | Перші слова відповіді за ≤ 2,5 с (p50) після кінця питання |
| S3 | Гаряча клавіша `Ctrl+Shift+Space` → «знайди питання в останніх 30 с і відповідай; якщо питання немає, коротко поясни, про що йшлося» | Працює навіть тоді, коли евристика нічого не знайшла |
| S4 | Гаряча клавіша `Ctrl+Shift+S` або кнопка → скриншот екрана → відповідь з урахуванням зображення (слайд, код, формула) | Під час знімка спливаюче вікно приховане й не потрапляє на скриншот |
| S5 | До курсу прикріплюються PDF, зображення, текстові нотатки. Перед стартом сесії обирається, що з них використовувати; набір фіксується на всю сесію | Матеріали йдуть у промпт як незмінний (кешований) префікс; якщо набір перевищує бюджет токенів, старт блокується з поясненням |
| S6 | Після «Стоп» генерується конспект: тези, Q&A, терміни, картки | Конспект можна редагувати й експортувати (Markdown, PDF, CSV для Anki) |
| S7 | Історія сесій: таблиця, пошук, перегляд транскрипту та відповідей (із затримками по етапах) | Дані зберігаються локально між перезапусками; транскрипт не губиться при падінні застосунку |

### 3.1. Поза межами MVP (можна після)
- автоматичні пояснення термінів, які прозвучали на лекції;
- окремий LLM-детектор питань (зараз його роль виконує відповідь моделі, див. 8.3);
- вибір матеріалів під кожне питання (RAG / TF-IDF): ламає кешування промпту, див. 8.4;
- конспект частинами (map-reduce) для дуже довгих записів;
- другий реальний провайдер STT/LLM і резервний провайдер на випадок збою (архітектура дозволяє додати одним класом);
- графік затримок, налаштовувані гарячі клавіші, зміна мови інтерфейсу без перезапуску;
- локальні моделі (Whisper.net, Ollama);
- macOS/Linux;
- бот, що підключається до зустрічі як учасник;
- розпізнавання мовців (diarization).

### 3.2. Свідомо НЕ робимо
- режим «невидимості» для демонстрації екрана або програм прокторингу (зокрема `SetWindowDisplayAffinity`). Це не наша цінність і створює ризики для академічної доброчесності;
- запис і збереження аудіо за замовчуванням (тільки за явним увімкненням).

---

## 4. Нефункціональні вимоги

### 4.1. Бюджет затримки (ціль p50 / p90)

| Етап | Ціль | Як досягаємо |
|---|---|---|
| Кінець фрази → фінальний сегмент STT | 0,5 / 1,0 с | Потоковий STT по WebSocket, endpointing провайдера |
| Детекція питання | < 5 мс | Евристика синхронно, лише на фінальних сегментах. Жодних правил, що вимагають чекати (паузи, «чи відповість викладач сам») |
| Побудова промпту | < 50 мс | Текст PDF витягується один раз при імпорті; набір матеріалів зафіксовано на сесію |
| LLM: перший токен | 0,7 / 1,5 с | Швидка модель, streaming, prompt caching: матеріали курсу — незмінний префікс |
| Перевірка «це не питання» | ≈ 0 мс у звичайному випадку | Модель відповідає маркером `NO_QUESTION`; текст показується, щойно початок відповіді розійшовся з маркером (див. 8.6) |
| Показ у спливаючому вікні | < 50 мс | Дозапис токенів, без перерендеру всього тексту |
| **Разом: перші слова відповіді** | **≤ 2,5 / 4 с** | Зі скриншотом допускається +1,5 с |

Кожен етап вимірюється в рантаймі (`LatencyTracker`, 8.9): останні значення й p50 видно в статус-барі головного вікна, а затримки кожної відповіді зберігаються й показуються в історії.

### 4.2. Надійність
- обрив WebSocket STT → автоперепідключення з експоненційною затримкою; аудіо за час обриву накопичується в обмеженому каналі (~10 с, найстаріше відкидається), після перепідключення досилається; часові мітки нового з'єднання зсуваються на час від початку сесії;
- таймаут LLM (8 с до першого токена) → повідомлення в спливаючому вікні й кнопка «Повторити»;
- фінальні сегменти й Q&A записуються в БД пакетами під час сесії, а не лише після «Стоп»;
- помилки ніколи не «ковтаються» мовчки: кожна логується і показується користувачу зрозумілою мовою.

### 4.3. Приватність і безпека
- API-ключі зберігаються зашифрованими через Windows DPAPI (`ProtectedData`) і **ніколи** не потрапляють у git;
- транскрипти, історія та копії матеріалів зберігаються лише локально (`%LOCALAPPDATA%\LectureMate`);
- при першому старті сесії показується нагадування: запис лекції може потребувати згоди викладача; аудіо й скриншоти передаються обраним хмарним провайдерам;
- у налаштуваннях видно, які дані куди надсилаються.

### 4.4. Вартість
Орієнтовно: STT $0,12–0,46 за годину залежно від провайдера, плюс LLM — центи за лекцію (кешування префікса здешевлює повторні запити). Під час розробки використовуємо безкоштовні кредити й режим «Демо» (8.9), який працює без мережі.

---

## 5. Технологічний стек (рішення прийнято)

**C# 14 / .NET 10 (LTS) + WPF, Windows 10/11.**

Чому: найпростіше захоплення системного звуку (WASAPI loopback через NAudio), зрілий MVVM (CommunityToolkit.Mvvm), BenchmarkDotNet, DocFX, офіційні SDK OpenAI та Anthropic, сильні generics для статичного поліморфізму.

Інші варіанти (Avalonia, Java/Kotlin + JavaFX, C++ + Qt, Python + PyQt) розглядалися й відкинуті: складніше захоплення системного звуку або Qt не зараховується для лаби 3. Перехід на Avalonia лишається можливим пізніше, бо `Core` не залежить від UI.

---

## 6. Архітектура

### 6.1. Структура рішення

```
LectureMate/
├─ LectureMate.sln
├─ src/
│  ├─ LectureMate.Core/            # домен, абстракції, сесія, відповіді, конспект. БЕЗ WPF і БЕЗ SDK провайдерів
│  ├─ LectureMate.Infrastructure/  # реалізації: NAudio, Soniox/…, OpenAI/Anthropic, PdfPig, EF Core, QuestPDF, DPAPI
│  │   └─ Demo/                    # WaveFileAudioSource, ScriptedTranscriber, FakeLlmClient
│  └─ LectureMate.App/             # WPF: Views, ViewModels, ресурси (.resx), DI-композиція — єдиний .exe
├─ tests/
│  ├─ LectureMate.Core.Tests/
│  └─ LectureMate.Infrastructure.Tests/
├─ benchmarks/LectureMate.Benchmarks/
├─ docs/                           # docfx.json, статті
├─ samples/                        # тестові WAV лекцій, JSON-сценарії, тестові PDF (без персональних даних)
├─ .editorconfig
├─ README.md  ELEMENTS.md  LIBRARIES.md  RETROSPECTIVE.md  AI_USAGE.md
└─ .github/workflows/              # build + test + docfx → GitHub Pages
```

Правило залежностей: `App → Infrastructure → Core`, `App → Core`. **`Core` залежить лише від BCL і `Microsoft.Extensions.Logging.Abstractions`.** Це дає тестованість і змінність реалізацій.

### 6.2. Потік даних (конвеєр)

```
 [IAudioSource] --AudioChunk (16 кГц, моно, PCM16)--> Channel<AudioChunk> (обмежений, ~10 с, DropOldest)
                                                                  |
                                                                  v
                                                  [ITranscriber (WebSocket, перепідключення)]
                                                                  |
                                                  TranscriptSegment (partial → UI, final → далі)
                                                                  v
                                   [TimeWindowBuffer<TranscriptSegment>] + запис у БД пакетами
                                                                  |
   Гаряча клавіша / кнопка (+ скриншот) ----+                     v
                                            +----> [HeuristicQuestionDetector] --QuestionCandidate-->
                                                                                                 |
 [SessionContext: матеріали, зафіксовані на старті] --+                                         v
                                                      +--> [PromptFactory] --> [ILlmClient.StreamAsync]
                                                                                                 |
                                       AnswerService: маркер NO_QUESTION? --так--> відхилити
                                                                  | ні
                                                                  v
                                   події AnswerUpdated --> ViewModel --> Dispatcher --> AnswerOverlayWindow
 Стоп сесії --> [SummaryService] --> LectureSummary --> [ILectureStore] / [ExporterBase]
```

### 6.3. Потоки й асинхронність
- аудіо приходить у потоці NAudio → буфер копіюється й одразу пишеться в `Channel<AudioChunk>` (`System.Threading.Channels`), без важкої роботи в колбеку. Канал обмежений (`BoundedChannelFullMode.DropOldest`): це і є буфер на час обриву, окремий кільцевий буфер не потрібен;
- STT, детекція й LLM виконуються в `async`-задачах з `CancellationToken`. Нове питання **скасовує** попередню незавершену відповідь;
- Core повідомляє про зміни звичайними подіями C# (`event`). ViewModel підписується й переводить оновлення в UI-потік через `Dispatcher.InvokeAsync`. Окремої шини подій немає;
- жодного `.Result` / `.Wait()` в UI-потоці.

### 6.4. Патерни (там, де вони розв'язують реальну задачу)
- **Strategy:** джерела звуку, транскрибери, LLM-клієнти, експортери, захоплення екрана;
- **Template Method:** `AudioSourceBase`, `TranscriberBase`, `LlmClientBase`, `ExporterBase` (спільні перевірки, повтори, логування, вимірювання в базовому класі; специфіка — у нащадках);
- **State:** `LectureSession` (явні дозволені переходи станів);
- **Factory:** `ProviderFactory` (режим «Демо» або реальні провайдери за налаштуваннями);
- **Repository:** `ICourseStore`, `ILectureStore` (специфічні сховища, без узагальненого репозиторію поверх EF);
- **Observer:** події C# між Core і ViewModel;
- **MVVM** в UI.

### 6.5. Принцип простоти (YAGNI)
- інтерфейс створюємо, коли є друга реалізація, підміна в тестах або потреба відгородити `Core` від бібліотеки чи ОС. Наприклад, `HeuristicQuestionDetector` — чистий клас без інтерфейсу: він детермінований і тестується напряму;
- не дублюємо механізми: одна система помилок (ієрархія винятків), один спосіб будувати промпти (`PromptFactory`), один шлях подій (C# events);
- вбудовані можливості .NET і бібліотек мають перевагу над власними (канали замість кільцевого буфера, `DbContext` замість узагальненого репозиторію).

---

## 7. Доменна модель (Core)

Записи (`record`) — незмінні DTO; класи з поведінкою мають інкапсульований стан. Формат звуку в конвеєрі один (16 кГц, моно, PCM16, константи в `AudioFormats`), тому окремого типу формату в кожному фрагменті немає.

```csharp
public interface ITimestamped { TimeSpan Start { get; } TimeSpan End { get; } }

public sealed record AudioChunk(ReadOnlyMemory<byte> Data, TimeSpan Timestamp);

public sealed record TranscriptSegment(string Text, TimeSpan Start, TimeSpan End,
                                       bool IsFinal, string Language) : ITimestamped;

public sealed record QuestionCandidate(string Text, TimeSpan Start, TimeSpan End,
                                       double Score, QuestionSource Source) : ITimestamped;
public enum QuestionSource { Heuristic, Manual, Screenshot }

public sealed record QaItem(Guid Id, QuestionCandidate Question, string AnswerText, QaStatus Status,
                            string Model, IReadOnlyDictionary<LatencyStage, TimeSpan> Latencies);
public enum QaStatus { Answering, Answered, Discarded, Failed }

public sealed record LatencySample(LatencyStage Stage, TimeSpan Start, TimeSpan End) : ITimestamped;
public enum LatencyStage { SttFinal, Detection, PromptBuild, FirstToken, Total }

public sealed record LectureSummary(string Title, IReadOnlyList<string> KeyPoints,
                                    IReadOnlyList<QaPair> Questions, IReadOnlyList<Term> Terms,
                                    IReadOnlyList<Flashcard> Flashcards);
public sealed record QaPair(string Question, string Answer);
public sealed record Term(string Name, string Definition);
public sealed record Flashcard(string Front, string Back);

public enum SessionState { Idle, Starting, Running, Paused, Stopping, Stopped, Faulted }
```

Що зберігається (EF Core, SQLite): `Course` (назва, мова, терміни для STT), матеріали (`ContextItem` з нащадками, мапінг TPH), `DetectorRule`, `Lecture` (сесія: курс, час, фінальні сегменти, `QaItem`, конспект у JSON-колонці). Доменні типи мапляться напряму через fluent-конфігурацію в Infrastructure; окремі класи-«записи» створюємо лише там, де прямий мапінг неможливий. Файли матеріалів копіюються в `%LOCALAPPDATA%\LectureMate\materials`, щоб посилання не ламалися.

---

## 8. Ключові абстракції та ієрархії

> Сигнатури орієнтовні: агент може уточнювати їх, але не змінювати суті ієрархій і не додавати нових рівнів без потреби (6.5).

### 8.1. Аудіо (динамічний поліморфізм, ієрархія з 3 класів)
`IAudioSource` — у Core; базовий клас і нащадки — в Infrastructure (обидва використовують NAudio).
```csharp
public interface IAudioSource : IAsyncDisposable
{
    string DisplayName { get; }
    event EventHandler<AudioChunk>? ChunkAvailable;   // завжди 16 кГц, моно, PCM16
    event EventHandler<float>? LevelChanged;          // 0..1, для індикатора рівня
    event EventHandler<Exception>? Faulted;
    Task StartAsync(CancellationToken ct);
    Task StopAsync();
}
public abstract class AudioSourceBase : IAudioSource       // Template Method: стан, перевірки, ресемплінг, рівень, події
{ protected abstract Task OnStartAsync(CancellationToken ct); protected abstract Task OnStopAsync(); ... }
public sealed class WasapiAudioSource   : AudioSourceBase  // NAudio: WasapiLoopbackCapture (звук ПК) або WasapiCapture (мікрофон) — за обраним пристроєм
public sealed class WaveFileAudioSource : AudioSourceBase  // WAV у реальному темпі: демо й тести без живої лекції
```

### 8.2. Розпізнавання мовлення
```csharp
public sealed record TranscriptionOptions(string Language, IReadOnlyList<string> Terms); // терміни курсу як підказка для STT

public interface ITranscriber : IAsyncDisposable
{
    event EventHandler<TranscriptSegment>? SegmentReceived;
    event EventHandler<TranscriberStatus>? StatusChanged;    // Connected / Reconnecting / Failed
    Task StartAsync(TranscriptionOptions options, ChannelReader<AudioChunk> audio, CancellationToken ct);
    Task StopAsync(CancellationToken ct);
}
public abstract class TranscriberBase : ITranscriber   // цикл «читати канал → надсилати / приймати сегменти»,
                                                       // перепідключення з backoff, зсув часу, логування
public sealed class SonioxTranscriber   : TranscriberBase  // або провайдер-переможець спайку (див. нижче)
public sealed class ScriptedTranscriber : TranscriberBase  // віддає сегменти з JSON-сценарію — демо й тести
```
Транскрибер сам читає канал аудіо: поки триває перепідключення, він не читає, і канал накопичує звук.

**Спайк першого тижня (M0):** записати 10 хв реальної української лекції, прогнати через Soniox, Deepgram і OpenAI (через їхні консолі або короткі скрипти, **не** через застосунок), порахувати WER/якість на око, обрати **одного** провайдера для MVP. Результат записати в `RETROSPECTIVE.md`. Якщо переможе Deepgram, `SonioxTranscriber` замінюється на `DeepgramTranscriber` (з офіційним SDK).

### 8.3. Детекція питань
```csharp
public sealed record DetectorRule(Guid Id, string Phrase, RuleKind Kind, double Weight, string Language);
public enum RuleKind { InterrogativeWord, TriggerPhrase, AntiTrigger }

public sealed class HeuristicQuestionDetector     // чистий клас, правила редагуються в GUI
{
    public HeuristicQuestionDetector(IReadOnlyList<DetectorRule> rules, double questionMarkWeight, double threshold);
    public QuestionCandidate? Analyze(TranscriptSegment finalSegment);   // синхронно, мікросекунди
}
```
Оцінка = сума ваг спрацьованих сигналів; питання, якщо оцінка ≥ порогу (повзунок чутливості).
Стартові сигнали (українська та англійська):
- сегмент закінчується на `?` (STT ставить пунктуацію) — вбудований сигнал з окремою вагою;
- питальні слова на початку: `що, чому, навіщо, як, який/яка/яке/які, коли, де, скільки, хто, чи`;
- фрази-тригери: `хто знає`, `хто може сказати`, `як ви думаєте`, `що буде, якщо`, `назвіть`, `поясніть`, `наведіть приклад`, `хто відповість`; `who knows`, `what do you think`, `can anyone`;
- антитригери (риторика) з від'ємною вагою: `чи не так`, `правда ж`.

Риторичні питання, які пройшли евристику, відсіює сама відповідь моделі (маркер `NO_QUESTION`, 8.6, 12). Тому окремий LLM-детектор і паралельна «спекулятивна» генерація не потрібні.

### 8.4. Контекст (ієрархія з 4 класів)
```csharp
public abstract class ContextItem
{
    public Guid Id { get; } public Guid CourseId { get; } public string Title { get; } public DateTimeOffset AddedAt { get; }
    public bool IsEnabled { get; set; }
    public abstract int EstimateTokens();
    public abstract IEnumerable<MessagePart> ToMessageParts();   // текст і/або зображення
}
public sealed class TextNoteContext    : ContextItem   // текст нотатки
public sealed class PdfDocumentContext : ContextItem   // текст сторінок, витягнутий один раз при імпорті (IPdfTextExtractor), + діапазон сторінок
public sealed class ImageContext       : ContextItem   // слайд, фото або скриншот; масштабується до ліміту провайдера при імпорті/знімку

public sealed class SessionContext     // знімок увімкнених матеріалів на старті сесії
{
    public static SessionContext Create(IEnumerable<ContextItem> items, int tokenBudget); // перевіряє бюджет
    public int EstimatedTokens { get; }
    public bool IsWithinBudget { get; }
    public IReadOnlyList<MessagePart> ToCachedParts();
}
```
Чому набір фіксується на сесію: матеріали йдуть у промпт як **незмінний префікс**, і провайдер кешує його. Вибір матеріалів під кожне питання змінював би префікс і вимикав би кешування, а це головний важіль затримки й ціни. Якщо набір не вміщується в бюджет, користувач звужує діапазон сторінок або вимикає частину матеріалів. Оцінка токенів груба (за кількістю символів), точність тут не критична.

### 8.5. Скриншоти
```csharp
public enum CaptureMode { ForegroundWindow, FullScreen }
public interface IScreenCapture { Task<ImageContext> CaptureAsync(CaptureMode mode, CancellationToken ct); }
public sealed class WindowsScreenCapture : IScreenCapture   // App/Infrastructure, Win32
```
Інтерфейс потрібен, бо реалізація залежить від ОС, а `Core` — ні. Перед знімком App ховає спливаюче вікно, чекає одне оновлення екрана (~50–100 мс), робить знімок і показує вікно знову. Скриншот не зберігається серед матеріалів курсу й не входить у кешований префікс.

### 8.6. LLM і відповіді
```csharp
public abstract record MessagePart;
public sealed record TextPart(string Text) : MessagePart;
public sealed record ImagePart(ReadOnlyMemory<byte> Bytes, string MediaType) : MessagePart;

// CachedContext — незмінний префікс (матеріали сесії); Prompt — змінна частина (транскрипт, питання, скриншот).
public sealed record LlmRequest(string System, IReadOnlyList<MessagePart> CachedContext,
                                IReadOnlyList<MessagePart> Prompt, int MaxTokens);

public interface ILlmClient
{
    string ProviderName { get; }
    IAsyncEnumerable<string> StreamAsync(LlmRequest request, CancellationToken ct);
    Task<T> CompleteJsonAsync<T>(LlmRequest request, CancellationToken ct);   // структурована відповідь (конспект)
}
public abstract class LlmClientBase : ILlmClient       // таймаут до першого токена, retry на 429/5xx до першого токена,
                                                       // логування, вимірювання; JSON-схема з T через System.Text.Json
public sealed class OpenAiLlmClient    : LlmClientBase // NuGet `OpenAI` }  у MVP реалізуємо ОДИН із двох:
public sealed class AnthropicLlmClient : LlmClientBase // NuGet `Anthropic` } вибір за вимірами на M0
public sealed class FakeLlmClient      : LlmClientBase // детерміновані відповіді із затримкою — демо й тести

public enum AnswerMode { Auto, Manual }
public sealed record AnswerUpdate(Guid QaId, AnswerUpdateKind Kind, string Text);
public enum AnswerUpdateKind { Started, Token, Completed, Discarded, Failed }

public sealed class AnswerService
{
    public event EventHandler<AnswerUpdate>? AnswerUpdated;
    public Task AskAsync(QuestionCandidate question, AnswerMode mode, ImageContext? screenshot, CancellationToken ct); // скасовує попередню відповідь
    public Task ElaborateAsync(Guid qaId, CancellationToken ct);  // кнопка «Детальніше»
}
```
Маркер `NO_QUESTION` (лише в режимі `Auto`): `AnswerService` порівнює початок потоку з маркером. Щойно перші символи розійшлися з ним, відповідь показується без затримки. Якщо потік збігся з маркером повністю, відповідь скасовується, а запис отримує статус `Discarded`. Вручну викликані відповіді (`Manual`) маркер не використовують.

### 8.7. Промпти
```csharp
public sealed class PromptFactory   // тексти з ресурсів (uk/en), без логіки провайдера
{
    public LlmRequest CreateAnswerRequest(AnswerPromptInput input);    // курс, мова, стиль, режим, SessionContext, транскрипт, питання, скриншот
    public LlmRequest CreateElaborateRequest(ElaboratePromptInput input);
    public LlmRequest CreateSummaryRequest(SummaryPromptInput input);
}
```

### 8.8. Узагальнені типи (статичний поліморфізм)
```csharp
public sealed class TimeWindowBuffer<T> where T : ITimestamped   // «останні N секунд»
// клієнти: LectureSession — TimeWindowBuffer<TranscriptSegment> (3 хв для промпту);
//          LatencyTracker — TimeWindowBuffer<LatencySample> (5 хв для p50/p90)

public static class Retry
{
    public static Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation, RetryOptions options,
                                          Func<Exception, bool> isRetryable, CancellationToken ct);
}   // клієнти: LlmClientBase (запит), TranscriberBase (відкриття з'єднання)

// ILlmClient.CompleteJsonAsync<T> — клієнт: SummaryService (T = LectureSummary)
```

### 8.9. Сесія та оркестрація
```csharp
public sealed class LectureSession   // State + конвеєр 6.2: аудіо → канал → STT → буфер → детектор → AnswerService
{
    public SessionState State { get; }
    public event EventHandler<SessionState>? StateChanged;
    public event EventHandler<TranscriptSegment>? SegmentReceived;
    public event EventHandler<QuestionCandidate>? QuestionDetected;
    public event EventHandler<float>? AudioLevelChanged;
    public Task StartAsync(SessionSetup setup, CancellationToken ct);   // setup: курс, джерело, транскрибер, SessionContext, налаштування
    public Task PauseAsync(); public Task ResumeAsync(); public Task StopAsync();
    public Task AskNowAsync(ImageContext? screenshot, CancellationToken ct);  // гаряча клавіша / кнопка
}
public sealed class LatencyTracker   // мітки по етапах, p50/p90 за останні 5 хв, значення для кожного QaItem
public interface IProviderFactory    // у Core; реалізація ProviderFactory — в Infrastructure
{
    IAudioSource CreateAudioSource(AudioSourceSettings s);
    ITranscriber CreateTranscriber();
    ILlmClient CreateLlmClient();
}
```
Режим **«Демо»** (`WaveFileAudioSource` + `ScriptedTranscriber` + `FakeLlmClient`) обирається в налаштуваннях і працює без мережі та ключів. Це і є демонстрація вимоги «клієнт обирає реалізацію без змін у своєму коді».

### 8.10. Конспект і експорт
```csharp
public sealed class SummaryService
{
    public Task<LectureSummary> GenerateAsync(Guid lectureId, CancellationToken ct);  // ОДИН запит із повним транскриптом
}
public abstract class ExporterBase   // Template Method: перевірки, кодування, обробка помилок
{
    public abstract string FormatName { get; }
    public abstract string FileExtension { get; }
    public Task ExportAsync(LectureSummary summary, Stream output, CancellationToken ct);
    protected abstract Task WriteAsync(LectureSummary summary, Stream output, CancellationToken ct);
}
public sealed class MarkdownExporter     : ExporterBase
public sealed class PdfExporter          : ExporterBase   // QuestPDF
public sealed class FlashcardCsvExporter : ExporterBase   // CSV для імпорту в Anki
```
Транскрипт лекції на 80–90 хв вміщується в контекст сучасних моделей, тому конспект робиться одним запитом. Якщо транскрипт перевищує ліміт обраної моделі, користувач бачить зрозуміле повідомлення (розбиття на частини — поза MVP).

### 8.11. Винятки (ще одна ієрархія)
```csharp
public abstract class LectureMateException : Exception
public sealed class AudioCaptureException   : LectureMateException
public sealed class TranscriptionException  : LectureMateException
public sealed class LlmProviderException    : LectureMateException   // + StatusCode, IsRetryable
public sealed class ContextLoadException    : LectureMateException
```
Очікувані ситуації без винятку (наприклад, перевищення бюджету токенів) повертаються як стан об'єкта (`SessionContext.IsWithinBudget`), а не через окремий тип `Result<T>`.

### 8.12. Межі з інфраструктурою (інтерфейси в Core)
| Інтерфейс | Реалізація (Infrastructure) | Навіщо інтерфейс |
|---|---|---|
| `ICourseStore` | `EfCourseStore` | курси, матеріали, правила детектора; тести без БД |
| `ILectureStore` | `EfLectureStore` | сесії, сегменти (пакетами), Q&A, конспект |
| `ISecretStore` | `DpapiSecretStore` | ключі API |
| `ISettingsStore` | `JsonSettingsStore` | налаштування в JSON-файлі |
| `IPdfTextExtractor` | `PdfPigTextExtractor` | `Core` не залежить від PdfPig |
| `IScreenCapture` | `WindowsScreenCapture` | `Core` не залежить від Win32 |
| `IProviderFactory` | `ProviderFactory` | вибір «Демо» / реальні провайдери |

---

## 9. Мапінг на Лабу 1 (на команду)

| Вимога | Як закрито | Клієнтський код |
|---|---|---|
| ≥ 9 класів | ~40 класів і записів (точний перелік у `ELEMENTS.md`) | — |
| Ієрархія ≥ 3 класів | `ContextItem` → TextNote / Pdf / Image (4); `ExporterBase` → 3 (4); `LectureMateException` → 4 (5) | — |
| Ще ієрархії | `AudioSourceBase` → Wasapi / WaveFile; `TranscriberBase` → Soniox / Scripted; `LlmClientBase` → OpenAi або Anthropic / Fake | — |
| Динамічний поліморфізм | `IAudioSource`, `ITranscriber` | `LectureSession` |
| Динамічний поліморфізм | `ILlmClient` / `LlmClientBase` | `AnswerService`, `SummaryService` |
| Динамічний поліморфізм | `ContextItem.ToMessageParts()` / `EstimateTokens()` | `SessionContext`, `PromptFactory` |
| Динамічний поліморфізм | `ExporterBase` | `SummaryViewModel` (вибір формату) |
| Статичний поліморфізм | `TimeWindowBuffer<T>` з двома типами | `LectureSession`, `LatencyTracker` |
| Статичний поліморфізм | `Retry.ExecuteAsync<T>`, `ILlmClient.CompleteJsonAsync<T>` | `LlmClientBase`, `TranscriberBase`, `SummaryService` |

Мінімуми лаби 1 закриваються з запасом уже на кінець M1 (Core + демо-провайдери). Точні кількості полів і нетривіальних методів ведуться в `ELEMENTS.md`; агент оновлює реєстр після кожного PR.

---

## 10. GUI (Лаба 2): 6 вікон

Шаблон: **MVVM** (CommunityToolkit.Mvvm). Code-behind містить лише UI-специфіку (drag&drop, фокус, перетягування вікна). Уся логіка живе у ViewModel і сервісах Core.

> Відкрите питання для викладача: чи зараховуються `ICommand` як «обробники подій». Щоб не ризикувати, у проєкті є й справжні обробники подій (`Loaded`, `Closing`, `SelectionChanged`, `Drop`, `TextChanged`, `MouseLeftButtonDown`, `Tick`, `HotkeyPressed` тощо): їх понад 10 навіть без команд.

1. **MainWindow**: керування сесією.
   Контроли: ComboBox курсу, ComboBox джерела (системний звук / мікрофон / демо-файл), кнопки Почати/Пауза/Стоп, кнопка «Відповісти зараз», кнопка «Скриншот + питання», ToggleButton «Автодетекція», **ListBox живого транскрипту** (віртуалізований), **ListView питань** (зі статусом: відповідь / відхилено / помилка), ProgressBar рівня звуку, StatusBar (стан, остання затримка, p50, мова), Menu (Матеріали / Історія / Налаштування / Довідка).
   Події: `Loaded`, `Closing` (підтвердження зупинки сесії), `SelectionChanged` курсу, автопрокрутка транскрипту, `HotkeyPressed` ×2, подвійний клік по питанню → відкрити відповідь у спливаючому вікні.
2. **AnswerOverlayWindow**: спливаюче вікно відповіді: `Topmost`, без значка в панелі задач, перетягується, Slider прозорості; TextBlock питання, TextBlock відповіді (дописується потоково), кнопки Копіювати / Детальніше / Закріпити / Закрити, індикатор джерела (евристика / вручну / скриншот), таймер автоприховування (пауза, поки курсор над вікном).
3. **MaterialsWindow**: **TreeView «курс → матеріали»**, додавання/перейменування/видалення курсу, кнопки Додати PDF / Зображення / Нотатку, **Drop файлів** у вікно, CheckBox «використовувати в сесії», панель перегляду (текст або зображення; нотатку можна редагувати), діапазон сторінок PDF, лічильник токенів обраного набору проти бюджету, поле «терміни для розпізнавання».
4. **HistoryWindow**: **DataGrid сесій** (дата, курс, тривалість, кількість питань), TextBox пошуку з фільтрацією, DatePicker'и періоду, TabControl деталей (транскрипт / Q&A із затримками по етапах), кнопки Видалити / Експорт / Відкрити конспект.
5. **SummaryWindow**: конспект і картки: редаговані тези й терміни, **ListView карток** з CRUD (додати/змінити/видалити), кнопка «Перегенерувати», ComboBox формату експорту, кнопка «Експорт».
6. **SettingsWindow**: TabControl:
   - *Провайдери:* режим (Демо / Реальні), PasswordBox для ключів, кнопка «Перевірити з'єднання», бюджет токенів;
   - *Звук:* список пристроїв, індикатор рівня, кнопка «Тест 5 с»;
   - *Детектор:* **CRUD правил** (DataGrid: фраза, тип, вага, мова; кнопки Додати/Редагувати/Видалити), TextBox тестової фрази з живим результатом, Slider чутливості;
   - *Загальне:* мова інтерфейсу (uk/en, застосовується після перезапуску), мова відповіді, стиль відповіді (коротко/детально), підказка з гарячими клавішами.

Разом: 6 вікон, понад 50 контролів, кілька контейнерів (TreeView, DataGrid, ListView, ListBox), понад 10 обробників подій.

---

## 11. Зовнішні бібліотеки (Лаба 3)

| Бібліотека (NuGet) | Для чого (які можливості) | Відповідальний |
|---|---|---|
| **NAudio** | WASAPI loopback і мікрофон, ресемплінг (WDL), читання WAV | A |
| **NHotkey.Wpf** | Глобальні гарячі клавіші | A |
| **Serilog** (+ Sinks.File, Extensions.Hosting) | Структуроване логування з ротацією файлів | A |
| **OpenAI** *або* **Anthropic** (офіційні SDK) | Streaming, vision, структуровані відповіді, prompt caching. У MVP один із двох (вибір на M0) | B |
| **UglyToad.PdfPig** | Текст із PDF посторінково | B |
| **QuestPDF** | Експорт конспекту в PDF (Community License) | B |
| **Microsoft.EntityFrameworkCore.Sqlite** | Локальна БД, міграції | B |
| **System.Security.Cryptography.ProtectedData** | DPAPI для ключів | B |
| **CommunityToolkit.Mvvm**, **Microsoft.Extensions.Hosting / DI** | MVVM, DI (спільна інфраструктура) | спільне |
| *Тести:* xUnit, NSubstitute; *бенчмарки:* BenchmarkDotNet; *документація:* DocFX | — | не для лаби 3 |

STT: Soniox — WebSocket API напряму через `ClientWebSocket` (без бібліотеки). Якщо спайк обере Deepgram, додається офіційний **Deepgram** .NET SDK.

Не використовувати: **FluentAssertions** (з v8 комерційна ліцензія), **Moq** (історія зі SponsorLink). Нову залежність агент **погоджує з командою** і одразу описує в `LIBRARIES.md`.

---

## 12. Промпти (стартові версії, зберігаються як ресурси)

**Відповідь (system):**
```
Ти — помічник студента на лекції з дисципліни «{course}». Мова відповіді: {language}.
Дай КОРОТКУ відповідь на питання викладача: 1–3 речення, без вступів.
Якщо доречно, додай одну ключову формулу, термін або приклад.
Спирайся на матеріали курсу та транскрипт; якщо береш факт зі слайда/PDF, вкажи це: (слайд/стор. N).
Якщо не впевнений або питання неоднозначне — скажи це одним реченням і запропонуй, як уточнити.
```
Додаток для режиму `Auto`:
```
Якщо останній фрагмент не є питанням, адресованим студентам (риторичне питання,
викладач одразу відповідає сам, організаційна репліка), відповідай рівно: NO_QUESTION
```
Додаток для гарячої клавіші (`Manual`, останні 30 с):
```
Знайди в останніх 30 секундах питання викладача і відповідай на нього.
Якщо питання немає, одним-двома реченнями поясни, про що зараз ішлося.
```
Порядок частин запиту: system → **матеріали курсу (кешований префікс)** → `Останні 3 хв лекції: …` → `Питання: …` → скриншот (якщо є). У Anthropic кешування вмикається явною позначкою на межі префікса, в OpenAI кешування префікса автоматичне; в обох випадках префікс має бути байт-у-байт однаковим між запитами.

**«Детальніше»:** те саме питання й контекст, обмеження «1–3 речення» замінюється на «розгорнуто, до 2 абзаців».

**Конспект (JSON за схемою):** `title`, `key_points[]`, `questions[{q,a}]`, `terms[{term,definition}]`, `flashcards[{front,back}]`. Один запит з повним транскриптом і списком Q&A сесії.

---

## 13. Обробка помилок

| Ситуація | Поведінка |
|---|---|
| Немає аудіопристрою / пристрій зник | `AudioCaptureException` → повідомлення, сесія в `Paused`, пропозиція обрати інший пристрій |
| Обрив WebSocket STT | До 5 спроб перепідключення (1, 2, 4, 8, 16 с), аудіо накопичується в каналі (~10 с); якщо обрив довший, у транскрипті позначається пропуск; після 5 невдач — `Faulted` |
| Невірний API-ключ (401) | Повідомлення + кнопка «Відкрити налаштування»; без повторів |
| 429 / 5xx від LLM | Retry з backoff (до 2 разів, лише до першого токена), далі повідомлення й «Повторити» |
| Таймаут LLM > 8 с до першого токена | Скасування, у спливаючому вікні «Не вдалося отримати відповідь» + «Повторити» |
| Модель відповіла `NO_QUESTION` | Відповідь не показується, запис у списку питань отримує статус «відхилено» |
| PDF не читається / зашифрований | `ContextLoadException`, матеріал позначається помилкою, решта працює |
| Набір матеріалів перевищує бюджет токенів | Старт сесії заблоковано з поясненням, що звузити |
| Транскрипт задовгий для конспекту | Зрозуміле повідомлення; транскрипт і Q&A залишаються доступними |
| Пошкоджена/заблокована БД | Лог + повідомлення; історія недоступна, живий режим працює |

Кожен рядок таблиці має **щонайменше один unit-тест** (через фейки та NSubstitute).

---

## 14. Якість: тести, бенчмарки, логи, документація, i18n, стиль

1. **Unit-тести (xUnit + NSubstitute):** евристики детектора (таблиці фраз uk/en), `TimeWindowBuffer`, `Retry`, `PromptFactory` (зокрема незмінність кешованого префікса між запитами), `SessionContext` (бюджет), обробка маркера `NO_QUESTION` в `AnswerService`, стан-машина `LectureSession`, перепідключення `TranscriberBase` (фейковий сокет), експортери, `EfCourseStore`/`EfLectureStore` (SQLite in-memory). Інтеграційний тест «WAV → ScriptedTranscriber → детектор → FakeLlm → відповідь» без мережі.
2. **Бенчмарки (BenchmarkDotNet):** ресемплінг 48 кГц стерео → 16 кГц моно, `HeuristicQuestionDetector.Analyze`, `PromptFactory.CreateAnswerRequest` з 50 сторінками PDF. Плюс **рантайм-метрики** через `LatencyTracker` (розділ 4.1).
3. **Логування (Serilog):** файли в `%LOCALAPPDATA%\LectureMate\logs`, ротація щодня, структуровані поля (`SessionId`, `Stage`, `LatencyMs`). Ключі API й повний текст промптів у логи **не пишемо** (лише довжину і хеш). Core пише через `ILogger<T>`.
4. **Документація:** XML-коментарі для всіх `public` типів і членів; DocFX → GitHub Pages через GitHub Actions.
5. **i18n:** `Resources/Strings.resx` (uk, за замовчуванням) + `Strings.en.resx`; у XAML — `{x:Static}`; культура встановлюється при старті, зміна мови інтерфейсу — після перезапуску; мова відповіді LLM налаштовується окремо від мови інтерфейсу.
6. **Стиль коду:** [.NET C# coding conventions](https://learn.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions) + `.editorconfig`; `dotnet format` у CI; `Nullable` увімкнено; `TreatWarningsAsErrors` у Core.

---

## 15. Розподіл роботи між студентами

Розподіл потрібен для паралельної роботи й зрозумілої відповідальності, а не для підрахунку елементів: мінімуми лаб рахуються на команду. Його можна змінювати за домовленістю.

| Студент A: «Звук, мова, сесія» | Студент B: «ШІ, контекст і дані» |
|---|---|
| Аудіо: `AudioSourceBase`, `WasapiAudioSource`, `WaveFileAudioSource`, ресемплінг | LLM: `LlmClientBase`, реальний клієнт, `FakeLlmClient` |
| STT: `TranscriberBase`, реальний транскрибер, `ScriptedTranscriber`; спайк якості української | `PromptFactory`, `AnswerService` (маркер, скасування, «Детальніше») |
| `HeuristicQuestionDetector`, `DetectorRule` | Контекст: `ContextItem` + 3 типи, `SessionContext`, `PdfPigTextExtractor` |
| `LectureSession`, `TimeWindowBuffer<T>`, `Retry`, `LatencyTracker`, `ProviderFactory` | БД: `AppDbContext`, `EfCourseStore`, `EfLectureStore`, міграції |
| `WindowsScreenCapture`, гарячі клавіші (NHotkey), Serilog, налаштування (`JsonSettingsStore`) | `SummaryService`, `ExporterBase` + 3 експортери, `DpapiSecretStore` |
| Вікна: Main, Settings, History | Вікна: AnswerOverlay, Materials, Summary |
| Інфраструктура i18n (`.resx`), бенчмарки, CI: build + test | CI: DocFX → Pages |

Обидва студенти рев'юють PR одне одного й мають розуміти весь код (вимога доброчесності, 2.4).

### 15.1. Формат `ELEMENTS.md`
```
| # | Тип | Назва | Файл | Примітка |
|---|-----|-------|------|----------|
| 1 | клас | WasapiAudioSource | src/LectureMate.Infrastructure/Audio/WasapiAudioSource.cs | ієрархія AudioSourceBase |
| 2 | метод (нетрив.) | HeuristicQuestionDetector.Analyze | ... | |
| 3 | поліморфізм (стат.) | TimeWindowBuffer<T> + клієнти LectureSession, LatencyTracker | ... | |
| 4 | вікно | MaterialsWindow | src/LectureMate.App/Views/... | |
| 5 | обробник | MaterialsWindow.OnFilesDropped | ... | |
```
Внизу файлу — **підсумкова таблиця кількостей на команду** проти мінімумів лаб 1 і 2.

### 15.2. Git
- `main` захищена; робота в `feature/<коротко>`; PR з рев'ю іншого учасника;
- коміти англійською в стилі Conventional Commits (`feat(audio): add loopback source`);
- щонайменше один коміт у кожен день роботи.

---

## 16. План (віхи)

> Дедлайн здачі — 17.11.2026. Віхи стиснуто в 21 день; розклад по днях у `PLAN.md`
> (M0+M1: 28.10–03.11, M2: 04.11–08.11, M3: 09.11–13.11, M4: 14.11–17.11).

| Віха | Зміст | Закриває |
|---|---|---|
| **M0** (тиждень 1) | Репозиторій, рішення, CI, `.editorconfig`, README. **Спайк:** якість української STT (Soniox/Deepgram/OpenAI на записі лекції) і час до першого токена українською для OpenAI vs Anthropic. Вибір по одному провайдеру; результат у `RETROSPECTIVE.md` | — |
| **M1** (тижні 2–4) | Core повністю: ієрархії, generics, `LectureSession`, детектор, `PromptFactory`, `AnswerService`, `SessionContext`; демо-провайдери (`WaveFileAudioSource`, `ScriptedTranscriber`, `FakeLlmClient`); unit-тести; `ELEMENTS.md` з кількостями | **Лаба 1** |
| **M2** (тижні 5–7) | Усі 6 вікон у режимі «Демо», MVVM, БД, історія, CRUD курсів/матеріалів, правил і карток, i18n | **Лаба 2** |
| **M3** (тижні 8–10) | Реальні провайдери (NAudio loopback, обраний STT, обраний LLM), PdfPig, скриншоти, QuestPDF, Serilog; `LIBRARIES.md`; ретроспектива | **Лаба 3** |
| **M4** (тижні 11–12) | Перевірка й налаштування затримки (кешування префікса, вибір моделі), бенчмарки, DocFX → Pages, полірування, демо на реальній лекції; за наявності часу — другий LLM-провайдер | бонуси |

Принцип: **режим «Демо» з першого дня**. Застосунок завжди має запускатися й демонструватися без інтернету та ключів (WAV + сценарій + фейкова LLM).

---

## 17. Правила для ШІ-агента

1. Не клади логіку в code-behind і не звертайся до SDK провайдерів із `Core` або ViewModel: тільки через інтерфейси.
2. Кожен новий `public` тип або член має XML-документацію. Кожна нова поведінка покривається тестом.
3. Жодних секретів у коді, тестах, логах і комітах. Ключі зберігаються лише через `ISecretStore`.
4. Не додавай залежностей без погодження. Нову бібліотеку одразу опиши в `LIBRARIES.md`.
5. Текст інтерфейсу пишеться тільки через `.resx`. Жодних «зашитих» рядків у XAML/C#.
6. Не додавай абстракцій «на майбутнє» (6.5). Якщо здається, що інтерфейс чи патерн потрібен, поясни навіщо в описі PR.
7. Після зміни онови `ELEMENTS.md` і підсумкову таблицю кількостей.
8. Допиши в `AI_USAGE.md`: дата, хто з команди працював з агентом, коротко задача/промпт, що згенеровано, що змінено вручну. Цього вимагає політика доброчесності курсу.
9. Пояснюй нетривіальні рішення в описі PR: обидва студенти мають розуміти й уміти захистити весь код.
10. Пиши невеликими кроками; одна задача = одна гілка = один PR.
11. Якщо щось у цьому документі заважає правильному дизайну, запропонуй зміну, але **не впроваджуй її мовчки**.

---

## 18. Відкриті питання (узгодити з викладачем)

1. Чи зараховуються `ICommand` (MVVM) як обробники подій для лаби 2?
2. Чи вважаються `Microsoft.Extensions.*` і `CommunityToolkit.Mvvm` зовнішніми бібліотеками для лаби 3?
3. Чи потрібна одна ретроспектива на команду чи окремо від кожного студента?
4. Чи можна використовувати запис реальної лекції (з дозволу викладача) як тестові дані в репозиторії?

---

## 19. Глосарій

- **STT** (speech-to-text): розпізнавання мовлення.
- **Partial / final сегмент**: проміжний (ще змінюється) і остаточний результат STT.
- **Endpointing**: визначення провайдером кінця фрази.
- **Loopback**: захоплення звуку, який відтворюється на ПК.
- **Prompt caching**: кешування незмінного префікса промпту в провайдера; зменшує затримку й ціну.
- **Маркер `NO_QUESTION`**: умовна відповідь моделі, якою вона повідомляє, що фрагмент не є питанням до студентів.
- **Режим «Демо»**: WAV-файл + JSON-сценарій транскрипту + фейкова LLM; працює без мережі.
- **p50 / p90**: медіана та 90-й перцентиль затримки.
- **WER** (word error rate): частка помилково розпізнаних слів.

---

## 20. Що змінилося у v2 і чому

| Було (v1) | Стало (v2) | Чому |
|---|---|---|
| Порівняння стеків | Рішення: C# 14 / .NET 10 + WPF | Стек обрано; .NET 10 постачається з C# 14 |
| Мінімуми лаб 1–2 окремо для кожного студента, атрибуція кожного елемента (2.4, 9, 15) | Мінімуми на команду; `ELEMENTS.md` без автора; розподіл роботи лише для паралельності | Рішення команди; прибрало класи й вікна, які існували тільки для підрахунку |
| 9 вікон | 6 вікон | Налаштування звуку й правила детектора — вкладки в Settings; монітор затримок — статус-бар і затримки в історії |
| Loopback, Microphone, WaveFile — 3 класи | `WasapiAudioSource` (loopback або мікрофон) + `WaveFileAudioSource` | Loopback і мікрофон відрізняються лише класом NAudio |
| `AudioConverter` у конвеєрі, `AudioFormat` у кожному фрагменті | Ресемплінг усередині джерела, один формат на весь конвеєр | Менше рухомих частин, формат не треба перевіряти на кожному кроці |
| `RingBuffer<T>` | Обмежений `Channel<AudioChunk>` з `DropOldest` | Та сама поведінка вбудована в .NET |
| `EventBus` | Події C# + `Dispatcher` у ViewModel | Власна шина дублює стандартний механізм |
| LLM-детектор + Composite + спекулятивна генерація | Евристика + маркер `NO_QUESTION` у запиті-відповіді | Один запит замість двох, без складного скасування, без додаткової затримки |
| Правила «пауза ≥ 1,5 с» і «викладач сам відповів за 5 с» | Прибрано | Обидва вимагають чекати й ламають бюджет 2,5 с; риторику відсіює модель |
| `IContextSelector` (усі / TF-IDF) | Набір матеріалів фіксується на сесію (`SessionContext`) з перевіркою бюджету | Вибір під кожне питання змінює префікс і вимикає prompt caching |
| `ScreenshotContext : ImageContext`, два класи захоплення | `ImageContext` + `WindowsScreenCapture` з режимом | Скриншот — це просто зображення; різниця між режимами — один параметр |
| `IPromptTemplate<T>` + fluent `PromptBuilder` | `PromptFactory` | Два механізми для однієї задачі |
| `EfRepository<T>` + `Result<T>` + окремі `*Record`-сутності | `ICourseStore` / `ILectureStore`, доменні типи мапляться напряму, помилки — винятками | Узагальнений репозиторій поверх EF дублює `DbContext`; дві системи помилок — зайве |
| `LectureSession` + `SessionOrchestrator` | `LectureSession` + `AnswerService` | Стан і конвеєр сесії разом; генерація відповідей окремо |
| Конспект map-reduce | Один запит | Лекція 80–90 хв вміщується в контекст сучасних моделей |
| 2 STT + 2 LLM + резервний провайдер | По одному реальному провайдеру (вибір на M0) + демо-провайдери | Другий провайдер — один клас на M4, якщо буде потреба |
| ScottPlot, WPFLocalizeExtension, Deepgram SDK | Прибрано (Deepgram — лише якщо переможе в спайку) | Графік і живе перемикання мови не потрібні для MVP |
| Налаштовувані гарячі клавіші, перемикання мови без перезапуску | Фіксовані клавіші, мова — після перезапуску | Мало користі для MVP, помітна складність |
| — | Інкрементний запис транскрипту, зсув часу після перепідключення, терміни курсу для STT | Закриті прогалини v1: втрата даних при падінні, зламані мітки часу |
