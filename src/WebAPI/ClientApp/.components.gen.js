import Vue from 'vue'
import Print from '@/components/DebugTools/Print.vue'
import AppBar from '@/components/AppBar/AppBar.vue'
import AppBarProgressBar from '@/components/AppBar/AppBarProgressBar.vue'
import NotificationButton from '@/components/AppBar/NotificationButton.vue'
import AlertDialog from '@/components/Dialogs/AlertDialog.vue'
import Footer from '@/components/Footer/Footer.vue'
import PBtn from '@/components/Extensions/PBtn.vue'
import PCheckbox from '@/components/Extensions/PCheckbox.vue'
import PSelect from '@/components/Extensions/PSelect.vue'
import PTextField from '@/components/Extensions/PTextField.vue'
import DownloadLimitInput from '@/components/Form/DownloadLimitInput.vue'
import EditableText from '@/components/Form/EditableText.vue'
import Background from '@/components/General/Background.vue'
import ConfirmationDialog from '@/components/General/ConfirmationDialog.vue'
import DarkModeToggle from '@/components/General/DarkModeToggle.vue'
import DateTime from '@/components/General/DateTime.vue'
import DirectoryBrowser from '@/components/General/DirectoryBrowser.vue'
import Duration from '@/components/General/Duration.vue'
import ExternalLink from '@/components/General/ExternalLink.vue'
import FileSize from '@/components/General/FileSize.vue'
import LoadingSpinner from '@/components/General/LoadingSpinner.vue'
import Logo from '@/components/General/Logo.vue'
import MediaTypeIcon from '@/components/General/MediaTypeIcon.vue'
import PageLoadOverlay from '@/components/General/PageLoadOverlay.vue'
import Status from '@/components/General/Status.vue'
import ValidIcon from '@/components/General/ValidIcon.vue'
import VerticalButton from '@/components/General/VerticalButton.vue'
import HelpDialog from '@/components/Help/HelpDialog.vue'
import HelpIcon from '@/components/Help/HelpIcon.vue'
import PSection from '@/components/Layout/PSection.vue'
import PageContainer from '@/components/Layout/PageContainer.vue'
import DetailsOverview from '@/components/MediaOverview/DetailsOverview.vue'
import MediaOverview from '@/components/MediaOverview/MediaOverview.vue'
import MediaOverviewBar from '@/components/MediaOverview/MediaOverviewBar.vue'
import AlphabetNavigation from '@/components/Navigation/AlphabetNavigation.vue'
import NavigationDrawer from '@/components/Navigation/NavigationDrawer.vue'
import ServerDialog from '@/components/Navigation/ServerDialog.vue'
import ServerDrawer from '@/components/Navigation/ServerDrawer.vue'
import AccountSetupProgress from '@/components/Progress/AccountSetupProgress.vue'
import ProgressComponent from '@/components/Progress/ProgressComponent.vue'
import NotificationsDrawer from '@/components/overviews/NotificationsDrawer.vue'
import PathsCustomOverview from '@/components/overviews/PathsCustomOverview.vue'
import PathsDefaultOverview from '@/components/overviews/PathsDefaultOverview.vue'
import VTreeViewTable from '@/components/General/VTreeViewTable/VTreeViewTable.vue'
import DownloadConfirmation from '@/components/MediaOverview/MediaTable/DownloadConfirmation.vue'
import MediaTable from '@/components/MediaOverview/MediaTable/MediaTable.vue'
import MediaPoster from '@/components/MediaOverview/PosterTable/MediaPoster.vue'
import PosterTable from '@/components/MediaOverview/PosterTable/PosterTable.vue'
import DownloadBar from '@/components/Views/Downloads/DownloadBar.vue'
import DownloadDetailsDialog from '@/components/Views/Downloads/DownloadDetailsDialog.vue'
import DownloadsTable from '@/components/Views/Downloads/DownloadsTable.vue'
import ServerDownloadStatus from '@/components/Views/Downloads/ServerDownloadStatus.vue'
import ConfirmationSection from '@/components/Views/Settings/ConfirmationSection.vue'
import DatabaseSection from '@/components/Views/Settings/DatabaseSection.vue'
import DateAndTimeSection from '@/components/Views/Settings/DateAndTimeSection.vue'
import DebugSection from '@/components/Views/Settings/DebugSection.vue'
import DownloadManagerSection from '@/components/Views/Settings/DownloadManagerSection.vue'
import LanguageSection from '@/components/Views/Settings/LanguageSection.vue'
import SetupSection from '@/components/Views/Settings/SetupSection.vue'
import NavigationBar from '@/components/Views/Setup/NavigationBar.vue'
import AccountCard from '@/components/overviews/AccountOverview/AccountCard.vue'
import AccountDialog from '@/components/overviews/AccountOverview/AccountDialog.vue'
import AccountForm from '@/components/overviews/AccountOverview/AccountForm.vue'
import AccountOverview from '@/components/overviews/AccountOverview/AccountOverview.vue'
import AccountVerificationCodeDialog from '@/components/overviews/AccountOverview/AccountVerificationCodeDialog.vue'
import ServerCommandsTabContent from '@/components/Navigation/ServerDialog/Tabs/ServerCommandsTabContent.vue'
import ServerConfigTabContent from '@/components/Navigation/ServerDialog/Tabs/ServerConfigTabContent.vue'
import ServerDataTabContent from '@/components/Navigation/ServerDialog/Tabs/ServerDataTabContent.vue'
import ServerLibraryDestinationsTabContent from '@/components/Navigation/ServerDialog/Tabs/ServerLibraryDestinationsTabContent.vue'
import Pages from '@/e'
import Downloads from '@/e'
import MoviesId from '@/e'
import Movies from '@/e'
import MusicId from '@/e'
import Music from '@/e'
import Accounts from '@/e'
import Advanced from '@/e'
import Debug from '@/e'
import Paths from '@/e'
import Ui from '@/e'
import Setup from '@/e'
import Id from '@/e'
import Tvshows from '@/e'

Vue.component('Print', Print)
Vue.component('LazyPrint', Print)
Vue.component('AppBar', AppBar)
Vue.component('LazyAppBar', AppBar)
Vue.component('AppBarProgressBar', AppBarProgressBar)
Vue.component('LazyAppBarProgressBar', AppBarProgressBar)
Vue.component('NotificationButton', NotificationButton)
Vue.component('LazyNotificationButton', NotificationButton)
Vue.component('AlertDialog', AlertDialog)
Vue.component('LazyAlertDialog', AlertDialog)
Vue.component('Footer', Footer)
Vue.component('LazyFooter', Footer)
Vue.component('PBtn', PBtn)
Vue.component('LazyPBtn', PBtn)
Vue.component('PCheckbox', PCheckbox)
Vue.component('LazyPCheckbox', PCheckbox)
Vue.component('PSelect', PSelect)
Vue.component('LazyPSelect', PSelect)
Vue.component('PTextField', PTextField)
Vue.component('LazyPTextField', PTextField)
Vue.component('DownloadLimitInput', DownloadLimitInput)
Vue.component('LazyDownloadLimitInput', DownloadLimitInput)
Vue.component('EditableText', EditableText)
Vue.component('LazyEditableText', EditableText)
Vue.component('Background', Background)
Vue.component('LazyBackground', Background)
Vue.component('ConfirmationDialog', ConfirmationDialog)
Vue.component('LazyConfirmationDialog', ConfirmationDialog)
Vue.component('DarkModeToggle', DarkModeToggle)
Vue.component('LazyDarkModeToggle', DarkModeToggle)
Vue.component('DateTime', DateTime)
Vue.component('LazyDateTime', DateTime)
Vue.component('DirectoryBrowser', DirectoryBrowser)
Vue.component('LazyDirectoryBrowser', DirectoryBrowser)
Vue.component('Duration', Duration)
Vue.component('LazyDuration', Duration)
Vue.component('ExternalLink', ExternalLink)
Vue.component('LazyExternalLink', ExternalLink)
Vue.component('FileSize', FileSize)
Vue.component('LazyFileSize', FileSize)
Vue.component('LoadingSpinner', LoadingSpinner)
Vue.component('LazyLoadingSpinner', LoadingSpinner)
Vue.component('Logo', Logo)
Vue.component('LazyLogo', Logo)
Vue.component('MediaTypeIcon', MediaTypeIcon)
Vue.component('LazyMediaTypeIcon', MediaTypeIcon)
Vue.component('PageLoadOverlay', PageLoadOverlay)
Vue.component('LazyPageLoadOverlay', PageLoadOverlay)
Vue.component('Status', Status)
Vue.component('LazyStatus', Status)
Vue.component('ValidIcon', ValidIcon)
Vue.component('LazyValidIcon', ValidIcon)
Vue.component('VerticalButton', VerticalButton)
Vue.component('LazyVerticalButton', VerticalButton)
Vue.component('HelpDialog', HelpDialog)
Vue.component('LazyHelpDialog', HelpDialog)
Vue.component('HelpIcon', HelpIcon)
Vue.component('LazyHelpIcon', HelpIcon)
Vue.component('PSection', PSection)
Vue.component('LazyPSection', PSection)
Vue.component('PageContainer', PageContainer)
Vue.component('LazyPageContainer', PageContainer)
Vue.component('DetailsOverview', DetailsOverview)
Vue.component('LazyDetailsOverview', DetailsOverview)
Vue.component('MediaOverview', MediaOverview)
Vue.component('LazyMediaOverview', MediaOverview)
Vue.component('MediaOverviewBar', MediaOverviewBar)
Vue.component('LazyMediaOverviewBar', MediaOverviewBar)
Vue.component('AlphabetNavigation', AlphabetNavigation)
Vue.component('LazyAlphabetNavigation', AlphabetNavigation)
Vue.component('NavigationDrawer', NavigationDrawer)
Vue.component('LazyNavigationDrawer', NavigationDrawer)
Vue.component('ServerDialog', ServerDialog)
Vue.component('LazyServerDialog', ServerDialog)
Vue.component('ServerDrawer', ServerDrawer)
Vue.component('LazyServerDrawer', ServerDrawer)
Vue.component('AccountSetupProgress', AccountSetupProgress)
Vue.component('LazyAccountSetupProgress', AccountSetupProgress)
Vue.component('ProgressComponent', ProgressComponent)
Vue.component('LazyProgressComponent', ProgressComponent)
Vue.component('NotificationsDrawer', NotificationsDrawer)
Vue.component('LazyNotificationsDrawer', NotificationsDrawer)
Vue.component('PathsCustomOverview', PathsCustomOverview)
Vue.component('LazyPathsCustomOverview', PathsCustomOverview)
Vue.component('PathsDefaultOverview', PathsDefaultOverview)
Vue.component('LazyPathsDefaultOverview', PathsDefaultOverview)
Vue.component('VTreeViewTable', VTreeViewTable)
Vue.component('LazyVTreeViewTable', VTreeViewTable)
Vue.component('DownloadConfirmation', DownloadConfirmation)
Vue.component('LazyDownloadConfirmation', DownloadConfirmation)
Vue.component('MediaTable', MediaTable)
Vue.component('LazyMediaTable', MediaTable)
Vue.component('MediaPoster', MediaPoster)
Vue.component('LazyMediaPoster', MediaPoster)
Vue.component('PosterTable', PosterTable)
Vue.component('LazyPosterTable', PosterTable)
Vue.component('DownloadBar', DownloadBar)
Vue.component('LazyDownloadBar', DownloadBar)
Vue.component('DownloadDetailsDialog', DownloadDetailsDialog)
Vue.component('LazyDownloadDetailsDialog', DownloadDetailsDialog)
Vue.component('DownloadsTable', DownloadsTable)
Vue.component('LazyDownloadsTable', DownloadsTable)
Vue.component('ServerDownloadStatus', ServerDownloadStatus)
Vue.component('LazyServerDownloadStatus', ServerDownloadStatus)
Vue.component('ConfirmationSection', ConfirmationSection)
Vue.component('LazyConfirmationSection', ConfirmationSection)
Vue.component('DatabaseSection', DatabaseSection)
Vue.component('LazyDatabaseSection', DatabaseSection)
Vue.component('DateAndTimeSection', DateAndTimeSection)
Vue.component('LazyDateAndTimeSection', DateAndTimeSection)
Vue.component('DebugSection', DebugSection)
Vue.component('LazyDebugSection', DebugSection)
Vue.component('DownloadManagerSection', DownloadManagerSection)
Vue.component('LazyDownloadManagerSection', DownloadManagerSection)
Vue.component('LanguageSection', LanguageSection)
Vue.component('LazyLanguageSection', LanguageSection)
Vue.component('SetupSection', SetupSection)
Vue.component('LazySetupSection', SetupSection)
Vue.component('NavigationBar', NavigationBar)
Vue.component('LazyNavigationBar', NavigationBar)
Vue.component('AccountCard', AccountCard)
Vue.component('LazyAccountCard', AccountCard)
Vue.component('AccountDialog', AccountDialog)
Vue.component('LazyAccountDialog', AccountDialog)
Vue.component('AccountForm', AccountForm)
Vue.component('LazyAccountForm', AccountForm)
Vue.component('AccountOverview', AccountOverview)
Vue.component('LazyAccountOverview', AccountOverview)
Vue.component('AccountVerificationCodeDialog', AccountVerificationCodeDialog)
Vue.component('LazyAccountVerificationCodeDialog', AccountVerificationCodeDialog)
Vue.component('ServerCommandsTabContent', ServerCommandsTabContent)
Vue.component('LazyServerCommandsTabContent', ServerCommandsTabContent)
Vue.component('ServerConfigTabContent', ServerConfigTabContent)
Vue.component('LazyServerConfigTabContent', ServerConfigTabContent)
Vue.component('ServerDataTabContent', ServerDataTabContent)
Vue.component('LazyServerDataTabContent', ServerDataTabContent)
Vue.component('ServerLibraryDestinationsTabContent', ServerLibraryDestinationsTabContent)
Vue.component('LazyServerLibraryDestinationsTabContent', ServerLibraryDestinationsTabContent)
Vue.component('Pages', Pages)
Vue.component('LazyPages', Pages)
Vue.component('Downloads', Downloads)
Vue.component('LazyDownloads', Downloads)
Vue.component('MoviesId', MoviesId)
Vue.component('LazyMoviesId', MoviesId)
Vue.component('Movies', Movies)
Vue.component('LazyMovies', Movies)
Vue.component('MusicId', MusicId)
Vue.component('LazyMusicId', MusicId)
Vue.component('Music', Music)
Vue.component('LazyMusic', Music)
Vue.component('Accounts', Accounts)
Vue.component('LazyAccounts', Accounts)
Vue.component('Advanced', Advanced)
Vue.component('LazyAdvanced', Advanced)
Vue.component('Debug', Debug)
Vue.component('LazyDebug', Debug)
Vue.component('Paths', Paths)
Vue.component('LazyPaths', Paths)
Vue.component('Ui', Ui)
Vue.component('LazyUi', Ui)
Vue.component('Setup', Setup)
Vue.component('LazySetup', Setup)
Vue.component('Id', Id)
Vue.component('LazyId', Id)
Vue.component('Tvshows', Tvshows)
Vue.component('LazyTvshows', Tvshows)
