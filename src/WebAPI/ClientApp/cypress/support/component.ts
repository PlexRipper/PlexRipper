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
import Vuetify, { VApp, VMain, VRow, VContainer, VCol } from 'vuetify/lib';
import { mount } from 'cypress/vue2';
import Vue, { Component, CreateElement } from 'vue';
import VueI18n, { I18nOptions } from 'vue-i18n';
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
// @ts-ignore
Cypress.Commands.add('mount', (component: Component<any>, args: MountOptionsArgument) => {
	const { ...props } = args;

	// pass event handlers to child component via `on` prop
	// const on = Object.entries(listeners || {}).reduce((acc, [event, handler]) => {
	// 	acc[event] = handler;
	// 	return acc;
	// }, {});
	return mount({
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
										[h(component, { ...props })],
									),
								],
							),
						],
					),
				]),
				h(Background),
			]),
	});
});
