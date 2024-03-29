import { defineNuxtPlugin } from '#app';
import PrimeVue from 'primevue/config';
import TreeTable from 'primevue/treetable';
import Column from 'primevue/column';
import Checkbox from 'primevue/checkbox';

export default defineNuxtPlugin((nuxtApp) => {
	nuxtApp.vueApp.use(PrimeVue, { ripple: true });
	nuxtApp.vueApp.component('TreeTable', TreeTable);
	nuxtApp.vueApp.component('Column', Column);
	nuxtApp.vueApp.component('Checkbox', Checkbox);
});
