#!/bin/bash
# Usage: ./test-suggestions.sh <driver> <json-file>
# Example: ./test-suggestions.sh mock math-lesson.json

if [ -z "$1" ] || [ -z "$2" ]; then
    echo "Usage: $0 <driver> <json-file>"
    echo ""
    echo "Drivers: mock, ollama, openai"
    echo ""
    echo "Examples:"
    echo "  $0 mock math-lesson.json"
    echo "  $0 ollama science-lesson.json"
    echo "  $0 openai connections-activity.json"
    exit 1
fi

DRIVER=$1
JSON_FILE=$2

echo "========================================"
echo "AI Suggestions Test"
echo "========================================"
echo "Driver:  $DRIVER"
echo "JSON:    $JSON_FILE"
echo "Endpoint: POST /api/test/ai/suggestions/$DRIVER"
echo "========================================"
echo ""

curl -X POST "http://localhost:5000/api/test/ai/suggestions/$DRIVER" \
     -H "Content-Type: application/json" \
     -d "@$JSON_FILE" \
     -w "\n\nStatus: %{http_code}\nTime: %{time_total}s\n"

echo ""
