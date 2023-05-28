#Base image taken from:https://github.com/cypress-io/cypress-docker-images
FROM cypress/browsers:node16.17.0-chrome106
#Create the folder where our project will be stored
RUN mkdir /cypress-plexripper
#We make it our workdirectory
WORKDIR /cypress-plexripper
#Let's copy the essential files that we MUST use to run our scripts.
COPY ./src/WebAPI/ClientApp/package.json .
COPY ./src/WebAPI/ClientApp/package-lock.json .
COPY ./src/WebAPI/ClientApp/cypress/cypress.config.ts .
COPY ./src/WebAPI/ClientApp/tsconfig.json .

COPY ./src/WebAPI/ClientApp/cypress ./cypress
COPY ./src/WebAPI/ClientApp/src ./src
#Install the cypress dependencies in the work directory
RUN npm install
#Executable commands the container will use[Exec Form]
ENTRYPOINT ["npx","cypress","run"]
#With CMD in this case, we can specify more parameters to the last entrypoint.
CMD [""]