import { route, headers } from '@fixtures';
import { generateFailedResultDTO, generateResultDTO } from '@mock';
import type { PlexAccountDTO, ValidatePlexCredentialsDTO } from '@dto';
import { PlexAccountPaths } from '@api-urls';

interface WorkflowState {
	accounts: PlexAccountDTO[];
	updateRequests: Partial<PlexAccountDTO>[];
	validationRequests: Record<string, unknown>[];
	firstValidationBody?: Record<string, unknown>;
	releaseFirstValidation?: () => void;
}

const originalAccountData = (account: PlexAccountDTO): PlexAccountDTO => ({
	...account,
	customAuthenticationToken: '',
	displayName: 'Saved Plex Account',
	email: 'saved@example.com',
	hasPassword: true,
	is2Fa: false,
	isEnabled: true,
	isValidated: true,
	password: 'OriginalPassword123!',
	title: 'Saved Plex Profile',
	username: 'original-user',
	validatedAt: '2026-10-19T00:00:00.000Z',
	verificationCode: '',
});

function getEnabledAccount(accounts: PlexAccountDTO[]): PlexAccountDTO {
	const account = accounts.find((candidate) => candidate.isEnabled);
	if (!account) {
		throw new Error('Expected an enabled Plex account in the page data');
	}
	return account;
}

function credentialValidationProfile(
	account: PlexAccountDTO,
	profile: Pick<ValidatePlexCredentialsDTO, 'clientId' | 'email' | 'password' | 'title' | 'username' | 'validatedAt'>,
): ValidatePlexCredentialsDTO {
	return {
		...profile,
		is2Fa: false,
		isUnAuthorized: false,
		isValidated: true,
		authenticationToken: account.authenticationToken,
		plexId: account.plexId,
		uuid: account.uuid,
	};
}

function enterCredentials(username: string, password: string) {
	cy.getCy('account-form-username-input').clear().type(username);
	cy.getCy('account-form-password-input').clear().type(password);
}

describe('Plex credential validation freshness', () => {
	let workflow: WorkflowState;

	beforeEach(() => {
		cy.basePageSetup({
			plexAccountCount: 1,
			plexServerCount: 5,
			override: {
				plexAccounts: (accounts) => accounts.map(originalAccountData),
			},
		}).then(({ plexAccounts }) => {
			workflow = {
				accounts: plexAccounts.map((account) => ({ ...account })),
				updateRequests: [],
				validationRequests: [],
			};

			cy.intercept('GET', PlexAccountPaths.getAllPlexAccountsEndpoint(), (request) => {
				request.reply({ statusCode: 200, body: generateResultDTO(workflow.accounts), ...headers });
			}).as('getAccounts');

			cy.intercept('PUT', PlexAccountPaths.updatePlexAccountByIdEndpoint(), (request) => {
				const requestBody = request.body as Partial<PlexAccountDTO>;
				const currentAccount = workflow.accounts.find((account) => account.id === requestBody.id);
				if (!currentAccount) {
					throw new Error('The account update must target an account returned by the page data');
				}

				const updatedAccount = { ...currentAccount, ...requestBody } as PlexAccountDTO;
				workflow.accounts = workflow.accounts.map((account) => account.id === updatedAccount.id ? updatedAccount : account);
				workflow.updateRequests.push(requestBody);
				request.reply({ statusCode: 200, body: generateResultDTO(updatedAccount), ...headers });
			}).as('updateAccount');

			cy.intercept('POST', PlexAccountPaths.validatePlexCredentialsEndpoint(), (request) => {
				workflow.validationRequests.push(request.body as Record<string, unknown>);
				request.reply({
					statusCode: 500,
					body: generateFailedResultDTO({ statusCode: 500 }),
					...headers,
				});
			}).as('unexpectedValidation');

			cy.visit(route('/settings/accounts'));
		});
	});

	it('saves metadata without revalidation, then requires current credentials before updating', () => {
		cy.getPageData().then(({ plexAccounts }) => {
			cy.wait('@getAccounts');
			const account = getEnabledAccount(plexAccounts);
			const renamedAccount = { ...account, displayName: 'Metadata-only rename' };

			cy.getCy(`account-card-id-${account.id}`).should('be.visible').click();
			cy.getCy('account-dialog-form').should('be.visible');
			cy.getCy('account-form-username-input').should('have.value', account.username);
			cy.getCy('account-form-password-input').should('have.value', account.password);

			cy.getCy('account-form-display-name-input').clear().type(renamedAccount.displayName);
			cy.getCy('account-dialog-update-button').should('not.be.disabled').click();
			cy.wait('@updateAccount').then(({ request, response }) => {
				expect(response?.statusCode).to.equal(200);
				expect(request.body).to.deep.equal({ ...account, displayName: renamedAccount.displayName });
			});
			cy.wait('@getAccounts').its('response.statusCode').should('equal', 200);
			cy.then(() => expect(workflow.validationRequests).to.have.length(0));
			cy.getCy('account-dialog-form').should('not.exist');

			cy.reload();
			cy.getPageData();
			cy.wait('@getAccounts').its('response.body.value').should('deep.include', renamedAccount);
			cy.getCy(`account-card-id-${account.id}`).should('contain.text', renamedAccount.displayName).click();
			cy.getCy('account-form-username-input').should('have.value', account.username);
			cy.getCy('account-form-password-input').should('have.value', account.password);
			cy.getCy('account-dialog-update-button').should('not.be.disabled');

			const changedUsername = 'current-user-change';
			const changedPassword = 'ChangedPassword456!';
			enterCredentials(changedUsername, changedPassword);
			cy.getCy('account-dialog-update-button').should('be.disabled');
			cy.then(() => expect(workflow.updateRequests).to.have.length(1));

			const normalizedUsername = 'provider-normalized-user';
			const validatedAt = '2026-10-20T00:00:00.000Z';
			cy.validatePlexCredentialsEndpoint({
				partialData: credentialValidationProfile(account, {
					clientId: 'provider-client-id',
					email: 'provider@example.com',
					password: changedPassword,
					title: 'Provider Plex Profile',
					username: normalizedUsername,
					validatedAt,
				}),
			}).as('validateCurrentCredentials');
			cy.getCy('account-dialog-validate-button').click();
			cy.wait('@validateCurrentCredentials').then(({ request, response }) => {
				expect(response?.statusCode).to.equal(200);
				expect(request.body.username).to.equal(changedUsername);
				expect(request.body.password).to.equal(changedPassword);
			});
			cy.getCy('account-form-username-input').should('have.value', normalizedUsername);
			cy.getCy('account-dialog-update-button').should('not.be.disabled');

			const validatedAccount = {
				...renamedAccount,
				clientId: 'provider-client-id',
				email: 'provider@example.com',
				password: changedPassword,
				title: 'Provider Plex Profile',
				username: normalizedUsername,
				validatedAt,
			};
			cy.getCy('account-dialog-update-button').click();
			cy.wait('@updateAccount').then(({ request, response }) => {
				expect(response?.statusCode).to.equal(200);
				expect(request.body).to.deep.equal(validatedAccount);
			});
			cy.wait('@getAccounts').its('response.statusCode').should('equal', 200);
			cy.getCy('account-dialog-form').should('not.exist');
			cy.then(() => expect(workflow.accounts.find((item) => item.id === account.id)).to.deep.include(validatedAccount));
		});
	});

	it('ignores an earlier successful validation after the inputs change and accepts only the newer result', () => {
		cy.getPageData().then(({ plexAccounts }) => {
			cy.wait('@getAccounts');
			const account = getEnabledAccount(plexAccounts);
			const firstUsername = 'first-user-change';
			const firstPassword = 'FirstPassword123!';
			const currentUsername = 'newer-user-change';
			const currentPassword = 'NewerPassword456!';
			const firstValidation = credentialValidationProfile(account, {
				clientId: 'stale-client-id',
				email: 'stale@example.com',
				password: firstPassword,
				title: 'Stale Provider Profile',
				username: 'stale-provider-user',
				validatedAt: '2026-10-20T00:01:00.000Z',
			});

			cy.getCy(`account-card-id-${account.id}`).click();
			cy.getCy('account-dialog-form').should('be.visible');
			enterCredentials(firstUsername, firstPassword);
			cy.getCy('account-dialog-update-button').should('be.disabled');

			cy.intercept('POST', PlexAccountPaths.validatePlexCredentialsEndpoint(), (request) => {
				workflow.validationRequests.push(request.body as Record<string, unknown>);
				workflow.firstValidationBody = request.body as Record<string, unknown>;
				return new Promise<void>((resolve) => {
					workflow.releaseFirstValidation = () => {
						workflow.releaseFirstValidation = undefined;
						request.reply({ statusCode: 200, body: generateResultDTO(firstValidation), ...headers });
						resolve();
					};
				});
			}).as('firstValidation');

			cy.getCy('account-dialog-validate-button').click();
			cy.wrap(null).should(() => expect(workflow.releaseFirstValidation).to.be.a('function'));
			cy.then(() => expect(workflow.firstValidationBody).to.deep.include({ username: firstUsername, password: firstPassword }));

			enterCredentials(currentUsername, currentPassword);
			cy.getCy('account-dialog-update-button').should('be.disabled');
			cy.then(() => workflow.releaseFirstValidation?.());
			cy.wait('@firstValidation');
			cy.getCy('account-form-username-input').should('have.value', currentUsername);
			cy.getCy('account-form-password-input').should('have.value', currentPassword);
			cy.getCy('account-dialog-update-button').should('be.disabled');
			cy.getCy('account-dialog-validate-button').should('not.have.attr', 'aria-label', 'Validated');

			cy.validatePlexCredentialsEndpoint({
				partialData: credentialValidationProfile(account, {
					clientId: 'newer-client-id',
					email: 'newer@example.com',
					password: currentPassword,
					title: 'Newer Provider Profile',
					username: 'newer-provider-user',
					validatedAt: '2026-10-20T00:02:00.000Z',
				}),
			}).as('validateNewerCredentials');
			cy.getCy('account-dialog-validate-button').click();
			cy.wait('@validateNewerCredentials').then(({ request, response }) => {
				expect(response?.statusCode).to.equal(200);
				expect(request.body.username).to.equal(currentUsername);
				expect(request.body.password).to.equal(currentPassword);
			});
			cy.getCy('account-form-username-input').should('have.value', 'newer-provider-user');
			cy.getCy('account-dialog-update-button').should('not.be.disabled');
			cy.then(() => expect(workflow.updateRequests).to.have.length(0));
		});
	});
});
