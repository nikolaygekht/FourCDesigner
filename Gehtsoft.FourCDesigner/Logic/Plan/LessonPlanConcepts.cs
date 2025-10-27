using System.Text.Json.Serialization;

namespace Gehtsoft.FourCDesigner.Logic.Plan;

/// <summary>
/// Represents the concepts phase of a lesson plan.
/// Learners take in information in a multi-sensory way.
/// </summary>
public class LessonPlanConcepts
{
    /// <summary>
    /// Gets or sets the time allocated for the concepts phase in minutes.
    /// Default is 15 minutes.
    /// </summary>
    [JsonPropertyName("timing")]
    public int Timing { get; set; }

    /// <summary>
    /// Gets or sets the list of need-to-know concepts.
    /// Essential concepts that learners must understand.
    /// </summary>
    [JsonPropertyName("needToKnow")]
    public string NeedToKnow { get; set; }

    /// <summary>
    /// Gets or sets the list of good-to-know concepts.
    /// Additional helpful concepts for learners.
    /// </summary>
    [JsonPropertyName("goodToKnow")]
    public string GoodToKnow { get; set; }

    /// <summary>
    /// Gets or sets the key theses to be delivered.
    /// </summary>
    [JsonPropertyName("theses")]
    public string Theses { get; set; }

    /// <summary>
    /// Gets or sets the structure of the lesson delivery.
    /// </summary>
    [JsonPropertyName("structure")]
    public string Structure { get; set; }

    /// <summary>
    /// Gets or sets the activities to engage all learning styles (VARK) and six trumps.
    /// </summary>
    [JsonPropertyName("activities")]
    public string Activities { get; set; }

    /// <summary>
    /// Gets or sets the list of materials to prepare for the concepts phase.
    /// </summary>
    [JsonPropertyName("materialsToPrepare")]
    public string MaterialsToPrepare { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LessonPlanConcepts"/> class.
    /// </summary>
    public LessonPlanConcepts()
    {
        Timing = 15;
        NeedToKnow = string.Empty;
        GoodToKnow = string.Empty;
        Theses = string.Empty;
        Structure = string.Empty;
        Activities = string.Empty;
        MaterialsToPrepare = string.Empty;
    }
}
