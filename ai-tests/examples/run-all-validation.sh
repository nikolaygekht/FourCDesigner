#!/bin/bash
# Batch runner for all validation tests
# Usage: ./run-all-validation.sh <driver>
# Example: ./run-all-validation.sh mock

if [ -z "$1" ]; then
    echo "Usage: ./run-all-validation.sh <driver>"
    echo "Available drivers: mock, ollama, openai"
    exit 1
fi

DRIVER=$1
BASE_URL="http://localhost:5000/api/test/ai"
VALIDATION_DIR="../validation"

echo "========================================"
echo "Running All Validation Tests"
echo "Driver: $DRIVER"
echo "========================================"
echo ""

echo "[1/3] Testing safe input..."
curl -X POST "$BASE_URL/$DRIVER/validate" \
     -H "Content-Type: application/json" \
     -d @$VALIDATION_DIR/safe-input.json \
     -w "\nHTTP Status: %{http_code}\nTime: %{time_total}s\n" \
     -s
echo ""
echo "----------------------------------------"
echo ""

echo "[2/3] Testing injection attempt..."
curl -X POST "$BASE_URL/$DRIVER/validate" \
     -H "Content-Type: application/json" \
     -d @$VALIDATION_DIR/injection-attempt.json \
     -w "\nHTTP Status: %{http_code}\nTime: %{time_total}s\n" \
     -s
echo ""
echo "----------------------------------------"
echo ""

echo "[3/3] Testing malicious content..."
curl -X POST "$BASE_URL/$DRIVER/validate" \
     -H "Content-Type: application/json" \
     -d @$VALIDATION_DIR/malicious-content.json \
     -w "\nHTTP Status: %{http_code}\nTime: %{time_total}s\n" \
     -s
echo ""
echo "========================================"
echo "All validation tests completed"
echo "========================================"
