# Setup Nuxt in docker
FROM node:12.16.2
WORKDIR /app

COPY package*.json ./
RUN npm install

COPY . . 

RUN npm run build