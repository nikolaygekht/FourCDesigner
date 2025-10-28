using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Gehtsoft.FourCDesigner.Logic.Plan;

namespace Gehtsoft.FourCDesigner.Controllers.Data;

/// <summary>
/// Request DTO for AI assistance operations on lesson plans.
/// </summary>
public class PlanAiRequest
{
    /// <summary>
    /// Gets or sets the client-side operation identifier (e.g., "review_topic", "suggest_audience").
    /// </summary>
    [Required(ErrorMessage = "Operation ID is required")]
    [JsonPropertyName("operationId")]
    public string OperationId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the lesson plan data to process.
    /// </summary>
    [Required(ErrorMessage = "Lesson plan is required")]
    [JsonPropertyName("plan")]
    public LessonPlan Plan { get; set; } = new LessonPlan();
}
