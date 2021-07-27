import Log from 'consola';
import { Observable, of, combineLatest } from 'rxjs';
import { createAccount, getAccount, getAllAccounts, updateAccount } from '@api/accountApi';
import { PlexAccountDTO } from '@dto/mainApi';
import { map, switchMap, tap } from 'rxjs/operators';
import { BaseService, GlobalService, SettingsService } from '@service';
import { Context } from '@nuxt/types';
import IStoreState from '@interfaces/IStoreState';
import ResultDTO from '@dto/ResultDTO';

export class AccountService extends BaseService {
	// region Constructor and Setup

	public constructor() {
		super({
			stateSliceSelector: (state: IStoreState) => {
				return {
					accounts: state.accounts,
				};
			},
		});
	}

	public setup(nuxtContext: Context): void {
		super.setup(nuxtContext);

		GlobalService.getAxiosReady()
			.pipe(switchMap(() => this.fetchAccounts()))
			.subscribe();
	}
	// endregion

	// region Fetch

	public fetchAccounts(): Observable<PlexAccountDTO[]> {
		return getAllAccounts().pipe(
			switchMap((accountResult) => of(accountResult?.value ?? [])),
			tap((accounts) => {
				Log.debug(`AccountService => Fetch Accounts`, accounts);
				this.setState({ accounts }, 'Fetch Accounts');
			}),
		);
	}

	public fetchAccount(accountId: Number): Observable<PlexAccountDTO | null> {
		return getAccount(accountId).pipe(
			switchMap((accountResult) => of(accountResult?.value ?? null)),
			tap((account) => {
				if (account) {
					Log.debug(`AccountService => Fetch Account`, account);
					this.updateStore('accounts', account);
				}
			}),
		);
	}

	// endregion

	public getAccounts(): Observable<PlexAccountDTO[]> {
		return this.stateChanged.pipe(switchMap((x) => of(x?.accounts ?? [])));
	}

	public getAccount(accountId: number): Observable<PlexAccountDTO | null> {
		return this.getAccounts().pipe(map((x) => x?.find((x) => x.id === accountId) ?? null));
	}

	public getActiveAccount(): Observable<PlexAccountDTO | null> {
		return combineLatest([SettingsService.getActiveAccountId(), this.getAccounts()]).pipe(
			switchMap((result: [number, PlexAccountDTO[]]) => {
				const activeAccountId = result[0];
				// Check if there is an valid account
				if (activeAccountId > 0) {
					return of(result[1].find((account) => account.id === activeAccountId) ?? null);
				}
				return of(null);
			}),
		);
	}

	/**
	 * Creates/Updates the PlexAccount and stores the new result in the store.
	 * If id is 0 then it is assumed to be an account that should be created.
	 * @param {PlexAccountDTO} account
	 * @returns {Observable<ResultDTO<PlexAccountDTO | null>>}
	 */
	public createOrUpdateAccount(account: PlexAccountDTO): Observable<ResultDTO<PlexAccountDTO | null>> {
		return of(1).pipe(
			tap(() => Log.debug(`${account.id === 0 ? 'Creating' : 'Updating'} account`, account)),
			switchMap(() => {
				return account.id === 0 ? createAccount(account) : updateAccount(account);
			}),
			tap((account) => {
				if (account.isSuccess) {
					this.updateStore('accounts', account.value);
				}
			}),
		);
	}
}

const accountService = new AccountService();
export default accountService;
