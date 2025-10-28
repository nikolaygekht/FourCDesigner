using System.Threading.Tasks;
using Gehtsoft.FourCDesigner.Logic.AI;

namespace Gehtsoft.FourCDesigner.Logic.Plan;

/// <summary>
/// Controller for AI-assisted lesson plan operations.
/// </summary>
public interface IPlanAiController
{
    /// <summary>
    /// Processes an AI assistance request for a lesson plan.
    /// </summary>
    /// <param name="requestId">The type of assistance request.</param>
    /// <param name="plan">The lesson plan to process.</param>
    /// <returns>An AIResult containing the AI response or error information.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown when plan is null.</exception>
    /// <exception cref="System.ArgumentException">Thrown when requestId is not recognized.</exception>
    Task<AIResult> Request(RequestId requestId, LessonPlan plan);
}
