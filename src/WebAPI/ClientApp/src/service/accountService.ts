import Log from 'consola';
import { Observable, of } from 'rxjs';
import { createAccount, deleteAccount, getAccount, getAllAccounts, updateAccount } from '@api/accountApi';
import { PlexAccountDTO } from '@dto/mainApi';
import { map, switchMap, tap } from 'rxjs/operators';
import { BaseService, GlobalService } from '@service';
import { Context } from '@nuxt/types';
import IStoreState from '@interfaces/service/IStoreState';
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

	public createPlexAccount(account: PlexAccountDTO): Observable<ResultDTO<PlexAccountDTO | null>> {
		return createAccount(account).pipe(
			tap((createdAccount) => {
				if (createdAccount.isSuccess) {
					return this.updateStore('accounts', createdAccount.value);
				}
				Log.error(`Failed to create account ${account.displayName}`, createdAccount);
			}),
		);
	}

	public updatePlexAccount(account: PlexAccountDTO, inspect: boolean = false): Observable<ResultDTO<PlexAccountDTO | null>> {
		return updateAccount(account, inspect).pipe(
			tap((updatedAccount) => {
				if (updatedAccount.isSuccess) {
					return this.updateStore('accounts', updatedAccount.value);
				}
				Log.error(`Failed to update account ${account.displayName}`, updatedAccount);
			}),
		);
	}

	public deleteAccount(accountId: number) {
		return deleteAccount(accountId).pipe(switchMap(() => this.fetchAccounts()));
	}
}

const accountService = new AccountService();
export default accountService;
