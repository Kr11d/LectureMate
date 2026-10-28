namespace LectureMate.Core.Domain;

/// <summary>
/// How a <see cref="QuestionCandidate"/> was produced.
/// </summary>
public enum QuestionSource
{
    /// <summary>Found automatically by the heuristic detector.</summary>
    Heuristic,

    /// <summary>Requested by the user with the hotkey or the button.</summary>
    Manual,

    /// <summary>Requested by the user together with a screenshot.</summary>
    Screenshot,
}
