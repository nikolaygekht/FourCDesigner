# AI Assistance API Operations

This document defines all AI assistance operations for the 4C Instructional Design Tool, organized by user journey phase.

## Overview

- **Total Operations:** 41 (40 field-specific + 1 holistic)
- **Review Operations:** 21 (provide critical feedback on existing content)
- **Suggest Operations:** 20 (generate new content/ideas)

## AI Roles

- **Knowledge Base:** Search and provide relevant information
- **Coach:** Ask proper questions that help refine content
- **Critic:** Provide both critical and positive feedback on content created

---

## 1. Overview Section (Foundation)

### 1.1 review_topic
- **Operation ID:** `review_topic`
- **Type:** Review
- **Target Field:** `topic`
- **Input Context:**
  ```json
  {
    "currentValue": "topic"
  }
  ```
- **Purpose:** Review if topic is clear and appropriate for TBR methodology
- **AI Role:** Critic
- **Expected Output:** Critical feedback on topic clarity and appropriateness

### 1.2 suggest_topic
- **Operation ID:** `suggest_topic`
- **Type:** Suggest
- **Target Field:** `topic`
- **Input Context:**
  ```json
  {
    "currentValue": "topic"
  }
  ```
- **Purpose:** Suggest improvements to make topic clearer
- **AI Role:** Coach
- **Expected Output:** Suggested topic refinements

### 1.3 review_audience
- **Operation ID:** `review_audience`
- **Type:** Review
- **Target Field:** `audience`
- **Input Context:**
  ```json
  {
    "topic": "...",
    "currentValue": "audience"
  }
  ```
- **Purpose:** Review if audience description is complete (age, education, needs, motivation)
- **AI Role:** Critic
- **Expected Output:** Critical feedback on audience description completeness

### 1.4 suggest_audience
- **Operation ID:** `suggest_audience`
- **Type:** Suggest
- **Target Field:** `audience`
- **Input Context:**
  ```json
  {
    "topic": "...",
    "currentValue": "audience"
  }
  ```
- **Purpose:** Suggest what additional audience characteristics to consider
- **AI Role:** Coach
- **Expected Output:** Suggested audience characteristics

### 1.5 review_outcomes
- **Operation ID:** `review_outcomes`
- **Type:** Review
- **Target Field:** `learningOutcomes`
- **Input Context:**
  ```json
  {
    "topic": "...",
    "audience": "...",
    "currentValue": "learningOutcomes"
  }
  ```
- **Purpose:** Review if outcomes are measurable, achievable, student-centered
- **AI Role:** Critic
- **Expected Output:** Critical feedback on learning outcomes quality

### 1.6 suggest_outcomes
- **Operation ID:** `suggest_outcomes`
- **Type:** Suggest
- **Target Field:** `learningOutcomes`
- **Input Context:**
  ```json
  {
    "topic": "...",
    "audience": "...",
    "currentValue": "learningOutcomes"
  }
  ```
- **Purpose:** Suggest specific, measurable learning outcomes
- **AI Role:** Coach
- **Expected Output:** Suggested learning outcomes

---

## 2. Connection Phase

### 2.1 review_conn_goal
- **Operation ID:** `review_conn_goal`
- **Type:** Review
- **Target Field:** `conn_goal`
- **Input Context:**
  ```json
  {
    "topic": "...",
    "audience": "...",
    "learningOutcomes": "...",
    "timing": "conn_timing",
    "currentValue": "conn_goal"
  }
  ```
- **Purpose:** Review if connection goal aligns with TBR principles (connecting learners to topic and to each other)
- **AI Role:** Critic
- **Expected Output:** Critical feedback on connection goal alignment

### 2.2 suggest_conn_goal
- **Operation ID:** `suggest_conn_goal`
- **Type:** Suggest
- **Target Field:** `conn_goal`
- **Input Context:**
  ```json
  {
    "topic": "...",
    "audience": "...",
    "learningOutcomes": "...",
    "timing": "conn_timing"
  }
  ```
- **Purpose:** Suggest connection goals based on topic and audience
- **AI Role:** Coach
- **Expected Output:** Suggested connection goals

### 2.3 review_conn_activities
- **Operation ID:** `review_conn_activities`
- **Type:** Review
- **Target Field:** `conn_activities`
- **Input Context:**
  ```json
  {
    "topic": "...",
    "audience": "...",
    "learningOutcomes": "...",
    "timing": "conn_timing",
    "goal": "conn_goal",
    "currentValue": "conn_activities"
  }
  ```
- **Purpose:** Review if activities achieve connection goal within allocated time
- **AI Role:** Critic
- **Expected Output:** Critical feedback on activity effectiveness and timing

### 2.4 suggest_conn_activities
- **Operation ID:** `suggest_conn_activities`
- **Type:** Suggest
- **Target Field:** `conn_activities`
- **Input Context:**
  ```json
  {
    "topic": "...",
    "audience": "...",
    "learningOutcomes": "...",
    "timing": "conn_timing",
    "goal": "conn_goal"
  }
  ```
- **Purpose:** Suggest specific connection activities
- **AI Role:** Coach
- **Expected Output:** Suggested connection activities

### 2.5 review_conn_materials
- **Operation ID:** `review_conn_materials`
- **Type:** Review
- **Target Field:** `conn_materials`
- **Input Context:**
  ```json
  {
    "topic": "...",
    "audience": "...",
    "timing": "conn_timing",
    "goal": "conn_goal",
    "activities": "conn_activities",
    "currentValue": "conn_materials"
  }
  ```
- **Purpose:** Review if materials list is complete for planned activities
- **AI Role:** Critic
- **Expected Output:** Critical feedback on materials completeness

### 2.6 suggest_conn_materials
- **Operation ID:** `suggest_conn_materials`
- **Type:** Suggest
- **Target Field:** `conn_materials`
- **Input Context:**
  ```json
  {
    "topic": "...",
    "audience": "...",
    "timing": "conn_timing",
    "goal": "conn_goal",
    "activities": "conn_activities"
  }
  ```
- **Purpose:** Suggest materials needed for activities
- **AI Role:** Coach
- **Expected Output:** Suggested materials list

---

## 3. Concepts Phase

### 3.1 review_concepts_needToKnow
- **Operation ID:** `review_concepts_needToKnow`
- **Type:** Review
- **Target Field:** `concepts_needToKnow`
- **Input Context:**
  ```json
  {
    "topic": "...",
    "audience": "...",
    "learningOutcomes": "...",
    "timing": "concepts_timing",
    "currentValue": "concepts_needToKnow"
  }
  ```
- **Purpose:** Review if essential concepts cover what's needed for outcomes
- **AI Role:** Critic + Knowledge Base
- **Expected Output:** Critical feedback on concept coverage and essentialness

### 3.2 suggest_concepts_needToKnow
- **Operation ID:** `suggest_concepts_needToKnow`
- **Type:** Suggest
- **Target Field:** `concepts_needToKnow`
- **Input Context:**
  ```json
  {
    "topic": "...",
    "audience": "...",
    "learningOutcomes": "...",
    "timing": "concepts_timing"
  }
  ```
- **Purpose:** Suggest essential concepts learners must understand
- **AI Role:** Knowledge Base
- **Expected Output:** Suggested "need to know" concepts

### 3.3 review_concepts_goodToKnow
- **Operation ID:** `review_concepts_goodToKnow`
- **Type:** Review
- **Target Field:** `concepts_goodToKnow`
- **Input Context:**
  ```json
  {
    "topic": "...",
    "audience": "...",
    "learningOutcomes": "...",
    "timing": "concepts_timing",
    "needToKnow": "concepts_needToKnow",
    "currentValue": "concepts_goodToKnow"
  }
  ```
- **Purpose:** Review if "good to know" is appropriate and doesn't overwhelm
- **AI Role:** Critic
- **Expected Output:** Critical feedback on concept appropriateness

### 3.4 suggest_concepts_goodToKnow
- **Operation ID:** `suggest_concepts_goodToKnow`
- **Type:** Suggest
- **Target Field:** `concepts_goodToKnow`
- **Input Context:**
  ```json
  {
    "topic": "...",
    "audience": "...",
    "learningOutcomes": "...",
    "timing": "concepts_timing",
    "needToKnow": "concepts_needToKnow"
  }
  ```
- **Purpose:** Suggest additional helpful concepts
- **AI Role:** Knowledge Base
- **Expected Output:** Suggested "good to know" concepts

### 3.5 review_concepts_theses
- **Operation ID:** `review_concepts_theses`
- **Type:** Review
- **Target Field:** `concepts_theses`
- **Input Context:**
  ```json
  {
    "topic": "...",
    "audience": "...",
    "timing": "concepts_timing",
    "needToKnow": "concepts_needToKnow",
    "goodToKnow": "concepts_goodToKnow",
    "currentValue": "concepts_theses"
  }
  ```
- **Purpose:** Review if theses effectively convey the concepts
- **AI Role:** Critic
- **Expected Output:** Critical feedback on thesis effectiveness

### 3.6 suggest_concepts_theses
- **Operation ID:** `suggest_concepts_theses`
- **Type:** Suggest
- **Target Field:** `concepts_theses`
- **Input Context:**
  ```json
  {
    "topic": "...",
    "audience": "...",
    "timing": "concepts_timing",
    "needToKnow": "concepts_needToKnow",
    "goodToKnow": "concepts_goodToKnow"
  }
  ```
- **Purpose:** Suggest key theses to deliver
- **AI Role:** Coach
- **Expected Output:** Suggested theses

### 3.7 review_concepts_structure
- **Operation ID:** `review_concepts_structure`
- **Type:** Review
- **Target Field:** `concepts_structure`
- **Input Context:**
  ```json
  {
    "topic": "...",
    "audience": "...",
    "timing": "concepts_timing",
    "theses": "concepts_theses",
    "currentValue": "concepts_structure"
  }
  ```
- **Purpose:** Review if delivery structure is logical and fits allocated time
- **AI Role:** Critic
- **Expected Output:** Critical feedback on structure and timing

### 3.8 suggest_concepts_structure
- **Operation ID:** `suggest_concepts_structure`
- **Type:** Suggest
- **Target Field:** `concepts_structure`
- **Input Context:**
  ```json
  {
    "topic": "...",
    "audience": "...",
    "timing": "concepts_timing",
    "theses": "concepts_theses"
  }
  ```
- **Purpose:** Suggest delivery structure
- **AI Role:** Coach
- **Expected Output:** Suggested delivery structure

### 3.9 review_concepts_activities
- **Operation ID:** `review_concepts_activities`
- **Type:** Review
- **Target Field:** `concepts_activities`
- **Input Context:**
  ```json
  {
    "topic": "...",
    "audience": "...",
    "timing": "concepts_timing",
    "needToKnow": "concepts_needToKnow",
    "theses": "concepts_theses",
    "structure": "concepts_structure",
    "currentValue": "concepts_activities"
  }
  ```
- **Purpose:** Review if activities engage VARK learners and apply six trumps (Movement > Sitting, Talking > Listening, Images > Words, Writing > Reading, Shorter > Longer, Different > Same)
- **AI Role:** Critic
- **Expected Output:** Critical feedback on multi-sensory engagement

### 3.10 suggest_concepts_activities
- **Operation ID:** `suggest_concepts_activities`
- **Type:** Suggest
- **Target Field:** `concepts_activities`
- **Input Context:**
  ```json
  {
    "topic": "...",
    "audience": "...",
    "timing": "concepts_timing",
    "needToKnow": "concepts_needToKnow",
    "theses": "concepts_theses",
    "structure": "concepts_structure"
  }
  ```
- **Purpose:** Suggest multi-sensory activities using six trumps
- **AI Role:** Coach + Knowledge Base
- **Expected Output:** Suggested VARK activities

### 3.11 review_concepts_materials
- **Operation ID:** `review_concepts_materials`
- **Type:** Review
- **Target Field:** `concepts_materials`
- **Input Context:**
  ```json
  {
    "topic": "...",
    "timing": "concepts_timing",
    "activities": "concepts_activities",
    "currentValue": "concepts_materials"
  }
  ```
- **Purpose:** Review if materials support the activities
- **AI Role:** Critic
- **Expected Output:** Critical feedback on material support

### 3.12 suggest_concepts_materials
- **Operation ID:** `suggest_concepts_materials`
- **Type:** Suggest
- **Target Field:** `concepts_materials`
- **Input Context:**
  ```json
  {
    "topic": "...",
    "timing": "concepts_timing",
    "activities": "concepts_activities"
  }
  ```
- **Purpose:** Suggest materials for activities
- **AI Role:** Coach
- **Expected Output:** Suggested materials list

---

## 4. Concrete Practice Phase

### 4.1 review_practice_output
- **Operation ID:** `review_practice_output`
- **Type:** Review
- **Target Field:** `practice_desiredOutput`
- **Input Context:**
  ```json
  {
    "topic": "...",
    "audience": "...",
    "learningOutcomes": "...",
    "timing": "practice_timing",
    "needToKnow": "concepts_needToKnow",
    "currentValue": "practice_desiredOutput"
  }
  ```
- **Purpose:** Review if desired output aligns with learning outcomes
- **AI Role:** Critic
- **Expected Output:** Critical feedback on output alignment

### 4.2 suggest_practice_output
- **Operation ID:** `suggest_practice_output`
- **Type:** Suggest
- **Target Field:** `practice_desiredOutput`
- **Input Context:**
  ```json
  {
    "topic": "...",
    "audience": "...",
    "learningOutcomes": "...",
    "timing": "practice_timing",
    "needToKnow": "concepts_needToKnow"
  }
  ```
- **Purpose:** Suggest what learners should produce
- **AI Role:** Coach
- **Expected Output:** Suggested desired outputs

### 4.3 review_practice_focus
- **Operation ID:** `review_practice_focus`
- **Type:** Review
- **Target Field:** `practice_focusArea`
- **Input Context:**
  ```json
  {
    "topic": "...",
    "audience": "...",
    "learningOutcomes": "...",
    "needToKnow": "concepts_needToKnow",
    "desiredOutput": "practice_desiredOutput",
    "currentValue": "practice_focusArea"
  }
  ```
- **Purpose:** Review if focus areas target key challenges
- **AI Role:** Critic
- **Expected Output:** Critical feedback on focus appropriateness

### 4.4 suggest_practice_focus
- **Operation ID:** `suggest_practice_focus`
- **Type:** Suggest
- **Target Field:** `practice_focusArea`
- **Input Context:**
  ```json
  {
    "topic": "...",
    "audience": "...",
    "learningOutcomes": "...",
    "needToKnow": "concepts_needToKnow",
    "desiredOutput": "practice_desiredOutput"
  }
  ```
- **Purpose:** Suggest where learners will struggle most
- **AI Role:** Knowledge Base
- **Expected Output:** Suggested focus areas

### 4.5 review_practice_activities
- **Operation ID:** `review_practice_activities`
- **Type:** Review
- **Target Field:** `practice_activities`
- **Input Context:**
  ```json
  {
    "topic": "...",
    "audience": "...",
    "timing": "practice_timing",
    "desiredOutput": "practice_desiredOutput",
    "focusArea": "practice_focusArea",
    "currentValue": "practice_activities"
  }
  ```
- **Purpose:** Review if activities lead to desired output
- **AI Role:** Critic
- **Expected Output:** Critical feedback on activity effectiveness

### 4.6 suggest_practice_activities
- **Operation ID:** `suggest_practice_activities`
- **Type:** Suggest
- **Target Field:** `practice_activities`
- **Input Context:**
  ```json
  {
    "topic": "...",
    "audience": "...",
    "timing": "practice_timing",
    "desiredOutput": "practice_desiredOutput",
    "focusArea": "practice_focusArea"
  }
  ```
- **Purpose:** Suggest practice activities
- **AI Role:** Coach
- **Expected Output:** Suggested practice activities

### 4.7 review_practice_details
- **Operation ID:** `review_practice_details`
- **Type:** Review
- **Target Field:** `practice_details`
- **Input Context:**
  ```json
  {
    "topic": "...",
    "audience": "...",
    "timing": "practice_timing",
    "desiredOutput": "practice_desiredOutput",
    "focusArea": "practice_focusArea",
    "activities": "practice_activities",
    "currentValue": "practice_details"
  }
  ```
- **Purpose:** Review if execution plan is clear and complete
- **AI Role:** Critic
- **Expected Output:** Critical feedback on plan clarity

### 4.8 suggest_practice_details
- **Operation ID:** `suggest_practice_details`
- **Type:** Suggest
- **Target Field:** `practice_details`
- **Input Context:**
  ```json
  {
    "topic": "...",
    "audience": "...",
    "timing": "practice_timing",
    "desiredOutput": "practice_desiredOutput",
    "focusArea": "practice_focusArea",
    "activities": "practice_activities"
  }
  ```
- **Purpose:** Suggest step-by-step execution plan
- **AI Role:** Coach
- **Expected Output:** Suggested detailed plan

### 4.9 review_practice_materials
- **Operation ID:** `review_practice_materials`
- **Type:** Review
- **Target Field:** `practice_materials`
- **Input Context:**
  ```json
  {
    "topic": "...",
    "timing": "practice_timing",
    "activities": "practice_activities",
    "details": "practice_details",
    "currentValue": "practice_materials"
  }
  ```
- **Purpose:** Review if materials are sufficient
- **AI Role:** Critic
- **Expected Output:** Critical feedback on material sufficiency

### 4.10 suggest_practice_materials
- **Operation ID:** `suggest_practice_materials`
- **Type:** Suggest
- **Target Field:** `practice_materials`
- **Input Context:**
  ```json
  {
    "topic": "...",
    "timing": "practice_timing",
    "activities": "practice_activities",
    "details": "practice_details"
  }
  ```
- **Purpose:** Suggest materials needed
- **AI Role:** Coach
- **Expected Output:** Suggested materials list

---

## 5. Conclusion Phase

### 5.1 review_concl_goal
- **Operation ID:** `review_concl_goal`
- **Type:** Review
- **Target Field:** `concl_goal`
- **Input Context:**
  ```json
  {
    "topic": "...",
    "audience": "...",
    "learningOutcomes": "...",
    "timing": "concl_timing",
    "currentValue": "concl_goal"
  }
  ```
- **Purpose:** Review if conclusion goal supports reflection and action planning
- **AI Role:** Critic
- **Expected Output:** Critical feedback on conclusion goal

### 5.2 suggest_concl_goal
- **Operation ID:** `suggest_concl_goal`
- **Type:** Suggest
- **Target Field:** `concl_goal`
- **Input Context:**
  ```json
  {
    "topic": "...",
    "audience": "...",
    "learningOutcomes": "...",
    "timing": "concl_timing"
  }
  ```
- **Purpose:** Suggest conclusion goals
- **AI Role:** Coach
- **Expected Output:** Suggested conclusion goals

### 5.3 review_concl_activities
- **Operation ID:** `review_concl_activities`
- **Type:** Review
- **Target Field:** `concl_activities`
- **Input Context:**
  ```json
  {
    "topic": "...",
    "audience": "...",
    "learningOutcomes": "...",
    "timing": "concl_timing",
    "goal": "concl_goal",
    "currentValue": "concl_activities"
  }
  ```
- **Purpose:** Review if activities achieve conclusion goals (summarize, evaluate, action plan, celebrate)
- **AI Role:** Critic
- **Expected Output:** Critical feedback on conclusion activities

### 5.4 suggest_concl_activities
- **Operation ID:** `suggest_concl_activities`
- **Type:** Suggest
- **Target Field:** `concl_activities`
- **Input Context:**
  ```json
  {
    "topic": "...",
    "audience": "...",
    "learningOutcomes": "...",
    "timing": "concl_timing",
    "goal": "concl_goal"
  }
  ```
- **Purpose:** Suggest conclusion activities (summarize, evaluate, action plan, celebrate)
- **AI Role:** Coach
- **Expected Output:** Suggested conclusion activities

### 5.5 review_concl_materials
- **Operation ID:** `review_concl_materials`
- **Type:** Review
- **Target Field:** `concl_materials`
- **Input Context:**
  ```json
  {
    "topic": "...",
    "timing": "concl_timing",
    "activities": "concl_activities",
    "currentValue": "concl_materials"
  }
  ```
- **Purpose:** Review if materials support conclusion activities
- **AI Role:** Critic
- **Expected Output:** Critical feedback on materials

### 5.6 suggest_concl_materials
- **Operation ID:** `suggest_concl_materials`
- **Type:** Suggest
- **Target Field:** `concl_materials`
- **Input Context:**
  ```json
  {
    "topic": "...",
    "timing": "concl_timing",
    "activities": "concl_activities"
  }
  ```
- **Purpose:** Suggest materials needed
- **AI Role:** Coach
- **Expected Output:** Suggested materials list

---

## 6. Whole Lesson Review

### 6.1 review_whole_lesson
- **Operation ID:** `review_whole_lesson`
- **Type:** Review
- **Target Field:** None (display only)
- **Input Context:**
  ```json
  {
    "topic": "...",
    "audience": "...",
    "learningOutcomes": "...",
    "connections": {
      "timing": 5,
      "goal": "...",
      "activities": "...",
      "materialsToPrepare": "..."
    },
    "concepts": {
      "timing": 15,
      "needToKnow": "...",
      "goodToKnow": "...",
      "theses": "...",
      "structure": "...",
      "activities": "...",
      "materialsToPrepare": "..."
    },
    "concretePractice": {
      "timing": 25,
      "desiredOutput": "...",
      "focusArea": "...",
      "activities": "...",
      "details": "...",
      "materialsToPrepare": "..."
    },
    "conclusions": {
      "timing": 5,
      "goal": "...",
      "activities": "...",
      "materialsToPrepare": "..."
    }
  }
  ```
- **Purpose:** Holistic review of entire lesson for coherence, timing balance, TBR principles adherence
- **AI Role:** Critic
- **Expected Output:** Comprehensive feedback on lesson plan quality
- **Special Note:** No field to update, feedback shown in modal only

---

## Key Patterns

### Context Dependencies

1. **Early fields** (overview) need minimal context
   - `topic` → no context
   - `audience` → topic
   - `learningOutcomes` → topic + audience

2. **Middle fields** need overview as foundation
   - All phase goals → topic + audience + learningOutcomes
   - Activities → goal + timing

3. **Activity fields** need timing + goal/purpose context
   - Ensures activities fit within allocated time
   - Ensures activities achieve stated goals

4. **Materials fields** need activity context
   - Materials derived from planned activities

5. **All operations** should have access to topic and audience for relevance

### Context Collection Strategy

For each operation, the client should:
1. Collect the full lesson plan data using `collectLessonData()`
2. Extract only the required fields for the specific operation
3. Send minimal context to API (reduces token usage)
4. Include `currentValue` for review operations

---

## API Endpoint Design Considerations

### Option A: Single Unified Endpoint
```
POST /api/assist
Body: {
  "operationId": "review_topic",
  "context": { ... }
}
```

**Pros:**
- Single endpoint to maintain
- Flexible for adding new operations
- Operation routing handled server-side

**Cons:**
- Large switch/case or mapping needed
- Less type-safe

### Option B: Operation-Specific Endpoints
```
POST /api/assist/review/topic
POST /api/assist/suggest/topic
POST /api/assist/review/audience
...
```

**Pros:**
- Clear, RESTful structure
- Easy to add middleware per operation
- Self-documenting API

**Cons:**
- 41 endpoints to maintain
- More boilerplate code

### Option C: Grouped Endpoints
```
POST /api/assist/overview/review
POST /api/assist/overview/suggest
POST /api/assist/connections/review
POST /api/assist/connections/suggest
...
```

**Pros:**
- Balanced approach
- Groups related operations
- Easier to apply phase-specific logic

**Cons:**
- Still requires field discrimination in body
- Moderate number of endpoints

### Recommendation
**Option A (Single Unified Endpoint)** is recommended for this PoC:
- Simplifies initial implementation
- Easy to test and iterate
- Operation-specific logic can be modularized internally
- Can refactor to Option B/C later if needed
