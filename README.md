# PlexRipper

**UNDER ACTIVE DEVELOPMENT - ALPHA STAGE**

Tired of searching for media on different torrent websites or paying for Usenet server access? Well look no further! You can now use PlexRipper to download "everything" from the Plex servers you have access to and expand your collection that way! Let others collect media for you and then just download everything!

## Goals

- Be the best crossplatform Plex downloader there is!
- Be on the same level of awesomeness as [Radarr](https://github.com/Radarr/Radarr) and [Sonarr](https://github.com/Sonarr/Sonarr)!

## Current Features

- Runs in a Docker container and can be deployed on any type of platform that can host a Docker container!
- Add multiple Plex accounts to easily download from the accessible Plex servers!
- A beautiful GUI which has a Light and Dark-mode theme!
- Multi-threaded downloading and moves your finished downloads straight to your own Plex media collection!
- Will still work even if Plex.tv is down by connecting directly to the media servers!
- Has a highly complex neural network which will automatically fix misconfigured servers to allow connecting!
  
## Upcoming features

- Download music and photo's, currently only movies and tv shows!
- Search through and filter media available for downloading!

## Back-end

- [Based on the Clean Architecture template](https://github.com/jasontaylordev/CleanArchitecture) adhering to the SOLID principles.
- .NET Core with a RESTful API
- SignalR
- Entity Framework Core with a SQLite database
- MediatR with CQRS
- Serilog
- Result Pattern with [FluentResults](https://github.com/altmann/FluentResults)
- Nswag
- RestSharp

## Front-end

- [Vue.js](https://vuejs.org/) with [Nuxt.js](https://nuxtjs.org/)
- Typescript
- Rx.js
- Vuetify

https://github.com/labs42io/clean-code-typescript

## Translate PlexRipper

Make PlexRipper easier to use for everyone by translating it to your native language.

1. Check the Github issues if someone else is already translating to your language.
2. Create a Github issue explaining the language you would like to translate to.
   - This is to avoid multiple people translating to the same language
3. Make a copy of [en-US.json](https://github.com/PlexRipper/PlexRipper/tree/master/src/WebUI/src/lang) into the "lang" folder.
4. Translate only the text after the ":"!
5. When finished, either create a pull request or send it to  [plexripper@protonmail.com](mailto:plexripper@protonmail.com?subject=[PlexRipper%20Translation])

## Similar Projects

- [plexmedia-downloader](https://github.com/codedninja/plexmedia-downloader)
- [Saverr](https://github.com/ninthwalker/saverr)
- [PlexDL](https://github.com/BRH-Media/PlexDL)
  
## Credits

An absolute massive thank you to the following people!

- [Ninthwalker](https://github.com/ninthwalker) for his great work on [Saverr](https://github.com/ninthwalker/saverr), which showed me the method of downloading media from Plex!
- [Michael Altmann](https://github.com/altmann) for his awesome work on [Fluent Results](https://github.com/altmann/FluentResults) and the time he spent helping with making Fluent Results work in PlexRipper!
- [Dan Wahlin](https://github.com/DanWahlin) for his awesome work on [Observable-Store](https://github.com/DanWahlin/Observable-Store) and the time he spent helping me out with my questions!
- [Lenty Sprangers](https://github.com/LentySprangers) for the logo design!
- [Jetbrains](https://www.jetbrains.com/) for their awesome development tools, in paticular [Rider](https://www.jetbrains.com/rider/) and [Webstorm](https://www.jetbrains.com/webstorm/)!
- The developers and contributers behind the amazing [Vue.js](https://vuejs.org/), [Vuetify](https://vuetifyjs.com/en/), [Nuxt.js](https://nuxtjs.org/) and [RxJS](https://www.learnrxjs.io/)!
- The developers and contributers behind the beautiful [Vanta.js](https://www.vantajs.com/) project that is used to show some of the awesome backgrounds in PlexRipper!
- Several Reddit users over in [/r/Plex](https://old.reddit.com/r/PleX/) for being a pain in the ass and motivating me to create PlexRipper.
