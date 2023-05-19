describe('PlexRipper Server Dialog', () => {
	beforeEach(() => {
		cy.basePageSetup({
			plexAccountCount: 1,
			plexServerCount: 5,
			plexLibraryCount: 5,
		});

		cy.visitEmptyPage();
	});

	it('Should navigate the server dialog tabs when the navigation tabs are used and then close again', () => {
		cy.getCy('server-dialog-2').click();

		for (let i = 1; i <= 5; i++) {
			cy.getCy('server-dialog-tab-' + i).click();
			cy.getCy('server-dialog-tab-content-' + i)
				.should('exist')
				.and('be.visible');
		}

		cy.getCy('server-dialog-close-btn').click();
		cy.getCy('server-dialog-cy').should('not.exist');
	});
});
