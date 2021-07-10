import Log from 'consola';
import { Observable, of, combineLatest } from 'rxjs';
import { getAllAccounts } from '@api/accountApi';
import { PlexAccountDTO } from '@dto/mainApi';
import { finalize, switchMap, tap } from 'rxjs/operators';
import SettingsService from '@state/settingsService';
import { BaseService } from '@state/baseService';
import IStoreState from '@interfaces/IStoreState';
import GlobalService from '@state/globalService';
import { Context } from '@nuxt/types';

export class AccountService extends BaseService {
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
				finalize(() => this.fetchAccounts()),
			)
			.subscribe();
	}

	public fetchAccounts(): void {
		getAllAccounts().subscribe((accounts) => {
			Log.debug(`AccountService => Fetch Accounts`, accounts);
			this.setState({ accounts });
		});
	}

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
