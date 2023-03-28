import { Quasar } from 'quasar';
import QTreeViewTable from '@components/Common/QTreeViewTable.vue';

describe('<QTreeViewTable />', () => {
	beforeEach(() => {
		cy.resetNuxt();
	});

	it('Given `CustomPage` When component `mount` Then `body` seen', () => {
		// Arrange
		cy.stubNuxtInject('policyApi', () => Promise.resolve({ body: 'body' }));
		cy.stubNuxtInject('q', Quasar);

		// Act
		cy.mount(QTreeViewTable, {
			attrs: {
				props: {
					columns: [
						{
							label: 'Title',
							field: 'title',
							name: 'title',
							align: 'left',
							sortable: true,
							required: true,
						},
						{
							label: 'Year',
							name: 'year',
							field: 'year',
							align: 'left',
							sortable: true,
						},
						{
							label: 'Size',
							field: 'mediaSize',
							name: 'size',
							align: 'left',
							sortable: true,
						},
					],
				},
			},
		});

		// Assert
		// cy.get('[data-v-app=""] > div').should('have.text', 'body');
	});
});
