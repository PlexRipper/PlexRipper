import Vue from 'vue';
import Page from '@components/General/Page.vue';
import Background from '@components/General/Background.vue';
import Logo from '@components/General/Logo.vue';
import PBtn from '@components/General/PlexRipperButton.vue';
import DateTime from '@components/General/DateTime.vue';

export default (): void => {
	const components = { Page, Background, Logo, PBtn, DateTime };

	Object.entries(components).forEach(([name, component]) => {
		Vue.component(name, component);
	});
};
