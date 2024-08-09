PlexRipper Changelog

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
