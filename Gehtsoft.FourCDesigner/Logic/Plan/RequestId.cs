namespace Gehtsoft.FourCDesigner.Logic.Plan;

/// <summary>
/// Identifies AI assistance operations for the 4C Instructional Design Tool.
/// Total of 41 operations organized by lesson phase and operation type (review/suggest).
/// </summary>
public enum RequestId
{
    // Overview Section (6 operations)

    /// <summary>
    /// Review if topic is clear and appropriate for TBR methodology.
    /// </summary>
    ReviewTopic,

    /// <summary>
    /// Suggest improvements to make topic clearer.
    /// </summary>
    SuggestTopic,

    /// <summary>
    /// Review if audience description is complete.
    /// </summary>
    ReviewAudience,

    /// <summary>
    /// Suggest additional audience characteristics to consider.
    /// </summary>
    SuggestAudience,

    /// <summary>
    /// Review if learning outcomes are measurable and achievable.
    /// </summary>
    ReviewOutcomes,

    /// <summary>
    /// Suggest specific, measurable learning outcomes.
    /// </summary>
    SuggestOutcomes,

    // Connection Phase (6 operations)

    /// <summary>
    /// Review if connection goal aligns with TBR principles.
    /// </summary>
    ReviewConnGoal,

    /// <summary>
    /// Suggest connection goals based on topic and audience.
    /// </summary>
    SuggestConnGoal,

    /// <summary>
    /// Review if activities achieve connection goal within allocated time.
    /// </summary>
    ReviewConnActivities,

    /// <summary>
    /// Suggest specific connection activities.
    /// </summary>
    SuggestConnActivities,

    /// <summary>
    /// Review if materials list is complete for planned activities.
    /// </summary>
    ReviewConnMaterials,

    /// <summary>
    /// Suggest materials needed for connection activities.
    /// </summary>
    SuggestConnMaterials,

    // Concepts Phase (12 operations)

    /// <summary>
    /// Review if essential concepts cover what's needed for outcomes.
    /// </summary>
    ReviewConceptsNeedToKnow,

    /// <summary>
    /// Suggest essential concepts learners must understand.
    /// </summary>
    SuggestConceptsNeedToKnow,

    /// <summary>
    /// Review if "good to know" is appropriate and doesn't overwhelm.
    /// </summary>
    ReviewConceptsGoodToKnow,

    /// <summary>
    /// Suggest additional helpful concepts.
    /// </summary>
    SuggestConceptsGoodToKnow,

    /// <summary>
    /// Review if theses effectively convey the concepts.
    /// </summary>
    ReviewConceptsTheses,

    /// <summary>
    /// Suggest key theses to deliver.
    /// </summary>
    SuggestConceptsTheses,

    /// <summary>
    /// Review if delivery structure is logical and fits allocated time.
    /// </summary>
    ReviewConceptsStructure,

    /// <summary>
    /// Suggest delivery structure.
    /// </summary>
    SuggestConceptsStructure,

    /// <summary>
    /// Review if activities engage VARK learners and apply six trumps.
    /// </summary>
    ReviewConceptsActivities,

    /// <summary>
    /// Suggest multi-sensory activities using six trumps.
    /// </summary>
    SuggestConceptsActivities,

    /// <summary>
    /// Review if materials support the activities.
    /// </summary>
    ReviewConceptsMaterials,

    /// <summary>
    /// Suggest materials for concept activities.
    /// </summary>
    SuggestConceptsMaterials,

    // Concrete Practice Phase (10 operations)

    /// <summary>
    /// Review if desired output aligns with learning outcomes.
    /// </summary>
    ReviewPracticeOutput,

    /// <summary>
    /// Suggest what learners should produce.
    /// </summary>
    SuggestPracticeOutput,

    /// <summary>
    /// Review if focus areas target key challenges.
    /// </summary>
    ReviewPracticeFocus,

    /// <summary>
    /// Suggest where learners will struggle most.
    /// </summary>
    SuggestPracticeFocus,

    /// <summary>
    /// Review if activities lead to desired output.
    /// </summary>
    ReviewPracticeActivities,

    /// <summary>
    /// Suggest practice activities.
    /// </summary>
    SuggestPracticeActivities,

    /// <summary>
    /// Review if execution plan is clear and complete.
    /// </summary>
    ReviewPracticeDetails,

    /// <summary>
    /// Suggest step-by-step execution plan.
    /// </summary>
    SuggestPracticeDetails,

    /// <summary>
    /// Review if materials are sufficient for practice.
    /// </summary>
    ReviewPracticeMaterials,

    /// <summary>
    /// Suggest materials needed for practice.
    /// </summary>
    SuggestPracticeMaterials,

    // Conclusion Phase (6 operations)

    /// <summary>
    /// Review if conclusion goal supports reflection and action planning.
    /// </summary>
    ReviewConclGoal,

    /// <summary>
    /// Suggest conclusion goals.
    /// </summary>
    SuggestConclGoal,

    /// <summary>
    /// Review if activities achieve conclusion goals.
    /// </summary>
    ReviewConclActivities,

    /// <summary>
    /// Suggest conclusion activities (summarize, evaluate, action plan, celebrate).
    /// </summary>
    SuggestConclActivities,

    /// <summary>
    /// Review if materials support conclusion activities.
    /// </summary>
    ReviewConclMaterials,

    /// <summary>
    /// Suggest materials needed for conclusion.
    /// </summary>
    SuggestConclMaterials,

    // Whole Lesson Review (1 operation)

    /// <summary>
    /// Holistic review of entire lesson for coherence, timing balance, and TBR principles adherence.
    /// </summary>
    ReviewWholeLesson
}
