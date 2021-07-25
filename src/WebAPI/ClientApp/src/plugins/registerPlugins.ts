import Vue from 'vue';
import VueRx from 'vue-rx';

import PerfectScrollbar from 'vue2-perfect-scrollbar';
import 'vue2-perfect-scrollbar/dist/vue2-perfect-scrollbar.css';
import StatusIndicator from 'vue-status-indicator';

export default (): void => {
	// Docs: https://github.com/vuejs/vue-rx
	Vue.use(VueRx);

	// Docs: https://github.com/mdbootstrap/perfect-scrollbar
	// Docs: https://github.com/mercs600/vue2-perfect-scrollbar
	Vue.use(PerfectScrollbar);

	Vue.use(StatusIndicator);
};
