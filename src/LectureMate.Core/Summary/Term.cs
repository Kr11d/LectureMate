namespace LectureMate.Core.Summary;

/// <summary>
/// A term introduced in the lecture.
/// </summary>
/// <param name="Name">The term itself.</param>
/// <param name="Definition">A short definition.</param>
public sealed record Term(string Name, string Definition);
