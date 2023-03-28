import QTreeViewTable from '@components/Common/QTreeViewTable.vue';

describe('<QTreeViewTable />', () => {
	beforeEach(() => {
		cy.resetNuxt();
	});

	it('Given `CustomPage` When component `mount` Then `body` seen', () => {
		// Arrange
		cy.stubNuxtInject('policyApi', () => Promise.resolve({ body: 'body' }));

		// Act
		cy.mount(QTreeViewTable);

		// Assert
		// cy.get('[data-v-app=""] > div').should('have.text', 'body');
	});
});
