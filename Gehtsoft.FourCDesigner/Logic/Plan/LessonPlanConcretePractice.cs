using System.Text.Json.Serialization;

namespace Gehtsoft.FourCDesigner.Logic.Plan;

/// <summary>
/// Represents the concrete practice phase of a lesson plan.
/// Learners practice a skill or repeat a procedure being learned.
/// </summary>
public class LessonPlanConcretePractice
{
    /// <summary>
    /// Gets or sets the time allocated for the concrete practice phase in minutes.
    /// Default is 25 minutes.
    /// </summary>
    [JsonPropertyName("timing")]
    public int Timing { get; set; }

    /// <summary>
    /// Gets or sets the desired output of the exercise.
    /// Describes what learners should create during the exercise.
    /// </summary>
    [JsonPropertyName("desiredOutput")]
    public string DesiredOutput { get; set; }

    /// <summary>
    /// Gets or sets the focus area for the exercise.
    /// Identifies which concepts, skills, or nuances are most important.
    /// </summary>
    [JsonPropertyName("focusArea")]
    public string FocusArea { get; set; }

    /// <summary>
    /// Gets or sets the activities to perform during the exercise.
    /// </summary>
    [JsonPropertyName("activities")]
    public string Activities { get; set; }

    /// <summary>
    /// Gets or sets the detailed plan on how to run the activities.
    /// Step-by-step description of activity execution.
    /// </summary>
    [JsonPropertyName("details")]
    public string Details { get; set; }

    /// <summary>
    /// Gets or sets the list of materials to prepare for the concrete practice phase.
    /// </summary>
    [JsonPropertyName("materialsToPrepare")]
    public string MaterialsToPrepare { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LessonPlanConcretePractice"/> class.
    /// </summary>
    public LessonPlanConcretePractice()
    {
        Timing = 25;
        DesiredOutput = string.Empty;
        FocusArea = string.Empty;
        Activities = string.Empty;
        Details = string.Empty;
        MaterialsToPrepare = string.Empty;
    }
}
