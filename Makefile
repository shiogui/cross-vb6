export DOTNET_ENVIRONMENT := "Development"
export ASPNETCORE_ENVIRONMENT := "Development"
export DOTNET_WATCH_RESTART_ON_RUDE_EDIT := true

run:
	dotnet watch --project ./IDE/IDE.csproj