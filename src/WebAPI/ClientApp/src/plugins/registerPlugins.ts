import Vue from 'vue';
import VueRx from 'vue-rx';
import vueScroll, { Config } from 'vuescroll';

import 'vue2-perfect-scrollbar/dist/vue2-perfect-scrollbar.css';
import StatusIndicator from 'vue-status-indicator/dist/vue-status-indicator.js';

export default (): void => {
	// Docs: https://github.com/vuejs/vue-rx
	Vue.use(VueRx);

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
