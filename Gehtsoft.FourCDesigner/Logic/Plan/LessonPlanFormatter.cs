using System;
using System.Collections.Generic;
using System.Text;

namespace Gehtsoft.FourCDesigner.Logic.Plan;

/// <summary>
/// Formats lesson plan data into structured user input for AI processing.
/// Registered as transient since it's stateless.
/// </summary>
public class LessonPlanFormatter : ILessonPlanFormatter
{
    /// <summary>
    /// Defines which fields to extract for each request type.
    /// </summary>
    private class FormatConfiguration
    {
        public List<(string Label, Func<LessonPlan, string> Extractor)> Fields { get; }

        public FormatConfiguration()
        {
            Fields = new List<(string, Func<LessonPlan, string>)>();
        }

        public FormatConfiguration Add(string label, Func<LessonPlan, string> extractor)
        {
            Fields.Add((label, extractor));
            return this;
        }
    }

    private static readonly Dictionary<RequestId, FormatConfiguration> gFormatConfigurations = BuildConfigurations();

    private static Dictionary<RequestId, FormatConfiguration> BuildConfigurations()
    {
        Dictionary<RequestId, FormatConfiguration> configs = new Dictionary<RequestId, FormatConfiguration>();

        // Overview Section
        configs[RequestId.ReviewTopic] = new FormatConfiguration()
            .Add("Topic to Review", p => p.Topic);

        configs[RequestId.SuggestTopic] = new FormatConfiguration()
            .Add("Current Topic", p => p.Topic);

        configs[RequestId.ReviewAudience] = new FormatConfiguration()
            .Add("Topic", p => p.Topic)
            .Add("Audience to Review", p => p.Audience);

        configs[RequestId.SuggestAudience] = new FormatConfiguration()
            .Add("Topic", p => p.Topic)
            .Add("Current Audience Description", p => p.Audience);

        configs[RequestId.ReviewOutcomes] = new FormatConfiguration()
            .Add("Topic", p => p.Topic)
            .Add("Audience", p => p.Audience)
            .Add("Learning Outcomes to Review", p => p.LearningOutcomes);

        configs[RequestId.SuggestOutcomes] = new FormatConfiguration()
            .Add("Topic", p => p.Topic)
            .Add("Audience", p => p.Audience)
            .Add("Current Learning Outcomes", p => p.LearningOutcomes);

        // Connection Phase
        configs[RequestId.ReviewConnGoal] = new FormatConfiguration()
            .Add("Topic", p => p.Topic)
            .Add("Audience", p => p.Audience)
            .Add("Learning Outcomes", p => p.LearningOutcomes)
            .Add("Connection Phase Timing", p => $"{p.Connections.Timing} minutes")
            .Add("Connection Goal to Review", p => p.Connections.Goal);

        configs[RequestId.SuggestConnGoal] = new FormatConfiguration()
            .Add("Topic", p => p.Topic)
            .Add("Audience", p => p.Audience)
            .Add("Learning Outcomes", p => p.LearningOutcomes)
            .Add("Connection Phase Timing", p => $"{p.Connections.Timing} minutes");

        configs[RequestId.ReviewConnActivities] = new FormatConfiguration()
            .Add("Topic", p => p.Topic)
            .Add("Audience", p => p.Audience)
            .Add("Learning Outcomes", p => p.LearningOutcomes)
            .Add("Connection Phase Timing", p => $"{p.Connections.Timing} minutes")
            .Add("Connection Goal", p => p.Connections.Goal)
            .Add("Connection Activities to Review", p => p.Connections.Activities);

        configs[RequestId.SuggestConnActivities] = new FormatConfiguration()
            .Add("Topic", p => p.Topic)
            .Add("Audience", p => p.Audience)
            .Add("Learning Outcomes", p => p.LearningOutcomes)
            .Add("Connection Phase Timing", p => $"{p.Connections.Timing} minutes")
            .Add("Connection Goal", p => p.Connections.Goal);

        configs[RequestId.ReviewConnMaterials] = new FormatConfiguration()
            .Add("Topic", p => p.Topic)
            .Add("Audience", p => p.Audience)
            .Add("Connection Phase Timing", p => $"{p.Connections.Timing} minutes")
            .Add("Connection Goal", p => p.Connections.Goal)
            .Add("Connection Activities", p => p.Connections.Activities)
            .Add("Materials to Review", p => p.Connections.MaterialsToPrepare);

        configs[RequestId.SuggestConnMaterials] = new FormatConfiguration()
            .Add("Topic", p => p.Topic)
            .Add("Audience", p => p.Audience)
            .Add("Connection Phase Timing", p => $"{p.Connections.Timing} minutes")
            .Add("Connection Goal", p => p.Connections.Goal)
            .Add("Connection Activities", p => p.Connections.Activities);

        // Concepts Phase
        configs[RequestId.ReviewConceptsNeedToKnow] = new FormatConfiguration()
            .Add("Topic", p => p.Topic)
            .Add("Audience", p => p.Audience)
            .Add("Learning Outcomes", p => p.LearningOutcomes)
            .Add("Concepts Phase Timing", p => $"{p.Concepts.Timing} minutes")
            .Add("Need to Know Concepts to Review", p => p.Concepts.NeedToKnow);

        configs[RequestId.SuggestConceptsNeedToKnow] = new FormatConfiguration()
            .Add("Topic", p => p.Topic)
            .Add("Audience", p => p.Audience)
            .Add("Learning Outcomes", p => p.LearningOutcomes)
            .Add("Concepts Phase Timing", p => $"{p.Concepts.Timing} minutes");

        configs[RequestId.ReviewConceptsGoodToKnow] = new FormatConfiguration()
            .Add("Topic", p => p.Topic)
            .Add("Audience", p => p.Audience)
            .Add("Learning Outcomes", p => p.LearningOutcomes)
            .Add("Concepts Phase Timing", p => $"{p.Concepts.Timing} minutes")
            .Add("Need to Know Concepts", p => p.Concepts.NeedToKnow)
            .Add("Good to Know Concepts to Review", p => p.Concepts.GoodToKnow);

        configs[RequestId.SuggestConceptsGoodToKnow] = new FormatConfiguration()
            .Add("Topic", p => p.Topic)
            .Add("Audience", p => p.Audience)
            .Add("Learning Outcomes", p => p.LearningOutcomes)
            .Add("Concepts Phase Timing", p => $"{p.Concepts.Timing} minutes")
            .Add("Need to Know Concepts", p => p.Concepts.NeedToKnow);

        configs[RequestId.ReviewConceptsTheses] = new FormatConfiguration()
            .Add("Topic", p => p.Topic)
            .Add("Audience", p => p.Audience)
            .Add("Concepts Phase Timing", p => $"{p.Concepts.Timing} minutes")
            .Add("Need to Know Concepts", p => p.Concepts.NeedToKnow)
            .Add("Good to Know Concepts", p => p.Concepts.GoodToKnow)
            .Add("Theses to Review", p => p.Concepts.Theses);

        configs[RequestId.SuggestConceptsTheses] = new FormatConfiguration()
            .Add("Topic", p => p.Topic)
            .Add("Audience", p => p.Audience)
            .Add("Concepts Phase Timing", p => $"{p.Concepts.Timing} minutes")
            .Add("Need to Know Concepts", p => p.Concepts.NeedToKnow)
            .Add("Good to Know Concepts", p => p.Concepts.GoodToKnow);

        configs[RequestId.ReviewConceptsStructure] = new FormatConfiguration()
            .Add("Topic", p => p.Topic)
            .Add("Audience", p => p.Audience)
            .Add("Concepts Phase Timing", p => $"{p.Concepts.Timing} minutes")
            .Add("Theses", p => p.Concepts.Theses)
            .Add("Structure to Review", p => p.Concepts.Structure);

        configs[RequestId.SuggestConceptsStructure] = new FormatConfiguration()
            .Add("Topic", p => p.Topic)
            .Add("Audience", p => p.Audience)
            .Add("Concepts Phase Timing", p => $"{p.Concepts.Timing} minutes")
            .Add("Theses", p => p.Concepts.Theses);

        configs[RequestId.ReviewConceptsActivities] = new FormatConfiguration()
            .Add("Topic", p => p.Topic)
            .Add("Audience", p => p.Audience)
            .Add("Concepts Phase Timing", p => $"{p.Concepts.Timing} minutes")
            .Add("Need to Know Concepts", p => p.Concepts.NeedToKnow)
            .Add("Theses", p => p.Concepts.Theses)
            .Add("Structure", p => p.Concepts.Structure)
            .Add("Activities to Review", p => p.Concepts.Activities);

        configs[RequestId.SuggestConceptsActivities] = new FormatConfiguration()
            .Add("Topic", p => p.Topic)
            .Add("Audience", p => p.Audience)
            .Add("Concepts Phase Timing", p => $"{p.Concepts.Timing} minutes")
            .Add("Need to Know Concepts", p => p.Concepts.NeedToKnow)
            .Add("Theses", p => p.Concepts.Theses)
            .Add("Structure", p => p.Concepts.Structure);

        configs[RequestId.ReviewConceptsMaterials] = new FormatConfiguration()
            .Add("Topic", p => p.Topic)
            .Add("Concepts Phase Timing", p => $"{p.Concepts.Timing} minutes")
            .Add("Activities", p => p.Concepts.Activities)
            .Add("Materials to Review", p => p.Concepts.MaterialsToPrepare);

        configs[RequestId.SuggestConceptsMaterials] = new FormatConfiguration()
            .Add("Topic", p => p.Topic)
            .Add("Concepts Phase Timing", p => $"{p.Concepts.Timing} minutes")
            .Add("Activities", p => p.Concepts.Activities);

        // Concrete Practice Phase
        configs[RequestId.ReviewPracticeOutput] = new FormatConfiguration()
            .Add("Topic", p => p.Topic)
            .Add("Audience", p => p.Audience)
            .Add("Learning Outcomes", p => p.LearningOutcomes)
            .Add("Practice Phase Timing", p => $"{p.ConcretePractice.Timing} minutes")
            .Add("Need to Know Concepts", p => p.Concepts.NeedToKnow)
            .Add("Desired Output to Review", p => p.ConcretePractice.DesiredOutput);

        configs[RequestId.SuggestPracticeOutput] = new FormatConfiguration()
            .Add("Topic", p => p.Topic)
            .Add("Audience", p => p.Audience)
            .Add("Learning Outcomes", p => p.LearningOutcomes)
            .Add("Practice Phase Timing", p => $"{p.ConcretePractice.Timing} minutes")
            .Add("Need to Know Concepts", p => p.Concepts.NeedToKnow);

        configs[RequestId.ReviewPracticeFocus] = new FormatConfiguration()
            .Add("Topic", p => p.Topic)
            .Add("Audience", p => p.Audience)
            .Add("Learning Outcomes", p => p.LearningOutcomes)
            .Add("Need to Know Concepts", p => p.Concepts.NeedToKnow)
            .Add("Desired Output", p => p.ConcretePractice.DesiredOutput)
            .Add("Focus Area to Review", p => p.ConcretePractice.FocusArea);

        configs[RequestId.SuggestPracticeFocus] = new FormatConfiguration()
            .Add("Topic", p => p.Topic)
            .Add("Audience", p => p.Audience)
            .Add("Learning Outcomes", p => p.LearningOutcomes)
            .Add("Need to Know Concepts", p => p.Concepts.NeedToKnow)
            .Add("Desired Output", p => p.ConcretePractice.DesiredOutput);

        configs[RequestId.ReviewPracticeActivities] = new FormatConfiguration()
            .Add("Topic", p => p.Topic)
            .Add("Audience", p => p.Audience)
            .Add("Practice Phase Timing", p => $"{p.ConcretePractice.Timing} minutes")
            .Add("Desired Output", p => p.ConcretePractice.DesiredOutput)
            .Add("Focus Area", p => p.ConcretePractice.FocusArea)
            .Add("Activities to Review", p => p.ConcretePractice.Activities);

        configs[RequestId.SuggestPracticeActivities] = new FormatConfiguration()
            .Add("Topic", p => p.Topic)
            .Add("Audience", p => p.Audience)
            .Add("Practice Phase Timing", p => $"{p.ConcretePractice.Timing} minutes")
            .Add("Desired Output", p => p.ConcretePractice.DesiredOutput)
            .Add("Focus Area", p => p.ConcretePractice.FocusArea);

        configs[RequestId.ReviewPracticeDetails] = new FormatConfiguration()
            .Add("Topic", p => p.Topic)
            .Add("Audience", p => p.Audience)
            .Add("Practice Phase Timing", p => $"{p.ConcretePractice.Timing} minutes")
            .Add("Desired Output", p => p.ConcretePractice.DesiredOutput)
            .Add("Focus Area", p => p.ConcretePractice.FocusArea)
            .Add("Activities", p => p.ConcretePractice.Activities)
            .Add("Details to Review", p => p.ConcretePractice.Details);

        configs[RequestId.SuggestPracticeDetails] = new FormatConfiguration()
            .Add("Topic", p => p.Topic)
            .Add("Audience", p => p.Audience)
            .Add("Practice Phase Timing", p => $"{p.ConcretePractice.Timing} minutes")
            .Add("Desired Output", p => p.ConcretePractice.DesiredOutput)
            .Add("Focus Area", p => p.ConcretePractice.FocusArea)
            .Add("Activities", p => p.ConcretePractice.Activities);

        configs[RequestId.ReviewPracticeMaterials] = new FormatConfiguration()
            .Add("Topic", p => p.Topic)
            .Add("Practice Phase Timing", p => $"{p.ConcretePractice.Timing} minutes")
            .Add("Activities", p => p.ConcretePractice.Activities)
            .Add("Details", p => p.ConcretePractice.Details)
            .Add("Materials to Review", p => p.ConcretePractice.MaterialsToPrepare);

        configs[RequestId.SuggestPracticeMaterials] = new FormatConfiguration()
            .Add("Topic", p => p.Topic)
            .Add("Practice Phase Timing", p => $"{p.ConcretePractice.Timing} minutes")
            .Add("Activities", p => p.ConcretePractice.Activities)
            .Add("Details", p => p.ConcretePractice.Details);

        // Conclusion Phase
        configs[RequestId.ReviewConclGoal] = new FormatConfiguration()
            .Add("Topic", p => p.Topic)
            .Add("Audience", p => p.Audience)
            .Add("Learning Outcomes", p => p.LearningOutcomes)
            .Add("Conclusion Phase Timing", p => $"{p.Conclusions.Timing} minutes")
            .Add("Conclusion Goal to Review", p => p.Conclusions.Goal);

        configs[RequestId.SuggestConclGoal] = new FormatConfiguration()
            .Add("Topic", p => p.Topic)
            .Add("Audience", p => p.Audience)
            .Add("Learning Outcomes", p => p.LearningOutcomes)
            .Add("Conclusion Phase Timing", p => $"{p.Conclusions.Timing} minutes");

        configs[RequestId.ReviewConclActivities] = new FormatConfiguration()
            .Add("Topic", p => p.Topic)
            .Add("Audience", p => p.Audience)
            .Add("Learning Outcomes", p => p.LearningOutcomes)
            .Add("Conclusion Phase Timing", p => $"{p.Conclusions.Timing} minutes")
            .Add("Conclusion Goal", p => p.Conclusions.Goal)
            .Add("Conclusion Activities to Review", p => p.Conclusions.Activities);

        configs[RequestId.SuggestConclActivities] = new FormatConfiguration()
            .Add("Topic", p => p.Topic)
            .Add("Audience", p => p.Audience)
            .Add("Learning Outcomes", p => p.LearningOutcomes)
            .Add("Conclusion Phase Timing", p => $"{p.Conclusions.Timing} minutes")
            .Add("Conclusion Goal", p => p.Conclusions.Goal);

        configs[RequestId.ReviewConclMaterials] = new FormatConfiguration()
            .Add("Topic", p => p.Topic)
            .Add("Conclusion Phase Timing", p => $"{p.Conclusions.Timing} minutes")
            .Add("Activities", p => p.Conclusions.Activities)
            .Add("Materials to Review", p => p.Conclusions.MaterialsToPrepare);

        configs[RequestId.SuggestConclMaterials] = new FormatConfiguration()
            .Add("Topic", p => p.Topic)
            .Add("Conclusion Phase Timing", p => $"{p.Conclusions.Timing} minutes")
            .Add("Activities", p => p.Conclusions.Activities);

        // Whole Lesson Review
        configs[RequestId.ReviewWholeLesson] = new FormatConfiguration()
            .Add("Topic", p => p.Topic)
            .Add("Audience", p => p.Audience)
            .Add("Learning Outcomes", p => p.LearningOutcomes)
            .Add("Connections Phase", p => FormatPhase(
                p.Connections.Timing,
                ("Goal", p.Connections.Goal),
                ("Activities", p.Connections.Activities),
                ("Materials to Prepare", p.Connections.MaterialsToPrepare)))
            .Add("Concepts Phase", p => FormatPhase(
                p.Concepts.Timing,
                ("Need to Know", p.Concepts.NeedToKnow),
                ("Good to Know", p.Concepts.GoodToKnow),
                ("Theses", p.Concepts.Theses),
                ("Structure", p.Concepts.Structure),
                ("Activities", p.Concepts.Activities),
                ("Materials to Prepare", p.Concepts.MaterialsToPrepare)))
            .Add("Concrete Practice Phase", p => FormatPhase(
                p.ConcretePractice.Timing,
                ("Desired Output", p.ConcretePractice.DesiredOutput),
                ("Focus Area", p.ConcretePractice.FocusArea),
                ("Activities", p.ConcretePractice.Activities),
                ("Details", p.ConcretePractice.Details),
                ("Materials to Prepare", p.ConcretePractice.MaterialsToPrepare)))
            .Add("Conclusions Phase", p => FormatPhase(
                p.Conclusions.Timing,
                ("Goal", p.Conclusions.Goal),
                ("Activities", p.Conclusions.Activities),
                ("Materials to Prepare", p.Conclusions.MaterialsToPrepare)));

        return configs;
    }

    private static string FormatPhase(int timing, params (string Label, string Value)[] fields)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"({timing} minutes)");

        for (int i = 0; i < fields.Length; i++)
        {
            (string label, string value) = fields[i];
            sb.AppendLine($"  {label}: {value}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Formats the lesson plan into a structured string appropriate for the specified request.
    /// Extracts only the relevant context fields needed for the operation.
    /// </summary>
    /// <param name="plan">The lesson plan to format.</param>
    /// <param name="requestId">The request identifier indicating which operation is being performed.</param>
    /// <returns>A structured string representation of the relevant lesson plan data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when plan is null.</exception>
    /// <exception cref="ArgumentException">Thrown when requestId is not recognized.</exception>
    public string FormatLessonPlan(LessonPlan plan, RequestId requestId)
    {
        if (plan == null)
            throw new ArgumentNullException(nameof(plan));

        if (!gFormatConfigurations.ContainsKey(requestId))
            throw new ArgumentException($"Unknown request ID: {requestId}", nameof(requestId));

        FormatConfiguration config = gFormatConfigurations[requestId];
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < config.Fields.Count; i++)
        {
            (string label, Func<LessonPlan, string> extractor) = config.Fields[i];
            string value = extractor(plan);

            sb.AppendLine($"# {label}");
            sb.AppendLine(value);

            if (i < config.Fields.Count - 1)
                sb.AppendLine();
        }

        return sb.ToString();
    }
}
