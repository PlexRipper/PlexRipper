import Vue from 'vue';
import PerfectScrollbar from 'vue2-perfect-scrollbar';
import 'vue2-perfect-scrollbar/dist/vue2-perfect-scrollbar.css';

export default (): void => {
	// Docs: https://github.com/mdbootstrap/perfect-scrollbar
	Vue.use(PerfectScrollbar);

	// Docs: https://github.com/brockpetrie/vue-moment
	Vue.use(require('vue-moment'));
};
