#!/bin/bash
# Batch runner for all suggestion tests
# Usage: ./run-all-suggestions.sh <driver>
# Example: ./run-all-suggestions.sh mock

if [ -z "$1" ]; then
    echo "Usage: ./run-all-suggestions.sh <driver>"
    echo "Available drivers: mock, ollama, openai"
    exit 1
fi

DRIVER=$1
BASE_URL="http://localhost:5000/api/test/ai"
SUGGESTIONS_DIR="../suggestions"

echo "========================================"
echo "Running All Suggestion Tests"
echo "Driver: $DRIVER"
echo "========================================"
echo ""

echo "[1/4] Testing math lesson (fractions/decimals)..."
curl -X POST "$BASE_URL/$DRIVER/suggestions" \
     -H "Content-Type: application/json" \
     -d @$SUGGESTIONS_DIR/math-lesson.json \
     -w "\nHTTP Status: %{http_code}\nTime: %{time_total}s\n" \
     -s
echo ""
echo "----------------------------------------"
echo ""

echo "[2/4] Testing science lesson (photosynthesis VARK)..."
curl -X POST "$BASE_URL/$DRIVER/suggestions" \
     -H "Content-Type: application/json" \
     -d @$SUGGESTIONS_DIR/science-lesson.json \
     -w "\nHTTP Status: %{http_code}\nTime: %{time_total}s\n" \
     -s
echo ""
echo "----------------------------------------"
echo ""

echo "[3/4] Testing connections activity (water cycle)..."
curl -X POST "$BASE_URL/$DRIVER/suggestions" \
     -H "Content-Type: application/json" \
     -d @$SUGGESTIONS_DIR/connections-activity.json \
     -w "\nHTTP Status: %{http_code}\nTime: %{time_total}s\n" \
     -s
echo ""
echo "----------------------------------------"
echo ""

echo "[4/4] Testing VARK engagement (area/perimeter)..."
curl -X POST "$BASE_URL/$DRIVER/suggestions" \
     -H "Content-Type: application/json" \
     -d @$SUGGESTIONS_DIR/vark-engagement.json \
     -w "\nHTTP Status: %{http_code}\nTime: %{time_total}s\n" \
     -s
echo ""
echo "========================================"
echo "All suggestion tests completed"
echo "========================================"
