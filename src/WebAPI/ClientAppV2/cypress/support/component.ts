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

import './commands';
import { mount } from 'cypress/vue';
import { h, Suspense } from 'vue';
import { getContext } from 'unctx';
import '../../src/assets/scss/style.scss';

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

Cypress.Commands.add('mount', (component, ...args) => {
	return mount(() => {
		return h(Suspense, {}, h(component));
	}, ...args);
});

const nuxtAppCtx = getContext('nuxt-app');
const nuxtCTX = <Record<string, any>>{
	static: { data: {} },
	payload: { data: {}, _errors: {} },
	hook: () => () => ({}),
	_asyncData: {},
	_asyncDataPromises: {},
	_useHead: () => ({}),
};
nuxtAppCtx.set(nuxtCTX);

Cypress.Commands.add('stubNuxtInject', (key, value) => {
	nuxtCTX['$' + key] = value;
});

Cypress.Commands.add('resetNuxt', () => {
	for (const f in nuxtCTX) {
		if (f.startsWith('$')) {
			nuxtCTX[f] = undefined;
		}
	}
	nuxtCTX.static = { data: {} };
	nuxtCTX.payload = { data: {}, _errors: {} };
	nuxtCTX._asyncData = {};
	nuxtCTX._asyncDataPromises = {};
});
