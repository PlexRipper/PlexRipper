describe('page load process', () => {
	it('Should display loading logo when the page is loading', () => {
		cy.basePageSetup({
			isLoggedIn: false,
		});

		cy.visitEmptyPage();
		cy.get('#__nuxt-loader').should('exist');
	});
});
