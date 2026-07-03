# =================================================================================================
# Build Container
# =================================================================================================
ARG YarnCacheId="yarn"

# Use Node.js v22
FROM node:22 AS build

# Create a directory for the application source
WORKDIR /usr/src

# Install app dependencies
# Build Speed Optimisation: Copy the package.*.json files first
# This allows the npm install to be done on a distinct layer, meaning if your dependencies don't change,
# a cached copy can be used.
COPY package.json yarn.lock .yarnrc.yml ./

# Force the use of corepack for Yarn. If 'yarnPath' is specified in .yarnrc.yml, we need to remove it,
# or an error will be thrown because the /.yarn/releases folder doesn't exist.
RUN corepack enable
RUN sed -i '/^yarnPath:/d' .yarnrc.yml

# Install packages - throw error if the yarn lock file differs from package.json
ARG YarnCacheId
RUN --mount=type=cache,id=${YarnCacheId},target=/root/.yarn,sharing=shared \
  yarn install --immutable

# Copy in the app source
COPY ./ ./

# Since .yarnrc.yml will have been re-copied, we need to remove yarnPath again (if present).
RUN sed -i '/^yarnPath:/d' .yarnrc.yml

# Build
RUN --mount=type=cache,id=${YarnCacheId},target=/root/.yarn,sharing=shared \
  yarn build

# =================================================================================================
# Run Container
# =================================================================================================
FROM nginx:stable-alpine AS runtime

# Copy the build into the nginx public html folder
COPY --from=build /usr/src/build /usr/share/nginx/html

# Expose nginx on port 80
EXPOSE 80

# Install bash
RUN apk add --no-cache bash

# Copy in the nginx configuration files
COPY ./deploy_config/nginx.conf ./deploy_config/nginx.*.conf /etc/nginx/configuration/

# Copy the env.sh script into the container
COPY ./deploy_config/env.sh /app/env.sh

# Make sure the line endings are unix format and make the env.sh shell script executable
RUN dos2unix /app/env.sh && chmod +x /app/env.sh

# Run env.sh and start nginx
CMD ["/bin/sh", "-c", "/app/env.sh && nginx -g \"daemon off;\""]
