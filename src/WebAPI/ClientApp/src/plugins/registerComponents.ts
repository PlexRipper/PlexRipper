import Vue from 'vue';
import Page from '@components/General/Page.vue';
import Background from '@components/General/Background.vue';
import Logo from '@components/General/Logo.vue';
import PBtn from '@components/General/PlexRipperButton.vue';
import DateTime from '@components/General/DateTime.vue';
import PCheckbox from '@components/General/PlexRipperCheckBox.vue';
import PSection from '@components/General/Section.vue';
import HelpIcon from '@components/Help/HelpIcon.vue';
import Status from '@components/General/Status.vue';
import FileSize from '@components/General/FileSize.vue';

export default (): void => {
	const components = { Page, Background, Logo, PBtn, PCheckbox, DateTime, PSection, HelpIcon, Status, FileSize };

	Object.entries(components).forEach(([name, component]) => {
		Vue.component(name, component);
	});
};
