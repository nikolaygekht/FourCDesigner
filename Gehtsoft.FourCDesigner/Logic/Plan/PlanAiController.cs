using System;
using System.Threading.Tasks;
using Gehtsoft.FourCDesigner.Logic.AI;
using Microsoft.Extensions.Logging;

namespace Gehtsoft.FourCDesigner.Logic.Plan;

/// <summary>
/// ECB Controller for AI-assisted lesson plan operations.
/// </summary>
public class PlanAiController : IPlanAiController
{
    private readonly IAIDriver mAiDriver;
    private readonly IPromptFactory mPromptFactory;
    private readonly ILessonPlanFormatter mFormatter;
    private readonly ILogger<PlanAiController> mLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlanAiController"/> class.
    /// </summary>
    /// <param name="aiDriver">The AI driver for making AI requests.</param>
    /// <param name="promptFactory">The factory for retrieving prompts.</param>
    /// <param name="formatter">The formatter for converting lesson plans to structured input.</param>
    /// <param name="logger">The logger instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public PlanAiController(
        IAIDriver aiDriver,
        IPromptFactory promptFactory,
        ILessonPlanFormatter formatter,
        ILogger<PlanAiController> logger)
    {
        if (aiDriver == null)
            throw new ArgumentNullException(nameof(aiDriver));

        if (promptFactory == null)
            throw new ArgumentNullException(nameof(promptFactory));

        if (formatter == null)
            throw new ArgumentNullException(nameof(formatter));

        if (logger == null)
            throw new ArgumentNullException(nameof(logger));

        mAiDriver = aiDriver;
        mPromptFactory = promptFactory;
        mFormatter = formatter;
        mLogger = logger;
    }

    /// <summary>
    /// Processes an AI assistance request for a lesson plan.
    /// </summary>
    /// <param name="requestId">The type of assistance request.</param>
    /// <param name="plan">The lesson plan to process.</param>
    /// <returns>An AIResult containing the AI response or error information.</returns>
    /// <exception cref="ArgumentNullException">Thrown when plan is null.</exception>
    /// <exception cref="ArgumentException">Thrown when requestId is not recognized.</exception>
    public async Task<AIResult> Request(RequestId requestId, LessonPlan plan)
    {
        if (plan == null)
            throw new ArgumentNullException(nameof(plan));

        mLogger.LogInformation(
            "Processing AI request {RequestId} for topic: {Topic}",
            requestId,
            plan.Topic);

        try
        {
            string prompt = mPromptFactory.GetPrompt(requestId);
            string userInput = mFormatter.FormatLessonPlan(plan, requestId);

            mLogger.LogDebug(
                "Validating user input for request {RequestId}",
                requestId);

            AIResult validationResult = await mAiDriver.ValidateUserInputAsync(userInput);

            if (!validationResult.Successful)
            {
                mLogger.LogWarning(
                    "User input validation failed for request {RequestId}: {ErrorCode}",
                    requestId,
                    validationResult.ErrorCode);

                return AIResult.Failed(
                    "VALIDATION_FAILED",
                    $"Input validation failed: {validationResult.ErrorCode}");
            }

            mLogger.LogDebug(
                "Sending AI request {RequestId}",
                requestId);

            AIResult result = await mAiDriver.GetSuggestionsAsync(prompt, userInput);

            if (result.Successful)
            {
                mLogger.LogInformation(
                    "Successfully processed AI request {RequestId}",
                    requestId);
            }
            else
            {
                mLogger.LogWarning(
                    "AI request {RequestId} failed: {ErrorCode}",
                    requestId,
                    result.ErrorCode);
            }

            return result;
        }
        catch (ArgumentException ex)
        {
            mLogger.LogError(
                ex,
                "Invalid request ID or configuration error for {RequestId}",
                requestId);

            return AIResult.Failed("INVALID_REQUEST", ex.Message);
        }
        catch (Exception ex)
        {
            mLogger.LogError(
                ex,
                "Unexpected error processing AI request {RequestId}",
                requestId);

            return AIResult.Failed("INTERNAL_ERROR", "An unexpected error occurred");
        }
    }
}
