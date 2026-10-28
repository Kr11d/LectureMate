namespace LectureMate.Core.Summary;

/// <summary>
/// A review card with a prompt on the front and the answer on the back.
/// </summary>
/// <param name="Front">The prompt side.</param>
/// <param name="Back">The answer side.</param>
public sealed record Flashcard(string Front, string Back);
