# NavigatorUi

> PASS5 NavigatorUI

modules are loaded on demand 







# Project CLI Setup

Project was setup with the following parameters:

\$ npx create-nuxt-app NavigatorUi

create-nuxt-app v2.12.0
✨ Generating Nuxt.js project in NavigatorUi
? Project name -> NavigatorUi
? Project description -> PASS5 NavigatorUI
? Author name -> CERTUS Port Automation
? Choose the package manager -> Npm  
? Choose UI framework -> Vuetify.js
? Choose custom server framework -> None (Recommended)
? Choose Nuxt.js modules -> Axios, Progressive Web App (PWA) Support, DotEnv
? Choose linting tools -> ESLint, Prettier, Lint staged files, StyleLint
? Choose test framework -> Jest
? Choose rendering mode -> Single Page App
? Choose development tools -> jsconfig.json (Recommended for VS Code)

## Build Setup

```bash
# install dependencies
$ npm run install

# serve with hot reload at localhost:3000
$ npm run dev

# build for production and launch server
$ npm run build
$ npm run start

# generate static project
$ npm run generate
```

For detailed explanation on how things work, check out [Nuxt.js docs](https://nuxtjs.org).

## Typescript conventions

When a file only exports one class or function then it should be preceded by the default
e.g:

export default interface RootState {
moduleContainers: BaseModule[]
}

A file should only contain one class or one interface

A file should be named according to the class or interface it contains

If a file contains an interface then the naming convention is to prefix the name with an "I"
e.g. IBasicModule
