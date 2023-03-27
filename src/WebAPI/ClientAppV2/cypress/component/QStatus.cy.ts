// import QStatus from '../../src/components/Common/QStatus.vue';
import QStatus from '@components/Common/QStatus.vue';

describe('<QStatus />', () => {
	it('renders', () => {
		// see: https://on.cypress.io/mounting-vue
		cy.mount(QStatus, {
			props: {
				value: true,
			},
		});
	});
});
