PlexRipper Changelog

## [0.29.1](https://github.com/PlexRipper/PlexRipper/compare/v0.29.0...v0.29.1) (2025-01-19)


### Bug Fixes

* **WebAPI:** Fixed an issue where two downloads on the same server could happen, now an extra check is added to prevent this ([1b33455](https://github.com/PlexRipper/PlexRipper/commit/1b33455d634ca98d156d454d95fe3e83e93ce81b))
* **Web-UI:** Fixed the duration display with the correct zero padding ([5d7b258](https://github.com/PlexRipper/PlexRipper/commit/5d7b25891c2dd6f3011cc7b1b186421fa48c7e39))
* **WebAPI:** Fixed the issue again where download task selection would stall due to incorrect status determination ([f300a0f](https://github.com/PlexRipper/PlexRipper/commit/f300a0f4fded8c7b5e56db4a13a4016a729be7c2))
* **WebAPI:** possible fixed the download exceptions that can happen due to bad connections and now PlexRipper retrying that ([961b65e](https://github.com/PlexRipper/PlexRipper/commit/961b65e50ecf8a741a1e1aa53770f0899e528380))


### Performance Improvements

* **WebAPI:** micro improvement by removing autofac dependency assembly scanning in Application project ([0e708a6](https://github.com/PlexRipper/PlexRipper/commit/0e708a68e4c27c928a58c7a114181cee26078242))

# [0.29.0](https://github.com/PlexRipper/PlexRipper/compare/v0.28.0...v0.29.0) (2025-01-18)


### Bug Fixes

* **WebAPI:** abort boot sequence when config or database cannot be set ([adc7337](https://github.com/PlexRipper/PlexRipper/commit/adc73371d9fc388a4b3e423fd9955cc6cbefe2ac))
* **Web-UI:** added a page load overlay when logging in ([37ac772](https://github.com/PlexRipper/PlexRipper/commit/37ac772af6cab5bbb3d58b6d3e6fd9cce32f8c2f))
* **Web-UI:** agreeing to the disclaimer is now stored in the settings file ([99e8a43](https://github.com/PlexRipper/PlexRipper/commit/99e8a43588acafd849af6ebe76afa33b32438373))
* **Web-UI:** allow authentication status endpoint to be viewed anon ([6509b9c](https://github.com/PlexRipper/PlexRipper/commit/6509b9ca99b30cbd0aad9ec6036460ebc5d845dc))
* **Web-UI:** always hide horizontal scroll bar, 99% is always a few pixels ([355b681](https://github.com/PlexRipper/PlexRipper/commit/355b68199853fa3bf41430edb40a1b6cda3f03a3))
* **WebAPI:** authentication now is valid for 6 hours ([c593e11](https://github.com/PlexRipper/PlexRipper/commit/c593e1197134253df61408087cffb58d33dd6756))
* **Web-UI:** convert API exceptions due to error codes to ResultDTO ([59d90f7](https://github.com/PlexRipper/PlexRipper/commit/59d90f76fbc5660f45e37699749b336a5292f2e0))
* **Web-UI:** Finishing the setup process will not require a full page reload anymore, making the transition much smoother ([ae6874b](https://github.com/PlexRipper/PlexRipper/commit/ae6874b34814848031bd97bfae67b209c4a1c483))
* **WebAPI:** fix a dev release being made when only the readme has changed ([ac14418](https://github.com/PlexRipper/PlexRipper/commit/ac14418ef3bd8fe91e6d67d5210352fa9a1a7322))
* **Web-UI:** fix blank screen when setting up a new database ([3f53c43](https://github.com/PlexRipper/PlexRipper/commit/3f53c43e0b9c25d102efd11ae67262d3ce2e08cf))
* **WebAPI:** fix misconfigured authentication, everything working perfect now ([e993733](https://github.com/PlexRipper/PlexRipper/commit/e9937332d763a88bc1c36c5f6bea61637ac61a9e))
* **Web-UI:** fix the alphabet letters not working and displaying when scrolling through the media ([c27f111](https://github.com/PlexRipper/PlexRipper/commit/c27f11131ed2b67703a8620a757e6621825b8ace))
* **Web-UI:** fix the cypress configuration to run in CI/CD ([9aa4283](https://github.com/PlexRipper/PlexRipper/commit/9aa428325e032f0275188de8c9af7e19bcce34b2))
* **Web-UI:** fix the extra scrollbar that appears on the media page ([b92f06b](https://github.com/PlexRipper/PlexRipper/commit/b92f06b1153f843b2bb8e090bed804e59d90d4b0))
* **Web-UI:** Fixed a bug where a PlexToken needed a special character to register ([51ea3ef](https://github.com/PlexRipper/PlexRipper/commit/51ea3ef1a4ab6d2cca6c8fc5d52d090bdf9efc7a))
* **WebAPI:** Fixed a download status to download task phase conversion issue that could cause errors when picking the next download ([acb5b88](https://github.com/PlexRipper/PlexRipper/commit/acb5b885ba218bca1580993e4a5b252572ccaab8))
* **WebAPI:** Fixed an aggregate download status conversion issue that could cause errors when picking the next download and stalling ([2510fb5](https://github.com/PlexRipper/PlexRipper/commit/2510fb57728a6fef0468658cf4127b8e4ded293e))
* **Web-UI:** fixed an issue were some stores were not reset when logging out ([916dbad](https://github.com/PlexRipper/PlexRipper/commit/916dbadcb1190b10c813adca01acf07acd5f617c))
* **WebAPI:** Fixed an issue where not all episodes were linked to a season due to missing data, this is now resolved by using a different parameter to match ([38148ff](https://github.com/PlexRipper/PlexRipper/commit/38148ff2ff0a2e624408539d7c0e3846e6b38a2d))
* **WebAPI:** fixed an issue where the PlexAPI returning and 401 will log the user out of PlexRipper ([ae6befc](https://github.com/PlexRipper/PlexRipper/commit/ae6befc7209d51800314dae1d65bfc1a91371ba4))
* **Web-UI:** fixed javascript exception when there is no media available in the media overview ([ad6c7ad](https://github.com/PlexRipper/PlexRipper/commit/ad6c7ad0bfdbd7504e27e22c9bd7a9560bd03768))
* **Web-UI:** fixed missing translations for the account input fields validation errors ([ddac5cb](https://github.com/PlexRipper/PlexRipper/commit/ddac5cbf08e97a02b3214658ad2348b49c669111))
* **Web-UI:** Fixed missing translations for the download-confirmation destination options ([b9ff5c2](https://github.com/PlexRipper/PlexRipper/commit/b9ff5c2b6a5e374c718deb9ebcefb9d013ee014b))
* **Web-UI:** fixed missing translations for the password input field validation errors ([b1178be](https://github.com/PlexRipper/PlexRipper/commit/b1178bebe1d4714fa406bd46864fa69c61725075))
* **Web-UI:** fixed missing translations from the background activity toggle ([a2d914d](https://github.com/PlexRipper/PlexRipper/commit/a2d914d7163440d1ea53ad3c83cffd4570f32a61))
* **Web-UI:** fixed the download confirmation destination path not triggering the download when selecting an episode ([9a0d5fb](https://github.com/PlexRipper/PlexRipper/commit/9a0d5fb30163c203070c4a074ea3ccfa2b3582a2))
* **Web-UI:** fixed the layout of the custom download destinations in the download confirmation window ([20f4dba](https://github.com/PlexRipper/PlexRipper/commit/20f4dba8ff0285da5fe8a8af52ee948ffb57afe7))
* **Web-UI:** fixed the missing title and ico ([006d10b](https://github.com/PlexRipper/PlexRipper/commit/006d10bf38077ed2d94a056f0e916794341845fa))
* **Web-UI:** fixed the outlining of the media table when viewing a media library ([ed11014](https://github.com/PlexRipper/PlexRipper/commit/ed110142e047bbb76acc18a937a3d390820ef6d8))
* **Web-UI:** fixed the setup and discord popups not appearing again when already on the setup page ([a0f8d37](https://github.com/PlexRipper/PlexRipper/commit/a0f8d37fbfb897cec3d7b70193605adef097060c))
* **Web-UI:** fixed the static background not stretching when viewing on 4k size ([9bd68d7](https://github.com/PlexRipper/PlexRipper/commit/9bd68d701d16f4ba4efab90b0c7be3aebf6c2e2c))
* **WebAPI:** Fixed the verbose log strings in the Download task logging, now its more clear ([67eb960](https://github.com/PlexRipper/PlexRipper/commit/67eb9607fe17c51cdc7a204c54f449c2c5402221))
* **WebAPI:** fixed the web ui not being visible when not logged in ([ca451e3](https://github.com/PlexRipper/PlexRipper/commit/ca451e3a9a56fd334332dc9d095f81e55b5fa36c))
* **Web-UI:** further cleanup of layout ([a376117](https://github.com/PlexRipper/PlexRipper/commit/a376117fa491d378073b4d587a1650daf6a50691))
* **Web-UI:** ignore .idea folder with eslint ([b538bac](https://github.com/PlexRipper/PlexRipper/commit/b538baca91a3fd60874c1287f32e714bd877d58d))
* **Web-UI:** prevent the settings store from failing when resetting and then using it ([612df2a](https://github.com/PlexRipper/PlexRipper/commit/612df2aef03ead5463e39feaf7ca441316ef60f1))
* **WebAPI:** removed duplicate call ConfigureApplicationCookie ([8ac26cf](https://github.com/PlexRipper/PlexRipper/commit/8ac26cf936095923cf597d87cdbaeaf640123aa4))
* **WebAPI:** stop container when failing to create config file ([451d94c](https://github.com/PlexRipper/PlexRipper/commit/451d94c5a0325fe69043577b26fb0eba4c58bec2))


### Features

* **Web-UI:** Add rememberMe option to the login screen and show when the user is timed-out due to failed attempts ([de979ae](https://github.com/PlexRipper/PlexRipper/commit/de979aeb3def9e6f7d4a2fc5d352d068faf8c537))
* **Web-UI:** Added a login page to finally secure PlexRipper ([7ea5804](https://github.com/PlexRipper/PlexRipper/commit/7ea58048fcf5c7cc701ef67ad8cb196830457c84))
* **WebAPI:** added endpoints to update and get the PlexRipper app credentials ([064557a](https://github.com/PlexRipper/PlexRipper/commit/064557aa521ba031c51d0cd8b5e9694f262f0120))
* **Web-UI:** added forgot password link to the login window ([9365296](https://github.com/PlexRipper/PlexRipper/commit/9365296179183477d12d692adeb622b71e983ded))
* **Web-UI:** added password strength to the Authentication section ([b70885b](https://github.com/PlexRipper/PlexRipper/commit/b70885b1e91ee70191a5b49e31b66baea8e8fe07))
* **WebAPI:** Added remember me to keep the being logged in when the browser is restarted ([12009aa](https://github.com/PlexRipper/PlexRipper/commit/12009aa1ce71f487ce07c7755828058999225894))
* **WebAPI:** dev releases not have their release notes generated, making it easier to see what is fixed  a31c5e ([6e639ff](https://github.com/PlexRipper/PlexRipper/commit/6e639ff1166cf00832711c17851fd550ad0b4b06))
* **WebAPI:** dev releases not have their release notes generated, making it easier to see what is fixed ([8199e4d](https://github.com/PlexRipper/PlexRipper/commit/8199e4de9b9c6ef442261be2f0bf4381f6c54f02))
* **Web-UI:** Greatly improved the layout of the settings and made it more screen size responsive, it's almost the same as to how Sonarr/Radarr does it, and it looks much better now ([8c7d03f](https://github.com/PlexRipper/PlexRipper/commit/8c7d03f3160c18f6e2ea7f01fed1a111efffd290))
* **Web-UI:** Removed the option to skip the setup process, now PlexRipper will automatically redirect to the setup page ([132ab0a](https://github.com/PlexRipper/PlexRipper/commit/132ab0a5a4b040e895cfe500bc9ecd2afe83ed38))
* **Web-UI:** removed the slider from the download speed limit as it was just annoying to use ([48a91e8](https://github.com/PlexRipper/PlexRipper/commit/48a91e8a3cf696d93cb57444555501cf5da38b76))
* **Web-UI:** revamped the setup process by adding a disclaimer and a way to change the default login credentials ([54eb361](https://github.com/PlexRipper/PlexRipper/commit/54eb361b962c781dcbc065ca9d6da8b4b8411114))
* **WebAPI:** The PlexRipper app user credentials can now be reset by setting the ResetCredentials to true in the settings file ([c4a2d6c](https://github.com/PlexRipper/PlexRipper/commit/c4a2d6c9fd96774568b0b1f9ab779651c1390a79))
* **Web-UI:** The PlexRipper login username and password can now be changed ([9d47d45](https://github.com/PlexRipper/PlexRipper/commit/9d47d4594b4a2e485a0bddda515b27c4c7b0c79b))
* **Web-UI:** turned the account selector button in the appbar to the same style as the other buttons ([328156f](https://github.com/PlexRipper/PlexRipper/commit/328156fbfa9511a414bddccfe490313418ba33b8))


### Reverts

* **Web-UI:** Revert upgrading swagger-typescript-api and Typescript ([eef29de](https://github.com/PlexRipper/PlexRipper/commit/eef29de2b4edcfe28f5abf8830854430b0afe853))

# [0.28.0](https://github.com/PlexRipper/PlexRipper/compare/v0.27.0...v0.28.0) (2024-12-22)


### Features

* **Web-UI:** In the DownloadConfirmation window, you can now select the destination for the media you're about to download ([35d3411](https://github.com/PlexRipper/PlexRipper/commit/35d3411915433ea89aabbf6fd23186695393288e))

# [0.27.0](https://github.com/PlexRipper/PlexRipper/compare/v0.26.0...v0.27.0) (2024-12-19)


### Bug Fixes

* **Web-UI:** Added a bit of padding around elements in the HelpRows ([22f7977](https://github.com/PlexRipper/PlexRipper/commit/22f7977e64b24a11d6acdc817e1db26024fee5e3))
* **Web-UI:** Added a more reliable way of displaying the library metadata by going of the media api call instead of the library call ([76a4194](https://github.com/PlexRipper/PlexRipper/commit/76a41946771f46406fdda294e81b9f59224f4dee))
* **WebAPI:** Added missing new download status to the aggregation method ([4d5cccf](https://github.com/PlexRipper/PlexRipper/commit/4d5cccf06b3aa38b372f81eb30676c746da000d0))
* **WebAPI:** Allow more retries when downloading a file before erroring out ([3e67bc4](https://github.com/PlexRipper/PlexRipper/commit/3e67bc4ce0dd15259fbcdb33d755177cb8fe719b))
* **WebAPI:** Allow TaskCanceledException for FileMergeJob when pausing the job ([b5ac742](https://github.com/PlexRipper/PlexRipper/commit/b5ac742265dc4bb6ab511ed73b5e4106450f5831))
* **WebAPI:** Allow X-PlexRipper-Version header in the CORS ([b69fc95](https://github.com/PlexRipper/PlexRipper/commit/b69fc95485c2e25d58dc1d350727914c3e2f2140))
* **WebAPI:** Calculate progress when manually updating the download task status during testing ([0cc8fca](https://github.com/PlexRipper/PlexRipper/commit/0cc8fcac825bd12d6278cfff303e1275c8d9284d))
* **Web-UI:** Clicking the discord button will now open the discord invite dialog and not the invite link directly ([56ae561](https://github.com/PlexRipper/PlexRipper/commit/56ae561a224dce2231a83ea9e538c5ab1d7a2d52))
* **WebAPI:** Display the dev version in the boot log correctly ([f275e1d](https://github.com/PlexRipper/PlexRipper/commit/f275e1dd7c209981863a30f3c927136a4e49706c))
* **WebAPI:** Fixed 0% percentage when downloadtask is completed ([9ba99de](https://github.com/PlexRipper/PlexRipper/commit/9ba99de686d576203401279fec38afa7979262e5))
* **WebAPI:** Fixed incorrect error message hiding the exception when a download is being retried ([c6d9840](https://github.com/PlexRipper/PlexRipper/commit/c6d9840aed87c931a78e50d495a695908e013ed9))
* **Web-UI:** Fixed incorrect server sorting in the server drawer, it's now all owned servers and then alphabetically ([ebbbbe2](https://github.com/PlexRipper/PlexRipper/commit/ebbbbe2a944fa0f990ed51630020e4e884f2f07c))
* **WebAPI:** Fixed merge/move progress miscalculating the transfer speed  and therefore the time remaining ([ade2109](https://github.com/PlexRipper/PlexRipper/commit/ade210969fe17d776c923492984516aa9841e967))
* **WebAPI:** Fixed missing mergePaused and movePaused status from DownloadTask parent tree status calculation ([41325e8](https://github.com/PlexRipper/PlexRipper/commit/41325e8d08d661b72474ba82275b555921ac7d79))
* **Web-UI:** Fixed Plex Accounts not refreshing when 1 is deleted ([f76b1c5](https://github.com/PlexRipper/PlexRipper/commit/f76b1c5184f17d8f747cda30919c1274e2ac46bb))
* **Web-UI:** Fixed the download button appearing without anything selected to download ([5e5dca1](https://github.com/PlexRipper/PlexRipper/commit/5e5dca1315b1999378f558966b400b57c48d4667))
* **WebAPI:** Fixed the DownloadWorkerTasks not being cleaned up after the download task is done ([c7c4044](https://github.com/PlexRipper/PlexRipper/commit/c7c4044ff06d1c7b583e10f142ed236a2e0230f5))
* **WebAPI:** Fixed the incorrect time remaining on the TvShow level, now it displays it based on the speed of the download and the remaining total data to be downloaded ([f1758f4](https://github.com/PlexRipper/PlexRipper/commit/f1758f485c9a4e917f29f60831a131f3d809ed07))
* **WebAPI:** Fixed tvshow folder being left behind after it has been moved/merged from the download folder ([21a9d32](https://github.com/PlexRipper/PlexRipper/commit/21a9d32a2acbdd03796b7eb3a9dac2482b759d6f))
* **Web-UI:** fixed typecheck and eslint errors ([5397b0d](https://github.com/PlexRipper/PlexRipper/commit/5397b0d91019c0011342ebaf8287a28895d35b09))
* **WebAPI:** Generate Test Database first before setting up Autofac container ([243dd0b](https://github.com/PlexRipper/PlexRipper/commit/243dd0b18a587fbf1e8cd6a5450b59bd2688c691))
* **WebAPI:** possible fix for inaccurate download speed calculation ([42ef74a](https://github.com/PlexRipper/PlexRipper/commit/42ef74ab0466e8d79663975fa46d603377fd02d2))
* **WebAPI:** Remove unneeded update of updating the DownloadWorkerTasks when merging files ([c9246a2](https://github.com/PlexRipper/PlexRipper/commit/c9246a29bb1e7cdb708561fe432a357931d2afb5))


### Features

* **Web-UI:** Added a new background activity dialog when the Plex library media is syncing ([c5269bd](https://github.com/PlexRipper/PlexRipper/commit/c5269bda84b38630759c7f99a2acbc83d4977e2b))
* **Web-UI:** Display a different version in the WebUI when using the development build ([9d50ab2](https://github.com/PlexRipper/PlexRipper/commit/9d50ab214afb5ae6f11e739b9684e3774ec4976d))
* **WebAPI:** Shutting down the docker container will pause the active downloads ([2e8a788](https://github.com/PlexRipper/PlexRipper/commit/2e8a78813924fadee8bea025e62778b32a136c4b))


### Performance Improvements

* **Web-UI:** fixed a memory leak that was creating way to many routes for Cypress to work ([3c725c8](https://github.com/PlexRipper/PlexRipper/commit/3c725c85671b2915ed6bc41b3db667ed69e8a3de))


### Reverts

* **Web-UI:** revert setting transparent background to !important as it messes up other coloring ([8c8c599](https://github.com/PlexRipper/PlexRipper/commit/8c8c5997069e9fae437b88f2be6dbd7afe9261f5))

# [0.26.0](https://github.com/PlexRipper/PlexRipper/compare/v0.25.0...v0.26.0) (2024-11-26)


### Bug Fixes

* **Web-UI:** Fixed download selection not triggering when selecting a tv show episode ([d726e50](https://github.com/PlexRipper/PlexRipper/commit/d726e5035457a0d123c442e238f8b61966056553))
* **Web-UI:** Fixed missing scrollbar when viewing many download tasks on the download page ([7afecc4](https://github.com/PlexRipper/PlexRipper/commit/7afecc41affbf5cbccfe78cbe2c5a814c2b97334))
* **Web-UI:** Fixed the media selection dialog not closing after setting the selection ([746d700](https://github.com/PlexRipper/PlexRipper/commit/746d700f00984b844de3aa6b7c11a9c007c4177d))


### Features

* **Web-UI:** Added discord invite dialog which is shown once ([73a9ab1](https://github.com/PlexRipper/PlexRipper/commit/73a9ab1f04cfa8fbf1cce6687a6bd141293585ee))

# [0.25.0](https://github.com/PlexRipper/PlexRipper/compare/v0.24.0...v0.25.0) (2024-11-21)


### Bug Fixes

* **Web-UI:** Added missing text when there is no background activity ([b9ec0e6](https://github.com/PlexRipper/PlexRipper/commit/b9ec0e678e059d88f1113174ff227932099338ac))
* **Web-UI:** cleaned up the styling of the download speed limit slider and input ([4e2da9e](https://github.com/PlexRipper/PlexRipper/commit/4e2da9e08b23467620f90854bd3f19045fd6a907))
* **WebAPI:** Custom connection that are added will not be deleted when the server is retrieving connections from the PlexAPI ([785d6c6](https://github.com/PlexRipper/PlexRipper/commit/785d6c66d4ec39bd7bf56807c662c05cc13d4b94))
* **WebAPI:** Don't refresh and sync library media when no connection can be established ([51508a9](https://github.com/PlexRipper/PlexRipper/commit/51508a96f3d0fc67d90ca024b4b7fc901e2204ec))
* **WebAPI:** Fixed an issue where dupplicate connections for PlexServers would all be discarded instead of at least one server being that connection assigned ([5221d5b](https://github.com/PlexRipper/PlexRipper/commit/5221d5bf5b8a4b78cac2cb0726cfdec23da5f148))
* **WebAPI:** Fixed an issue where missing properties that were marked required when deserializing JSON would throw exceptions, now it will use the default value set without exceptions ([97c450e](https://github.com/PlexRipper/PlexRipper/commit/97c450eb474bc8304744fc6ddd05578e19161c1e))
* **Web-UI:** Fixed an issue where the root selection box would not work on the tv show detail page until a child selection had been made ([3cf237a](https://github.com/PlexRipper/PlexRipper/commit/3cf237a963cb2336afb1273c560c8886f305eb16))
* **Web-UI:** Fixed auto scroll to last item viewed not working when viewing all TvShows ([9e91b22](https://github.com/PlexRipper/PlexRipper/commit/9e91b22539141182a62f2d988421810fb3843f38))
* **Web-UI:** Fixed bug where when a server has no-connections that it would be displayed as if it did have in the checkServerConnectionsDialog ([2dc7e01](https://github.com/PlexRipper/PlexRipper/commit/2dc7e010e4f531483a82a7e0097654fff7741433))
* **Web-UI:** Fixed changed job name causing the connections progress dialog to not show ([016fac2](https://github.com/PlexRipper/PlexRipper/commit/016fac27784e45b2ec6699a1eb5a71b6ebbe286b))
* **WebAPI:** Fixed inaccurate downloadspeed, it now uses the last few seconds as a metric instead of the entire download duration ([b634756](https://github.com/PlexRipper/PlexRipper/commit/b6347569a502317aea90ebdc162238bf9f425713))
* **WebAPI:** Fixed missing plex token to retrieve media poster when viewing tv show detail ([479924e](https://github.com/PlexRipper/PlexRipper/commit/479924ecbdaa4bda9931eb97f934554c82dcace7))
* **Web-UI:** Fixed missing scrolling bars when opening a tv show detail page and opening the season overview ([6d1fc03](https://github.com/PlexRipper/PlexRipper/commit/6d1fc038c1f43a99a19f109fb50f6a7c6f9a3d53))
* **WebAPI:** Fixed not retuning the next download task when it is downloading ([cd9861a](https://github.com/PlexRipper/PlexRipper/commit/cd9861a30ba2dd6f52c706c3ab0d89a14f8c51f9))
* **WebAPI:** Fixed the DownloadQueue not correctly determining if a server is online ([7794792](https://github.com/PlexRipper/PlexRipper/commit/7794792d579617d16ba5ad4835becc323dec57f3))
* **WebAPI:** Fixed the endpoint for hiding notifications ([726e54c](https://github.com/PlexRipper/PlexRipper/commit/726e54c8c1add58236cd14f749edb7f59bb53177))
* **Web-UI:** Fixed the incorrect help text for the Re-sync media command in the server settings ([1ee526c](https://github.com/PlexRipper/PlexRipper/commit/1ee526c463676233527399c8edbb85a694506a05))
* **WebAPI:** Fixed the incorrect sortIndex when requesting all media ([58a82cb](https://github.com/PlexRipper/PlexRipper/commit/58a82cbffe74ee0af43d1139a95b6f941b43b19f))
* **Web-UI:** Fixed the jumpy effect when loading the page with all the media posters which goes from 1 column to the correct width ([ee4ed8f](https://github.com/PlexRipper/PlexRipper/commit/ee4ed8f298aa31bfd4f5b7239e433e21b1edb7b1))
* **Web-UI:** Fixed the LanguageSelect size ([444a680](https://github.com/PlexRipper/PlexRipper/commit/444a6802fe20b72808ea30282ce3fa756cb33876))
* **Web-UI:** Fixed the low resolution of media images in the posters ([ad498b5](https://github.com/PlexRipper/PlexRipper/commit/ad498b5dc19a06b26d9acdf8f23a564531747983))
* **Web-UI:** Fixed the missing background when viewing a tv-show detail ([a28951d](https://github.com/PlexRipper/PlexRipper/commit/a28951d98c30ce981512585d4078eddd97833d0e))
* **Web-UI:** Fixed the notification drawer visibility and displaying notifications ([417b5b7](https://github.com/PlexRipper/PlexRipper/commit/417b5b79fae5b3b9560b0199b5c8bad5f50e56c3))
* **WebAPI:** Fixed the Plex account setup process failing when there is a server with no connections ([88504cf](https://github.com/PlexRipper/PlexRipper/commit/88504cfc3a5ba48053e55d61d87d98702cc269d2))
* **WebAPI:** Fixed the stopped status for a DownloadWorker not propagated properly due to premature termination and disposal ([fdce761](https://github.com/PlexRipper/PlexRipper/commit/fdce7611c989897a0b7f6c8f6cf08aaa00af7190))
* **Web-UI:** Fixed the ugly check background when clicking the view button in the media overview ([4462274](https://github.com/PlexRipper/PlexRipper/commit/446227487c561c796348e38e5b7ba88a59880423))
* **WebAPI:** Fixed the verificationCode initialized to null when returning PlexAccounts from the API ([3bc1a04](https://github.com/PlexRipper/PlexRipper/commit/3bc1a0435e93e62c5432c29d6dcddaf2cda87542))
* **Web-UI:** Fixed typescript errors ([0f8c8cb](https://github.com/PlexRipper/PlexRipper/commit/0f8c8cbcf002e9908f63693ba08ddc3a9c652026))
* **Web-UI:** Lot of polish fixes to the detail page for tv show media ([2e4c336](https://github.com/PlexRipper/PlexRipper/commit/2e4c336d084ab6358f02c4faf50c1376061a7b86))
* **Web-UI:** Prevent unneeded all media overview refresh when no media options have changed in the dialog on close ([ec1a613](https://github.com/PlexRipper/PlexRipper/commit/ec1a6134d5838ffd110679f913deb1480734f6af))
* **WebAPI:** The IsOwned on a server is now determined if there is an PlexAccount setup that is the owner ([f04f9bc](https://github.com/PlexRipper/PlexRipper/commit/f04f9bc6890fa242186c5bb85f1a8b894ff956ed))


### Features

* **Web-UI:** Add media switch for the overview page by clicking on the header title ([305a96e](https://github.com/PlexRipper/PlexRipper/commit/305a96e30c9704c32cfdd0916942675262af8e9e))
* **Web-UI:** Added background activity button to the appbar and removed the background animation toggle button ([c3adaf3](https://github.com/PlexRipper/PlexRipper/commit/c3adaf30efc0dd39b133811f4111a02d12921e3a))
* **Web-UI:** Added button in the Plex server settings to create custom connections to connect to a Plex server ([65c792e](https://github.com/PlexRipper/PlexRipper/commit/65c792ee82381b20ea073c36e7b17ed430c3d5db))
* **Web-UI:** Added button to freely generate tokens in the Account dialog when an account has a username and password authentication ([87a822e](https://github.com/PlexRipper/PlexRipper/commit/87a822e50deb3b2181db2ef9d49fc39f3c39ab2a))
* **WebAPI:** Added CreateConnectionEndpoint ([64a07eb](https://github.com/PlexRipper/PlexRipper/commit/64a07eb8bcad0cfd57b0765491303604cae70ada))
* **WebAPI:** Added endpoint for generating tokens for Plex accounts ([a67da40](https://github.com/PlexRipper/PlexRipper/commit/a67da40a16b894c9aac582cfb01558fb75f5f575))
* **WebAPI:** Added endpoints for updating and deleting Plex connections ([4d9a8ed](https://github.com/PlexRipper/PlexRipper/commit/4d9a8ed6779f0669713917b0320e376c89aa2d96))
* **Web-UI:** Added media options button to the media overview ([faf9e55](https://github.com/PlexRipper/PlexRipper/commit/faf9e5575e33eb6fb73829c6e5214efd34d77b35))
* **Web-UI:** Added media overview filter options to hide media from owned servers and from offline libraries ([fd49fd4](https://github.com/PlexRipper/PlexRipper/commit/fd49fd4d43b0df3f130441745a15c498dd05772c))
* **Web-UI:** Added menu to the activity button of running jobs ([b4ec627](https://github.com/PlexRipper/PlexRipper/commit/b4ec6277b118efad0717c7299685727c77ad54fd))
* **WebAPI:** Added new endpoint for retrieving all media of all servers to have 1 big overview ([b3f5f7d](https://github.com/PlexRipper/PlexRipper/commit/b3f5f7d6b1e4db9795d9901ce5f2f938cee8279b))
* **Web-UI:** Added override for data types in Cypress tests ([3a53c0e](https://github.com/PlexRipper/PlexRipper/commit/3a53c0e8f073857d7767d3919a5e9324be2eea03))
* **Web-UI:** Added Server Unreachable status to a download when the server goes down while downloading ([1f8bd86](https://github.com/PlexRipper/PlexRipper/commit/1f8bd86c92351cde3968de9dd559379610d37ab4))
* **Web-UI:** Added the quality of the episode in the tv show detail overview ([33c1080](https://github.com/PlexRipper/PlexRipper/commit/33c1080fb0108364bf2e7813726f332fe8706063))
* **Web-UI:** Added validation check to prevent adding duplicate connection urls when it already exists for a different server ([18a4d4d](https://github.com/PlexRipper/PlexRipper/commit/18a4d4d6ab9897d8b27680c8dc1984f4b70823ab))
* **WebAPI:** Added validation endpoint for a server connection url to ensure that it works ([14c3121](https://github.com/PlexRipper/PlexRipper/commit/14c3121cb180e28fb119cdbf4f5aca30db466cc5))
* **Web-UI:** Added Webstorm Nuxt Debug run profile ([627a85f](https://github.com/PlexRipper/PlexRipper/commit/627a85fdfe79514c2a209bfe51d1207fdcd74f92))
* **Web-UI:** All dialogs are now much more size responsive and work through the power of CSS grids ([5067225](https://github.com/PlexRipper/PlexRipper/commit/5067225b5f543e94d2506472a9eb137211a59d65))
* **Web-UI:** Allow the editing and deletion of connections in the server settings ([55b714f](https://github.com/PlexRipper/PlexRipper/commit/55b714fcca3382c94c0545b93e0da3e1ad67d69f))
* **Web-UI:** Polish the media statistics display in the media overview bar ([fdebd9b](https://github.com/PlexRipper/PlexRipper/commit/fdebd9b5d90b7a5bcaf7622c5f4a50b67380e94c))
* **Web-UI:** The question if you want to start the setup process is now a dialog pop-up window ([6f42ee9](https://github.com/PlexRipper/PlexRipper/commit/6f42ee9377cf19ce78d33306d0d4122e498dc78f))
* **Web-UI:** Toggling the media overview type is now saved when going to the home page ([20d9d83](https://github.com/PlexRipper/PlexRipper/commit/20d9d83958a8601bf268f5fabe8b25d19eb6f6fa))
* **WebAPI:** When a DownloadTask is stopped due to offline server, it will now auto-resume when the server is back-online ([63abda9](https://github.com/PlexRipper/PlexRipper/commit/63abda9f240395b12e6d6a04ddf55cb6440a2619))


### Performance Improvements

* **WebAPI:** Calculate media list statistics in the back-end before sending it to the front-end ([f521702](https://github.com/PlexRipper/PlexRipper/commit/f52170245be57f8166c9cd32d2b76501e63fd638))
* **WebAPI:** Further slight improvements to performance with sorting of querying media ([48f3500](https://github.com/PlexRipper/PlexRipper/commit/48f35004562a1c3f6d044adad6cca1906a0e1eb7))
* **WebAPI:** Improved performance of querying media by projecting entity to DTO and moving the thumbnail url compiling to the front-end ([271d0d0](https://github.com/PlexRipper/PlexRipper/commit/271d0d0b9393c5b7883da9a2c2e11e779a131017))
* **Web-UI:** the full thumbnail url is now compiled in the front-end since that data is already sent beforehand ([2b9535f](https://github.com/PlexRipper/PlexRipper/commit/2b9535f87dcf1dee7790024993b3fb743b342ac4))

# [0.24.0](https://github.com/PlexRipper/PlexRipper/compare/v0.23.2...v0.24.0) (2024-10-26)


### Bug Fixes

* **WebAPI:** Added an IsServerOnline check before picking the next downloadTask to download ([1e9f67f](https://github.com/PlexRipper/PlexRipper/commit/1e9f67fe78ad1feb13e21ad2b0051824bc5082b0))
* **WebAPI:** Correctly configured CORS to only allow same-origin requests to the server ([e90bfb9](https://github.com/PlexRipper/PlexRipper/commit/e90bfb9480281d3d9e85ce77e3b0d27571f5a9bc))
* **Web-UI:** Display fallback image when the poster thumbnail cannot be loaded ([dcb77f2](https://github.com/PlexRipper/PlexRipper/commit/dcb77f2773150a9beb60b670c4cfcce9acfd08dc))
* **WebAPI:** Fix Result.Value exception when accessed in failed mode ([69da8d0](https://github.com/PlexRipper/PlexRipper/commit/69da8d0b4ae838512922c1dd62a7bca0e4262a21))
* **WebAPI:** Fixed a bug where PlexServers that have the same (local) connections are causing media/libraries to be messed up. These are now filtered out ([c7afef8](https://github.com/PlexRipper/PlexRipper/commit/c7afef8c16b648fda6947d6cbaef5523e4bf7e4c))
* **WebAPI:** Fixed account data that couldn't be saved when in tokenMode due to validation restrictions ([e34eb5b](https://github.com/PlexRipper/PlexRipper/commit/e34eb5b1048871ba68e5fbd3058d7f8cb462d510))
* **WebAPI:** Fixed exception when no valid connection could be found and the thumbnail link had to be generated ([2482e12](https://github.com/PlexRipper/PlexRipper/commit/2482e122ad11d805d2a881b09ae133d773be0eca))
* **Web-UI:** Fixed going back when viewing a tv show to the media overview not loading the last page correctly ([7ebbbb2](https://github.com/PlexRipper/PlexRipper/commit/7ebbbb2e525400d2c17ae9212794b2d203da74b3))
* **WebAPI:** Fixed incorrect index numbers appearing next to media ([e1b3cf0](https://github.com/PlexRipper/PlexRipper/commit/e1b3cf07fefebb11d6a8d6810fcb697250da8edd))
* **WebAPI:** Fixed life cycle hooks not being triggered by a container shutdown ([3b6b2bd](https://github.com/PlexRipper/PlexRipper/commit/3b6b2bd05b26b66eb61539145e24af369248b783))
* **Web-UI:** Fixed missing default media poster when media has no thumbnail ([4bf3ea8](https://github.com/PlexRipper/PlexRipper/commit/4bf3ea8adbe4ab84df17775d2a4184ab57b32170))
* **WebAPI:** Fixed out or range unix time errors by parsing long from milliseconds instead of seconds ([dd66e9d](https://github.com/PlexRipper/PlexRipper/commit/dd66e9d4846f1c19e1e3326c5f7079407f9baf22))
* **WebAPI:** Fixed signalR not working due to wrong order of initialization ([bc87ef0](https://github.com/PlexRipper/PlexRipper/commit/bc87ef026d41c8f70af1a51c1c62650a4a376ae9))
* **Web-UI:** Fixed the download segments value being half-way hidden due to UI cut-off ([d348b27](https://github.com/PlexRipper/PlexRipper/commit/d348b273bdffa1d2828ae87e671c02cfff617874))
* **WebAPI:** fixed the download task not pausing when not actually downloading ([7f9d287](https://github.com/PlexRipper/PlexRipper/commit/7f9d28745dcc3006a30cc1b6eabb604918c9449a))
* **Web-UI:** Fixed the highlight effect not showing when sometimes navigating with the alphabet navigation ([087c1de](https://github.com/PlexRipper/PlexRipper/commit/087c1ded8a9e9c631b49abc6a30bd712bf8c2586))
* **Web-UI:** Fixed the missing styling of the scrollbar in firefox ([3610ccf](https://github.com/PlexRipper/PlexRipper/commit/3610ccffeb8421b51fff0faef1ceda5162153e98))
* **WebAPI:** fixed the PlexAPI not returning unauthorized error when the token has become invalid ([f3604fb](https://github.com/PlexRipper/PlexRipper/commit/f3604fb975a564e36696d62139aa0d7bdc837382))
* **WebAPI:** Fixed the sortIndex not set on TvShows when syncing media ([bc1cb79](https://github.com/PlexRipper/PlexRipper/commit/bc1cb799cccebb874ac905a94d0f0977ef3a4c42))
* **Web-UI:** Folderpaths can now be edited freely even when download tasks are active, changes will now apply to downloadtask that have to start ([fe2d80e](https://github.com/PlexRipper/PlexRipper/commit/fe2d80ea3f04796a614baf4a8e487295d99de157))
* **WebAPI:** hopeful fix that adress files to always be created as root ([5a03cb6](https://github.com/PlexRipper/PlexRipper/commit/5a03cb6087af0df3978f62a857e57a10740e153f))
* **WebAPI:** Only a single copy of the server status will now be kept in the database, this makes getting the latest status much easier and any online checks ([8f9dac1](https://github.com/PlexRipper/PlexRipper/commit/8f9dac165cd2b8bb4e84d87f36312079f47d0443))
* **WebAPI:** Possible fix that should resolve SQLite Error 5: 'database is locked'. by waiting until the database is unlocked ([0c1151a](https://github.com/PlexRipper/PlexRipper/commit/0c1151a9175b43f600cf631fb66d77c731836dd7))
* **WebAPI:** Quartz crash when database has to be created ([d7e81de](https://github.com/PlexRipper/PlexRipper/commit/d7e81de36889be0d43115cf5a265deb160ca25ae))
* **Web-UI:** Remove unneeded scrollbar on the page in firefox ([bb81824](https://github.com/PlexRipper/PlexRipper/commit/bb818246b519548eb361fd55a7bd3a4c5c59d100))
* **WebAPI:** set stj serializer in quartz test setup ([6badce7](https://github.com/PlexRipper/PlexRipper/commit/6badce74f9bea0f274008dfada9904b1a9ce3ba1))
* **WebAPI:** Some Plex Libraries have some random invalid GUID so were going to save it as a string in the database instead of a GUID ([a6f92c1](https://github.com/PlexRipper/PlexRipper/commit/a6f92c163d85778dc997cdb58a516aa61dd9ada3))
* **WebAPI:** unset default timeout, this might conflict with CommandTimeout ([b1ba28b](https://github.com/PlexRipper/PlexRipper/commit/b1ba28b32149356af16bcda032f9f8bf0d7e25db))


### Features

* **Web-UI:** Add tv-show download button on the media poster to quick download ([945e836](https://github.com/PlexRipper/PlexRipper/commit/945e836a68317f00cd851bc33f44d857b24fa0d0))
* **Web-UI:** Added a search feature to libraries which will search within that library. More general search will come later ([7e573f8](https://github.com/PlexRipper/PlexRipper/commit/7e573f8ee8482f26289c1b186faf12d67f1710c1))
* **WebAPI:** Added failsafe that shutsdown the container when the database cannot be setup ([55c47af](https://github.com/PlexRipper/PlexRipper/commit/55c47afc48a341f551a6506a5b1e3f383f2931dd))
* **WebAPI:** Added method to easily mock and set up a httpclient for unit tests ([3f26d93](https://github.com/PlexRipper/PlexRipper/commit/3f26d939be6ee46abc52f34d572e30a82a44c07f))
* **Web-UI:** Added missing pages for photos and unknown to display that they are not supported yet ([e789ad2](https://github.com/PlexRipper/PlexRipper/commit/e789ad2521f83778d9b9b38aa5c207d8b92c6019))
* **WebAPI:** Added UNMASKED env variable to unmask sensitive data in the logs ([41ff0ae](https://github.com/PlexRipper/PlexRipper/commit/41ff0ae90974e6b1c65085f11e92933e011d791b))
* **Web-UI:** keep retrying to reconnect with signalr when connection is lost due to offline back-end ([70ccbf1](https://github.com/PlexRipper/PlexRipper/commit/70ccbf1a581de3c36e793a4a91704a40cb0a78cc))
* **Web-UI:** keep retrying to reconnect with signalr when connection is lost due to offline back-end ([bd5a0c3](https://github.com/PlexRipper/PlexRipper/commit/bd5a0c3fe2ac0037cfa7f788cbfc7781c8880f3b))
* **WebAPI:** Plex server status online/offline is now checked every 5 minutes ([c4ccf39](https://github.com/PlexRipper/PlexRipper/commit/c4ccf39f0128aff456082f4914a503dde2931374))
* **WebAPI:** set auto status checker to 10 minutes ([b5f0308](https://github.com/PlexRipper/PlexRipper/commit/b5f03089a334f5220d1efaec5168de29578f89a7))
* **Web-UI:** The library media icon will now flash when the library is syncing, stay grey when it has not been synced yet and turned white when it has been synced of its media ([8df1304](https://github.com/PlexRipper/PlexRipper/commit/8df13047229fd893e9c1a490abd9b26bc5ac5405))


### Performance Improvements

* **Web-UI:** Major navigation performance improvement when using the alphabet navigation to the right ([273eef8](https://github.com/PlexRipper/PlexRipper/commit/273eef86a915d479f185b2ff947f952e9e2f1403))
* **WebAPI:** minor performance improvements with big databases, stop searching for more media when the media count has been reached ([075aa69](https://github.com/PlexRipper/PlexRipper/commit/075aa698ba51a0510e745319d2fc1f54296d15d1))
* **WebAPI:** Sorting now happens once when the library media is updated which is then stored as an index, which should be a big performance upgrade and save diskspace ([1676baa](https://github.com/PlexRipper/PlexRipper/commit/1676baa4e6ed41ffa9aaa8ad7aac6f810a58908a))


### Reverts

* Revert "Update Dockerfile" ([9f2738d](https://github.com/PlexRipper/PlexRipper/commit/9f2738d02611c9bb14efd1e7310a8b8321615870))

## [0.23.2](https://github.com/PlexRipper/PlexRipper/compare/v0.23.1...v0.23.2) (2024-10-03)


### Bug Fixes

* **WebAPI:** added retry on timeout to the Plex api client ([baed12d](https://github.com/PlexRipper/PlexRipper/commit/baed12d5796e89f070c9c36cf4158f1849ae6b09))
* **WebAPI:** remove database transactions when writing media data, this might have been causing locks to the database ([cf641ec](https://github.com/PlexRipper/PlexRipper/commit/cf641ec748a09f7178b47024575eb0ff7205da44))


### Performance Improvements

* **WebAPI:** Enable sqlite connection pooling and increase the busy time-out to hopefully solve database locks ([1deb3b5](https://github.com/PlexRipper/PlexRipper/commit/1deb3b57d241cbdf18557528dad9d2551e69ba49))
* **WebAPI:** Greatly increased performance of opening a TvShow ([2567b98](https://github.com/PlexRipper/PlexRipper/commit/2567b98e7a4a5bc8710271076e515d2203236e0f))

## [0.23.1](https://github.com/PlexRipper/PlexRipper/compare/v0.23.0...v0.23.1) (2024-10-03)


### Bug Fixes

* **WebAPI:** Fixed exception when syncing certain libraries ([c716bae](https://github.com/PlexRipper/PlexRipper/commit/c716baeb142475a41ca84ca437266cecf25fbed2))
* **WebAPI:** Fixed movie libraries not syncing ([4eb55f0](https://github.com/PlexRipper/PlexRipper/commit/4eb55f027fe03fd64b2dd75be30392474b4ab257))
* **Web-UI:** fixed the total steps displayed when refreshing a library to be the correct number based on the library type ([99c1fcd](https://github.com/PlexRipper/PlexRipper/commit/99c1fcd6fbc332242010a7dbb860ec2711c68da3))
* **Web-UI:** fixed the ugly empty space underneath tv-show posters ([1c7d2bf](https://github.com/PlexRipper/PlexRipper/commit/1c7d2bf1439378ee2b2743d375d0943a7cb6aa0f))
* **Web-UI:** fixed typecheck errors ([8961da5](https://github.com/PlexRipper/PlexRipper/commit/8961da54f2205f233d64d13c31c6971889cf0a5a))

# [0.23.0](https://github.com/PlexRipper/PlexRipper/compare/v0.22.0...v0.23.0) (2024-10-03)


### Bug Fixes

* **Web-UI:** Added better error messages to the refreshing library page ([3029e5d](https://github.com/PlexRipper/PlexRipper/commit/3029e5deafa463a41585b5e80cc1960a2639575a))
* **WebAPI:** Fixed an issue where certain users could not log in due to serialization issues ([4f3019d](https://github.com/PlexRipper/PlexRipper/commit/4f3019df20b47fea14cce1a0cd2afffd517f11b3))
* **WebAPI:** Fixed empty PlexLibraries causing errors, being empty is valid ([a69a6e7](https://github.com/PlexRipper/PlexRipper/commit/a69a6e73c4d8d9bd0a17b3648935d3da46a223bf))
* **WebAPI:** fixed isRefresh always being false when sent in the library progress ([df54fdd](https://github.com/PlexRipper/PlexRipper/commit/df54fddf8cd76c675f5d4bfe89b3ff9e709816a9))
* **Web-UI:** fixed layout issue in dialogs ([4e1d56f](https://github.com/PlexRipper/PlexRipper/commit/4e1d56fa0e17f41bbb2bed9a47cc6aab11c5697a))
* **WebAPI:** Fixed several exceptions due to invalid json parse ([0b74ecc](https://github.com/PlexRipper/PlexRipper/commit/0b74ecc3e0ba53d53225fcc02d5a9407d6ddb9f2))
* **Web-UI:** Fixed typescript errors ([78948b1](https://github.com/PlexRipper/PlexRipper/commit/78948b15207fcde9c7f3f9c9a6e83dc7fff72fa6))


### Features

* **WebAPI:** Added a time remaining to sync data from the Plex API ([1b3923f](https://github.com/PlexRipper/PlexRipper/commit/1b3923f84db357e66458b89a125d240813d3152b))
* **Web-UI:** Added countdown to the library refresh page ([551ef8b](https://github.com/PlexRipper/PlexRipper/commit/551ef8bb6573596f0e893f78a5b6c256413777ee))
* **Web-UI:** Added the steps of the refresh process to the time remaining ([2b8057b](https://github.com/PlexRipper/PlexRipper/commit/2b8057b8edda53fdc7a90e83cfc125a4ce3465f4))


### Performance Improvements

* **WebAPI:** Big performance improvement on the merging of media data coming from the Plex API ([6154a22](https://github.com/PlexRipper/PlexRipper/commit/6154a22ed21ddb009369e80f367f243d68d12454))
* **Web-UI:** Huge performance improvement when loading huge libraries, no more crashing UI! ([6c0e7f7](https://github.com/PlexRipper/PlexRipper/commit/6c0e7f712c98e33d225f34fba1b821bcfc862358))
* **WebAPI:** Major performance improvement of writing tv shows to the database, 20min -> 2 seconds ([00be9d5](https://github.com/PlexRipper/PlexRipper/commit/00be9d59b7590673af6d8a934a988247fa0741f7))
* **WebAPI:** Start syncing media data  when all connections fora server have been tested ([afc0e21](https://github.com/PlexRipper/PlexRipper/commit/afc0e210d9b0db0b21c55684b8942505c9297c6a))

# [0.22.0](https://github.com/PlexRipper/PlexRipper/compare/v0.21.0...v0.22.0) (2024-09-30)


### Bug Fixes

* **WebAPI:** Added ways to catch exceptions from Plex.SDK ([21b4e2d](https://github.com/PlexRipper/PlexRipper/commit/21b4e2dfe5e24bbc6047cac8343fcb8d06115535))
* **Web-UI:** corrected spacing ([3e98ff4](https://github.com/PlexRipper/PlexRipper/commit/3e98ff4c0ddb783d8cc9665f20130472d5d78916))
* **Web-UI:** corrected vertical alignment of the title and media icon in the details dialog ([7350f87](https://github.com/PlexRipper/PlexRipper/commit/7350f87107f8a42c228bf0fb099e7a74f9819ae4))
* **WebAPI:** Fix full windows path names being used as a filename for media files ([a51d078](https://github.com/PlexRipper/PlexRipper/commit/a51d078baf2ba521dc39b6c7f464399d6e446f18))
* **WebAPI:** Fix minimum logLevel in serilog not being honored ([c039973](https://github.com/PlexRipper/PlexRipper/commit/c039973670897af3ddb31ed096234b7df1ce6b35))
* **WebAPI:** Fixed an issue where a PlexServer could time-out because PlexRipper requested too much data at once, this is now in batches of 500tv shows per request ([59e0e07](https://github.com/PlexRipper/PlexRipper/commit/59e0e07b4ea2ed32b5bfa3831225242cfaf9ca13))
* **WebAPI:** Fixed default destination for libraries not being able to change from the UI ([a8aaea2](https://github.com/PlexRipper/PlexRipper/commit/a8aaea2145794843ee2bd78c8ffed5fda5168c48))
* **WebAPI:** Fixed exceptions not catched correctly when interacting with the Plex API SDK ([c2c5e91](https://github.com/PlexRipper/PlexRipper/commit/c2c5e914542e4fd6629ddcdee320f3ada2e199d6))
* **Web-UI:** fixed homepage alignment ([dde11ce](https://github.com/PlexRipper/PlexRipper/commit/dde11cef245f098b6df1f728e30f89e99113ccd1))
* **WebAPI:** fixed missing destination and download path from the download task details dialog ([c0afb3f](https://github.com/PlexRipper/PlexRipper/commit/c0afb3f9a5b598e9a8d594205f46ec8d16768092))
* **Web-UI:** Fixed missing translations for the download status and columns of various ui ([4567ed7](https://github.com/PlexRipper/PlexRipper/commit/4567ed720b91f29af8a6ff4fc89819464c490406))
* **WebAPI:** fixed the downdload queue breaking when repeatly pausing and resuming ([a2d277d](https://github.com/PlexRipper/PlexRipper/commit/a2d277dba75a97aa63604042952aa1f56cc23b64))
* **WebAPI:** Fixed the download commands not working when clicked on the parent, works for pause and stop now ([c5456bb](https://github.com/PlexRipper/PlexRipper/commit/c5456bbe55a049f52e45a01f35c8cd365aa85871))
* **WebAPI:** Fixed the download commands not working when clicked on the parent, works for start and resume now ([7925f62](https://github.com/PlexRipper/PlexRipper/commit/7925f623683deb3544f8ae0f72041309fff957c7))
* **WebAPI:** Fixed the download progress not resetting when a download task is stopped and the files are deleted ([87ad153](https://github.com/PlexRipper/PlexRipper/commit/87ad153afff9c95b81b706a9c77fb078e6600f45))
* **WebAPI:** Fixed the incorrect and slow progress updates from refreshing a library ([eaa54de](https://github.com/PlexRipper/PlexRipper/commit/eaa54deb94c645fce29956886e68f4fad9664b58))
* **Web-UI:** Fixed the incorrect display of connection text when testing a connection ([97e4d75](https://github.com/PlexRipper/PlexRipper/commit/97e4d75311632a0f2530a592c1df0c813839b1c2))
* **Web-UI:** Fixed the library bar with the refresh button being hidden when the library is empty ([0d32285](https://github.com/PlexRipper/PlexRipper/commit/0d3228520d0319fe92adf2bbee01836621e7e81f))
* **WebAPI:** Fixed the LOG_LEVEL env variablenot being respected for the backend logging ([238e50c](https://github.com/PlexRipper/PlexRipper/commit/238e50cf33fcaf28334a922000fb840e2bde799a))
* **Web-UI:** Fixed type errors ([520c2d7](https://github.com/PlexRipper/PlexRipper/commit/520c2d7d2d8cb6851816c782c36802cd751458da))
* **Web-UI:** Fixed type errors ([36b7131](https://github.com/PlexRipper/PlexRipper/commit/36b713187d6a14903429366f7cbe9b273f238c0b))
* **Web-UI:** fixed typecheck test ([8356c71](https://github.com/PlexRipper/PlexRipper/commit/8356c7132fcaab2feedc79232b6e865ec56ff216))
* **Web-UI:** fixed wrong translation in the download table column header ([b90be63](https://github.com/PlexRipper/PlexRipper/commit/b90be634ab6a6f1e6160103387def0e0e64dbe54))
* **Web-UI:** Made the title inside the refresh accounts menu more clear ([f9614d0](https://github.com/PlexRipper/PlexRipper/commit/f9614d007b5e3eaa38f828db116c7d5160e535b4))


### Features

* **WebAPI:** Added download task log endpoint ([dfffc47](https://github.com/PlexRipper/PlexRipper/commit/dfffc4740e16f7bf3426b68a0bd0cd21b34624a3))
* **Web-UI:** Added download task logs to the UI, see the details button on a download row and then click logs ([31edebd](https://github.com/PlexRipper/PlexRipper/commit/31edebda3c346fac5f5fe6a89eca728ef6d3ba63))


### Reverts

* **Web-UI:** hide search bar again until it's ready ([918fbc5](https://github.com/PlexRipper/PlexRipper/commit/918fbc5bac64a83f88f611b28832c8f5cb95a5ef))

# [0.21.0](https://github.com/PlexRipper/PlexRipper/compare/v0.20.0...v0.21.0) (2024-08-17)


### Bug Fixes

* **WebAPI:** Added missing FileMergeJob Type to JobTypes ([d5daea5](https://github.com/PlexRipper/PlexRipper/commit/d5daea5144a0c5944cf7b04ba458e201a91714f7))
* **Web-UI:** Fixed a PlexAccount not updating correctly in the UI ([e2b7abf](https://github.com/PlexRipper/PlexRipper/commit/e2b7abf9c96ecf12c2d3a33606f9cdedf156e21f))
* **WebAPI:** Fixed incorrect method of creating a Guid ([0062c2b](https://github.com/PlexRipper/PlexRipper/commit/0062c2b9e9b2338380c7f0ff601978d1475e7539))
* **WebAPI:** Fixed PlexLibraries incorrectly updating causing some to be missing ([3111183](https://github.com/PlexRipper/PlexRipper/commit/311118341f2714896bbffbf0752b4ce1ddbcbc30))
* **Web-UI:** Fixed the language flags being inconsistent ([0eee59e](https://github.com/PlexRipper/PlexRipper/commit/0eee59ef2094d8d1c665f5c04632d3b066bf51ca))
* **WebAPI:** Fixed the missing plexLibrary problem and the lack of Plexmedia ([1f64be2](https://github.com/PlexRipper/PlexRipper/commit/1f64be2db4f5654d5aad013167906e90a9c8676a))
* **WebAPI:** Fixed the PlexAccountId getting lost when validating ([7b9bc16](https://github.com/PlexRipper/PlexRipper/commit/7b9bc1626be89913e4ec9731658e72f674994b0b))
* **Web-UI:** Pre-translate missing translations for German and French ([fc1859c](https://github.com/PlexRipper/PlexRipper/commit/fc1859cdf299fdd9f997af3af3bd5d921fa92a35))


### Features

* **Web-UI:** Added Language Select to the setup page ([87d1287](https://github.com/PlexRipper/PlexRipper/commit/87d1287b396841adf3778b5e1db84b1e28efcc92))
* **Web-UI:** Added Polish language into babel-edit save file ([4ad23c8](https://github.com/PlexRipper/PlexRipper/commit/4ad23c8344d2f3c2b6f3dcef812e2636996f96c6))
* **Web-UI:** Added Polish language option ([d9ae110](https://github.com/PlexRipper/PlexRipper/commit/d9ae1108a0696b450293ca54d6f53049f41c739c))
* **Web-UI:** Disabling a PlexAccount will no also hide PlexServers from the sidebar when there are no accounts that have access anymore ([6a9dec3](https://github.com/PlexRipper/PlexRipper/commit/6a9dec3bcc890c904dcc99f357068b68bf21d572))

# [0.20.0](https://github.com/PlexRipper/PlexRipper/compare/v0.19.1...v0.20.0) (2024-08-12)


### Bug Fixes

* **WebAPI:** Fixed an issue where connections to PlexServer where not synced correctly with the database ([5bbc1d0](https://github.com/PlexRipper/PlexRipper/commit/5bbc1d025b429c5bce318259888845e7317a07b3))
* **Web-UI:** Fixed the percentages displayed being 100 times to high ([820b78a](https://github.com/PlexRipper/PlexRipper/commit/820b78a4fa3192ef4fbf14448585341c903b5e44))


### Features

* **Web-UI:** Better sort the Plex server connections when they are displayed ([bb031f8](https://github.com/PlexRipper/PlexRipper/commit/bb031f81681412f83745f275924f1f9249db9c2a))
* **WebAPI:** Hiding a PlexServer will now also disable it from use when executing operations ([5ff8796](https://github.com/PlexRipper/PlexRipper/commit/5ff8796ad3e4127f67ec15347212dbf4389028af))

## [0.19.1](https://github.com/PlexRipper/PlexRipper/compare/v0.19.0...v0.19.1) (2024-08-11)


### Bug Fixes

* **WebAPI:** Fixed a bug where the verification of MFA enabled accounts did not pop-up ([aa3c62c](https://github.com/PlexRipper/PlexRipper/commit/aa3c62caa6e8b9edb8cef8bbb9bcf8ecf10757db))

# [0.19.0](https://github.com/PlexRipper/PlexRipper/compare/v0.18.0...v0.19.0) (2024-08-10)


### Bug Fixes

* **WebAPI:** Fixed 401 errors not being returned from api endpoints ([8e76e03](https://github.com/PlexRipper/PlexRipper/commit/8e76e03fa7655d21d0cba329e070926f88dd4c6e))
* **WebAPI:** Fixed client devices being seen as a server, this is now checked and clients should not be added anymore ([9d576db](https://github.com/PlexRipper/PlexRipper/commit/9d576dbc1a8a77dc47f0d965c16cbdcc4de7cf17))
* **WebAPI:** Fixed the library metadata not being updated when syncing media ([e285ede](https://github.com/PlexRipper/PlexRipper/commit/e285edeedbef71a36eae3556d9c11de9dc0f2089))
* **Web-UI:** Made the url column for the check connections dialog wider ([286ca8a](https://github.com/PlexRipper/PlexRipper/commit/286ca8a91f66a9939dbfb7f7c22a7b233a485003))


### Features

* **Web-UI:** An plex.tv auth token can now be directly added in a account creation screen to avoid having to register with an username and password ([bff6ba4](https://github.com/PlexRipper/PlexRipper/commit/bff6ba4a0221d1839f166d5dc7ddc651b59d9672))
* **Web-UI:** Display the invalid token dialog when an invalid token has been given ([5d258df](https://github.com/PlexRipper/PlexRipper/commit/5d258df88438721af8309f5518eb8945614e0d39))

# [0.18.0](https://github.com/PlexRipper/PlexRipper/compare/v0.17.0...v0.18.0) (2024-08-09)


### Bug Fixes

* **Web-UI:** Cleanup the headers that were to big ([a576647](https://github.com/PlexRipper/PlexRipper/commit/a5766471ffe3587772642163ab22cb54d98a6752))
* **WebAPI:** Fixed a crash during library syncing where dupplicate keys were created in media ([fe13f2b](https://github.com/PlexRipper/PlexRipper/commit/fe13f2bb61891ae1f7bd31eb3d1ea92d74818729))
* **Web-UI:** Fixed broken percentage display due to missing translation ([9aa3167](https://github.com/PlexRipper/PlexRipper/commit/9aa3167e2c3056483a8d2403742a2efd7b1b284f))
* **Web-UI:** Fixed Cypress pathing by re setting the baseUrl ([bafe90d](https://github.com/PlexRipper/PlexRipper/commit/bafe90d6bfd5ec1a886146bbd1c7132dddc9f140))
* **Web-UI:** Fixed percentage that was 10000% ([58c0c15](https://github.com/PlexRipper/PlexRipper/commit/58c0c15b94e3040e41a4576f2ed6a5efc8b1cdb2))
* Fixed the duration of media beind incorrectly displayed due to milliseconds being treated as seconds ([78989a8](https://github.com/PlexRipper/PlexRipper/commit/78989a8955887a4132a60b3e3eb6cdfec8df2ef7))
* **WebAPI:** Fixed the library not being marked as synced, thus syncing media needlessly when there were no changes ([8a6cc00](https://github.com/PlexRipper/PlexRipper/commit/8a6cc00c6b85c981520c154ce363d565340db821))
* **Web-UI:** Fixed the missing server media sync progress bar when media is syncing ([5338729](https://github.com/PlexRipper/PlexRipper/commit/533872962c20fd232b536f549c80e7912480c3fd))
* **Web-UI:** Fixed warnings about unused props on QStatus ([dfae4b7](https://github.com/PlexRipper/PlexRipper/commit/dfae4b70e9e4771877968de5dae930b02d867125))


### Features

* **WebAPI:** Added places where a notification is sent to the front-end when data in the back-end is updated ([7c9bcda](https://github.com/PlexRipper/PlexRipper/commit/7c9bcdaa445f95355bc8027a590d2f8dd9ddd380))
* **WebAPI:** Store additional connections per plex server to allow plex.direct connections when a server has limited connectivity ([4b2f13c](https://github.com/PlexRipper/PlexRipper/commit/4b2f13ce14af2958ac744809f72a79c9e48156fe))


### Performance Improvements

* **WebAPI:** Refactored the RefreshLibraryAccessCommand to do multiple servers in parrallel ([6925dec](https://github.com/PlexRipper/PlexRipper/commit/6925decc81da63803a4d3fff5353a7a3e1220eb8))

# [0.18.0](https://github.com/PlexRipper/PlexRipper/compare/v0.17.0...v0.18.0) (2024-08-09)


### Bug Fixes

* **Web-UI:** Cleanup the headers that were to big ([a576647](https://github.com/PlexRipper/PlexRipper/commit/a5766471ffe3587772642163ab22cb54d98a6752))
* **WebAPI:** Fixed a crash during library syncing where dupplicate keys were created in media ([fe13f2b](https://github.com/PlexRipper/PlexRipper/commit/fe13f2bb61891ae1f7bd31eb3d1ea92d74818729))
* **Web-UI:** Fixed broken percentage display due to missing translation ([9aa3167](https://github.com/PlexRipper/PlexRipper/commit/9aa3167e2c3056483a8d2403742a2efd7b1b284f))
* **Web-UI:** Fixed Cypress pathing by re setting the baseUrl ([bafe90d](https://github.com/PlexRipper/PlexRipper/commit/bafe90d6bfd5ec1a886146bbd1c7132dddc9f140))
* **Web-UI:** Fixed percentage that was 10000% ([58c0c15](https://github.com/PlexRipper/PlexRipper/commit/58c0c15b94e3040e41a4576f2ed6a5efc8b1cdb2))
* Fixed the duration of media beind incorrectly displayed due to milliseconds being treated as seconds ([78989a8](https://github.com/PlexRipper/PlexRipper/commit/78989a8955887a4132a60b3e3eb6cdfec8df2ef7))
* **WebAPI:** Fixed the library not being marked as synced, thus syncing media needlessly when there were no changes ([8a6cc00](https://github.com/PlexRipper/PlexRipper/commit/8a6cc00c6b85c981520c154ce363d565340db821))
* **Web-UI:** Fixed the missing server media sync progress bar when media is syncing ([5338729](https://github.com/PlexRipper/PlexRipper/commit/533872962c20fd232b536f549c80e7912480c3fd))
* **Web-UI:** Fixed warnings about unused props on QStatus ([dfae4b7](https://github.com/PlexRipper/PlexRipper/commit/dfae4b70e9e4771877968de5dae930b02d867125))


### Features

* **WebAPI:** Added places where a notification is sent to the front-end when data in the back-end is updated ([7c9bcda](https://github.com/PlexRipper/PlexRipper/commit/7c9bcdaa445f95355bc8027a590d2f8dd9ddd380))
* **WebAPI:** Store additional connections per plex server to allow plex.direct connections when a server has limited connectivity ([4b2f13c](https://github.com/PlexRipper/PlexRipper/commit/4b2f13ce14af2958ac744809f72a79c9e48156fe))


### Performance Improvements

* **WebAPI:** Refactored the RefreshLibraryAccessCommand to do multiple servers in parrallel ([6925dec](https://github.com/PlexRipper/PlexRipper/commit/6925decc81da63803a4d3fff5353a7a3e1220eb8))

# [0.17.0](https://github.com/PlexRipper/PlexRipper/compare/v0.16.0...v0.17.0) (2024-08-06)


### Features

* Added new PlexRipper logo with space around it ([8dbefaf](https://github.com/PlexRipper/PlexRipper/commit/8dbefaf9ac0c1285dda188710fb012693a8fde0f))

# [0.17.0](https://github.com/PlexRipper/PlexRipper/compare/v0.16.0...v0.17.0) (2024-08-06)


### Features

* Added new PlexRipper logo with space around it ([8dbefaf](https://github.com/PlexRipper/PlexRipper/commit/8dbefaf9ac0c1285dda188710fb012693a8fde0f))

# [0.16.0](https://github.com/PlexRipper/PlexRipper/compare/v0.15.0...v0.16.0) (2024-08-02)


### Bug Fixes

* **Web-UI:** A server can now be hidden from the server sidebar by opening the server dialog and clicking hide ([8f45d65](https://github.com/PlexRipper/PlexRipper/commit/8f45d652f185dc022f80739dfa8fa412e5b5690f))
* **WebAPI:** Fixed a bug where if a authtoken failed to be retrieved then it would crash the media call ([c8f8a44](https://github.com/PlexRipper/PlexRipper/commit/c8f8a44fc09b84acac11ebab00be75a59dfb14ff))
* **WebAPI:** Fixed an issue where seperate injected settingsmodules were not reactive to the parent usersettings ([d54e5d9](https://github.com/PlexRipper/PlexRipper/commit/d54e5d9a9e7e6ef21f81f277321958924e397052))
* **Web-UI:** Fixed the download table selectAll checkbox not selecting its children ([8dd200b](https://github.com/PlexRipper/PlexRipper/commit/8dd200b68de7e3acf5d4eef38fc4e1d25280383d))
* **Web-UI:** Typecheck errors fix ([382bb9d](https://github.com/PlexRipper/PlexRipper/commit/382bb9d0fc5fc44cd9381624ceffbd308eb2864c))


### Features

* add hide/unhide server functionality in UI settings ([47b0862](https://github.com/PlexRipper/PlexRipper/commit/47b08626eda073855491838ff7692f4de841885c))
* **WebAPI:** Added an endpoint to hide a server from view ([10c2149](https://github.com/PlexRipper/PlexRipper/commit/10c2149c15c97d45442ea2c6fafd4b122fa52ece))
* **Web-UI:** Migrated prettier config to eslint ([63c7dce](https://github.com/PlexRipper/PlexRipper/commit/63c7dcec802855d495ddd92bba3c343a2fa5b699))


### Performance Improvements

* **WebAPI:** Made the instantiation of JsonSerializerOptions more performanat by reusing instances ([5780b39](https://github.com/PlexRipper/PlexRipper/commit/5780b39783d91ca5770725b4dfe1fe0566b4b1d4))

# [0.15.0](https://github.com/PlexRipper/PlexRipper/compare/v0.14.0...v0.15.0) (2024-07-29)


### Bug Fixes

* **WebAPI:** Added FluentValidation validator to the GenerateDownloadTaskHandlers ([fd8f52b](https://github.com/PlexRipper/PlexRipper/commit/fd8f52b1a97163ce678edaeb1174947b5644dbd6))
* **WebAPI:** Added migration for deleting the Data total property ([9bc82d4](https://github.com/PlexRipper/PlexRipper/commit/9bc82d4a1233a408b03e6259fcba5af763b72309))
* **Translations:** Added missing translations for the download details dialog ([b97cf8e](https://github.com/PlexRipper/PlexRipper/commit/b97cf8e6982cbb7efe38ca240866d11e19c47745))
* **Web-UI:** Added type to imports and fixed Primevue import ([dac5740](https://github.com/PlexRipper/PlexRipper/commit/dac574073408115ccf5362c6b52c94019d5fae0e))
* **WebAPI:** Allow an id to be set when creating a PlexAccount, this will now be set to 0 ([621ab66](https://github.com/PlexRipper/PlexRipper/commit/621ab66dbc6466f3ef4e140b80ef3d06020c0040))
* **Web-UI:** Disabled dark-mode toggle as the light theme is too broken ([98d06c7](https://github.com/PlexRipper/PlexRipper/commit/98d06c7b72ee5f5bdfc4bdee1f99621ab8e27546))
* **WebAPI:** Fix the active account access refresh ([627c27e](https://github.com/PlexRipper/PlexRipper/commit/627c27e24d7744bffaa8e6e389d782cb6357e86d))
* **Web-UI:** Fixed "this" in the various Pinia stores instead of correct actions/getters reference ([47f479e](https://github.com/PlexRipper/PlexRipper/commit/47f479e969d336dbe3bb06bb1e2748f5f0a33912))
* **Web-UI:** Fixed an extra scrollbar appearing ([25e48bc](https://github.com/PlexRipper/PlexRipper/commit/25e48bce0192eadfc315f0d596f322d5381ef42a))
* **WebAPI:** Fixed an issue where an invalid PlexServer token would not lead to an error state in a download ([9cae79b](https://github.com/PlexRipper/PlexRipper/commit/9cae79b8a3e29744f19e082b3e0e7bfa7adabefc))
* **Web-UI:** Fixed an issue where the folderPath could not be confirmed in the DirectoryBrowser ([5a1e571](https://github.com/PlexRipper/PlexRipper/commit/5a1e5718b6135707d35f1eb18ebd22c89b4923c2))
* **WebAPI:** Fixed an issue where UpdateDownloadProgress could update DownloadProgress of entities that do not have those properties in the database scheme ([db76f0f](https://github.com/PlexRipper/PlexRipper/commit/db76f0f09a5931bce773aaa163cc26ef7d24c41b))
* **Web-UI:** Fixed bug in mock data generation ([fc52a60](https://github.com/PlexRipper/PlexRipper/commit/fc52a60ce0bb85249d3709823df9ab626dfc9357))
* **Web-UI:** Fixed color styling of PrimeVue elements ([c993078](https://github.com/PlexRipper/PlexRipper/commit/c9930780d8f78d9fd01b03e18606482c36263d03))
* **Web-UI:** Fixed console error: Failed to load module script: Expected a JavaScript module script but the server responded with a MIME type of "text/html". Strict MIME type checking is enforced for module scripts per HTML spec. ([54accf7](https://github.com/PlexRipper/PlexRipper/commit/54accf7f40dd2741e9d2774cc49439ae70c615ef))
* **WebAPI:** Fixed conversion error in the JobStatusUpdateMapper ([a3b9500](https://github.com/PlexRipper/PlexRipper/commit/a3b95000adc4056d15862f5e6f6d7238f4172b22))
* **WebAPI:** Fixed DbContext error where the same context could be used while it was already in use by adding DownloadTaskWorkerLogNotification ([a6bfc84](https://github.com/PlexRipper/PlexRipper/commit/a6bfc84fad19a23b83808b2a0779e83a95aecbf4))
* **WebAPI:** Fixed DownloadPreviewMapper to work with query projection ([341ecc6](https://github.com/PlexRipper/PlexRipper/commit/341ecc6112c18aaed8712f420366530d603a6552))
* **WebAPI:** Fixed DownloadQueue repicking the same downloadTask when there are only DownloadFinished ([77c52f0](https://github.com/PlexRipper/PlexRipper/commit/77c52f0eb479b65ffafa73806b7191e845a6cdbb))
* **WebAPI:** Fixed endpoint exception ([6aa66a5](https://github.com/PlexRipper/PlexRipper/commit/6aa66a53f708444af0656371fdf739255fd8e099))
* **WebAPI:** Fixed enums being sent as integers instead of strings ([1185912](https://github.com/PlexRipper/PlexRipper/commit/1185912540d421b7ef7c3f173b25961fb002bb2c))
* **Web-UI:** Fixed error "Error: SyntaxError: Unexpected token 'export' " by adding "type="module"" ([9402322](https://github.com/PlexRipper/PlexRipper/commit/94023226f82513318009cb9e6a443ec72ecf501f))
* **WebAPI:** Fixed false postive for an username already in use ([3c7bd7d](https://github.com/PlexRipper/PlexRipper/commit/3c7bd7d47b850ae6c1059313161ce8cd3967c395))
* **WebAPI:** Fixed folderPath not saving correctly due to request validation ([cd06599](https://github.com/PlexRipper/PlexRipper/commit/cd065993f4c07a9d26c8edb9355bf3d531a196da))
* **WebAPI:** Fixed Integration tests boot due to WebAppication start process changing ([4525cd8](https://github.com/PlexRipper/PlexRipper/commit/4525cd8ce4b1cc29268d67a497e18f9d524b3b96))
* **WebAPI:** Fixed integration tests ([71218d4](https://github.com/PlexRipper/PlexRipper/commit/71218d496cd7bf9f2cbba9901e104020c78c7064))
* **WebAPI:** Fixed mapping issues when sending DownloadTasksDTO's to the front-end ([513b364](https://github.com/PlexRipper/PlexRipper/commit/513b364322e4fac7646fe5616036558181a37db1))
* **WebAPI:** Fixed missing sort title, now every piece of media will always have a sort title ([d9085ff](https://github.com/PlexRipper/PlexRipper/commit/d9085ff52a246bebc7421d2ccf5a021090d69dc0))
* **WebAPI:** Fixed missing Title and FullTitle for DownloadTaskFiles ([21ea4ac](https://github.com/PlexRipper/PlexRipper/commit/21ea4ac84564727ee57fb94d63d12df89a9c3ec1))
* **WebAPI:** Fixed old reference in dockerfile ([87ae521](https://github.com/PlexRipper/PlexRipper/commit/87ae5216bb16119947c7875e5dde5b9f959f7e15))
* **WebAPI:** Fixed old reference in unit test solution file to DownloadManager ([db3d3b5](https://github.com/PlexRipper/PlexRipper/commit/db3d3b5858c6ea9763eb7b21ec37e875b628e1a7))
* **WebAPI:** Fixed open api spec signatures of several endpoints ([5c4ef1f](https://github.com/PlexRipper/PlexRipper/commit/5c4ef1f5fb9508b2ec4be411f8bc01da0c40f353))
* **WebAPI:** Fixed parent DownloadTasks status update not being set ([e94f48d](https://github.com/PlexRipper/PlexRipper/commit/e94f48d2d12f462afa9fb5ef877c75c2855bfdce))
* **WebAPI:** Fixed plex account data not being cleaned up when the account is deleted ([aa0d04b](https://github.com/PlexRipper/PlexRipper/commit/aa0d04bb794b2706bf44d8bf85a064efd10f1619))
* **WebAPI:** Fixed PlexLibraryMapper mapping errors ([6803926](https://github.com/PlexRipper/PlexRipper/commit/68039262fd0143b41757b381a21b9a74c3fca6ec))
* **WebAPI:** Fixed possible null reference exception when refreshing a library media ([4e5b5b6](https://github.com/PlexRipper/PlexRipper/commit/4e5b5b601824e99248f3d74f829e82b75a6f11a9))
* **WebAPI:** Fixed several config issues in relation to fastendpoints and swagger ([b47ab1c](https://github.com/PlexRipper/PlexRipper/commit/b47ab1ceed02764b9f237b04c5f7404ef822adb9))
* **Web-UI:** Fixed the add button misalignment when adding a new FolderPath ([180c97d](https://github.com/PlexRipper/PlexRipper/commit/180c97d0a6fed81c079e4e07d677cdc90842e6d1))
* **Web-UI:** Fixed the alphabet navigation to displaying the correct letters based on the displayed media ([3453660](https://github.com/PlexRipper/PlexRipper/commit/3453660ff64259f2548904c2db2c568dde018799))
* **WebAPI:** Fixed the autofac registration for IPlexRipperBbContext ([9f9c7dd](https://github.com/PlexRipper/PlexRipper/commit/9f9c7dd9d0bd87e7d6656d577b9b6d9d96b6d772))
* **Web-UI:** Fixed the check server status button to check all connection at once ([c885fc9](https://github.com/PlexRipper/PlexRipper/commit/c885fc9fc4be249fd2d87bc4aca2eeb63d0e2943))
* **WebAPI:** Fixed the DbContext nog being available in Auto mock during unit testing ([ad2ad80](https://github.com/PlexRipper/PlexRipper/commit/ad2ad807fb4306f5594c74a44e6f1de75f3312d7))
* **WebAPI:** Fixed the delayed api call when setting up an account ([8bac31e](https://github.com/PlexRipper/PlexRipper/commit/8bac31e85081e0dfb41f343f641eeb17334f4f4e))
* **Web-UI:** Fixed the download bar commands not working with the selected downloadtasks in the downloadTable ([bc336ae](https://github.com/PlexRipper/PlexRipper/commit/bc336ae19b4af090ce581aebdd0567eec90c21d2))
* **Web-UI:** Fixed the download details dialog not opening when there are errors ([f519526](https://github.com/PlexRipper/PlexRipper/commit/f51952636be46d1f706f16ad8461f032b7bd1738))
* **WebAPI:** Fixed the download restart command not working ([e62b8e9](https://github.com/PlexRipper/PlexRipper/commit/e62b8e98b1c05b58ca14bf6c111c4b7b4fb3336b))
* **WebAPI:** Fixed the DownloadKey mapper ([8581325](https://github.com/PlexRipper/PlexRipper/commit/8581325aac9948b85756a3b6013ea28b9a930721))
* **WebAPI:** Fixed the DownloadTasks not being ordered by CreatedAt ([91e0b8b](https://github.com/PlexRipper/PlexRipper/commit/91e0b8bcbc231a90770fc64b31be5e9be14c182f))
* **WebAPI:** Fixed the DownloadWorkerTasks not registered correctly under DownloadTasks ([b1cc46b](https://github.com/PlexRipper/PlexRipper/commit/b1cc46b7c05ecc73d970baccee5bb68d40039007))
* **Web-UI:** Fixed the file size not taking localization into account ([05bdd5a](https://github.com/PlexRipper/PlexRipper/commit/05bdd5a738727485bb674c4061a48ce839375bd1))
* **WebAPI:** Fixed the FullThumbUrl to be a valid link ([97b7269](https://github.com/PlexRipper/PlexRipper/commit/97b7269b8a9afcb2a0610dfec0cfdac2b344563a))
* **WebAPI:** Fixed the FullThumbUrl to be a valid link ([1c0ae22](https://github.com/PlexRipper/PlexRipper/commit/1c0ae22d2f72a9467fd7d951b019742abfbf55d1))
* **Web-UI:** Fixed the generated plexAccount test data not being returned ([dfa6bac](https://github.com/PlexRipper/PlexRipper/commit/dfa6bac03c4b427dc24c7254757b2ef4f7787fb4))
* **WebAPI:** Fixed the incorrect JobStatus being sent to the front-end when creating an PlexAccount ([b48fa60](https://github.com/PlexRipper/PlexRipper/commit/b48fa6049ff49b0da1e8e755c394d0e77d25816f))
* **Web-UI:** Fixed the incorrect server name being displayed in the check server connection dialog ([e9fa27e](https://github.com/PlexRipper/PlexRipper/commit/e9fa27eb22e1d7d6dbd19728b7057fcb1835f335))
* **Web-UI:** Fixed the load transition when the page hydration happens to ensure we don't give users seizure attacks ([500d0eb](https://github.com/PlexRipper/PlexRipper/commit/500d0eb199c5e78f3b3b4c0fc160079d78780d7a))
* **Web-UI:** Fixed the media image in detail view not requesting the image directly from the PlexServer ([b5b9ca0](https://github.com/PlexRipper/PlexRipper/commit/b5b9ca089b73f6494d724e94e53a32d386356925))
* **WebAPI:** Fixed the missing ThumbUrls from tvshows when requested ([145359f](https://github.com/PlexRipper/PlexRipper/commit/145359f6ec233ba7901af90abe983f1364e5782d))
* **WebAPI:** Fixed the missing types in SignalR swagger generation ([2d91d7e](https://github.com/PlexRipper/PlexRipper/commit/2d91d7e635403b78ad7d3665771a381c075a2cfb))
* **WebAPI:** Fixed the parent download ETA not displaying ([1692796](https://github.com/PlexRipper/PlexRipper/commit/16927962d6dcfd03931d1ac4071f62ca298c07df))
* **Web-UI:** Fixed the percentage in the download progress bar not being rounded to 2 decimals ([2fa6591](https://github.com/PlexRipper/PlexRipper/commit/2fa65916ce917c4859a098bb562d653ef19ea94e))
* **WebAPI:** Fixed the PlexLibraries media not loading in the front-end ([d079843](https://github.com/PlexRipper/PlexRipper/commit/d079843f0d35346ed72a25d596ecc72c657532e4))
* **Web-UI:** Fixed the preferred server connection selection not being visible in the plex server settings ([dcb325a](https://github.com/PlexRipper/PlexRipper/commit/dcb325a012e197cd430b378ef0a1ca9597626e24))
* **WebAPI:** Fixed the refreshing of tvshow media libraries not storing the data correctly on update ([eb45e33](https://github.com/PlexRipper/PlexRipper/commit/eb45e33838dd44fd3f5a892d5960c977032f1e9c))
* **Web-UI:** Fixed the scrollbar always appearing on QPage component ([fe969a1](https://github.com/PlexRipper/PlexRipper/commit/fe969a11cd0b972e54ee02ec42152ff8048e9a50))
* **Web-UI:** Fixed the scrolling issue where the page content went underneath the top bar of the page ([34a704d](https://github.com/PlexRipper/PlexRipper/commit/34a704dcfa810b04c7ad6ddfbc8fba622c70ae25))
* **Web-UI:** Fixed the server and library name not being masked in the media overview bar ([8853a65](https://github.com/PlexRipper/PlexRipper/commit/8853a65255324aa29e9fcf1c6ccbac7677d5a8f2))
* **Web-UI:** Fixed the server connections check not showing up when a PlexAccount is setup. ([4443e0b](https://github.com/PlexRipper/PlexRipper/commit/4443e0b63b6807209eb39abc0e29859007b9601a))
* **WebAPI:** Fixed the server status check to be 5 seconds instead of 10 seconds ([c7578ea](https://github.com/PlexRipper/PlexRipper/commit/c7578ea2becdfad65d876424c00cb86cf176672e))
* **WebAPI:** Fixed the usage of DownloadTaskKey when doing a FileTask query ([d256364](https://github.com/PlexRipper/PlexRipper/commit/d256364f0ad953ecce6169a2730dd37c02821268))
* **Web-UI:** Fixed the vertical outline of the server status icon ([c03654c](https://github.com/PlexRipper/PlexRipper/commit/c03654c3b9cc272f9a2755c7e1946c042f31cbe3))
* **Web-UI:** Fixed typescript errors ([8e8bf9f](https://github.com/PlexRipper/PlexRipper/commit/8e8bf9f0ae7d08feb13f04413a4211e30def8a9a))
* **Web-UI:** Fixed typescript errors ([fb7e884](https://github.com/PlexRipper/PlexRipper/commit/fb7e8848848e5a1d1329509e80264fe9fe57beb3))
* **Web-UI:** Fixed typescript errors ([6974205](https://github.com/PlexRipper/PlexRipper/commit/6974205eceb4484e3520c4426ed271665447531a))
* **WebAPI:** Fixed unit tests ([a83d245](https://github.com/PlexRipper/PlexRipper/commit/a83d245e186395a853369591249af3888f021a0b))
* **Web-UI:** Fixed Vue warning about missing prop in FolderPathsOverview.vue ([87e6d50](https://github.com/PlexRipper/PlexRipper/commit/87e6d50417e815bc22a8f38f95c09c87e702d124))
* **WebAPI:** Improved the unique number generator such that it is now guaranteed to generate an unique number ([1223d05](https://github.com/PlexRipper/PlexRipper/commit/1223d05a4800e31454a451aebeee30d238848d4d))
* **Cypress:** Increased the timeout of the page load wait due to the new way PlexRipper loads by sending the page earlier, which triggers the countdown earlier ([47efdc9](https://github.com/PlexRipper/PlexRipper/commit/47efdc98f1702eb3d66641e08996e1e1016437c4))
* **WebAPI:** Reimplemented the restart of a DownloadTask ([1e91e11](https://github.com/PlexRipper/PlexRipper/commit/1e91e118ebe861021f5b0d94c6454bde6df54311))
* **Web-UI:** Removed IMediaOverviewBus as it caused building errors and it wan't used anymore ([0077edc](https://github.com/PlexRipper/PlexRipper/commit/0077edc23d5713b0a1051e7a78ffd3e90676d0e6))
* **Web-UI:** Removed unused InspectPlexServerByPlexAccountIdJob ([baa3bd1](https://github.com/PlexRipper/PlexRipper/commit/baa3bd1cba70ee16a394eaacab5368f72818f9db))
* **WebAPI:** Reverted adding required to props as the PlexAPI is not accurate yet ([5f0991b](https://github.com/PlexRipper/PlexRipper/commit/5f0991b4c4d8433ea36f536d8bb09eb9b8f50148))
* **Web-UI:** Type error fix ([39b5878](https://github.com/PlexRipper/PlexRipper/commit/39b587802b625345d7624b18e4f06a3249cd35a0))


### Features

* **WebAPI:** Add disable background animation option in the PlexRipper settings ([a7a42d9](https://github.com/PlexRipper/PlexRipper/commit/a7a42d9bb8bddbc975b8c706d77f723f1bcc3603))
* **Web-UI:** Added a connection status icon next to the server name on the downloads page ([6531dc3](https://github.com/PlexRipper/PlexRipper/commit/6531dc366f2ce6f485a130294c8382da58b16fa0))
* **Web-UI:** Added a edit icon to te editable folder path display name to make it clear this name can be changed ([5ac7508](https://github.com/PlexRipper/PlexRipper/commit/5ac7508198680f43454f826fdb8929dc59d6d7ac))
* **Web-UI:** Added a scratchpad page for testing components under the debug navigation menu ([b9d8877](https://github.com/PlexRipper/PlexRipper/commit/b9d8877eb92a9498ec6f7526687e33008b10d793))
* **Web-UI:** Added a select all button to the downloads table for every Plex server ([5fc8711](https://github.com/PlexRipper/PlexRipper/commit/5fc8711cf4bd2f1ebc94b1c99d4779ab10bb184c))
* **Web-UI:** Added a tooltip to the connection status of servers to indicate what it means ([f1c3189](https://github.com/PlexRipper/PlexRipper/commit/f1c31891ac2ba42aa06b523874989a9b7f78a463))
* **Web-UI:** Added an option to disable the animated background effect in the UI settings ([b74d3d8](https://github.com/PlexRipper/PlexRipper/commit/b74d3d88183cf22be44e4a1848fb0c460a70af3c))
* **Web-UI:** Added an toggle option to the app bar to disable the background ([085c241](https://github.com/PlexRipper/PlexRipper/commit/085c2419dc541ae3592787d9d1473a404452f636))
* **Web-UI:** Added debug options to mask server and library names, useful when taking screenshots ([0e424b0](https://github.com/PlexRipper/PlexRipper/commit/0e424b076552c0f4b6a5c91156b40dffc5788c96))
* **WebAPI:** Added flag whether a drive/directory has read or write permission when using the directory browser ([e3f0c8e](https://github.com/PlexRipper/PlexRipper/commit/e3f0c8eaaa45c2538bf93a15d4daa1604e359645))
* **Cypress:** Added way to generate DownloadTaskDTO test data for Cypress E2E tests ([204a4dc](https://github.com/PlexRipper/PlexRipper/commit/204a4dc3c4b1e63d355323b9bf2e9bd2acc64835))
* **WebAPI:** Allow server alias to be stored in the user settings and clean-up unused methods ([f4c0ffe](https://github.com/PlexRipper/PlexRipper/commit/f4c0ffee380628827f65686d3a95dac437d8a2e8))
* **Web-UI:** Allowed the server name to be editable in the server dialog ([344660c](https://github.com/PlexRipper/PlexRipper/commit/344660cacd57d613d1e8d76b52ca740a1fd153ce))
* **Web-UI:** Created a new user interface for the Downloads page which should be more performant ([64da99f](https://github.com/PlexRipper/PlexRipper/commit/64da99fa470f5ae6449e1b50c9d99797dc8b5d06))
* **Web-UI:** Display when a directory is not readable of writable in the DirectoryBrowser ([7d845d3](https://github.com/PlexRipper/PlexRipper/commit/7d845d34bcad8c5f6d121e00a3d1c532aae92354))
* **Web-UI:** Increases font size and color of every tooltip to increase readability ([3415568](https://github.com/PlexRipper/PlexRipper/commit/34155681bf385bf5092e51d50754de63e8d3e2ab))
* **WebAPI:** Migrating the startup process to the new webapplicationbuilder ([b7acf96](https://github.com/PlexRipper/PlexRipper/commit/b7acf964b8e72ac405a84e58ea77034ce1a6e2f7))
* **WebAPI:** Setup FastEndpoints and added test endpoint ([8a3d1d5](https://github.com/PlexRipper/PlexRipper/commit/8a3d1d593df32ef23b2e4defbc9667ddb381000d))
* **WebAPI:** The FolderPaths can now be changed for DownloadTasks that have not yet been started ([8c7bc13](https://github.com/PlexRipper/PlexRipper/commit/8c7bc13dea7e48baeb7a41741a8835a848bb99a1))
* **Web-UI:** The Print debug component now has an always expanded option ([fc41634](https://github.com/PlexRipper/PlexRipper/commit/fc416349d5b0018f0ecc0233bbfbd669b6024dd8))
* **Web-UI:** Toggeling the expansion of the download rows in the download table can now be done by clicking the row ([d058b59](https://github.com/PlexRipper/PlexRipper/commit/d058b59a05e7dac2e75ca2af75657ffb878c439b))


### Performance Improvements

* **Web-UI:** Added no transition option to improve performance for the QTreeViewTable ([ae32949](https://github.com/PlexRipper/PlexRipper/commit/ae32949cb52628a1752f4d505b9dc44c80bb2028))
* **Web-UI:** Enabled pagination for the Downloads which should significantly improve performance when downloading a huge number of media ([5ccde7c](https://github.com/PlexRipper/PlexRipper/commit/5ccde7c0a5e4e10652675668ac6fc2504265a546))
* **Web-UI:** PlexRipper should load faster now by preloading the background on initial page load ([1d57be0](https://github.com/PlexRipper/PlexRipper/commit/1d57be062e884614d4853565528935e4c429c284))


### Reverts

* **Web-UI:** Revert mainAPI.ts ([3414be6](https://github.com/PlexRipper/PlexRipper/commit/3414be61833f5bafb6be81facde9897740b28be4))
* **WebAPI:** Revert OpenAPIPlex integration ([e50993a](https://github.com/PlexRipper/PlexRipper/commit/e50993ae2ef98556c82464fe0a72784a6fd4be31))

# [0.14.0](https://github.com/PlexRipper/PlexRipper/compare/v0.13.0...v0.14.0) (2023-06-22)


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
