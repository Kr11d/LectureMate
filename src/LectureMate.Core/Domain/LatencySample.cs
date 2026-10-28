namespace LectureMate.Core.Domain;

/// <summary>
/// One measurement of a pipeline stage.
/// </summary>
/// <param name="Stage">Measured stage.</param>
/// <param name="Start">Moment the stage started, relative to the session start.</param>
/// <param name="End">Moment the stage finished, relative to the session start.</param>
public sealed record LatencySample(LatencyStage Stage, TimeSpan Start, TimeSpan End) : ITimestamped;
