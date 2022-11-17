import Log from 'consola';
import { Observable, of } from 'rxjs';
import { map, switchMap, take, tap } from 'rxjs/operators';
import { Context } from '@nuxt/types';
import { createAccount, deleteAccount, getAccount, getAllAccounts, updateAccount } from '@api/accountApi';
import { PlexAccountDTO, PlexServerDTO } from '@dto/mainApi';
import { BaseService, GlobalService } from '@service';
import IStoreState from '@interfaces/service/IStoreState';
import { getPlexServers } from '@api/plexServerApi';

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

	public setup(nuxtContext: Context): Observable<any> {
		super.setNuxtContext(nuxtContext);
		return this.refreshAccounts().pipe(take(1));
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

	public createPlexAccount(account: PlexAccountDTO): Observable<PlexAccountDTO | null> {
		return createAccount(account).pipe(
			map((accountResult): PlexAccountDTO | null => accountResult?.value ?? null),
			tap((createdAccount) => {
				if (createdAccount) {
					return this.updateStore('accounts', createdAccount);
				}
				Log.error(`Failed to create account ${account.displayName}`, createdAccount);
			}),
			switchMap((newAccount) => this.getAccount(newAccount?.id ?? 0)),
		);
	}

	public updatePlexAccount(account: PlexAccountDTO, inspect: boolean = false): Observable<PlexAccountDTO | null> {
		return updateAccount(account, inspect).pipe(
			map((accountResult): PlexAccountDTO | null => accountResult?.value ?? null),
			tap((updatedAccount) => {
				if (updatedAccount) {
					return this.updateStore('accounts', updatedAccount);
				}
				Log.error(`Failed to update account ${account.displayName}`, updatedAccount);
			}),
			switchMap((newAccount) => this.getAccount(newAccount?.id ?? 0)),
		);
	}

	public deleteAccount(accountId: number) {
		return deleteAccount(accountId).pipe(switchMap(() => this.refreshAccounts()));
	}
}

const accountService = new AccountService();
export default accountService;
