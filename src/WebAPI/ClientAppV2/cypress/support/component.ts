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

import 'quasar/dist/quasar.css';
import '../../src/assets/scss/style.scss';

import { mount } from 'cypress/vue';
import { h, Suspense } from 'vue';
import { getContext } from 'unctx';
import { Quasar } from 'quasar';
// @ts-ignore
import ComponentTestContainer from '@components/DebugTools/ComponentTestContainer.vue';

type MountParams = Parameters<typeof mount>;
type OptionsParam = MountParams[1];

export type Class = new (...args: any) => any;

declare global {
	namespace Cypress {
		interface Chainable {
			mount(component: any, options?: OptionsParam): Chainable<any>;

			stubNuxtInject(key: string, value: any): void;

			resetNuxt(): void;
		}
	}
}
const $q = Quasar;

Cypress.Commands.add('mount', (component, options = {}) => {
	options.global = options.global || {};
	options.global.provide = {
		$q,
	};
	options.global.plugins = [Quasar];
	// const props = options?.attrs?.props;

	return mount(() => {
		return h(Suspense, {}, [h(ComponentTestContainer, [h(component)])]);
	});
});

const nuxtAppCtx = getContext('nuxt-app');

const generateNuxtCTX = (): Record<string, any> => ({
	static: { data: {} },
	payload: { data: {}, _errors: {} },
	hook: () => () => ({}),
	hooks: {
		callHook: () => Promise.resolve(),
	},
	_asyncData: {},
	_asyncDataPromises: {},
	_useHead: () => ({}),
	$q,
});

let nuxtCTX = generateNuxtCTX();

nuxtAppCtx.set(nuxtCTX);

Cypress.Commands.add('stubNuxtInject', (key, value) => {
	nuxtCTX['$' + key] = value;
});

Cypress.Commands.add('resetNuxt', () => {
	nuxtCTX = generateNuxtCTX();
	nuxtAppCtx.unset();
	nuxtAppCtx.set(nuxtCTX);
});
