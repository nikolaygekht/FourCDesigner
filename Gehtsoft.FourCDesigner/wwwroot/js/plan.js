/**
 * Lesson Plan Page - Local Storage and Navigation
 * Handles accordion navigation and field-level data persistence
 */

(function() {
 'use strict';

 // ============================================
 // CONSTANTS
 // ============================================
 const STORAGE_KEY = 'lessonPlan';

 const SECTION_FIELDS = {
  overview: ['context', 'topic', 'audience', 'learningOutcomes'],
  connections: ['connections.timing', 'connections.goal', 'connections.activities', 'connections.materialsToPrepare'],
  concepts: ['concepts.timing', 'concepts.needToKnow', 'concepts.goodToKnow', 'concepts.theses', 'concepts.structure', 'concepts.activities', 'concepts.materialsToPrepare'],
  concretePractice: ['concretePractice.timing', 'concretePractice.desiredOutput', 'concretePractice.focusArea', 'concretePractice.activities', 'concretePractice.details', 'concretePractice.materialsToPrepare'],
  conclusions: ['conclusions.timing', 'conclusions.goal', 'conclusions.activities', 'conclusions.materialsToPrepare']
 };

 const FIELD_TO_ID_MAP = {
  'context': 'context',
  'topic': 'topic',
  'audience': 'audience',
  'learningOutcomes': 'learningOutcomes',
  'connections.timing': 'conn_timing',
  'connections.goal': 'conn_goal',
  'connections.activities': 'conn_activities',
  'connections.materialsToPrepare': 'conn_materials',
  'concepts.timing': 'concepts_timing',
  'concepts.needToKnow': 'concepts_needToKnow',
  'concepts.goodToKnow': 'concepts_goodToKnow',
  'concepts.theses': 'concepts_theses',
  'concepts.structure': 'concepts_structure',
  'concepts.activities': 'concepts_activities',
  'concepts.materialsToPrepare': 'concepts_materials',
  'concretePractice.timing': 'practice_timing',
  'concretePractice.desiredOutput': 'practice_desiredOutput',
  'concretePractice.focusArea': 'practice_focusArea',
  'concretePractice.activities': 'practice_activities',
  'concretePractice.details': 'practice_details',
  'concretePractice.materialsToPrepare': 'practice_materials',
  'conclusions.timing': 'concl_timing',
  'conclusions.goal': 'concl_goal',
  'conclusions.activities': 'concl_activities',
  'conclusions.materialsToPrepare': 'concl_materials'
 };

 // ============================================
 // UTILITY FUNCTIONS
 // ============================================

 /**
  * Expands section names to individual field paths
  * @param {Array<string>} fields - Array of section names or field paths
  * @returns {Array<string>} - Array of individual field paths
  */
 function expandFieldsToCollect(fields) {
  const result = [];
  fields.forEach(function(field) {
   if (SECTION_FIELDS[field]) {
    result.push.apply(result, SECTION_FIELDS[field]);
   } else {
    result.push(field);
   }
  });
  return result;
 }

 /**
  * Gets value from DOM element for a specific field path
  * @param {string} fieldPath - Field path (e.g., 'topic' or 'connections.goal')
  * @returns {string|number} - Field value
  */
 function getFieldValue(fieldPath) {
  const elementId = FIELD_TO_ID_MAP[fieldPath];
  if (!elementId) {
   console.warn('Unknown field path:', fieldPath);
   return '';
  }

  const element = document.getElementById(elementId);
  if (!element) {
   console.warn('Element not found for field:', fieldPath, 'id:', elementId);
   return '';
  }

  if (element.type === 'number') {
   return parseInt(element.value, 10) || 0;
  }
  return element.value || '';
 }

 /**
  * Sets value in DOM element for a specific field path
  * @param {string} fieldPath - Field path
  * @param {string|number} value - Value to set
  */
 function setFieldValue(fieldPath, value) {
  const elementId = FIELD_TO_ID_MAP[fieldPath];
  if (!elementId) {
   console.warn('Unknown field path:', fieldPath);
   return;
  }

  const element = document.getElementById(elementId);
  if (!element) {
   console.warn('Element not found for field:', fieldPath, 'id:', elementId);
   return;
  }

  element.value = value !== undefined && value !== null ? value : '';
 }

 /**
  * Gets nested value from object using dot notation path
  * @param {Object} obj - Source object
  * @param {string} path - Dot notation path (e.g., 'connections.goal')
  * @returns {*} - Value at path or undefined
  */
 function getNestedValue(obj, path) {
  const parts = path.split('.');
  let current = obj;
  for (let i = 0; i < parts.length; i++) {
   if (current === undefined || current === null) {
    return undefined;
   }
   current = current[parts[i]];
  }
  return current;
 }

 /**
  * Sets nested value in object using dot notation path
  * @param {Object} obj - Target object
  * @param {string} path - Dot notation path
  * @param {*} value - Value to set
  */
 function setNestedValue(obj, path, value) {
  const parts = path.split('.');
  let current = obj;
  for (let i = 0; i < parts.length - 1; i++) {
   const part = parts[i];
   if (!current[part] || typeof current[part] !== 'object') {
    current[part] = {};
   }
   current = current[part];
  }
  current[parts[parts.length - 1]] = value;
 }

 /**
  * Deep merge two objects
  * @param {Object} target - Target object
  * @param {Object} source - Source object
  * @returns {Object} - Merged object
  */
 function mergeDeep(target, source) {
  const output = Object.assign({}, target);
  if (isObject(target) && isObject(source)) {
   Object.keys(source).forEach(function(key) {
    if (isObject(source[key])) {
     if (!(key in target)) {
      Object.assign(output, { [key]: source[key] });
     } else {
      output[key] = mergeDeep(target[key], source[key]);
     }
    } else {
     Object.assign(output, { [key]: source[key] });
    }
   });
  }
  return output;
 }

 /**
  * Check if value is an object
  * @param {*} item - Value to check
  * @returns {boolean} - True if object
  */
 function isObject(item) {
  return item && typeof item === 'object' && !Array.isArray(item);
 }

 /**
  * Gets all field paths from a data object
  * @param {Object} data - Data object
  * @returns {Array<string>} - Array of field paths
  */
 function getAllFieldPaths(data) {
  const paths = [];
  function traverse(obj, prefix) {
   Object.keys(obj).forEach(function(key) {
    const path = prefix ? prefix + '.' + key : key;
    if (isObject(obj[key])) {
     traverse(obj[key], path);
    } else {
     paths.push(path);
    }
   });
  }
  traverse(data, '');
  return paths;
 }

 // ============================================
 // DATA COLLECTION FUNCTIONS
 // ============================================

 /**
  * Collects lesson data from form fields
  * @param {Array<string>} fields - Optional array of field paths or section names to collect
  * @returns {Object} - Lesson data object
  */
 function collectLessonData(fields) {
  if (!fields || fields.length === 0) {
   fields = ['overview', 'connections', 'concepts', 'concretePractice', 'conclusions'];
  }

  const expandedFields = expandFieldsToCollect(fields);
  const result = {};

  expandedFields.forEach(function(fieldPath) {
   const value = getFieldValue(fieldPath);
   setNestedValue(result, fieldPath, value);
  });

  return result;
 }

 // ============================================
 // STORAGE FUNCTIONS
 // ============================================

 /**
  * Saves lesson data to localStorage
  * @param {Array<string>} fields - Optional array of field paths to save
  */
 function saveLessonData(fields) {
  try {
   let dataToSave;

   if (!fields || fields.length === 0) {
    dataToSave = collectLessonData();
   } else {
    const existingData = loadLessonData() || {};
    const newData = collectLessonData(fields);
    dataToSave = mergeDeep(existingData, newData);
   }

   localStorage.setItem(STORAGE_KEY, JSON.stringify(dataToSave));
  } catch (error) {
   console.error('Failed to save lesson data:', error);
  }
 }

 /**
  * Loads lesson data from localStorage
  * @returns {Object|null} - Lesson data object or null
  */
 function loadLessonData() {
  try {
   const data = localStorage.getItem(STORAGE_KEY);
   return data ? JSON.parse(data) : null;
  } catch (error) {
   console.error('Failed to load lesson data:', error);
   return null;
  }
 }

 /**
  * Clears lesson data from localStorage
  */
 function clearLessonData() {
  try {
   localStorage.removeItem(STORAGE_KEY);
  } catch (error) {
   console.error('Failed to clear lesson data:', error);
  }
 }

 // ============================================
 // DATA APPLICATION FUNCTIONS
 // ============================================

 /**
  * Applies loaded data to form fields
  * @param {Object} data - Lesson data object
  * @param {Array<string>} fields - Optional array of field paths to apply
  */
 function applyLessonData(data, fields) {
  if (!data) return;

  if (!fields || fields.length === 0) {
   fields = getAllFieldPaths(data);
  } else {
   fields = expandFieldsToCollect(fields);
  }

  fields.forEach(function(fieldPath) {
   const value = getNestedValue(data, fieldPath);
   if (value !== undefined) {
    setFieldValue(fieldPath, value);
   }
  });
 }

 // ============================================
 // UI UPDATE FUNCTIONS
 // ============================================

 /**
  * Updates the review section with current data
  */
 function updateReviewDisplay() {
  const data = collectLessonData();

  document.getElementById('review-context').textContent = data.context || 'Not yet defined';
  document.getElementById('review-topic').textContent = data.topic || 'Not yet defined';
  document.getElementById('review-audience').textContent = data.audience || 'Not yet defined';
  document.getElementById('review-learningOutcomes').textContent = data.learningOutcomes || 'Not yet defined';

  if (data.connections) {
   document.getElementById('review-conn-timing').textContent = data.connections.timing + ' min';
   document.getElementById('review-conn-goal').textContent = data.connections.goal || 'Not yet defined';
   document.getElementById('review-conn-activities').textContent = data.connections.activities || 'Not yet defined';
   document.getElementById('review-conn-materials').textContent = data.connections.materialsToPrepare || 'Not yet defined';
  }

  if (data.concepts) {
   document.getElementById('review-concepts-timing').textContent = data.concepts.timing + ' min';
   document.getElementById('review-concepts-needToKnow').textContent = data.concepts.needToKnow || 'Not yet defined';
   document.getElementById('review-concepts-goodToKnow').textContent = data.concepts.goodToKnow || 'Not yet defined';
   document.getElementById('review-concepts-theses').textContent = data.concepts.theses || 'Not yet defined';
   document.getElementById('review-concepts-structure').textContent = data.concepts.structure || 'Not yet defined';
   document.getElementById('review-concepts-activities').textContent = data.concepts.activities || 'Not yet defined';
   document.getElementById('review-concepts-materials').textContent = data.concepts.materialsToPrepare || 'Not yet defined';
  }

  if (data.concretePractice) {
   document.getElementById('review-practice-timing').textContent = data.concretePractice.timing + ' min';
   document.getElementById('review-practice-desiredOutput').textContent = data.concretePractice.desiredOutput || 'Not yet defined';
   document.getElementById('review-practice-focusArea').textContent = data.concretePractice.focusArea || 'Not yet defined';
   document.getElementById('review-practice-activities').textContent = data.concretePractice.activities || 'Not yet defined';
   document.getElementById('review-practice-details').textContent = data.concretePractice.details || 'Not yet defined';
   document.getElementById('review-practice-materials').textContent = data.concretePractice.materialsToPrepare || 'Not yet defined';
  }

  if (data.conclusions) {
   document.getElementById('review-concl-timing').textContent = data.conclusions.timing + ' min';
   document.getElementById('review-concl-goal').textContent = data.conclusions.goal || 'Not yet defined';
   document.getElementById('review-concl-activities').textContent = data.conclusions.activities || 'Not yet defined';
   document.getElementById('review-concl-materials').textContent = data.conclusions.materialsToPrepare || 'Not yet defined';
  }
 }

 /**
  * Gets the currently active section name
  * @returns {string|null} - Section name or null
  */
 function getCurrentSection() {
  const activeNav = document.querySelector('.plan-nav-item.active');
  if (!activeNav) return null;

  const sectionName = activeNav.getAttribute('data-section');
  const sectionMap = {
   'overview': 'overview',
   'connection': 'connections',
   'concepts': 'concepts',
   'practice': 'concretePractice',
   'conclusion': 'conclusions'
  };

  return sectionMap[sectionName] || null;
 }

 // ============================================
 // NOTIFICATION SYSTEM
 // ============================================

 /**
  * Shows a floating notification in the bottom-right corner
  * @param {string} message - The message to display
  * @param {string} type - The notification type ('success', 'error', 'info')
  */
 function showNotification(message, type) {
  type = type || 'success';

  const notification = document.createElement('div');
  notification.className = 'floating-notification floating-notification-' + type;
  notification.textContent = message;

  document.body.appendChild(notification);

  setTimeout(function() {
   notification.classList.add('show');
  }, 10);

  setTimeout(function() {
   notification.classList.remove('show');
   setTimeout(function() {
    document.body.removeChild(notification);
   }, 300);
  }, 1000);
 }

 // ============================================
 // AI ASSIST OPERATIONS
 // ============================================

 /**
  * Calls the plan assistance API
  * @param {string} operationId - The operation identifier
  * @returns {Promise<string>} - Result text from AI
  */
 function callPlanApi(operationId) {
  return new Promise(function(resolve, reject) {
   // Get session ID from localStorage (server will validate if needed)
   const sessionId = localStorage.getItem('sessionId') || '';

   // Collect current lesson plan data
   const lessonPlan = collectLessonData();

   // Prepare request payload
   const requestData = {
    operationId: operationId,
    plan: lessonPlan
   };

   // Make API call
   fetch('/api/plan/assistance', {
    method: 'POST',
    headers: {
     'Content-Type': 'application/json',
     'X-fourc-session': sessionId
    },
    body: JSON.stringify(requestData)
   })
   .then(function(response) {
    if (!response.ok) {
     // Provide user-friendly error messages based on status code
     var errorMessage;
     switch (response.status) {
      case 401:
       errorMessage = 'Authentication required. Please log in.';
       break;
      case 403:
       errorMessage = 'Access denied. You do not have permission to perform this action.';
       break;
      case 429:
       errorMessage = 'Too many requests. Please wait a moment and try again.';
       break;
      case 500:
       errorMessage = 'Server error. Please try again later.';
       break;
      case 503:
       errorMessage = 'Service temporarily unavailable. Please try again later.';
       break;
      default:
       errorMessage = 'Request failed. Please try again.';
     }

     // Try to get more specific error from response
     return response.json().then(function(data) {
      throw new Error(data.error || errorMessage);
     }).catch(function(jsonError) {
      // If JSON parsing fails, use the default message
      throw new Error(errorMessage);
     });
    }
    return response.json();
   })
   .then(function(result) {
    if (result.successful) {
     resolve(result.output);
    } else {
     reject(new Error(result.errorCode + ': ' + result.output));
    }
   })
   .catch(function(error) {
    reject(error);
   });
  });
 }

 // Review operations
 function reviewContext() {
  return callPlanApi('review_context');
 }

 function reviewTopic() {
  return callPlanApi('review_topic');
 }

 function reviewAudience() {
  return callPlanApi('review_audience');
 }

 function reviewOutcomes() {
  return callPlanApi('review_outcomes');
 }

 function reviewConnGoal() {
  return callPlanApi('review_conn_goal');
 }

 function reviewConnActivities() {
  return callPlanApi('review_conn_activities');
 }

 function reviewConnMaterials() {
  return callPlanApi('review_conn_materials');
 }

 function reviewConceptsNeedToKnow() {
  return callPlanApi('review_concepts_needToKnow');
 }

 function reviewConceptsGoodToKnow() {
  return callPlanApi('review_concepts_goodToKnow');
 }

 function reviewConceptsTheses() {
  return callPlanApi('review_concepts_theses');
 }

 function reviewConceptsStructure() {
  return callPlanApi('review_concepts_structure');
 }

 function reviewConceptsActivities() {
  return callPlanApi('review_concepts_activities');
 }

 function reviewConceptsMaterials() {
  return callPlanApi('review_concepts_materials');
 }

 function reviewPracticeOutput() {
  return callPlanApi('review_practice_output');
 }

 function reviewPracticeFocus() {
  return callPlanApi('review_practice_focus');
 }

 function reviewPracticeActivities() {
  return callPlanApi('review_practice_activities');
 }

 function reviewPracticeDetails() {
  return callPlanApi('review_practice_details');
 }

 function reviewPracticeMaterials() {
  return callPlanApi('review_practice_materials');
 }

 function reviewConclGoal() {
  return callPlanApi('review_concl_goal');
 }

 function reviewConclActivities() {
  return callPlanApi('review_concl_activities');
 }

 function reviewConclMaterials() {
  return callPlanApi('review_concl_materials');
 }

 function reviewWholeLesson() {
  return callPlanApi('review_whole_lesson');
 }

 // Suggest operations
 function suggestContext() {
  return callPlanApi('suggest_context');
 }

 function suggestTopic() {
  return callPlanApi('suggest_topic');
 }

 function suggestAudience() {
  return callPlanApi('suggest_audience');
 }

 function suggestOutcomes() {
  return callPlanApi('suggest_outcomes');
 }

 function suggestConnGoal() {
  return callPlanApi('suggest_conn_goal');
 }

 function suggestConnActivities() {
  return callPlanApi('suggest_conn_activities');
 }

 function suggestConnMaterials() {
  return callPlanApi('suggest_conn_materials');
 }

 function suggestConceptsNeedToKnow() {
  return callPlanApi('suggest_concepts_needToKnow');
 }

 function suggestConceptsGoodToKnow() {
  return callPlanApi('suggest_concepts_goodToKnow');
 }

 function suggestConceptsTheses() {
  return callPlanApi('suggest_concepts_theses');
 }

 function suggestConceptsStructure() {
  return callPlanApi('suggest_concepts_structure');
 }

 function suggestConceptsActivities() {
  return callPlanApi('suggest_concepts_activities');
 }

 function suggestConceptsMaterials() {
  return callPlanApi('suggest_concepts_materials');
 }

 function suggestPracticeOutput() {
  return callPlanApi('suggest_practice_output');
 }

 function suggestPracticeFocus() {
  return callPlanApi('suggest_practice_focus');
 }

 function suggestPracticeActivities() {
  return callPlanApi('suggest_practice_activities');
 }

 function suggestPracticeDetails() {
  return callPlanApi('suggest_practice_details');
 }

 function suggestPracticeMaterials() {
  return callPlanApi('suggest_practice_materials');
 }

 function suggestConclGoal() {
  return callPlanApi('suggest_concl_goal');
 }

 function suggestConclActivities() {
  return callPlanApi('suggest_concl_activities');
 }

 function suggestConclMaterials() {
  return callPlanApi('suggest_concl_materials');
 }

 // ============================================
 // AI ASSIST OPERATION MAPPING
 // ============================================

 /**
  * Maps operation IDs to [fieldId, operationFunction] tuples
  */
 const OPERATION_MAP = {
  'review_context': ['context', reviewContext],
  'suggest_context': ['context', suggestContext],
  'review_topic': ['topic', reviewTopic],
  'suggest_topic': ['topic', suggestTopic],
  'review_audience': ['audience', reviewAudience],
  'suggest_audience': ['audience', suggestAudience],
  'review_outcomes': ['learningOutcomes', reviewOutcomes],
  'suggest_outcomes': ['learningOutcomes', suggestOutcomes],
  'review_conn_goal': ['conn_goal', reviewConnGoal],
  'suggest_conn_goal': ['conn_goal', suggestConnGoal],
  'review_conn_activities': ['conn_activities', reviewConnActivities],
  'suggest_conn_activities': ['conn_activities', suggestConnActivities],
  'review_conn_materials': ['conn_materials', reviewConnMaterials],
  'suggest_conn_materials': ['conn_materials', suggestConnMaterials],
  'review_concepts_needToKnow': ['concepts_needToKnow', reviewConceptsNeedToKnow],
  'suggest_concepts_needToKnow': ['concepts_needToKnow', suggestConceptsNeedToKnow],
  'review_concepts_goodToKnow': ['concepts_goodToKnow', reviewConceptsGoodToKnow],
  'suggest_concepts_goodToKnow': ['concepts_goodToKnow', suggestConceptsGoodToKnow],
  'review_concepts_theses': ['concepts_theses', reviewConceptsTheses],
  'suggest_concepts_theses': ['concepts_theses', suggestConceptsTheses],
  'review_concepts_structure': ['concepts_structure', reviewConceptsStructure],
  'suggest_concepts_structure': ['concepts_structure', suggestConceptsStructure],
  'review_concepts_activities': ['concepts_activities', reviewConceptsActivities],
  'suggest_concepts_activities': ['concepts_activities', suggestConceptsActivities],
  'review_concepts_materials': ['concepts_materials', reviewConceptsMaterials],
  'suggest_concepts_materials': ['concepts_materials', suggestConceptsMaterials],
  'review_practice_output': ['practice_desiredOutput', reviewPracticeOutput],
  'suggest_practice_output': ['practice_desiredOutput', suggestPracticeOutput],
  'review_practice_focus': ['practice_focusArea', reviewPracticeFocus],
  'suggest_practice_focus': ['practice_focusArea', suggestPracticeFocus],
  'review_practice_activities': ['practice_activities', reviewPracticeActivities],
  'suggest_practice_activities': ['practice_activities', suggestPracticeActivities],
  'review_practice_details': ['practice_details', reviewPracticeDetails],
  'suggest_practice_details': ['practice_details', suggestPracticeDetails],
  'review_practice_materials': ['practice_materials', reviewPracticeMaterials],
  'suggest_practice_materials': ['practice_materials', suggestPracticeMaterials],
  'review_concl_goal': ['concl_goal', reviewConclGoal],
  'suggest_concl_goal': ['concl_goal', suggestConclGoal],
  'review_concl_activities': ['concl_activities', reviewConclActivities],
  'suggest_concl_activities': ['concl_activities', suggestConclActivities],
  'review_concl_materials': ['concl_materials', reviewConclMaterials],
  'suggest_concl_materials': ['concl_materials', suggestConclMaterials],
  'review_whole_lesson': [null, reviewWholeLesson]
 };

 // ============================================
 // AI ASSIST MODAL MANAGEMENT
 // ============================================

 var currentModalInstance = null;
 var currentFieldId = null;
 var currentResultText = null;

 /**
  * Shows the modal in loading state
  * @returns {Object} - Bootstrap modal instance
  */
 function showLoadingModal() {
  const modal = document.getElementById('ai-assist-modal');
  const loadingDiv = document.getElementById('ai-assist-loading');
  const resultDiv = document.getElementById('ai-assist-result');
  const resultText = document.getElementById('ai-assist-result-text');
  const actionButtons = document.getElementById('ai-assist-action-buttons');

  // Reset loading div to show spinner and progress text
  const spinner = loadingDiv.querySelector('.spinner-border');
  const textSpan = loadingDiv.querySelector('span:not(.visually-hidden)');
  if (spinner) {
   spinner.style.display = '';
  }
  if (textSpan) {
   textSpan.textContent = 'The request is in progress...';
   textSpan.style.color = '';
  }

  // Clear previous result content to prevent flashing
  if (resultText) {
   resultText.innerHTML = '';
  }

  loadingDiv.style.display = 'block';
  resultDiv.style.display = 'none';
  actionButtons.style.display = 'none';

  if (!currentModalInstance) {
   currentModalInstance = new bootstrap.Modal(modal);
  }
  currentModalInstance.show();

  return currentModalInstance;
 }

 /**
  * Formats text with basic markdown-style formatting to HTML
  * @param {string} text - The text to format
  * @returns {string} - Formatted HTML
  */
 function formatTextToHtml(text) {
  // Escape HTML to prevent injection
  var escaped = text
   .replace(/&/g, '&amp;')
   .replace(/</g, '&lt;')
   .replace(/>/g, '&gt;')
   .replace(/"/g, '&quot;')
   .replace(/'/g, '&#039;');

  // Format headings: # text -> larger bold text with line break
  escaped = escaped.replace(/^#\s+(.+)$/gm, '<span style="font-size: 1.2em;"><b>$1</b></span><br>');

  // Format bold: **text** -> <b>text</b>
  escaped = escaped.replace(/\*\*([^\*]+)\*\*/g, '<b>$1</b>');

  // Convert newlines to <br>
  escaped = escaped.replace(/\n/g, '<br>');

  return escaped;
 }

 /**
  * Shows the modal in result state with AI response
  * @param {string} text - The result text from AI
  * @param {string} fieldId - The field ID to apply the result to
  */
 function showResultModal(text, fieldId) {
  const loadingDiv = document.getElementById('ai-assist-loading');
  const resultDiv = document.getElementById('ai-assist-result');
  const resultText = document.getElementById('ai-assist-result-text');
  const actionButtons = document.getElementById('ai-assist-action-buttons');
  const closeBtn = document.getElementById('ai-assist-close-btn');

  loadingDiv.style.display = 'none';
  resultDiv.style.display = 'block';
  resultText.innerHTML = formatTextToHtml(text);

  if (fieldId) {
   actionButtons.style.display = 'inline-block';
   closeBtn.style.display = 'inline-block';
  } else {
   actionButtons.style.display = 'none';
   closeBtn.style.display = 'inline-block';
  }

  currentFieldId = fieldId;
  currentResultText = text;
 }

 /**
  * Closes the modal
  */
 function closeModal() {
  if (currentModalInstance) {
   currentModalInstance.hide();
  }

  // Clear result content to prevent showing stale data on next open
  const resultText = document.getElementById('ai-assist-result-text');
  if (resultText) {
   resultText.innerHTML = '';
  }

  currentFieldId = null;
  currentResultText = null;
 }

 // ============================================
 // AI ASSIST FIELD MANIPULATION
 // ============================================

 /**
  * Appends text to a field
  * @param {string} fieldId - The field element ID
  * @param {string} text - The text to append
  */
 function appendToField(fieldId, text) {
  const element = document.getElementById(fieldId);
  if (!element) {
   console.error('Field not found:', fieldId);
   return;
  }

  const currentValue = element.value || '';
  const separator = currentValue && !currentValue.endsWith('\n') ? '\n\n' : '';
  element.value = currentValue + separator + text;

  closeModal();
  showNotification('Text appended successfully!', 'success');
 }

 /**
  * Replaces field content with text
  * @param {string} fieldId - The field element ID
  * @param {string} text - The text to set
  */
 function replaceField(fieldId, text) {
  const element = document.getElementById(fieldId);
  if (!element) {
   console.error('Field not found:', fieldId);
   return;
  }

  element.value = text;

  closeModal();
  showNotification('Text replaced successfully!', 'success');
 }

 // ============================================
 // AI ASSIST BUTTON HANDLERS
 // ============================================

 /**
  * Shows error message in the loading div
  * @param {string} errorMessage - The error message to display
  */
 function showErrorInLoadingDiv(errorMessage) {
  const loadingDiv = document.getElementById('ai-assist-loading');
  const spinner = loadingDiv.querySelector('.spinner-border');
  const textSpan = loadingDiv.querySelector('span:not(.visually-hidden)');

  if (spinner) {
   spinner.style.display = 'none';
  }
  if (textSpan) {
   textSpan.textContent = 'Error: ' + errorMessage;
   textSpan.style.color = '#dc3545'; // Bootstrap danger color
  }
 }

 /**
  * Main handler for AI assist button clicks
  * @param {string} operationId - The operation identifier
  */
 function handleAssistButtonClick(operationId) {
  const mapping = OPERATION_MAP[operationId];
  if (!mapping) {
   console.error('Unknown operation:', operationId);
   return;
  }

  const fieldId = mapping[0];
  const operationFunction = mapping[1];

  showLoadingModal();

  operationFunction()
   .then(function(resultText) {
    showResultModal(resultText, fieldId);
   })
   .catch(function(error) {
    console.error('Operation failed:', error);
    showErrorInLoadingDiv(error.message);
   });
 }

 /**
  * Initialize AI assist buttons
  */
 function initializeAssistButtons() {
  const appendBtn = document.getElementById('ai-assist-append-btn');
  const replaceBtn = document.getElementById('ai-assist-replace-btn');

  if (appendBtn) {
   appendBtn.addEventListener('click', function() {
    if (currentFieldId && currentResultText) {
     appendToField(currentFieldId, currentResultText);
    }
   });
  }

  if (replaceBtn) {
   replaceBtn.addEventListener('click', function() {
    if (currentFieldId && currentResultText) {
     replaceField(currentFieldId, currentResultText);
    }
   });
  }

  const assistButtons = document.querySelectorAll('[data-action]');
  assistButtons.forEach(function(button) {
   const action = button.getAttribute('data-action');
   if (OPERATION_MAP[action]) {
    button.addEventListener('click', function() {
     handleAssistButtonClick(action);
    });
   }
  });
 }

 // ============================================
 // BUTTON HANDLERS
 // ============================================

 /**
  * Resets all form fields to default values
  */
 function resetToDefaults() {
  document.getElementById('context').value = '';
  document.getElementById('topic').value = '';
  document.getElementById('audience').value = '';
  document.getElementById('learningOutcomes').value = '';

  document.getElementById('conn_timing').value = 5;
  document.getElementById('conn_goal').value = '';
  document.getElementById('conn_activities').value = '';
  document.getElementById('conn_materials').value = '';

  document.getElementById('concepts_timing').value = 15;
  document.getElementById('concepts_needToKnow').value = '';
  document.getElementById('concepts_goodToKnow').value = '';
  document.getElementById('concepts_theses').value = '';
  document.getElementById('concepts_structure').value = '';
  document.getElementById('concepts_activities').value = '';
  document.getElementById('concepts_materials').value = '';

  document.getElementById('practice_timing').value = 25;
  document.getElementById('practice_desiredOutput').value = '';
  document.getElementById('practice_focusArea').value = '';
  document.getElementById('practice_activities').value = '';
  document.getElementById('practice_details').value = '';
  document.getElementById('practice_materials').value = '';

  document.getElementById('concl_timing').value = 5;
  document.getElementById('concl_goal').value = '';
  document.getElementById('concl_activities').value = '';
  document.getElementById('concl_materials').value = '';
 }

 /**
  * Initialize button handlers
  */
 function initializeButtons() {
  const startOverButton = document.getElementById('start-over-button');
  if (startOverButton) {
   startOverButton.addEventListener('click', function() {
    if (confirm('Are you sure you want to start over? All current data will be lost.')) {
     clearLessonData();
     resetToDefaults();
    }
   });
  }

  const saveButtons = document.querySelectorAll('.save-section-button');
  saveButtons.forEach(function(button) {
   button.addEventListener('click', function() {
    saveLessonData();
    showNotification('Lesson plan saved successfully!', 'success');
   });
  });
 }

 // ============================================
 // ACCORDION NAVIGATION
 // ============================================

 /**
  * Initialize accordion navigation
  */
 function initializeAccordion() {
  const navItems = document.querySelectorAll('.plan-nav-item');
  const sections = document.querySelectorAll('.plan-section');

  navItems.forEach(function(navItem) {
   navItem.addEventListener('click', function(e) {
    e.preventDefault();

    const currentSection = getCurrentSection();
    if (currentSection) {
     saveLessonData([currentSection]);
    }

    const targetSection = this.getAttribute('data-section');

    navItems.forEach(function(item) {
     item.classList.remove('active');
    });
    this.classList.add('active');

    sections.forEach(function(section) {
     section.classList.remove('active');
    });

    const targetElement = document.getElementById('section-' + targetSection);
    if (targetElement) {
     targetElement.classList.add('active');
    }

    if (targetSection === 'review') {
     updateReviewDisplay();
    }
   });
  });

  const quickNavButtons = document.querySelectorAll('[data-navigate]');
  quickNavButtons.forEach(function(button) {
   button.addEventListener('click', function() {
    const targetSection = this.getAttribute('data-navigate');
    const navItem = document.querySelector('.plan-nav-item[data-section="' + targetSection + '"]');
    if (navItem) {
     navItem.click();
    }
   });
  });
 }

 // ============================================
 // INITIALIZATION
 // ============================================

 document.addEventListener('DOMContentLoaded', function() {
  const savedData = loadLessonData();
  if (savedData) {
   applyLessonData(savedData);
  }

  initializeAccordion();
  initializeButtons();
  initializeAssistButtons();
 });

 window.addEventListener('beforeunload', function() {
  saveLessonData();
 });

 // ============================================
 // PUBLIC API (for future use)
 // ============================================
 window.LessonPlanManager = {
  save: saveLessonData,
  load: loadLessonData,
  clear: clearLessonData,
  collect: collectLessonData,
  apply: applyLessonData
 };

})();
