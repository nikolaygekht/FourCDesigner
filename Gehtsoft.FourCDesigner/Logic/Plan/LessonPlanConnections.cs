using System.Text.Json.Serialization;

namespace Gehtsoft.FourCDesigner.Logic.Plan;

/// <summary>
/// Represents the connections phase of a lesson plan.
/// Learners make connections with what they already know and with each other.
/// </summary>
public class LessonPlanConnections
{
    /// <summary>
    /// Gets or sets the time allocated for the connections phase in minutes.
    /// Default is 5 minutes.
    /// </summary>
    [JsonPropertyName("timing")]
    public int Timing { get; set; }

    /// <summary>
    /// Gets or sets the goal of the connections phase.
    /// Describes what connections learners should make.
    /// </summary>
    [JsonPropertyName("goal")]
    public string Goal { get; set; }

    /// <summary>
    /// Gets or sets the activities to perform during the connections phase.
    /// </summary>
    [JsonPropertyName("activities")]
    public string Activities { get; set; }

    /// <summary>
    /// Gets or sets the list of materials to prepare for the connections phase.
    /// </summary>
    [JsonPropertyName("materialsToPrepare")]
    public string MaterialsToPrepare { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LessonPlanConnections"/> class.
    /// </summary>
    public LessonPlanConnections()
    {
        Timing = 5;
        Goal = string.Empty;
        Activities = string.Empty;
        MaterialsToPrepare = string.Empty;
    }
}
