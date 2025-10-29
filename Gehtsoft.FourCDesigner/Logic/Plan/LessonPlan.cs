using System.Text.Json.Serialization;

namespace Gehtsoft.FourCDesigner.Logic.Plan;

/// <summary>
/// Represents a complete 4C lesson plan.
/// Includes topic, audience, learning outcomes, and all four phases (Connections, Concepts, Concrete Practice, Conclusions).
/// </summary>
public class LessonPlan
{
    /// <summary>
    /// Gets or sets the context of the lesson.
    /// Important information about the plan required to properly understand the intent of the lesson designer.
    /// This includes purpose, intent, scope, limitations, and conditions of the lesson.
    /// </summary>
    [JsonPropertyName("context")]
    public string Context { get; set; }

    /// <summary>
    /// Gets or sets the topic of the lesson.
    /// A short description of the desired purpose of the lesson.
    /// </summary>
    [JsonPropertyName("topic")]
    public string Topic { get; set; }

    /// <summary>
    /// Gets or sets the target audience description.
    /// Describes who the learners are, their characteristics (age, education, job function, etc.) and their needs.
    /// </summary>
    [JsonPropertyName("audience")]
    public string Audience { get; set; }

    /// <summary>
    /// Gets or sets the learning outcomes.
    /// Description of what learners will be able to do after attending the lesson.
    /// </summary>
    [JsonPropertyName("learningOutcomes")]
    public string LearningOutcomes { get; set; }

    /// <summary>
    /// Gets or sets the connections phase of the lesson.
    /// </summary>
    [JsonPropertyName("connections")]
    public LessonPlanConnections Connections { get; set; }

    /// <summary>
    /// Gets or sets the concepts phase of the lesson.
    /// </summary>
    [JsonPropertyName("concepts")]
    public LessonPlanConcepts Concepts { get; set; }

    /// <summary>
    /// Gets or sets the concrete practice phase of the lesson.
    /// </summary>
    [JsonPropertyName("concretePractice")]
    public LessonPlanConcretePractice ConcretePractice { get; set; }

    /// <summary>
    /// Gets or sets the conclusions phase of the lesson.
    /// </summary>
    [JsonPropertyName("conclusions")]
    public LessonPlanConclusions Conclusions { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LessonPlan"/> class.
    /// </summary>
    public LessonPlan()
    {
        Context = string.Empty;
        Topic = string.Empty;
        Audience = string.Empty;
        LearningOutcomes = string.Empty;
        Connections = new LessonPlanConnections();
        Concepts = new LessonPlanConcepts();
        ConcretePractice = new LessonPlanConcretePractice();
        Conclusions = new LessonPlanConclusions();
    }
}
