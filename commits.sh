#!/bin/bash

REPO="NethermindEth/nethermind"
SINCE=$(date -v-1m +%Y-%m-%dT%H:%M:%SZ)
GITHUB_TOKEN=${GITHUB_TOKEN:-""}  # Optional: export GITHUB_TOKEN=your_token
AUTH_HEADER=""
[ -n "$GITHUB_TOKEN" ] && AUTH_HEADER="Authorization: token $GITHUB_TOKEN"

echo "🔍 Counting commits since $SINCE across all branches in $REPO..."

TOTAL=0

# Get all branch names
BRANCHES=$(curl -s -H "$AUTH_HEADER" "https://api.github.com/repos/$REPO/branches?per_page=100" | jq -r '.[].name')

for BRANCH in $BRANCHES; do
  PAGE=1
  COUNT=0

  while true; do
    COMMITS=$(curl -s -H "$AUTH_HEADER" \
      "https://api.github.com/repos/$REPO/commits?sha=$BRANCH&since=$SINCE&per_page=100&page=$PAGE")

    NUM_COMMITS=$(echo "$COMMITS" | jq length)

    # GitHub may return an error as a string, skip if so
    if [[ "$NUM_COMMITS" == "null" ]] || [[ "$NUM_COMMITS" -eq 0 ]]; then
      break
    fi

    COUNT=$((COUNT + NUM_COMMITS))
    if [ "$NUM_COMMITS" -lt 100 ]; then
      break
    fi

    PAGE=$((PAGE + 1))
  done

  echo "✅ Branch '$BRANCH': $COUNT commits"
  TOTAL=$((TOTAL + COUNT))
done

echo "🔢 Total commits in the last month across all branches: $TOTAL"
