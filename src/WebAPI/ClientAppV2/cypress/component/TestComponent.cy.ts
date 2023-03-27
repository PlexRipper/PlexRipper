import TestComponent from '../../src/components/DebugTools/TestComponent.vue';

describe('<TestComponent />', () => {
	it('renders', () => {
		// see: https://on.cypress.io/mounting-vue
		cy.mount(TestComponent);
	});
});
