using System.Text;

namespace LlmLatency;

/// <summary>
/// The request contents: HANDOFF §12 system prompt, course materials as the cached prefix,
/// and a lecture transcript with a question as the variable part.
/// </summary>
internal static class Scenario
{
    // HANDOFF §12: answer prompt plus the Auto-mode addendum (the main scenario S2),
    // with {course} and {language} filled in.
    public static readonly string SystemPrompt = """
        Ти — помічник студента на лекції з дисципліни «Основи об'єктно-орієнтованого програмування». Мова відповіді: українська.
        Дай КОРОТКУ відповідь на питання викладача: 1–3 речення, без вступів.
        Якщо доречно, додай одну ключову формулу, термін або приклад.
        Спирайся на матеріали курсу та транскрипт; якщо береш факт зі слайда/PDF, вкажи це: (слайд/стор. N).
        Якщо не впевнений або питання неоднозначне — скажи це одним реченням і запропонуй, як уточнити.
        Якщо останній фрагмент не є питанням, адресованим студентам (риторичне питання,
        викладач одразу відповідає сам, організаційна репліка), відповідай рівно: NO_QUESTION
        """.ReplaceLineEndings("\n");

    public const string NoQuestionMarker = "NO_QUESTION";

    public static readonly IReadOnlyList<string> Questions =
    [
        "Чим абстрактний клас відрізняється від інтерфейсу?",
        "Що таке інкапсуляція і навіщо приховувати поля класу?",
        "Яку проблему розв'язує принцип підстановки Лісков? Наведіть приклад порушення.",
        "Чим статичний поліморфізм відрізняється від динамічного?",
        "Коли варто обрати композицію замість успадкування?",
        "Яку задачу розв'язує патерн «Стратегія»?",
        "Чому не можна залишати порожній блок catch?",
        "Як під час виконання обирається реалізація віртуального методу?",
        "Навіщо потрібен принцип інверсії залежностей?",
        "Що дає обмеження where в узагальнених типах?",
    ];

    public static string BuildPrompt(string question)
    {
        const string transcript = """
        …так, повертаємося. Минулого разу ми зупинилися на тому, що об'єкт — це не просто набір даних,
        а дані разом із поведінкою, і що клас сам вирішує, що показати назовні. Сьогодні я хочу зв'язати
        це з успадкуванням і поліморфізмом. Подивіться на слайд із фігурами: є базовий клас, є нащадки,
        і клієнтський код працює з посиланням на базовий тип, не знаючи, який саме об'єкт прийде.
        Це те, що ми називаємо динамічним поліморфізмом. А в кінці минулої пари ви питали про generics —
        до них ми теж сьогодні дійдемо, бо це інший, статичний вид поліморфізму. І ще одне: у лабораторній
        багато хто зробив глибокі ієрархії на п'ять рівнів. Ми поговоримо, чому це часто погана ідея
        і що пропонують принципи SOLID. Добре. Тепер питання до вас, подумайте хвилинку.
        """;
        return $"Останні 3 хв лекції: {transcript.ReplaceLineEndings(" ")} {question}\n\nПитання: {question}";
    }

    public static string LoadMaterials(string path)
    {
        var files = Directory.Exists(path)
            ? Directory.GetFiles(path, "*.md").Order(StringComparer.Ordinal).ToArray()
            : [path];
        if (files.Length == 0)
        {
            throw new FileNotFoundException($"No *.md materials found in '{path}'.");
        }

        var builder = new StringBuilder();
        foreach (var file in files)
        {
            builder.Append("=== Документ: ").Append(Path.GetFileNameWithoutExtension(file)).Append(" ===\n\n");
            builder.Append(File.ReadAllText(file, Encoding.UTF8).ReplaceLineEndings("\n").Trim()).Append("\n\n");
        }

        return builder.ToString();
    }

    /// <summary>
    /// Builds the cached prefix. The per-run set id makes the first request of each run
    /// a guaranteed cache miss; within a run the prefix is byte-for-byte identical.
    /// </summary>
    public static string BuildCachedPrefix(string materials, Guid setId) =>
        $"Матеріали курсу (набір {setId:N}):\n\n{materials}";
}
