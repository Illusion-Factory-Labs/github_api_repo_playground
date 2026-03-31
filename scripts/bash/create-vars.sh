#!/bin/bash
# Script para criar variáveis de ambiente temporárias para importação
# As variáveis serão criadas com o prefixo "TEMP_VAR_" seguido do nome do ambiente (ex. TEMP_VAR_PRD, TEMP_VAR_TQS)

# Verificar se o argumento foi fornecido
if [ -z "$1" ]; then
  echo "Uso: $0 <ambientes>"
  echo "Exemplo: $0 prd-vars,tqs-vars"
  exit 1
fi

GITHUB_REPOSITORY="Illusion-Factory-Labs/github_api_repo_playground"
declare -A ENV_VARS=(
    ["ENV_VAR_EXAMPLE_1"]="env_var_value_1"
    ["ENV_VAR_EXAMPLE_2"]="env_var_value_2"
    ["ENV_VAR_EXAMPLE_3"]="env_var_value_3"
    ["ENV_VAR_EXAMPLE_4"]="env_var_value_4"
    ["ENV_VAR_EXAMPLE_5"]="env_var_value_5"
    ["ENV_VAR_EXAMPLE_6"]="env_var_value_6"
    ["ENV_VAR_EXAMPLE_7"]="env_var_value_7"
    ["ENV_VAR_EXAMPLE_8"]="env_var_value_8"
    ["ENV_VAR_EXAMPLE_9"]="env_var_value_9"
    ["ENV_VAR_EXAMPLE_10"]="env_var_value_10"
    ["ENV_VAR_EXAMPLE_11"]="env_var_value_11"
    ["ENV_VAR_EXAMPLE_12"]="env_var_value_12"
    ["ENV_VAR_EXAMPLE_13"]="env_var_value_13"
    ["ENV_VAR_EXAMPLE_14"]="env_var_value_14"
    ["ENV_VAR_EXAMPLE_15"]="env_var_value_15"
)


# Criar variáveis de ambiente temporárias para cada ambiente
GH_API_ENV_ENDPOINT="repos/${GITHUB_REPOSITORY}/environments"

# Converter a string de ambientes em um array
IFS=',' read -ra AMBIENTES <<< "$1"

for AMBIENTE in "${AMBIENTES[@]}"; do
    for ENV_VAR in "${!ENV_VARS[@]}"; do
        gh api --method POST "${GH_API_ENV_ENDPOINT}/${AMBIENTE}/variables" -f name="$ENV_VAR" -f value="${ENV_VARS[$ENV_VAR]}"
    done
done