import { Observable, of } from 'rxjs';
import { map, mergeMap, switchMap, take, tap } from 'rxjs/operators';
import { createAccount, deleteAccount, getAccount, getAllAccounts, updateAccount } from '@api/accountApi';
import { PlexAccountDTO } from '@dto/mainApi';
import { BaseService, ServerService, LibraryService } from '@service';
import IStoreState from '@interfaces/service/IStoreState';
import ISetupResult from '@interfaces/service/ISetupResult';

export class AccountService extends BaseService {
	// region Constructor and Setup

	public constructor() {
		super('AccountService', {
			// Note: Each service file can only have "unique" state slices which are not also used in other service files
			stateSliceSelector: (state: IStoreState) => {
				return {
					accounts: state.accounts,
				};
			},
		});
	}

	public setup(): Observable<ISetupResult> {
		super.setup();
		return this.refreshAccounts().pipe(
			switchMap(() => of({ name: this._name, isSuccess: true })),
			take(1),
		);
	}

	// endregion

	// region Fetch

	public refreshAccounts(): Observable<PlexAccountDTO[]> {
		return getAllAccounts().pipe(
			tap((plexAccounts) => {
				if (plexAccounts.isSuccess) {
					this.setStoreProperty('accounts', plexAccounts.value);
				}
			}),
			switchMap(() => this.getAccounts()),
		);
	}

	public fetchAccount(accountId: Number): Observable<PlexAccountDTO | null> {
		return getAccount(accountId).pipe(
			map((accountResult) => accountResult?.value ?? null),
			tap((account) => {
				if (account) {
					this.updateStore('accounts', account);
				}
			}),
		);
	}

	// endregion

	// region Get

	public getAccounts(): Observable<PlexAccountDTO[]> {
		return this.stateChanged.pipe(switchMap((x) => of(x?.accounts ?? [])));
	}

	public getAccount(accountId: number): Observable<PlexAccountDTO | null> {
		return this.getAccounts().pipe(map((x) => x?.find((x) => x.id === accountId) ?? null));
	}

	// endregion

	/**
	 * Creates a PlexAccount in the database, returns the new accountId and then also refreshes all the Plex Servers that are accessible
	 * @param {PlexAccountDTO} account
	 */
	public createPlexAccount(account: PlexAccountDTO): Observable<PlexAccountDTO | null> {
		return createAccount(account).pipe(
			map((accountResult): PlexAccountDTO | null => accountResult?.value ?? null),
			mergeMap((account) =>
				account ? ServerService.refreshPlexServers().pipe(switchMap(() => this.fetchAccount(account.id))) : of(null),
			),
		);
	}

	public updatePlexAccount(account: PlexAccountDTO, inspect = false): Observable<PlexAccountDTO | null> {
		return updateAccount(account, inspect).pipe(
			map((accountResult): PlexAccountDTO | null => accountResult?.value ?? null),
			mergeMap((account) =>
				account ? ServerService.refreshPlexServers().pipe(switchMap(() => this.fetchAccount(account.id))) : of(null),
			),
		);
	}

	public deleteAccount(accountId: number) {
		return deleteAccount(accountId).pipe(
			switchMap(() => this.refreshAccounts()),
			switchMap(() => ServerService.refreshPlexServers()),
			switchMap(() => LibraryService.refreshLibraries()),
		);
	}
}

const accountService = new AccountService();
export default accountService;
