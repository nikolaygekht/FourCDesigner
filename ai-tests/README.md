# AI Driver Testing Scripts

## Quick Start

### Prerequisites

1. **Start the application in DEBUG mode**
   ```bash
   dotnet run --project Gehtsoft.FourCDesigner
   ```

2. **For OpenAI Testing:**
   - Get an OpenAI API key from https://platform.openai.com/
   - Create `Config/appsettings.local.json`:
     ```json
     {
       "ai": {
         "openai": {
           "key": "sk-your-actual-api-key-here",
           "model": "gpt-3.5-turbo"
         }
       }
     }
     ```

3. **For Ollama Testing:**
   - Install Ollama from https://ollama.ai/
   - Pull a model: `ollama pull llama2`
   - Start Ollama service
   - Configuration already set in `appsettings.json`

## Test Validation (Single Test)

### Windows:
```bash
cd ai-tests\validation
test-validation.bat mock safe-input.json
test-validation.bat ollama injection-attempt.json
test-validation.bat openai malicious-content.json
```

### Linux/Mac:
```bash
cd ai-tests/validation
chmod +x test-validation.sh
./test-validation.sh mock safe-input.json
./test-validation.sh ollama injection-attempt.json
./test-validation.sh openai malicious-content.json
```

## Test Suggestions (Single Test)

### Windows:
```bash
cd ai-tests\suggestions
test-suggestions.bat mock math-lesson.json
test-suggestions.bat ollama science-lesson.json
test-suggestions.bat openai connections-activity.json
```

### Linux/Mac:
```bash
cd ai-tests/suggestions
chmod +x test-suggestions.sh
./test-suggestions.sh mock math-lesson.json
./test-suggestions.sh ollama science-lesson.json
./test-suggestions.sh openai connections-activity.json
```

## Run All Tests

### Windows:
```bash
cd ai-tests\examples
run-all-validation.bat mock
run-all-validation.bat ollama
run-all-suggestions.bat openai
```

### Linux/Mac:
```bash
cd ai-tests/examples
chmod +x *.sh
./run-all-validation.sh mock
./run-all-suggestions.sh ollama
```

## Creating Custom Tests

1. Create a new JSON file in the appropriate folder
2. Run the test script with your JSON file

### Example JSON structures:

**Validation:**
```json
{
  "userInput": "Your text to validate"
}
```

**Suggestions:**
```json
{
  "instructions": "Your instructions to the AI",
  "userInput": "Your content to process"
}
```

## Drivers

- **mock**: Uses predefined responses from JSON file (fast, no external dependencies)
- **ollama**: Uses local Ollama service (free, requires setup)
- **openai**: Uses OpenAI API (costs money, requires API key)

## Tips

- Start with `mock` driver to verify scripts work
- Use `mock` for developing and debugging JSON test cases
- Use `ollama` for free AI testing (after setup)
- Use `openai` for production-quality responses (costs apply)

## Troubleshooting

### Windows Issues:
- **curl not found**: Install curl or use Git Bash
- **Script execution error**: Run as Administrator if needed

### Linux/Mac Issues:
- **Permission denied**: Run `chmod +x *.sh` in the script directory
- **curl not found**: Install with `sudo apt install curl` (Ubuntu) or `brew install curl` (Mac)

### OpenAI Issues:
- **401 Unauthorized**: Check API key in `appsettings.local.json`
- **429 Rate Limit**: Wait or upgrade OpenAI plan
- **Timeout**: Check internet connection

### Ollama Issues:
- **Connection refused**: Ensure Ollama service is running (`ollama serve`)
- **Model not found**: Pull the model with `ollama pull llama2`
- **Slow responses**: Consider smaller model or better hardware

## Expected Response Format

All endpoints return JSON with the following structure:

```json
{
  "successful": true,
  "errorCode": "",
  "output": "The AI response or error message",
  "driverType": "mock",
  "elapsedMs": 123
}
```

## Folder Structure

```
ai-tests/
├── README.md                      # This file
├── validation/                    # Validation tests
│   ├── test-validation.bat       # Windows script
│   ├── test-validation.sh        # Linux/Mac script
│   ├── safe-input.json           # Safe content test
│   ├── injection-attempt.json    # Injection test
│   └── malicious-content.json    # Malicious content test
├── suggestions/                   # Suggestion tests
│   ├── test-suggestions.bat      # Windows script
│   ├── test-suggestions.sh       # Linux/Mac script
│   ├── math-lesson.json          # Math lesson test
│   ├── science-lesson.json       # Science lesson test
│   ├── connections-activity.json # 4C connections test
│   └── vark-engagement.json      # VARK multi-sensory test
└── examples/                      # Batch test runners
    ├── run-all-validation.bat    # Run all validation (Windows)
    ├── run-all-validation.sh     # Run all validation (Linux/Mac)
    ├── run-all-suggestions.bat   # Run all suggestions (Windows)
    └── run-all-suggestions.sh    # Run all suggestions (Linux/Mac)
```
