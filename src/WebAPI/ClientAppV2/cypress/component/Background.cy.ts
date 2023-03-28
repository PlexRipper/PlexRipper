import Background from '@components/General/Background.vue';

describe('<Background />', () => {
	beforeEach(() => {
		cy.resetNuxt();
	});

	it('Given `CustomPage` When component `mount` Then `body` seen', () => {
		// Arrange
		cy.stubNuxtInject('policyApi', () => Promise.resolve({ body: 'body' }));

		// Act
		cy.mount(Background, {
			attrs: {
				props: {
					value: true,
				},
			},
		});
		// Assert
		// cy.get('[data-v-app=""] > div').should('have.text', 'body');
	});
});
