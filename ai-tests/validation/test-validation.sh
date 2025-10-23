#!/bin/bash
# Usage: ./test-validation.sh <driver> <json-file>
# Example: ./test-validation.sh mock safe-input.json

if [ -z "$1" ] || [ -z "$2" ]; then
    echo "Usage: $0 <driver> <json-file>"
    echo ""
    echo "Drivers: mock, ollama, openai"
    echo ""
    echo "Examples:"
    echo "  $0 mock safe-input.json"
    echo "  $0 ollama injection-attempt.json"
    echo "  $0 openai malicious-content.json"
    exit 1
fi

DRIVER=$1
JSON_FILE=$2

echo "========================================"
echo "AI Validation Test"
echo "========================================"
echo "Driver:  $DRIVER"
echo "JSON:    $JSON_FILE"
echo "Endpoint: POST /api/test/ai/validate/$DRIVER"
echo "========================================"
echo ""

curl -X POST "http://localhost:5000/api/test/ai/validate/$DRIVER" \
     -H "Content-Type: application/json" \
     -d "@$JSON_FILE" \
     -w "\n\nStatus: %{http_code}\nTime: %{time_total}s\n"

echo ""
