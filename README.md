
# <img src="./export/plexripper/logo/full/full-logo-256.png" alt="PlexRipper" width="32"> PlexRipper

![Docker Pulls](https://img.shields.io/docker/pulls/plexripper/plexripper?label=Docker%20Pulls&style=flat-square)
![Docker Image Size (tag)](https://img.shields.io/docker/image-size/plexripper/plexripper/latest?label=PlexRipper%20Latest%20Image%20Size&style=flat-square)
![Docker Image Size (tag)](https://img.shields.io/docker/image-size/plexripper/plexripper/dev?label=PlexRipper%20Dev%20Image%20Size&style=flat-square)
![Lines of code](https://img.shields.io/tokei/lines/github/plexripper/plexripper?label=Lines%20of%20Code&style=flat-square)
![GitHub issues](https://img.shields.io/github/issues/plexripper/plexripper?label=Github%20Issues&style=flat-square)

![GitHub](https://img.shields.io/github/license/plexripper/plexripper?style=flat-square)
![GitHub followers](https://img.shields.io/github/followers/plexripper?style=social)
![GitHub Repo stars](https://img.shields.io/github/stars/plexripper/plexripper?style=social)

**UNDER ACTIVE DEVELOPMENT - ALPHA STAGE => IT "WORKS", BUT NOT ALWAYS!**

Tired of searching for media on different torrent websites or paying for Usenet server access? Well look no further! You can now use PlexRipper to download everything* from the Plex servers you have access to and expand your collection that way!

Let others collect media for you and then just download everything!

## Feedback

Any feedback, good or bad, is very much appreciated! You can create an [Github issue](https://github.com/PlexRipper/PlexRipper/issues) or send an e-mail to [PlexRipper@protonmail.com](mailto:PlexRipper@protonmail.com?subject=[GitHub]%20Feedback%20PlexRipper). If you like to stay up to date with the progress, then consider following the [project](https://github.com/PlexRipper) and giving it a star if you like it!

## Goals

- Be the best cross-platform Plex downloader there is!

- Be on the same level of awesomeness as [Radarr](https://github.com/Radarr/Radarr) and [Sonarr](https://github.com/Sonarr/Sonarr)!

## Current Features

- Runs in a Docker container and can be deployed on any type of platform that can host a Docker container!
- A beautiful GUI which has a Light and Dark-mode theme!

- Add multiple Plex accounts to easily download from the accessible Plex servers!

- Multi-threaded downloading and PlexRipper moves your finished downloads straight to your own Plex media collection!
  
- Multi-language support, currently with English and French

- Will still work even if Plex.tv is down by connecting directly to the media servers!

- Has a "highly complex neural network" which will automatically fix misconfigured servers to allow connecting!

## Upcoming features

- Download music and photo's, currently only movies and tv shows!
- Search through and filter media available for downloading!
- Sonarr and Radarr integration
- Download Plex Collections with custom banner and thumbnail

- **PlexRipper Authentication, right now there is ZERO security so make sure to NOT expose the Web UI to the public internet! e.g. forwarding a domain to the container etc**

## Installation

### Docker

| Tag                                                                                        | Description                                      | Command                                    |
| ------------------------------------------------------------------------------------------ | ------------------------------------------------ | ------------------------------------------ |
| [latest](https://hub.docker.com/r/plexripper/plexripper/tags?page=1&ordering=last_updated) | Stable branch, only the most stable code is here | `docker pull plexripper/plexripper:latest` |
| [dev](https://hub.docker.com/r/plexripper/plexripper/tags?page=1&ordering=last_updated)    | Development branch, latest changes are made here | `docker pull plexripper/plexripper:dev`    |

### Unraid

Note: [Unraid support thread](https://forums.unraid.net/topic/114103-support-plexripper-the-best-cross-platform-plex-media-downloader-there-is/)

1. Ensure you have [Community Applications](https://unraid.net/community/apps?q=plexripper#r) installed.
2. Search for PlexRipper and click install.
3. A Docker Template will be shown, fill this in with the correct volume mounts
4. Create it and then visit [http://localhost:7000/setup](http://localhost:7000/setup)
   - Port 7000 is the default port

## Front-end

- [Vue.js](https://vuejs.org/)
- [Nuxt.js](https://nuxtjs.org/)
- [Typescript](https://www.typescriptlang.org/)
- [Rx.js](https://rxjs.dev/)
- [Vuetify](https://vuetifyjs.com/en/)

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

## Translate PlexRipper

Make PlexRipper easier to use for everyone by translating it to your native language.

1. Check the Github issues if someone else is already translating to your language.

2. Create a Github issue explaining the language you would like to translate to.
   - This is to avoid multiple people translating to the same language

3. Make a copy of [en-US.json](https://github.com/PlexRipper/PlexRipper/tree/master/src/WebAPI/ClientApp/src/lang) into the "lang" folder and rename it to your language. e.g. "fr.json, es.json".

4. Translate only the text after the ":"!

5. When finished, either create a pull request or send it to [plexripper@protonmail.com](mailto:plexripper@protonmail.com?subject=[PlexRipper%20Translation])

## Similar Projects

- [plexmedia-downloader](https://github.com/codedninja/plexmedia-downloader)

- [Saverr](https://github.com/ninthwalker/saverr)

- [PlexDL](https://github.com/BRH-Media/PlexDL)

## Acknowledgements

An absolute massive thank you to the following people!

- [Starnakin](https://github.com/starnakin) for his awesome work on translating PlexRipper to French!
- 
- [Padso4tw](https://github.com/padso4tw) for his awesome work on translating PlexRipper to German!

- [Ninthwalker](https://github.com/ninthwalker) for his great work on [Saverr](https://github.com/ninthwalker/saverr), which showed me the method of downloading media from Plex!

- [Michael Altmann](https://github.com/altmann) for his awesome work on [Fluent Results](https://github.com/altmann/FluentResults) and the time he spent helping with making Fluent Results work in PlexRipper!

- [Dan Wahlin](https://github.com/DanWahlin) for his awesome work on [Observable-Store](https://github.com/DanWahlin/Observable-Store) and the time he spent helping me out with my questions!

- [Lenty Sprangers](https://github.com/LentySprangers) for the logo design!

- [Jetbrains](https://www.jetbrains.com/) for their awesome development tools, in paticular [Rider](https://www.jetbrains.com/rider/) and [Webstorm](https://www.jetbrains.com/webstorm/)!

- The developers and contributers behind the amazing [Vue.js](https://vuejs.org/), [Vuetify](https://vuetifyjs.com/en/), [Nuxt.js](https://nuxtjs.org/) and [RxJS](https://www.learnrxjs.io/)!

- The developers and contributers behind the beautiful [Vanta.js](https://www.vantajs.com/) project that is used to show some of the awesome backgrounds in PlexRipper!

- Several Reddit users over in [/r/Plex](https://www.reddit.com/r/PleX/) for being a pain in the ass and motivating me to create PlexRipper.

## JetBrains

Huge thank you to [<img src="./export/jetbrains/jetbrains.svg" alt="JetBrains" width="32"> JetBrains](http://www.jetbrains.com/) for supporting open source projects and providing us with free licenses of their great tools!

- [<img src="./export/jetbrains/resharper.svg" alt="ReSharper" width="32"> ReSharper](http://www.jetbrains.com/resharper/)
- [<img src="./export/jetbrains/webstorm.svg" alt="WebStorm" width="32"> WebStorm](http://www.jetbrains.com/webstorm/)
- [<img src="./export/jetbrains/rider.svg" alt="Rider" width="32"> Rider](http://www.jetbrains.com/rider/)

- [<img src="./export/jetbrains/dottrace.svg" alt="dotTrace" width="32"> dotTrace](http://www.jetbrains.com/dottrace/)
