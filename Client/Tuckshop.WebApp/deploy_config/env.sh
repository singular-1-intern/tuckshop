#!/bin/bash
LIGHTCYAN='\033[0;96m'
CYAN='\033[0;36m'
YELLOW='\033[0;33m'
GRAY='\033[0;90m'
WHITE='\033[0;97m'
NC='\033[0m' # No Color

printf "${LIGHTCYAN}Configuration Injection Script${NC}\n"

# Environment Variables to inject into the configuration file
settings=(
  "Environment"
)

# These are the defaults which will be used if the environment variables are not set
declare -A defaults
defaults["Environment"]="Production"

# Get the name of the configuration file to use
if [ -z "$ConfigName" ]
then
  # Not set, so use the default
  configFile="config.Release.json"
else
  configFile="config.$ConfigName.json"
fi

printf "\n${LIGHTCYAN}Copying configuration file template${NC}\n"
configFilePath="/usr/share/nginx/html/$configFile"
targetFilePath="/usr/share/nginx/html/config.json"

# Use for local testing:
# configFilePath="../public/$configFile" 
# targetFilePath="test.json"

if test -f "$configFilePath"; then
  # Copy the environment specific config file over the build's config.json
  printf "${WHITE}Using config file: '$configFile'${NC}\n"
  cp -R "$configFilePath" "$targetFilePath"
else
  printf "${YELLOW}WARNING: Release config file at path '$configFilePath' does not exist. The application configuration is defaulted to development settings.${NC}\n"
fi

# Inject each setting into the configuration file
printf "\n${LIGHTCYAN}Injecting configuration${NC}\n"
for setting in "${settings[@]}"
do
  value="${!setting}"

  # If the value is not set, check for a default
  if [ -z "$value" ]
  then
    if [[ -z "${defaults[$setting]+defaultcheck}" ]]; then
      printf "${YELLOW}WARNING: Environment variable for '$setting' was not set, and there is no default. A blank string will be used.${NC}\n"
      value=""
    else
      value="${defaults[$setting]}"
      printf "INFO: Environment variable '$setting' is not set, using default value.\n"
    fi
  fi

  # Inject the Domain Name into the config file
  printf "${WHITE}Setting '$setting' to '$value'${NC}\n"
  sed -i 's/${'$setting'}/'$value'/g' "$targetFilePath"
done

# Ensure Environment is lowercased
Environment=$(echo "${Environment}" | tr '[:upper:]' '[:lower:]')
echo "Environment: $Environment"

# Overwrite the Nginx configuration file based on the Environment variable that's set
printf "\n${LIGHTCYAN}Setting up Nginx configuration based on Environment variable${NC}\n"
configFilePath="/etc/nginx/configuration/nginx.${Environment}.conf"

if test -f "$configFilePath"; then
  printf "${WHITE}Using Nginx config file: '$configFilePath'${NC}\n"
  cp "$configFilePath" /etc/nginx/conf.d/default.conf
else
  printf "${YELLOW}WARNING: Nginx config file at path '$configFilePath' does not exist. Defaulting to base settings.${NC}\n"
  cp /etc/nginx/configuration/nginx.conf /etc/nginx/conf.d/default.conf
fi