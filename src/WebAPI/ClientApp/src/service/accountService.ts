import Log from 'consola';
import { Observable, of, combineLatest } from 'rxjs';
import { getAccount, getAllAccounts } from '@api/accountApi';
import { PlexAccountDTO } from '@dto/mainApi';
import { switchMap, tap } from 'rxjs/operators';
import { BaseService, GlobalService, SettingsService } from '@service';
import { Context } from '@nuxt/types';
import IStoreState from '@interfaces/IStoreState';

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
			.pipe(
				tap(() => Log.debug('Retrieving all accounts')),
				switchMap(() => this.fetchAccounts()),
			)
			.subscribe();
	}
	// endregion

	// region Fetch
	public fetchAccount(accountId: Number): Observable<PlexAccountDTO | null> {
		return getAccount(accountId).pipe(
			switchMap((accountResult) => of(accountResult?.value ?? null)),
			tap((account) => {
				if (account) {
					Log.debug(`AccountService => Fetch Account`, account);
					this.setState(
						{ accounts: this.getState().accounts.addOrReplace((x) => x.id === account.id, account) },
						'Account Fetched',
					);
				}
			}),
		);
	}

	public fetchAccounts(): Observable<PlexAccountDTO[]> {
		return getAllAccounts().pipe(
			switchMap((accountResult) => of(accountResult?.value ?? [])),
			tap((accounts) => {
				Log.debug(`AccountService => Fetch Accounts`, accounts);
				this.setState({ accounts }, 'Fetch Accounts');
			}),
		);
	}
	// endregion

	public getAccounts(): Observable<PlexAccountDTO[]> {
		return this.stateChanged.pipe(switchMap((x) => of(x?.accounts ?? [])));
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
}

const accountService = new AccountService();
export default accountService;
