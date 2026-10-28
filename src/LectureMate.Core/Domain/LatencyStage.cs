namespace LectureMate.Core.Domain;

/// <summary>
/// A measured stage of the answer pipeline (HANDOFF section 4.1).
/// </summary>
public enum LatencyStage
{
    /// <summary>End of speech to the final transcript segment.</summary>
    SttFinal,

    /// <summary>Question detection.</summary>
    Detection,

    /// <summary>Building the prompt.</summary>
    PromptBuild,

    /// <summary>Request sent to the first token received from the model.</summary>
    FirstToken,

    /// <summary>End of the question to the first words of the answer.</summary>
    Total,
}
