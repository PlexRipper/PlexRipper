import { Module, Mutation, Action, VuexModule } from 'vuex-module-decorators';
import IAccount from '@dto/IAccount';
import { GetAllAccounts } from '@api/accountApi';
import Log from 'consola';
import IPlexServer from '@dto/IPlexServer';

// Doc: https://typescript.nuxtjs.org/cookbook/store.html#class-based
@Module({ name: 'userStore', namespaced: true, stateFactory: true })
export default class UserStore extends VuexModule {
	activeAccountId: number = 0;

	accounts: IAccount[] = [];

	get getActiveAccount(): IAccount | undefined {
		if (this.activeAccountId <= 0) {
			Log.error(`activeAccountId is invalid: ${this.activeAccountId}`);
			return undefined;
		}
		return this.accounts.find((x) => x.id === this.activeAccountId);
	}

	get getAccounts(): IAccount[] {
		return this.accounts;
	}

	get getEnabledAccounts(): IAccount[] {
		return this.accounts.filter((x) => x.isEnabled);
	}

	get getServers(): IPlexServer[] {
		return this.accounts.find((x) => x.id === this.activeAccountId)?.plexAccount.plexServers ?? [];
	}

	selectAccount(): void {}

	@Mutation
	setAccounts(accounts: IAccount[]): void {
		this.accounts = accounts;

		// Set the default selected account to the first instance
		if (this.accounts.length > 0 && this.activeAccountId <= 0) {
			this.activeAccountId = this.accounts[0].id;
			Log.debug(`No account was selected so the default is: ${this.activeAccountId}`);
		}
	}

	@Action({ commit: 'setAccounts' })
	async refreshAccounts(): Promise<IAccount[]> {
		return await GetAllAccounts();
	}

	@Action
	nuxtServerInit(): void {
		Log.debug('ServerInit');
	}
}
