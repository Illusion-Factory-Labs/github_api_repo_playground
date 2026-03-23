#!/bin/zsh
set -euo pipefail

gh api repos/iLLusion-Factory-Labs/github-repo-playground/environments/prd/variables | jq -c '.variables[]' | while read var; do
    name=$(echo "$var" | jq -r '.name')
    value=$(echo "$var" | jq -r '.value')
done
