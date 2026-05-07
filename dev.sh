#!/usr/bin/env bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

dotnet watch run --project "$SCRIPT_DIR/CrossVB6.Host/CrossVB6.Host.csproj"
