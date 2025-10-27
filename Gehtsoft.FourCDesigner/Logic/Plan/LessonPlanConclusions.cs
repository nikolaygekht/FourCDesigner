using System.Text.Json.Serialization;

namespace Gehtsoft.FourCDesigner.Logic.Plan;

/// <summary>
/// Represents the conclusions phase of a lesson plan.
/// Learners summarize what they have learned and make action plans.
/// </summary>
public class LessonPlanConclusions
{
    /// <summary>
    /// Gets or sets the time allocated for the conclusions phase in minutes.
    /// Default is 5 minutes.
    /// </summary>
    [JsonPropertyName("timing")]
    public int Timing { get; set; }

    /// <summary>
    /// Gets or sets the goal of the conclusions phase.
    /// Describes what learners should accomplish in the conclusion.
    /// </summary>
    [JsonPropertyName("goal")]
    public string Goal { get; set; }

    /// <summary>
    /// Gets or sets the activities to perform during the conclusions phase.
    /// </summary>
    [JsonPropertyName("activities")]
    public string Activities { get; set; }

    /// <summary>
    /// Gets or sets the list of materials to prepare for the conclusions phase.
    /// </summary>
    [JsonPropertyName("materialsToPrepare")]
    public string MaterialsToPrepare { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LessonPlanConclusions"/> class.
    /// </summary>
    public LessonPlanConclusions()
    {
        Timing = 5;
        Goal = string.Empty;
        Activities = string.Empty;
        MaterialsToPrepare = string.Empty;
    }
}
