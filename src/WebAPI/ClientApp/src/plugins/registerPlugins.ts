import Vue from 'vue';
import VueRx from 'vue-rx';
import vueScroll, { Config } from 'vuescroll';
// import vuescroll from 'vuescroll/dist/vuescroll-native';

import PerfectScrollbar from 'vue2-perfect-scrollbar';
import 'vue2-perfect-scrollbar/dist/vue2-perfect-scrollbar.css';
import StatusIndicator from 'vue-status-indicator';

export default (): void => {
	// Docs: https://github.com/vuejs/vue-rx
	Vue.use(VueRx);

	// Docs: https://github.com/mdbootstrap/perfect-scrollbar
	// Docs: https://github.com/mercs600/vue2-perfect-scrollbar
	Vue.use(PerfectScrollbar);

	// Docs: https://vuescrolljs.yvescoding.org/guide/getting-started.html
	Vue.use(vueScroll, {
		// The global config
		ops: {
			vuescroll: {
				mode: 'native',
			},
			bar: {
				background: '#999',
			},
			rail: {
				background: '#000',
				opacity: 0.1,
				size: '8px',
			},
			scrollButton: {
				enable: false,
			},
		} as Config,
		name: 'vue-scroll',
	});
	Vue.use(StatusIndicator);
};
