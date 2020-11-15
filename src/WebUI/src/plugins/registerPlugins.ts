import Vue from 'vue';
import PerfectScrollbar from 'vue2-perfect-scrollbar';
import 'vue2-perfect-scrollbar/dist/vue2-perfect-scrollbar.css';
import { Context } from '@nuxt/types';
import { Inject } from '@nuxt/types/app';

export default (ctx: Context, inject: Inject): void => {
	// Docs: https://github.com/mdbootstrap/perfect-scrollbar
	Vue.use(PerfectScrollbar);

	// Docs: https://github.com/brockpetrie/vue-moment
	Vue.use(require('vue-moment'));
};
