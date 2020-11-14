import Vue from 'vue';
import Page from '@components/General/Page.vue';
import Background from '@components/General/Background.vue';
import Logo from '@components/General/Logo.vue';

export default (): void => {
	const components = { Page, Background, Logo };

	Object.entries(components).forEach(([name, component]) => {
		Vue.component(name, component);
	});
};
