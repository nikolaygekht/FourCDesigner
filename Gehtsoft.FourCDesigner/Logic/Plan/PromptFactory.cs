using System;
using System.Collections.Generic;

namespace Gehtsoft.FourCDesigner.Logic.Plan;

/// <summary>
/// Type of AI assistance request.
/// </summary>
public enum RequestType
{
    /// <summary>
    /// Review existing content and provide feedback.
    /// </summary>
    Review,

    /// <summary>
    /// Suggest new content to use.
    /// </summary>
    Suggest
}

/// <summary>
/// Template for AI prompt with metadata.
/// </summary>
public readonly struct PromptTemplate
{
    /// <summary>
    /// Gets the request identifier.
    /// </summary>
    public RequestId RequestId { get; }

    /// <summary>
    /// Gets the type of request (Review or Suggest).
    /// </summary>
    public RequestType Type { get; }

    /// <summary>
    /// Gets the specific instruction for this operation.
    /// </summary>
    public string Instruction { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PromptTemplate"/> struct.
    /// </summary>
    /// <param name="requestId">The request identifier.</param>
    /// <param name="type">The type of request.</param>
    /// <param name="instruction">The specific instruction.</param>
    public PromptTemplate(RequestId requestId, RequestType type, string instruction)
    {
        RequestId = requestId;
        Type = type;
        Instruction = instruction;
    }
}

/// <summary>
/// Factory for retrieving AI prompts for lesson plan assistance operations.
/// Registered as singleton since prompts are static.
/// </summary>
public class PromptFactory : IPromptFactory
{
    private static readonly string gAiRole = @"You are an AI assistant for instructional designers working with the Training from the Back of the Room (TBR) methodology. You have three roles:

1. **Knowledge Base**: Search and provide relevant information about instructional design, learning theories, and teaching methodologies.

2. **Coach**: Ask proper questions that help instructors refine their lesson content, clarify their thinking, and develop better learning experiences.

3. **Critic**: Provide both critical and positive feedback on content created, focusing on alignment with TBR principles and effective learning outcomes.";

    private static readonly string gContext = @"## Training from the Back of the Room (TBR) - 4C Approach

The 4C approach structures every lesson into four phases:

### 1. Connections
Learners make connections with what they already know about the topic and what they want to learn. Learners also connect with each other.

### 2. Concepts
Learners take in information in a multi-sensory way, engaging all four learning styles: Visual (learning by seeing), Aural (learning by listening), Reading, and Kinesthetic (learning by application).

### 3. Concrete Practice
Learners practice a skill or repeat a procedure being learned, and review the content.

### 4. Conclusions
Learners summarize what they have learned, evaluate it, make action plans to use it, and celebrate the learning.

## Six Trumps Principles

When it comes to effective learning:
- Movement trumps sitting
- Talking trumps listening
- Images trump words
- Writing trumps reading
- Shorter trumps longer
- Different trumps same

## Learning Outcomes

Learning outcomes are specific statements of what students will be able to do when they successfully complete a learning experience. They must be student-centered, measurable, concise, meaningful, and achievable.";

    private static readonly PromptTemplate[] gPromptTemplates =
    [
        // Overview Section
        new PromptTemplate(RequestId.ReviewContext, RequestType.Review, "Review the lesson context for completeness and clarity. Does it provide sufficient background information about the prequisites, intent, scope, limitations, and conditions of the lesson? Will it help you understand the lesson designer's intent to avoid guesswork? Provide critical feedback."),

        new PromptTemplate(RequestId.SuggestContext, RequestType.Suggest, "Suggest context information that would help clarify the lesson designer's purpose, intent, scope, limitations, prequisities, and conditions. What background information would be most useful?"),

        new PromptTemplate(RequestId.ReviewTopic, RequestType.Review, "Review the lesson topic for clarity and appropriateness for TBR methodology. Is it clear, focused, and suitable for a structured learning experience? Provide critical feedback on its strengths and weaknesses."),

        new PromptTemplate(RequestId.SuggestTopic, RequestType.Suggest, "Based on the current topic, suggest specific improvements to make it clearer, more focused, and better aligned with TBR principles. Ask clarifying questions if needed."),

        new PromptTemplate(RequestId.ReviewAudience, RequestType.Review, "Review the audience description for completeness. Does it adequately describe age, education level, job functions, language capabilities, motivation, and learning needs? Provide critical feedback on what's missing or unclear."),

        new PromptTemplate(RequestId.SuggestAudience, RequestType.Suggest, "Suggest additional audience characteristics that should be considered for this lesson. What important details about the learners would help design a more effective learning experience?"),

        new PromptTemplate(RequestId.ReviewOutcomes, RequestType.Review, "Review the learning outcomes for quality. Are they specific, measurable, achievable, student-centered, and aligned with the topic and audience? Provide critical feedback."),

        new PromptTemplate(RequestId.SuggestOutcomes, RequestType.Suggest, "Based on the topic and audience, suggest specific, measurable learning outcomes. What should learners be able to do after completing this lesson?"),

        // Connection Phase
        new PromptTemplate(RequestId.ReviewConnGoal, RequestType.Review, "Review the connection phase goal. Does it effectively help learners connect to the topic and to each other? Is it aligned with TBR principles? Provide critical feedback."),

        new PromptTemplate(RequestId.SuggestConnGoal, RequestType.Suggest, "Suggest specific goals for the connection phase that will help learners connect to the topic and to each other, appropriate for the given audience and topic."),

        new PromptTemplate(RequestId.ReviewConnActivities, RequestType.Review, "Review the connection activities. Will they achieve the connection goal within the allocated time? Are they engaging and appropriate for the audience? Provide critical feedback."),

        new PromptTemplate(RequestId.SuggestConnActivities, RequestType.Suggest, "Suggest specific connection activities that will help learners connect to the topic and each other within the allocated time."),

        new PromptTemplate(RequestId.ReviewConnMaterials, RequestType.Review, "Review the materials list for the connection phase. Is it complete for the planned activities? Are there any missing items? Provide critical feedback."),

        new PromptTemplate(RequestId.SuggestConnMaterials, RequestType.Suggest, "Based on the planned connection activities, suggest what materials the instructor needs to prepare."),

        // Concepts Phase
        new PromptTemplate(RequestId.ReviewConceptsNeedToKnow, RequestType.Review, "Review the 'need to know' concepts. Do they cover essential knowledge required for the learning outcomes? Are they focused and not overwhelming? Provide critical feedback."),

        new PromptTemplate(RequestId.SuggestConceptsNeedToKnow, RequestType.Suggest, "Based on the topic, audience, and learning outcomes, suggest essential concepts that learners must understand. Focus on what's truly necessary, not nice-to-have."),

        new PromptTemplate(RequestId.ReviewConceptsGoodToKnow, RequestType.Review, "Review the 'good to know' concepts. Are they appropriate supplements that don't overwhelm learners? Do they add value without distracting from essential concepts? Provide critical feedback."),

        new PromptTemplate(RequestId.SuggestConceptsGoodToKnow, RequestType.Suggest, "Based on the essential concepts, suggest additional 'good to know' information that would be helpful but not essential for learners."),

        new PromptTemplate(RequestId.ReviewConceptsTheses, RequestType.Review, "Review the key theses to be delivered. Do they effectively convey the concepts in a clear, memorable way? Provide critical feedback."),

        new PromptTemplate(RequestId.SuggestConceptsTheses, RequestType.Suggest, "Based on the concepts to be taught, suggest key theses or main points that should be delivered to learners."),

        new PromptTemplate(RequestId.ReviewConceptsStructure, RequestType.Review, "Review the delivery structure for the concepts phase. Is it logical? Does it fit within the allocated time? Does it build understanding progressively? Provide critical feedback."),

        new PromptTemplate(RequestId.SuggestConceptsStructure, RequestType.Suggest, "Suggest a logical structure for delivering the concepts within the allocated time. How should the information be organized and sequenced?"),

        new PromptTemplate(RequestId.ReviewConceptsActivities, RequestType.Review, "Review the concept activities for multi-sensory engagement. Do they engage VARK learning styles (Visual, Aural, Reading, Kinesthetic)? Do they apply the six trumps principles? Provide critical feedback."),

        new PromptTemplate(RequestId.SuggestConceptsActivities, RequestType.Suggest, "Suggest multi-sensory activities that engage VARK learning styles and apply the six trumps principles (movement > sitting, talking > listening, images > words, writing > reading, shorter > longer, different > same)."),

        new PromptTemplate(RequestId.ReviewConceptsMaterials, RequestType.Review, "Review the materials list for the concepts phase. Do the materials adequately support the planned activities? Are there any gaps? Provide critical feedback."),

        new PromptTemplate(RequestId.SuggestConceptsMaterials, RequestType.Suggest, "Based on the planned concept activities, suggest what materials the instructor needs to prepare."),

        // Concrete Practice Phase
        new PromptTemplate(RequestId.ReviewPracticeOutput, RequestType.Review, "Review the desired output for the practice phase. Does it align with the learning outcomes? Is it achievable within the timeframe? Provide critical feedback."),

        new PromptTemplate(RequestId.SuggestPracticeOutput, RequestType.Suggest, "Based on the learning outcomes and concepts taught, suggest what learners should produce during the concrete practice phase."),

        new PromptTemplate(RequestId.ReviewPracticeFocus, RequestType.Review, "Review the focus areas for practice. Do they target where learners are likely to struggle most? Are they appropriate for the skill level? Provide critical feedback."),

        new PromptTemplate(RequestId.SuggestPracticeFocus, RequestType.Suggest, "Based on the topic and desired output, suggest where learners are likely to struggle most and what areas should receive focused attention during practice."),

        new PromptTemplate(RequestId.ReviewPracticeActivities, RequestType.Review, "Review the practice activities. Will they effectively lead learners to produce the desired output? Are they appropriately scaffolded? Provide critical feedback."),

        new PromptTemplate(RequestId.SuggestPracticeActivities, RequestType.Suggest, "Suggest specific practice activities that will help learners produce the desired output while focusing on the identified challenge areas."),

        new PromptTemplate(RequestId.ReviewPracticeDetails, RequestType.Review, "Review the execution plan for practice activities. Is it clear and complete? Will instructors be able to follow it? Are the steps logical? Provide critical feedback."),

        new PromptTemplate(RequestId.SuggestPracticeDetails, RequestType.Suggest, "Based on the practice activities, suggest a detailed step-by-step execution plan that instructors can follow."),

        new PromptTemplate(RequestId.ReviewPracticeMaterials, RequestType.Review, "Review the materials list for the practice phase. Are the materials sufficient for the planned activities? Are there any missing items? Provide critical feedback."),

        new PromptTemplate(RequestId.SuggestPracticeMaterials, RequestType.Suggest, "Based on the planned practice activities, suggest what materials the instructor needs to prepare."),

        // Conclusion Phase
        new PromptTemplate(RequestId.ReviewConclGoal, RequestType.Review, "Review the conclusion phase goal. Does it support reflection, evaluation, and action planning? Is it appropriate for wrapping up the learning experience? Provide critical feedback."),

        new PromptTemplate(RequestId.SuggestConclGoal, RequestType.Suggest, "Suggest specific goals for the conclusion phase that will help learners reflect, evaluate their learning, and plan for application."),

        new PromptTemplate(RequestId.ReviewConclActivities, RequestType.Review, "Review the conclusion activities. Do they achieve the four key purposes: summarize learning, evaluate understanding, create action plans, and celebrate? Provide critical feedback."),

        new PromptTemplate(RequestId.SuggestConclActivities, RequestType.Suggest, "Suggest conclusion activities that help learners summarize what they learned, evaluate their understanding, make action plans for applying the knowledge, and celebrate their learning."),

        new PromptTemplate(RequestId.ReviewConclMaterials, RequestType.Review, "Review the materials list for the conclusion phase. Do the materials support the planned conclusion activities? Are there any gaps? Provide critical feedback."),

        new PromptTemplate(RequestId.SuggestConclMaterials, RequestType.Suggest, "Based on the planned conclusion activities, suggest what materials the instructor needs to prepare."),

        // Whole Lesson Review
        new PromptTemplate(RequestId.ReviewWholeLesson, RequestType.Review, "Provide a holistic review of the entire lesson plan. Evaluate: 1) Overall coherence and flow between phases, 2) Timing balance across the 4Cs, 3) Alignment with TBR principles and six trumps, 4) Whether learning outcomes are achievable through the planned activities, 5) Audience appropriateness throughout. Provide comprehensive critical feedback highlighting both strengths and areas for improvement.")
    ];

    private readonly Dictionary<RequestId, string> mPrompts;

    /// <summary>
    /// Initializes a new instance of the <see cref="PromptFactory"/> class.
    /// </summary>
    public PromptFactory()
    {
        mPrompts = new Dictionary<RequestId, string>();

        for (int i = 0; i < gPromptTemplates.Length; i++)
        {
            PromptTemplate template = gPromptTemplates[i];
            mPrompts[template.RequestId] = BuildFullPrompt(template);
        }
    }

    /// <summary>
    /// Builds the full prompt by combining role, context, and specific instruction.
    /// For suggest operations, adds instructions to return only the value without explanations.
    /// For review operations, adds instructions to limit response to 2-3 paragraphs.
    /// </summary>
    /// <param name="template">The prompt template with request metadata.</param>
    /// <returns>The complete prompt text.</returns>
    private static string BuildFullPrompt(PromptTemplate template)
    {
        string contextNote = "\n\n**CRITICAL**: The user provided a 'Context' field containing important " +
                            "background information about the lesson plan. This context explains the purpose, intent, " +
                            "scope, limitations, and conditions of the lesson. You MUST carefully review and consider this " +
                            "context in all your responses to understand what the lesson designer is trying to achieve and avoid " +
                            "making assumptions or guesses about their intent or create output that goes beyond the scope and context " +
                            "defined.";

        string basePrompt = $"{gAiRole}\n\n{gContext}{contextNote}\n\n## Your Task\n\n{template.Instruction}\n\nBased on the lesson plan information provided by the user, provide your response.";

        // For suggest operations, add instruction to return only the value without explanations
        if (template.Type == RequestType.Suggest)
        {
            basePrompt += "\n\n**IMPORTANT**: " +
                "If the user input contains '???' or '*suggest*' markers in specific fields, focus your suggestions on those areas that need improvement. " +
                "Return ONLY the suggested content without any introductory text, explanations, or meta-commentary. " +
                "The response should be ready to use directly in the lesson plan field.";
        }
        // For review operations, add instruction to limit response length
        else if (template.Type == RequestType.Review)
        {
            basePrompt += "\n\n**IMPORTANT**: Keep your feedback concise and focused. Limit your response to 2-3 paragraphs maximum, highlighting the most important points.";
        }

        return basePrompt;
    }

    /// <summary>
    /// Gets the AI prompt for the specified request.
    /// </summary>
    /// <param name="requestId">The request identifier.</param>
    /// <returns>The prompt text for the AI operation.</returns>
    /// <exception cref="ArgumentException">Thrown when requestId is not recognized.</exception>
    public string GetPrompt(RequestId requestId)
    {
        if (!mPrompts.ContainsKey(requestId))
            throw new ArgumentException($"Unknown request ID: {requestId}", nameof(requestId));

        return mPrompts[requestId];
    }
}
