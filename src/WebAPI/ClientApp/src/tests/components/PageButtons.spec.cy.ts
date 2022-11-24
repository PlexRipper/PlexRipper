import Vue from 'vue';
import Log from 'consola';
import { BaseButton, NavigationNextButton, NavigationFinishSetupButton, NavigationPreviousButton } from '@buttons';

describe('PageButtons.spec.cy.ts', () => {
	it('playground', () => {
		cy.mount(
			Vue.extend({
				render(h, listeners) {
					const buttons = [
						h(BaseButton),
						h(NavigationPreviousButton),
						h(NavigationNextButton),
						h(NavigationFinishSetupButton),
					];
					return h('div', buttons);
				},
			}),
		);
	});
});
