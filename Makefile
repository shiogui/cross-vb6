# Variables
DOTNET = dotnet
PROJECT_PATH = AvaloniaVisualBasic.Desktop/AvaloniaVisualBasic.Desktop.csproj

.PHONY: build run clean

# Build the AvaloniaVisualBasic.Desktop app
build:
	$(DOTNET) build $(PROJECT_PATH) -f net10.0

# Run the AvaloniaVisualBasic.Desktop app
run:
	$(DOTNET) run --project $(PROJECT_PATH) -f net10.0

# Clean the build artifacts
clean:
	$(DOTNET) clean $(PROJECT_PATH) -f net10.0
