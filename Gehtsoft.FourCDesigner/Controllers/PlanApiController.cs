using Gehtsoft.FourCDesigner.Controllers.Data;
using Gehtsoft.FourCDesigner.Logic.AI;
using Gehtsoft.FourCDesigner.Logic.Plan;
using Gehtsoft.FourCDesigner.Middleware.Authorization;
using Gehtsoft.FourCDesigner.Middleware.Throttling;
using Microsoft.AspNetCore.Mvc;

namespace Gehtsoft.FourCDesigner.Controllers;

/// <summary>
/// API Controller for AI-assisted lesson plan operations.
/// </summary>
[ApiController]
[Route("api/plan")]
[AuthorizationRequired]
public class PlanApiController : ControllerBase
{
    private readonly IPlanAiController mPlanAiController;
    private readonly ILogger<PlanApiController> mLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlanApiController"/> class.
    /// </summary>
    /// <param name="planAiController">The plan AI controller.</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public PlanApiController(
        IPlanAiController planAiController,
        ILogger<PlanApiController> logger)
    {
        mPlanAiController = planAiController ?? throw new ArgumentNullException(nameof(planAiController));
        mLogger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Requests AI assistance for a lesson plan operation.
    /// </summary>
    /// <param name="request">The AI assistance request containing operation ID and lesson plan.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>AI result containing suggestions or feedback.</returns>
    [HttpPost("assistance")]
    [Throttle(5000, 100, false)]
    public async Task<IActionResult> RequestAssistance(
        [FromBody] PlanAiRequest request,
        CancellationToken cancellationToken)
    {
        // Validate request model
        if (!ModelState.IsValid)
        {
            mLogger.LogWarning("Plan AI request validation failed");
            return BadRequest(ModelState);
        }

        // Validate and convert operation ID
        if (!RequestIdConverter.TryConvert(request.OperationId, out RequestId requestId))
        {
            mLogger.LogWarning(
                "Invalid operation ID received: {OperationId}",
                request.OperationId);

            return BadRequest(new
            {
                error = "Invalid operation ID",
                operationId = request.OperationId
            });
        }

        mLogger.LogInformation(
            "AI assistance requested for operation: {OperationId} (RequestId: {RequestId})",
            request.OperationId,
            requestId);

        try
        {
            // Call business logic
            AIResult result = await mPlanAiController.Request(requestId, request.Plan);

            if (result.Successful)
            {
                mLogger.LogInformation(
                    "AI assistance completed successfully for operation: {OperationId}",
                    request.OperationId);

                return Ok(result);
            }

            mLogger.LogWarning(
                "AI assistance failed for operation: {OperationId}, Error: {ErrorCode}",
                request.OperationId,
                result.ErrorCode);

            return StatusCode(500, result);
        }
        catch (ArgumentNullException ex)
        {
            mLogger.LogError(
                ex,
                "Null argument error during AI assistance for operation: {OperationId}",
                request.OperationId);

            return BadRequest(AIResult.Failed(
                "INVALID_REQUEST",
                "Invalid request data"));
        }
        catch (ArgumentException ex)
        {
            mLogger.LogError(
                ex,
                "Invalid argument error during AI assistance for operation: {OperationId}",
                request.OperationId);

            return BadRequest(AIResult.Failed(
                "INVALID_REQUEST",
                ex.Message));
        }
        catch (Exception ex)
        {
            mLogger.LogError(
                ex,
                "Unexpected error during AI assistance for operation: {OperationId}",
                request.OperationId);

            return StatusCode(500, AIResult.Failed(
                "INTERNAL_ERROR",
                "An unexpected error occurred"));
        }
    }
}
