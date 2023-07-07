import { defineNuxtPlugin } from '#app';
import PrimeVue from 'primevue/config';
import TreeTable from 'primevue/treetable';
import Column from 'primevue/column';
import Checkbox from 'primevue/checkbox';

export default defineNuxtPlugin((nuxtApp) => {
	nuxtApp.vueApp.use(PrimeVue, { ripple: true, theme: 'plexripper-theme' });
	nuxtApp.vueApp.component('PTreeTable', TreeTable);
	nuxtApp.vueApp.component('PColumn', Column);
	nuxtApp.vueApp.component('Checkbox', Checkbox);
});
