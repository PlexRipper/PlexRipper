import Vue from 'vue';
import Page from '@components/Layout/Page.vue';
import Background from '@components/General/Background.vue';
import Logo from '@components/General/Logo.vue';
import PBtn from '@components/Extensions/PlexRipperButton.vue';
import PTextField from '@components/Extensions/PlexRipperTextField.vue';
import DateTime from '@components/General/DateTime.vue';
import PCheckbox from '@components/Extensions/PlexRipperCheckBox.vue';
import PSection from '@components/Layout/Section.vue';
import HelpIcon from '@components/Help/HelpIcon.vue';
import Status from '@components/General/Status.vue';
import FileSize from '@components/General/FileSize.vue';
import Duration from '@components/General/Duration.vue';
import VTreeViewTable from '@vTreeViewTable/VTreeViewTable.vue';
import MediaTypeIcon from '@components/General/MediaTypeIcon.vue';
import ConfirmationDialog from '@components/General/ConfirmationDialog.vue';
import LoadingSpinner from '@components/LoadingSpinner.vue';
import Print from '@components/DebugTools/Print.vue';

export default (): void => {
	const components = {
		Page,
		Background,
		Logo,
		PBtn,
		PTextField,
		PCheckbox,
		DateTime,
		PSection,
		HelpIcon,
		Status,
		FileSize,
		Duration,
		VTreeViewTable,
		MediaTypeIcon,
		Print,
		ConfirmationDialog,
		LoadingSpinner,
	};

	Object.entries(components).forEach(([name, component]) => {
		Vue.component(name, component);
	});
};
