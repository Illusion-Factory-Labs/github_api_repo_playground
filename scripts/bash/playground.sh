#!/bin/zsh
set -euo pipefail

SOURCE_ENVIRONMENT="$1"
DESTINATION_ENVIRONMENT="$2"

if [[ "$SOURCE_ENVIRONMENT" != "prd" && "$SOURCE_ENVIRONMENT" != "tqs" ]]; then
    echo "Invalid source environment for run script: $SOURCE_ENVIRONMENT. Script will only run if values are 'prd' or 'tqs'."
  exit 0
fi

response="$(gh api repos/iLLusion-Factory-Labs/github-repo-playground/environments/$SOURCE_ENVIRONMENT/variables)"

if [[ "$(jq -r '(.variables // []) | length' <<< "$response")" -eq 0 ]]; then
  echo "No variables found for environment: $SOURCE_ENVIRONMENT"
  exit 0
fi

jq -c '(.variables // [])[]' <<< "$response" | while read -r var; do
    name=$(echo "$var" | jq -r '.name')
    value=$(echo "$var" | jq -r '.value')
    

    if gh api repos/iLLusion-Factory-Labs/github-repo-playground/environments/$DESTINATION_ENVIRONMENT/variables/$name >/dev/null 2>&1; then
        echo "'$name' variable already exists. Skipping creation for '$name'."
    else
        echo "Creating variable '$name' in environment '$DESTINATION_ENVIRONMENT'."
        gh api --method POST -H 'Accept: application/vnd.github+json' "repos/iLLusion-Factory-Labs/github-repo-playground/environments/$DESTINATION_ENVIRONMENT/variables" -f name="$name" -f value="$value"
    fi
done

echo "Variable copy process completed."
