#!/bin/bash

# Script to check for GitHub updates, pull, build, and run the Raspberry Pi Control project
# Usage: ./update-and-run.sh

set -euo pipefail  # Exit on error, unset vars are errors, and fail pipelines early
IFS=$'\n\t'

PROJECT_DIR="/home/andrew/Desktop/cstest/nocluee/ConsoleApp1"
PROJECT_FILE="ConsoleApp1.csproj"

echo "=========================================="
echo "Raspberry Pi Control - Update & Run"
echo "=========================================="
echo ""

# Navigate to project directory
cd "$PROJECT_DIR"

# Check if git repository
if [ ! -d ".git" ]; then
    echo "Error: Not a git repository at $PROJECT_DIR"
    exit 1
fi

# Resolve dotnet command (PATH -> $HOME/dotnet/dotnet -> $HOME/net/dotnet)
resolve_dotnet() {
    if command -v dotnet >/dev/null 2>&1; then
        echo "dotnet"
        return 0
    fi
    if [ -x "$HOME/dotnet/dotnet" ]; then
        echo "$HOME/dotnet/dotnet"
        return 0
    fi
    if [ -x "$HOME/net/dotnet" ]; then
        echo "$HOME/net/dotnet"
        return 0
    fi
    return 1
}

DOTNET_CMD=""
if DOTNET_CMD=$(resolve_dotnet); then
    echo "Using dotnet at: $DOTNET_CMD"
else
    echo "Error: dotnet SDK not found in PATH, $HOME/dotnet/dotnet, or $HOME/net/dotnet"
    echo "Please install .NET SDK or add it to PATH."
    exit 1
fi

echo "📡 Checking for updates from GitHub..."

# Determine a suitable remote: prefer 'origin', else 'Raspberry', else first listed remote
PREFERRED_REMOTE="origin"
if git remote | grep -qx "$PREFERRED_REMOTE"; then
    GIT_REMOTE="$PREFERRED_REMOTE"
elif git remote | grep -qx "Raspberry"; then
    GIT_REMOTE="Raspberry"
else
    # Pick the first remote if any
    if [ "$(git remote | wc -l)" -gt 0 ]; then
        GIT_REMOTE=$(git remote | head -n1)
    else
        echo "Error: No git remotes configured. Please add a remote and try again."
        exit 1
    fi
fi

echo "Using remote: $GIT_REMOTE"

# Get current branch
CURRENT_BRANCH=$(git rev-parse --abbrev-ref HEAD)
echo "Current branch: $CURRENT_BRANCH"

# Fetch latest changes from selected remote
git fetch "$GIT_REMOTE"

# Compute local and remote commit SHAs without relying on upstream config
LOCAL=$(git rev-parse HEAD)
REMOTE_REF="refs/remotes/$GIT_REMOTE/$CURRENT_BRANCH"
if git show-ref --verify --quiet "$REMOTE_REF"; then
    REMOTE=$(git rev-parse "$REMOTE_REF")
else
    echo "Warning: Remote branch $GIT_REMOTE/$CURRENT_BRANCH not found. Skipping update check."
    REMOTE=""
fi

if [ -n "$REMOTE" ]; then
    BASE=$(git merge-base HEAD "$REMOTE_REF" || echo "")
    if [ "$LOCAL" = "$REMOTE" ]; then
        echo "✅ Already up to date!"
    elif [ -n "$BASE" ] && [ "$LOCAL" = "$BASE" ]; then
        echo "🔄 Updates available! Pulling changes from $GIT_REMOTE/$CURRENT_BRANCH..."
        git pull "$GIT_REMOTE" "$CURRENT_BRANCH"
        echo "✅ Successfully pulled updates!"
    else
        echo "⚠️  Local changes or diverged history detected. Pulling with merge from $GIT_REMOTE/$CURRENT_BRANCH..."
        git pull "$GIT_REMOTE" "$CURRENT_BRANCH"
    fi
else
    echo "ℹ️  Proceeding without pulling because remote branch wasn't found."
fi

echo ""
echo "=========================================="
echo "🔨 Building project..."
echo "=========================================="
"$DOTNET_CMD" build "$PROJECT_FILE"

echo ""
echo "✅ Build finished"
echo ""
echo "=========================================="
echo "🚀 Running project..."
echo "=========================================="
echo ""

# Run the project
"$DOTNET_CMD" run --project "$PROJECT_FILE"
