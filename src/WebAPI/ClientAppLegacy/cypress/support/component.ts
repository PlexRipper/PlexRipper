// ***********************************************************
// This example support/component.ts is processed and
// loaded automatically before your test files.
//
// This is a great place to put global configuration and
// behavior that modifies Cypress.
//
// You can change the location of this file or turn off
// automatically serving support files with the
// 'supportFile' configuration option.
//
// You can read more here:
// https://on.cypress.io/configuration
// ***********************************************************

// Doc: https://docs.cypress.io/guides/component-testing/vue/examples#Replicating-the-expected-Component-Hierarchy
import Vuetify, { VApp, VCol, VContainer, VMain, VRow } from 'vuetify/lib';
import { mount } from 'cypress/vue2';
import Vue, { Component, CreateElement } from 'vue';
import VueI18n from 'vue-i18n';
import Background from '../../src/components/General/Background.vue';
import 'vuetify/dist/vuetify.min.css';
import '../../src/assets/scss/style.scss';

// Import commands.js using ES2015 syntax:
import './commands';
import enUs from '../../src/lang/en-US.json';
import { vuetifyConfigOptions } from '~/plugins/vuetify';

Vue.use(Vuetify);
Vue.use(VueI18n);

// Override default command mount to use it with Vuetify

declare global {
	// eslint-disable-next-line @typescript-eslint/no-namespace
	namespace Cypress {
		interface Chainable {
			mount: typeof mount;
		}
	}
}

// @ts-ignore
Cypress.Commands.add('mount', (component: Component<any>, ...args) => {
	// args.global = args.global || {}
	// args.global.plugins = args.global.plugins || []
	// args.global.plugins.push(createPinia())
	// args.global.plugins.push(createI18n())

	return mount(
		{
			vuetify: new Vuetify(vuetifyConfigOptions),
			i18n: new VueI18n({
				locale: 'en-US', // set locale
				messages: {
					'en-US': enUs,
				},
			}),
			render: (h: CreateElement) =>
				h(VApp, [
					h(VMain, [
						h(
							VContainer,
							{
								class: {
									'fill-height': true,
								},
							},
							[
								h(
									VRow,
									{
										props: {
											noGutters: true,
											justify: 'center',
										},
									},
									[
										h(
											VCol,
											{
												props: {
													cols: 'auto',
												},
											},
											[h(component)],
										),
									],
								),
							],
						),
					]),
					h(Background),
				]),
		},
		...args,
	);
});
