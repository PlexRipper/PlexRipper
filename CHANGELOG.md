PlexRipper Changelog

# [0.14.0-dev.1](https://github.com/PlexRipper/PlexRipper/compare/v0.13.0...v0.14.0-dev.1) (2023-06-22)


### Bug Fixes

* **Translations:** Added translations for the media selection dialog ([8f0c808](https://github.com/PlexRipper/PlexRipper/commit/8f0c8080038669365c3753ab384000687adbb6a6))
* **Translations:** Added translations for the Plex Account selector in the top right corner ([3aca494](https://github.com/PlexRipper/PlexRipper/commit/3aca49467c527df938ee8b9a627e944d59e9a151))
* **Web-UI:** Ensured the media selection range cannot exceed the other value, prevents min > max and max < min ([0776baf](https://github.com/PlexRipper/PlexRipper/commit/0776bafe00c049f761d9cdf80405f226073b98b0))
* **Web-UI:** Fixed empty help dialog appearing for options that have no text available ([8aeb3ef](https://github.com/PlexRipper/PlexRipper/commit/8aeb3efff460867b2b8035d261ed6db4fcb6821b))
* **Web-UI:** Fixed missing translations for the account selector in the top right corner ([943f12f](https://github.com/PlexRipper/PlexRipper/commit/943f12ff6444c7024302dbff5af6a10c9e3f61e1))
* **Web-UI:** Fixed the download commands not working when download confirmations have been disabled ([8cbd736](https://github.com/PlexRipper/PlexRipper/commit/8cbd736253358b4b29a9f3cf009d9ab8caf6543e))
* **Web-UI:** Fixed the DownloadSpeedLimit not displaying the correct value when changing in the server dialog ([e4a059f](https://github.com/PlexRipper/PlexRipper/commit/e4a059f02a40ede3ed42fc5f58763e67398fa080))
* **Web-UI:** Fixed the index shown in the MediaTable not being the same as the one coming from the back-end ([2100fef](https://github.com/PlexRipper/PlexRipper/commit/2100fef77d605d002c59dc7f69c9d4404a6eacff))
* **WebAPI:** Fixed the media data not being properly sorted, this will now use Natural sort by using the title instead of the sortTitle ([603d1fe](https://github.com/PlexRipper/PlexRipper/commit/603d1fe209283ab27161c56cbcb30432caecb602))
* **Web-UI:** Fixed the media table sort requiring two clicks to begin sorting ([e024617](https://github.com/PlexRipper/PlexRipper/commit/e024617babb4c47f3cee625fe082e2622365c05c))
* **Web-UI:** Fixed the MediaTable sorting not correctly using the same sorting as the back-end ([27613f1](https://github.com/PlexRipper/PlexRipper/commit/27613f1fdc055b744595843767aa791ae5806eaa))
* **WebAPI:** Fixed the PlexServerConnections connection status not being retrieved when requesting the PlexServers ([53b236e](https://github.com/PlexRipper/PlexRipper/commit/53b236ecd6b6f97634bc352ca2151d71b7b7f46d))
* **Web-UI:** Fixed the PlexStatus icon displayed next to the server names not being updated when it changes ([29b4b49](https://github.com/PlexRipper/PlexRipper/commit/29b4b49f75725937a7a4f268f4f1153457eb2afa))
* **Web-UI:** Fixed the Poster / Table dropdown selector not working due to Pinia migration ([c308986](https://github.com/PlexRipper/PlexRipper/commit/c308986e629493ccf0c2c7b8c84d84ea747eea1d))
* **Web-UI:** Fixed the service layer initializing too early when for example Pinia is not ready yet, this is now dependent on a Nuxt hook ([289de1d](https://github.com/PlexRipper/PlexRipper/commit/289de1de57c92c48efb4eee1c385aca191c41d68))
* **Web-UI:** Fixed the settings being updated on page load when nothing could have changed ([7c44fd5](https://github.com/PlexRipper/PlexRipper/commit/7c44fd5d9d3b0abc68751de6877bf9b59cb30df6))


### Features

* **Web-UI:** Added a selection button to the MediaTable to make it way easier to make media selections ([0323cbf](https://github.com/PlexRipper/PlexRipper/commit/0323cbf289f91277bc77fcd55f147bf845aa35a2))


### Performance Improvements

* **Web-UI:** Replaced the settings system with Pinia instead of an observable store, this should improve performance ([a628731](https://github.com/PlexRipper/PlexRipper/commit/a62873196bd51b019ef2ee3310c02ec0f699b30c))
* **Web-UI:** Settings are now updated once every second when something changes, this debounce effect prevents DDOS'ing the back-end ([a8116d3](https://github.com/PlexRipper/PlexRipper/commit/a8116d3ee533c41167c55f53efe298ca78965d32))

# [0.13.0](https://github.com/PlexRipper/PlexRipper/compare/v0.12.0...v0.13.0) (2023-06-13)


### Bug Fixes

* **WebAPI:** Fixed Plex server HTTPS connections not working due to missing user-agent ([8f9a48a](https://github.com/PlexRipper/PlexRipper/commit/8f9a48a1376a09e2ba897cc4847d85e900bfe3af))
* **GithubActions:** Fixed the caching of back-end nuget packages by specificing the package.lock.json path ([d5e67b3](https://github.com/PlexRipper/PlexRipper/commit/d5e67b3d91a8fc4af173dfc23bc3b91d8e932373))
* **WebAPI:** Fixed the CreatedAt and LastSeen values being incorrect when displaying the Server Dialog ([4c0bd15](https://github.com/PlexRipper/PlexRipper/commit/4c0bd159a608815d3f39bce05bcaaa3224a3d07a))
* **WebAPI:** Fixed the download confirmation dialog displaying a checkbox which is not needed ([8c894cb](https://github.com/PlexRipper/PlexRipper/commit/8c894cb9cb9a0da869e529b73303d7360c889ce2))
* **WebAPI:** Fixed the downloading failing when connecting with an HTTPS connection ([2ed6c48](https://github.com/PlexRipper/PlexRipper/commit/2ed6c48b817d68d61d3601265144a56b0e14a586))
* **Web-UI:** Fixed the empty Server Dialog -> Download Destinations page when no libraries are available by displaying an error text ([c527d6b](https://github.com/PlexRipper/PlexRipper/commit/c527d6bc5d8fee9987f19fda1b6adc7ae7d5afa9))
* **WebAPI:** Fixed the logging being formatted in a bloated redundant way, it should now be more compact ([9bc1b8d](https://github.com/PlexRipper/PlexRipper/commit/9bc1b8d651337f70addbd57a1625cf04d323779e))
* **WebAPI:** Fixed the UrlMaskingOperator being matched on everything ([e5a5579](https://github.com/PlexRipper/PlexRipper/commit/e5a5579b3b44af213045954000b6a28ea5194b32))
* **Web-UI:** Fixed the width being too large of the download details dialog ([3d87b7a](https://github.com/PlexRipper/PlexRipper/commit/3d87b7a7d8c8d7ff4d51b5742e5d30549ff8db01))
* **GithubActions:** Specified the solution to build ([0f62a44](https://github.com/PlexRipper/PlexRipper/commit/0f62a44ccd9ef4a53ff7a205e9a1b84c5ffb839f))
* **GithubActions:** Specified the solution to restore ([43a188b](https://github.com/PlexRipper/PlexRipper/commit/43a188b9f4b1584eea62a0a228e6aa95603ae2b1))


### Features

* **WebAPI:** All sensitive data that is logged is now masked, which should make sharing the logs much safer. ([de96ee3](https://github.com/PlexRipper/PlexRipper/commit/de96ee3d7036325e5c15085d98b29cc2d3e57dec))
* **Web-UI:** Improved the layout of the server data displayed in the Server settings dialog ([b456ed0](https://github.com/PlexRipper/PlexRipper/commit/b456ed06cf530de69fc19d1a3e85de25a4478806))


### Performance Improvements

* **Docker:** Added --no-restore to the build step and used --locked-mode to force the use of the NuGet package.lock.json ([fb1a63f](https://github.com/PlexRipper/PlexRipper/commit/fb1a63f80754a0665302f949936e4e0aa0269a4c))
* **GithubActions:** Possible optimization to share the build step for the back-end testing jobs ([6e38832](https://github.com/PlexRipper/PlexRipper/commit/6e38832a228e4516808a16082cb55fc0e5354e94))


### Reverts

* **GithubActions:** Revert attempt to split the build step for back-end testing ([77e4ec8](https://github.com/PlexRipper/PlexRipper/commit/77e4ec874e1745aa49adf1c8c303ba703acb97b4))

# [0.13.0-dev.1](https://github.com/PlexRipper/PlexRipper/compare/v0.12.0...v0.13.0-dev.1) (2023-06-13)


### Bug Fixes

* **WebAPI:** Fixed Plex server HTTPS connections not working due to missing user-agent ([8f9a48a](https://github.com/PlexRipper/PlexRipper/commit/8f9a48a1376a09e2ba897cc4847d85e900bfe3af))
* **GithubActions:** Fixed the caching of back-end nuget packages by specificing the package.lock.json path ([d5e67b3](https://github.com/PlexRipper/PlexRipper/commit/d5e67b3d91a8fc4af173dfc23bc3b91d8e932373))
* **WebAPI:** Fixed the CreatedAt and LastSeen values being incorrect when displaying the Server Dialog ([4c0bd15](https://github.com/PlexRipper/PlexRipper/commit/4c0bd159a608815d3f39bce05bcaaa3224a3d07a))
* **WebAPI:** Fixed the download confirmation dialog displaying a checkbox which is not needed ([8c894cb](https://github.com/PlexRipper/PlexRipper/commit/8c894cb9cb9a0da869e529b73303d7360c889ce2))
* **WebAPI:** Fixed the downloading failing when connecting with an HTTPS connection ([2ed6c48](https://github.com/PlexRipper/PlexRipper/commit/2ed6c48b817d68d61d3601265144a56b0e14a586))
* **Web-UI:** Fixed the empty Server Dialog -> Download Destinations page when no libraries are available by displaying an error text ([c527d6b](https://github.com/PlexRipper/PlexRipper/commit/c527d6bc5d8fee9987f19fda1b6adc7ae7d5afa9))
* **WebAPI:** Fixed the logging being formatted in a bloated redundant way, it should now be more compact ([9bc1b8d](https://github.com/PlexRipper/PlexRipper/commit/9bc1b8d651337f70addbd57a1625cf04d323779e))
* **WebAPI:** Fixed the UrlMaskingOperator being matched on everything ([e5a5579](https://github.com/PlexRipper/PlexRipper/commit/e5a5579b3b44af213045954000b6a28ea5194b32))
* **Web-UI:** Fixed the width being too large of the download details dialog ([3d87b7a](https://github.com/PlexRipper/PlexRipper/commit/3d87b7a7d8c8d7ff4d51b5742e5d30549ff8db01))
* **GithubActions:** Specified the solution to build ([0f62a44](https://github.com/PlexRipper/PlexRipper/commit/0f62a44ccd9ef4a53ff7a205e9a1b84c5ffb839f))
* **GithubActions:** Specified the solution to restore ([43a188b](https://github.com/PlexRipper/PlexRipper/commit/43a188b9f4b1584eea62a0a228e6aa95603ae2b1))


### Features

* **WebAPI:** All sensitive data that is logged is now masked, which should make sharing the logs much safer. ([de96ee3](https://github.com/PlexRipper/PlexRipper/commit/de96ee3d7036325e5c15085d98b29cc2d3e57dec))
* **Web-UI:** Improved the layout of the server data displayed in the Server settings dialog ([b456ed0](https://github.com/PlexRipper/PlexRipper/commit/b456ed06cf530de69fc19d1a3e85de25a4478806))


### Performance Improvements

* **Docker:** Added --no-restore to the build step and used --locked-mode to force the use of the NuGet package.lock.json ([fb1a63f](https://github.com/PlexRipper/PlexRipper/commit/fb1a63f80754a0665302f949936e4e0aa0269a4c))
* **GithubActions:** Possible optimization to share the build step for the back-end testing jobs ([6e38832](https://github.com/PlexRipper/PlexRipper/commit/6e38832a228e4516808a16082cb55fc0e5354e94))


### Reverts

* **GithubActions:** Revert attempt to split the build step for back-end testing ([77e4ec8](https://github.com/PlexRipper/PlexRipper/commit/77e4ec874e1745aa49adf1c8c303ba703acb97b4))

# [0.12.0](https://github.com/PlexRipper/PlexRipper/compare/v0.11.1...v0.12.0) (2023-05-29)


### Bug Fixes

* **Database:** Fixed a null exception when a library with no media is returned ([970473e](https://github.com/PlexRipper/PlexRipper/commit/970473e490687c4cc7b9b3fb35716fb63d8b3e8f))
* **Web-UI:** Fixed the single column media posters jumping to the correct size when navigating libraries ([9c6cad9](https://github.com/PlexRipper/PlexRipper/commit/9c6cad9fb0a14328e7d5e0bd128849110624b57d))
* **Web-UI:** Fixed the verification dialog not appearing when 2FA is enabled ([8150978](https://github.com/PlexRipper/PlexRipper/commit/815097817684ab7141b8657ce85d9c03331d15b6))


### Features

* **Web-UI:** A home icon is displayed next to the server list when that server is owned by a Plex account ([fe8ed71](https://github.com/PlexRipper/PlexRipper/commit/fe8ed71bcdab45f26323973fd33b37c49ea76726))
* **Web-UI:** The Plex servers on the left side are now ordered from owned to then sorted by name ([dec9bb0](https://github.com/PlexRipper/PlexRipper/commit/dec9bb0646e3ded70c03b6f96e1d2a2b95f8f005))

# [0.12.0-dev.1](https://github.com/PlexRipper/PlexRipper/compare/v0.11.1...v0.12.0-dev.1) (2023-05-29)


### Bug Fixes

* **Database:** Fixed a null exception when a library with no media is returned ([970473e](https://github.com/PlexRipper/PlexRipper/commit/970473e490687c4cc7b9b3fb35716fb63d8b3e8f))
* **Web-UI:** Fixed the single column media posters jumping to the correct size when navigating libraries ([9c6cad9](https://github.com/PlexRipper/PlexRipper/commit/9c6cad9fb0a14328e7d5e0bd128849110624b57d))
* **Web-UI:** Fixed the verification dialog not appearing when 2FA is enabled ([8150978](https://github.com/PlexRipper/PlexRipper/commit/815097817684ab7141b8657ce85d9c03331d15b6))


### Features

* **Web-UI:** A home icon is displayed next to the server list when that server is owned by a Plex account ([fe8ed71](https://github.com/PlexRipper/PlexRipper/commit/fe8ed71bcdab45f26323973fd33b37c49ea76726))
* **Web-UI:** The Plex servers on the left side are now ordered from owned to then sorted by name ([dec9bb0](https://github.com/PlexRipper/PlexRipper/commit/dec9bb0646e3ded70c03b6f96e1d2a2b95f8f005))

## [0.11.1](https://github.com/PlexRipper/PlexRipper/compare/v0.11.0...v0.11.1) (2023-05-28)


### Bug Fixes

* **Web-UI:** Fixed the boot loop logo due to wrong url used when opening the web ui ([768e08b](https://github.com/PlexRipper/PlexRipper/commit/768e08bab1867c1b8b514e9dfd0242852c32b533))

# [0.11.0](https://github.com/PlexRipper/PlexRipper/compare/v0.10.0...v0.11.0) (2023-05-27)


### Bug Fixes

* **Web-UI:** Cleaned up and fixed the 2FA process of creating a PlexAccount ([0332b8e](https://github.com/PlexRipper/PlexRipper/commit/0332b8e92b6239eabbd6d60095330a65c4f2f011))
* **Web-UI:** CSS .glass-background now changes color based on dark/light mode ([29f0c1f](https://github.com/PlexRipper/PlexRipper/commit/29f0c1f563e3ac6e6c98c4dd449855697c89efc7))
* **Web-UI:** Display the default image when failing to request a thumbnail in the PosterView of media ([5edad67](https://github.com/PlexRipper/PlexRipper/commit/5edad67a3bcd96c04201b31d9f1e34f4e10a74da))
* **Web-UI:** Ensured the plugins are only running on client when SSR: true ([23939a6](https://github.com/PlexRipper/PlexRipper/commit/23939a693d54bf0b926168b98258b1371b226ff6))
* **Web-UI:** Fixed alignment of the setup page tab contents ([ef51256](https://github.com/PlexRipper/PlexRipper/commit/ef51256f2bb109186c4ae2f43fa4f1bf7ef0e3f5))
* **Web-UI:** Fixed the AccountDialog not closing after an account has been deleted ([6411108](https://github.com/PlexRipper/PlexRipper/commit/6411108aa1434d04494f85ade68f5f0d2208d2de))
* **Web-UI:** Fixed the Alphabet navigation for the Poster view of the media collection ([13ca76c](https://github.com/PlexRipper/PlexRipper/commit/13ca76c72c633fe6d7b303479468c29d23112dec))
* **Web-UI:** Fixed the BaseButton not allowing the passing in of the default slot content ([4164634](https://github.com/PlexRipper/PlexRipper/commit/4164634f9184539d81387e181d0fc5bbf3c433db))
* **Web-UI:** Fixed the confirmation dialog not showing wen pressing the Reset Database button under settings => advanced ([1dcf2df](https://github.com/PlexRipper/PlexRipper/commit/1dcf2df66e766ede25ea52032d46de28fa2cd5fd))
* **Web-UI:** Fixed the connections not being displayed when checking the server connections ([232686a](https://github.com/PlexRipper/PlexRipper/commit/232686a572e3de7889789f0f5fa80a66cf6ab6b8))
* **Web-UI:** Fixed the DetailsOverview navigating and loading correctly ([9b6c13c](https://github.com/PlexRipper/PlexRipper/commit/9b6c13c25cf5494d2e5fe78f5d63bee3c1eedc65))
* **Web-UI:** Fixed the display of refreshing the library data ([1a4a56c](https://github.com/PlexRipper/PlexRipper/commit/1a4a56c8d9d676d846a474166276a986b70cdcbc))
* **Web-UI:** Fixed the download table action buttons wraping due to not enough space ([0f25689](https://github.com/PlexRipper/PlexRipper/commit/0f256896812de181f205087a277bf8159d9c2155))
* **Web-UI:** Fixed the highlight causing big margins around the elements that it's applies to ([af044a6](https://github.com/PlexRipper/PlexRipper/commit/af044a6344984a8592864257ed149bc4e823ad48))
* **Web-UI:** Fixed the hover and click not working on TVshow links to display the details page ([34eadc1](https://github.com/PlexRipper/PlexRipper/commit/34eadc13ec052a6c7f38d01b6f61a4f6acad922d))
* **Web-UI:** Fixed the inconsistent heights in relation to the content of the Dialogs based on QCardDialog ([95cc50b](https://github.com/PlexRipper/PlexRipper/commit/95cc50b9ee4e4a49f201b2665d16c8632567de23))
* **Web-UI:** Fixed the media details not displaying and the opening/closing of the DetailsOverview is now handled by eventbus. ([70a1535](https://github.com/PlexRipper/PlexRipper/commit/70a15359a050df61062ffc794eab2507367e4836))
* **Web-UI:** Fixed the MediaDetailsDialog css being applied to all dialog components ([9d565a9](https://github.com/PlexRipper/PlexRipper/commit/9d565a9930a5b62ce9ff3ce1d19d8dfdebdd5e68))
* **Web-UI:** Fixed the navigating between TvShow and TvShowDetail that it maintains the scroll of the MediaOverview ([afd8ebe](https://github.com/PlexRipper/PlexRipper/commit/afd8ebe9f932dcf757badd1ad6f5696c3e35d5b7))
* **Web-UI:** Fixed the PlexRipper logo margin in the AppBar at the top ([ea117d7](https://github.com/PlexRipper/PlexRipper/commit/ea117d7e962d3ae81514a0edcefb7683d5a59e67))
* **Web-UI:** Fixed the poster media view not handling huge 50.000+ media libraries ([2603092](https://github.com/PlexRipper/PlexRipper/commit/2603092c6322f934e9a8f3233a3bceb2053f7bf0))
* **Web-UI:** Fixed the prop persistent not being passed into q-dialog in the QCardDialog ([0fac6b0](https://github.com/PlexRipper/PlexRipper/commit/0fac6b0d5cd8b4f9d260e97bbfe14e9514a206c6))
* **Web-UI:** Fixed the repeated Library refresh request issue ([2defbed](https://github.com/PlexRipper/PlexRipper/commit/2defbedfcb1fc9b4dee7581d3edb79413535e1ca))
* **Web-UI:** Fixed the selection of rows and passing through the download commands ([50561d1](https://github.com/PlexRipper/PlexRipper/commit/50561d12a5bcadd8b7b491969e89c9383b7b08bf))
* **Web-UI:** Fixed the wrong color value passed in GoToButton on the home page ([7f74ea4](https://github.com/PlexRipper/PlexRipper/commit/7f74ea48a26dbf43cf57423c17dc8fd827d78cfd))
* **Web-UI:** Hidden the Database section under settings as the "Reset Database" button is not working in the short term ([cb39d06](https://github.com/PlexRipper/PlexRipper/commit/cb39d061e6081fb14616725dd92886936ab8f968))
* **Web-UI:** Improved the vertical letter navigation on the media overview page to align and stretch properly while making it a bit bigger ([7d59121](https://github.com/PlexRipper/PlexRipper/commit/7d59121fc97fb5806b6518a84bff5b24efc2a51b))
* **Web-UI:** include input,select and textarea when determining the text-color ([74d1da9](https://github.com/PlexRipper/PlexRipper/commit/74d1da99ebd4e5b694f3ece21667e4e34fbe4da5))
* **Web-UI:** Mock data now respects the seed setting and will auto-increment when multiple elements are generated ([c6ed02a](https://github.com/PlexRipper/PlexRipper/commit/c6ed02a2f1ab16692d7a95a4309f5113e54b1cff))
* **Web-UI:** On completion of checking all server connections in the dialog, the treeview will now collapse ([de3681c](https://github.com/PlexRipper/PlexRipper/commit/de3681cb27159e0200674a47f28ac730ecc33631))
* **Web-UI:** Set a max-width for the confirmation dialog to not take up 100% screen width ([d354bb6](https://github.com/PlexRipper/PlexRipper/commit/d354bb60c5b74a9b5fcdcde195c427a54eab73e4))
* **WebAPI:** The media quality formats are now added to the PlexMediaSlim object ([3b15052](https://github.com/PlexRipper/PlexRipper/commit/3b15052837701da225beba35bf2880b2bd9a299a))


### Features

* **WebAPI:** Added a "debugMode" settings property that will show/hide debugging functionality ([5b90812](https://github.com/PlexRipper/PlexRipper/commit/5b908121abbc44cbae9fff276994232c8489309f))
* **Web-UI:** Added a highlight animation when navigating with the Alphabet navigation on the media page ([cb7a27d](https://github.com/PlexRipper/PlexRipper/commit/cb7a27d8afd169072f68347dee14abd1494b5908))
* **Web-UI:** Added a highlight around the poster when navigating with the Alphabet navigation in the poster media view ([a9b4def](https://github.com/PlexRipper/PlexRipper/commit/a9b4def5a0d54b618ff8067a97406e99e4442e4c))
* **WebAPI:** Added a separate endpoint for getting the full TvShow media data ([9e1d7f9](https://github.com/PlexRipper/PlexRipper/commit/9e1d7f94f50fbd0d99342238557d3f01a021326c))
* **Web-UI:** Added debug section under advanced settings which has a toggle to enable/disable debug mode ([0b65743](https://github.com/PlexRipper/PlexRipper/commit/0b65743407977f19e11d6227d789cf3a479fb40e))
* **Web-UI:** Added factory to generate mock Plex media data ([2035a4c](https://github.com/PlexRipper/PlexRipper/commit/2035a4c6f48fbe9924b50c6a766bdca17b5b0887))
* **WebAPI:** Added index to the PlexMediaSlimDTO ([f32f093](https://github.com/PlexRipper/PlexRipper/commit/f32f0936f68ce336cf0c13807ceda8d17db4d246))
* **WebAPI:** Enabled XML-documentation display in the Nswag/Swagger UI ([225c453](https://github.com/PlexRipper/PlexRipper/commit/225c4531ae78a439be05fc51e16da269e7613355))
* **Web-UI:** The debug menu items are now only displayed when debugMode is enabled ([a8e4349](https://github.com/PlexRipper/PlexRipper/commit/a8e4349dbe807bf8f81c96abd0084d681eaf69e0))
* **Web-UI:** The Print component will now only be displayed when debugMode is enabled ([23654c8](https://github.com/PlexRipper/PlexRipper/commit/23654c85bcd857bdf4534ece39c3ad9dd66df5d6))
* **Web-UI:** The table overview when viewing the media in a library now works with virtual scrolling, meaning it can view huge libraries with ease ([b9beea4](https://github.com/PlexRipper/PlexRipper/commit/b9beea49c99267dd2253bea466e8e018021ceaba))


### Performance Improvements

* **Web-UI:** Calculating which letters to display in the Alphabet Navigation is now 90% faster ([45f116e](https://github.com/PlexRipper/PlexRipper/commit/45f116e12537a790700a21025140fe6036e4d302))
* **WebAPI:** Removed the allMedia option from the api endpoint /PlexLibrary/{id} ([736112a](https://github.com/PlexRipper/PlexRipper/commit/736112a351c88bce1fd0f538c1e8ae5403f53c22))
* **WebAPI:** The PlexMediaSlim now contains the thumbnail image url, this greatly speeds up the loading of the thumbnail ([3637711](https://github.com/PlexRipper/PlexRipper/commit/3637711cffcdb5dfe03fb4bcbcfb7fd63d2fcaa1))
* **WebAPI:** The TvShowDetail API endpoint now returns onl the data that is needs to significiantly increase performance when viewing Seasons ([4f0966e](https://github.com/PlexRipper/PlexRipper/commit/4f0966ed00f4c746e141b0c682e0739c2b1bd6bb))

## [0.11.0]

**Major migration of the front-end from Vue 2 / Nuxt 2 and Vuetify v2 to Vue 3 / Nuxt 3 and Quasar v2. Basically a complete rewrite of the front-end.**

### Added
- Added sorting to the library media page with the ability to sort by title, date added, date updated, and size.

### Changed
 - Major front-end migration to Vue 3 / Nuxt 3 and Quasar v2
 - Added a selection count to the library media page and media detail page.
 - Viewing a huge library media collection should be much smoother due to paginated loading and virtual scrolling.
 - Re-designed the PlexRipper setup wizard to flow better and be more intuitive.
 - The estimated time shown when downloading a media item is now shown in a shorter notation, 06:45:09.

### Optimizations
 - Optimized the media viewing to be more performant and use less memory.
 - Calculating the download preview, which is shown when a pop-up opens that asks if you're sure that you want to download the selected media, is now done in the background and not on the fly.
 -

### Fixed
 - Fixed the laggyness when scrolling on the media pages when selecting a plex library.
 - Fixed the checkbox selection not working in the media pages when selecting a plex library.
 - Fixed the seasons when viewing a tvShow not being sorted correctly.
 - Fixed the logging spam "Something failed but no errors were available in the result"
 - Fixed the DownloadDetail dialog not containing the DownloadUrl when a task can be downloaded.
 - Fixed the Downloads count that is displayed in the menu to the left displaying an incorrect number, this number is now based on the amount of downloadable media entries


## [0.10.0-RC1]

This is most, not all, of the many changes that have been made in this version.

### Added
 - Cypress Front-end testing

### Changed
 - Each PlexServer now has multiple connections available which can be chosen individually, this should solve proxy and connection issues with servers.
 - Each PlexServer can now have a preferred connection that can be set in the server settings.
 - Changed the infrastructure for the download and fileMerge process to be hosted in the Quartz Background services.
 - Big re-organization of the PlexAccount setup process.
 - Upgraded to .NET 7.0 and Entity Framework 7.0, as well as many other dependencies.

### Fixed
 - Fixed the two-factor authentication not working due to the Plex error not being passed on from the http client
 - Fixed the front-end page data not refreshing when deleting an PlexAccount
 - Fixed download process not working when the PlexServer is behind a proxy
 - Fixed the timeout issue when communicating with a big PlexServer, it's currently 60 seconds before it times out.
 - Fixed "cannot access the file" exceptions which were due to resources not being disposed correctly.

### Optimizations
 - Optimized the token retrieval process for Downloads, this should make downloads more resilient to token refreshes.
 - Optimized the download progress to the front-end, this should make the download progress more accurate and less laggy.
 - Optimized all the project to a vertical slice architecture, this should make the project more maintainable and easier to extend.
 - Optimized the logging to be more consistent and easier to read.

## [0.9.1]

### Added
 - Added a setup question on the home page instead of forcefully redirecting to the setup page when the setup hasn't been yet skipped or completed
### Changed
 - Made the updating of settings in the front-end a true observable to make it chain-able and await-able to only do stuff after it has updated the settings
### Fixed
 - Fixed the setup loop that happened for some users where they got redirected back to the setup screen after finishing or skipping by removing the redirect
 - Fixed the downloadSegments setting not updating correctly when changed

## [0.9.0]

This has been a major refactoring with many unit and integration tests added to ensure stability.
This means you will have to empty the config folder before installing to ensure proper workings.

Note: This, by a long shot, doesn't encompasses all the changes and fixes that have been made in this version due to the large scope of the changes.

### Added
 - Per server configurable download speed limit (See server settings > Server Configuration)
 - Per server and library configurable folder destination where media downloaded will automatically be moved too.
 - German UI language (Thanks to [Padso4tw](https://github.com/padso4tw)!)
 - Jest and Cypress testing infrastructure
 - Added a new server command in the server settings to inspect a server connection and attempt to fix it.
 - Added a loading screen with a rotating logo

### Changed
 - Removed batch commands from the download page, these were not working and overcomplicated things too much.
 - Migrated project to .NET 6, which brings many performance improvements
 - Added a loading icon to the button when checking the server status in the server configuration
 - The server command "Re-sync Library media" now displays a loading animation.
 - Replaced every button with a more performant and consistent button construction
 - Thumbnails displayed for movies and tvShows on the library pages in poster mode are now displayed from cache when navigating around PlexRipper.
 - Rewrote the code for the entire download process to be more stable and allow for future features
 - Rewrote the user settings modules (PlexRipperConfig.json) which is now easy to extend and more resilient.
 - Many performance improvements!

### Fixed
 - Fixed the setup of Plex accounts in PlexRipper which was failing due to a login format change on the side of Plex
 - Fixed the slow downloads by increasing the download buffer, might make it configurable based on the hardware PlexRipper is run on. (Thanks to [BakasuraRCE](https://github.com/BakasuraRCE)!)
 - Fixed the opening of the server settings not defaulting back to its first tab
 - Fixed the download progress not updating after a while due to SignalR disconnects
 - Fixed the retrieval of the ServerStatus not working when a timeout happens.
 - Fixed the notifications not always being shown and updated correctly when an error happens.
 - Fixed the download confirmation window not hiding after clicking confirm #122
 - Fixed the page background effect breaking when the browser does not support WebGL. It will now display a still image of the background.

## [0.8.7]

### Added

- Added two-factor authentication compatibility, meaning PlexRipper now works with two-factor authentication protected Plex accounts.
- Added the awesome French translation from @starnakin, thank you so much!
- Added a progress window showing the individual servers being connected to when setting up an PlexAccount.
- Added a warning that deleting an plexAccount might take a long time due to the amount of data which has to be deleted.
- Added a thank you for the awesome contribution from Starnakin to the README.
- Added migration check for adding ClientId to already created Plex accounts, this avoids users having to re-setup their accounts in PlexRipper.

### Changed

- When confirming an action in the confirmation dialog, a loading spinner will now show.
- When a translation is missing, it will now show the English variant.
- When no downloads have been selected in the download page, the "Clear Completed" is now disabled.
- Updated to the new PlexAPI SignIn process
- Improved the PlexApi HttpClient to return errors given by Plex.

### Fixed

- Fixed the "New device connected" spam that Plex server owners would get due to a randomly generated ClientId's being used per request by PlexRipper. This is now unique and consistent for every PlexAccount.
- Fixed the delete button missing from the Plex account update window.
- Fixed the confirmation window prevented from being closed.
- Fixed the menu titles now being translatable.
- Fixed the DateTime settings not being translatable.
- Fixed the browser client not updating its store after resetting the database.
- Fixed the error window having a very wide window due to unwrapped text.
- Fixed the TvShows libraries getting stuck in a infinite loading loop when viewed, TV show libraries can now be viewed again.

## [0.8.6]

### Added

- Added an improved Notification sidebar
- Added all text as translations keys to the language file to ensure everything can get translated.
- Added Language switcher under Settings => UI, only supported English and the to be translated French.

### Changed

- Notifications can now be cleared with a click of a button
- Fixed the lang folder link in the README
- Added the French language file and language support (Still needs translations).
- Added the language option to the config file
- Rewritten how settings are saved, they should now work much better
- Added feedback section to the ReadMe
- Ensured the config/settings file is now better protected against corruption and invalid values

### Fixed

- Fixed the Letter navigation in Poster view not scrolling the page.
- Fixed the setup page not shown automatically on a new install.
- Fixed the percentage in download speed having too many decimals, namely in the Downloads page.
- Fixed where empty help buttons would show up next to options.
- Fixed the checkboxes in settings not working.
- Fixed the misalignment of the checkboxes in the settings page.
- Fixed an issue where statically generating the WebUI in development would return the wrong api url.
- Fixed an error when trying to add a Movie or TvShow destination folder under Settings => Paths, should now work correctly
- Fixed the "no downloads" message not always showing correctly when there are no downloads in progress.
- Fixed the setup page not showing the correct background and throwing an error when that happens
- Fixed the skip button during the setup wizard not appearing
