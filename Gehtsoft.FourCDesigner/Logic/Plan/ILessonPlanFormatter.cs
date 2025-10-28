namespace Gehtsoft.FourCDesigner.Logic.Plan;

/// <summary>
/// Formats lesson plan data into structured user input for AI processing.
/// </summary>
public interface ILessonPlanFormatter
{
    /// <summary>
    /// Formats the lesson plan into a structured string appropriate for the specified request.
    /// Extracts only the relevant context fields needed for the operation.
    /// </summary>
    /// <param name="plan">The lesson plan to format.</param>
    /// <param name="requestId">The request identifier indicating which operation is being performed.</param>
    /// <returns>A structured string representation of the relevant lesson plan data.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown when plan is null.</exception>
    /// <exception cref="System.ArgumentException">Thrown when requestId is not recognized.</exception>
    string FormatLessonPlan(LessonPlan plan, RequestId requestId);
}
