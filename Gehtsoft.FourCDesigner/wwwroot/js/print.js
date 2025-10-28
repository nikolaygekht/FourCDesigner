/**
 * Print Page - Load and Display Lesson Plan
 * Loads lesson plan data from localStorage and displays it in print format
 */

(function() {
    "use strict";

    // ============================================
    // CONSTANTS
    // ============================================
    const STORAGE_KEY = "lessonPlan";

    // ============================================
    // UTILITY FUNCTIONS
    // ============================================

    /**
     * Loads lesson data from localStorage
     * @returns {Object|null} - Lesson data object or null
     */
    function loadLessonData() {
        try {
            var data = localStorage.getItem(STORAGE_KEY);
            return data ? JSON.parse(data) : null;
        } catch (error) {
            console.error("Failed to load lesson data:", error);
            return null;
        }
    }

    /**
     * Formats text content for display
     * @param {string} text - The text to format
     * @returns {string} - Formatted text
     */
    function formatText(text) {
        if (!text || text === "")
            return "Not yet defined";

        return text;
    }

    /**
     * Sets content of an element by ID
     * @param {string} elementId - The element ID
     * @param {string} content - The content to set
     */
    function setElementContent(elementId, content) {
        var element = document.getElementById(elementId);
        if (element)
            element.textContent = formatText(content);
    }

    /**
     * Calculates total duration of the lesson
     * @param {Object} data - Lesson data object
     * @returns {number} - Total duration in minutes
     */
    function calculateTotalDuration(data) {
        var total = 0;

        if (data.connections && data.connections.timing)
            total += parseInt(data.connections.timing, 10) || 0;

        if (data.concepts && data.concepts.timing)
            total += parseInt(data.concepts.timing, 10) || 0;

        if (data.concretePractice && data.concretePractice.timing)
            total += parseInt(data.concretePractice.timing, 10) || 0;

        if (data.conclusions && data.conclusions.timing)
            total += parseInt(data.conclusions.timing, 10) || 0;

        return total;
    }

    // ============================================
    // DATA POPULATION FUNCTIONS
    // ============================================

    /**
     * Populates the print view with lesson data
     * @param {Object} data - Lesson data object
     */
    function populatePrintView(data) {
        if (!data) {
            console.warn("No lesson data found");
            return;
        }

        // Set current date
        var currentDate = new Date();
        var dateString = currentDate.toLocaleDateString("en-US", {
            year: "numeric",
            month: "long",
            day: "numeric"
        });
        setElementContent("print-date", dateString);

        // Overview section
        setElementContent("print-topic", data.topic);
        setElementContent("print-audience", data.audience);
        setElementContent("print-learningOutcomes", data.learningOutcomes);

        // Connections section
        if (data.connections) {
            var connTiming = data.connections.timing || 0;
            setElementContent("print-conn-timing", connTiming + " minutes");
            setElementContent("print-conn-goal", data.connections.goal);
            setElementContent("print-conn-activities", data.connections.activities);
            setElementContent("print-conn-materials", data.connections.materialsToPrepare);
        }

        // Concepts section
        if (data.concepts) {
            var conceptsTiming = data.concepts.timing || 0;
            setElementContent("print-concepts-timing", conceptsTiming + " minutes");
            setElementContent("print-concepts-needToKnow", data.concepts.needToKnow);
            setElementContent("print-concepts-goodToKnow", data.concepts.goodToKnow);
            setElementContent("print-concepts-theses", data.concepts.theses);
            setElementContent("print-concepts-structure", data.concepts.structure);
            setElementContent("print-concepts-activities", data.concepts.activities);
            setElementContent("print-concepts-materials", data.concepts.materialsToPrepare);
        }

        // Concrete Practice section
        if (data.concretePractice) {
            var practiceTiming = data.concretePractice.timing || 0;
            setElementContent("print-practice-timing", practiceTiming + " minutes");
            setElementContent("print-practice-desiredOutput", data.concretePractice.desiredOutput);
            setElementContent("print-practice-focusArea", data.concretePractice.focusArea);
            setElementContent("print-practice-activities", data.concretePractice.activities);
            setElementContent("print-practice-details", data.concretePractice.details);
            setElementContent("print-practice-materials", data.concretePractice.materialsToPrepare);
        }

        // Conclusions section
        if (data.conclusions) {
            var conclusionsTiming = data.conclusions.timing || 0;
            setElementContent("print-concl-timing", conclusionsTiming + " minutes");
            setElementContent("print-concl-goal", data.conclusions.goal);
            setElementContent("print-concl-activities", data.conclusions.activities);
            setElementContent("print-concl-materials", data.conclusions.materialsToPrepare);
        }

        // Calculate and display total duration
        var totalDuration = calculateTotalDuration(data);
        setElementContent("print-total-duration", totalDuration + " minutes");
    }

    // ============================================
    // PRINT FUNCTION
    // ============================================

    /**
     * Triggers the browser print dialog
     */
    function printPage() {
        window.print();
    }

    /**
     * Initialize print button handler
     */
    function initializePrintButton() {
        var printButton = document.getElementById("print-button");
        if (printButton) {
            printButton.addEventListener("click", function() {
                printPage();
            });
        }
    }

    // ============================================
    // INITIALIZATION
    // ============================================

    document.addEventListener("DOMContentLoaded", function() {
        var lessonData = loadLessonData();

        if (!lessonData) {
            alert("No lesson plan data found. Please create a lesson plan first.");
            window.location.href = "/plan.html";
            return;
        }

        populatePrintView(lessonData);
        initializePrintButton();
    });

})();
