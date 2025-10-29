using System;
using System.Collections.Generic;
using Gehtsoft.FourCDesigner.Logic.Plan;

namespace Gehtsoft.FourCDesigner.Controllers.Data;

/// <summary>
/// Converts client-side operation identifiers (snake_case strings) to RequestId enum values.
/// </summary>
public static class RequestIdConverter
{
    private static readonly Dictionary<string, RequestId> gOperationMap = BuildOperationMap();

    private static Dictionary<string, RequestId> BuildOperationMap()
    {
        return new Dictionary<string, RequestId>(StringComparer.Ordinal)
        {
            // Overview Section
            ["review_context"] = RequestId.ReviewContext,
            ["suggest_context"] = RequestId.SuggestContext,
            ["review_topic"] = RequestId.ReviewTopic,
            ["suggest_topic"] = RequestId.SuggestTopic,
            ["review_audience"] = RequestId.ReviewAudience,
            ["suggest_audience"] = RequestId.SuggestAudience,
            ["review_outcomes"] = RequestId.ReviewOutcomes,
            ["suggest_outcomes"] = RequestId.SuggestOutcomes,

            // Connection Phase
            ["review_conn_goal"] = RequestId.ReviewConnGoal,
            ["suggest_conn_goal"] = RequestId.SuggestConnGoal,
            ["review_conn_activities"] = RequestId.ReviewConnActivities,
            ["suggest_conn_activities"] = RequestId.SuggestConnActivities,
            ["review_conn_materials"] = RequestId.ReviewConnMaterials,
            ["suggest_conn_materials"] = RequestId.SuggestConnMaterials,

            // Concepts Phase
            ["review_concepts_needToKnow"] = RequestId.ReviewConceptsNeedToKnow,
            ["suggest_concepts_needToKnow"] = RequestId.SuggestConceptsNeedToKnow,
            ["review_concepts_goodToKnow"] = RequestId.ReviewConceptsGoodToKnow,
            ["suggest_concepts_goodToKnow"] = RequestId.SuggestConceptsGoodToKnow,
            ["review_concepts_theses"] = RequestId.ReviewConceptsTheses,
            ["suggest_concepts_theses"] = RequestId.SuggestConceptsTheses,
            ["review_concepts_structure"] = RequestId.ReviewConceptsStructure,
            ["suggest_concepts_structure"] = RequestId.SuggestConceptsStructure,
            ["review_concepts_activities"] = RequestId.ReviewConceptsActivities,
            ["suggest_concepts_activities"] = RequestId.SuggestConceptsActivities,
            ["review_concepts_materials"] = RequestId.ReviewConceptsMaterials,
            ["suggest_concepts_materials"] = RequestId.SuggestConceptsMaterials,

            // Concrete Practice Phase
            ["review_practice_output"] = RequestId.ReviewPracticeOutput,
            ["suggest_practice_output"] = RequestId.SuggestPracticeOutput,
            ["review_practice_focus"] = RequestId.ReviewPracticeFocus,
            ["suggest_practice_focus"] = RequestId.SuggestPracticeFocus,
            ["review_practice_activities"] = RequestId.ReviewPracticeActivities,
            ["suggest_practice_activities"] = RequestId.SuggestPracticeActivities,
            ["review_practice_details"] = RequestId.ReviewPracticeDetails,
            ["suggest_practice_details"] = RequestId.SuggestPracticeDetails,
            ["review_practice_materials"] = RequestId.ReviewPracticeMaterials,
            ["suggest_practice_materials"] = RequestId.SuggestPracticeMaterials,

            // Conclusion Phase
            ["review_concl_goal"] = RequestId.ReviewConclGoal,
            ["suggest_concl_goal"] = RequestId.SuggestConclGoal,
            ["review_concl_activities"] = RequestId.ReviewConclActivities,
            ["suggest_concl_activities"] = RequestId.SuggestConclActivities,
            ["review_concl_materials"] = RequestId.ReviewConclMaterials,
            ["suggest_concl_materials"] = RequestId.SuggestConclMaterials,

            // Whole Lesson Review
            ["review_whole_lesson"] = RequestId.ReviewWholeLesson
        };
    }

    /// <summary>
    /// Attempts to convert a client-side operation identifier to a RequestId enum value.
    /// </summary>
    /// <param name="operationId">The client-side operation identifier (e.g., "review_topic").</param>
    /// <param name="requestId">The converted RequestId value, or default if conversion fails.</param>
    /// <returns>True if conversion succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when operationId is null.</exception>
    public static bool TryConvert(string operationId, out RequestId requestId)
    {
        if (operationId == null)
            throw new ArgumentNullException(nameof(operationId));

        return gOperationMap.TryGetValue(operationId, out requestId);
    }

    /// <summary>
    /// Converts a client-side operation identifier to a RequestId enum value.
    /// </summary>
    /// <param name="operationId">The client-side operation identifier (e.g., "review_topic").</param>
    /// <returns>The converted RequestId value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when operationId is null.</exception>
    /// <exception cref="ArgumentException">Thrown when operationId is not a valid operation identifier.</exception>
    public static RequestId Convert(string operationId)
    {
        if (operationId == null)
            throw new ArgumentNullException(nameof(operationId));

        if (!gOperationMap.TryGetValue(operationId, out RequestId requestId))
            throw new ArgumentException($"Unknown operation identifier: '{operationId}'", nameof(operationId));

        return requestId;
    }

    /// <summary>
    /// Checks if the specified operation identifier is valid.
    /// </summary>
    /// <param name="operationId">The client-side operation identifier to check.</param>
    /// <returns>True if the operation identifier is valid; otherwise, false.</returns>
    public static bool IsValid(string operationId)
    {
        if (operationId == null)
            return false;

        return gOperationMap.ContainsKey(operationId);
    }
}
