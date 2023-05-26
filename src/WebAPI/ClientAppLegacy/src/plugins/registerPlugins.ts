import Vue from 'vue';
import vueScroll, { Config } from 'vuescroll';
import StatusIndicator from 'vue-status-indicator';
import CodeInput from 'vue-verification-code-input';

export default (): void => {
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
	Vue.use(CodeInput);
};
