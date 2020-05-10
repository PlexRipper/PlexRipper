import { Module, Mutation, Action, VuexModule } from 'vuex-module-decorators';
import IAccount from '@dto/IAccount';
import { GetAllAccounts } from '@api/accountApi';
import Log from 'consola';

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

	@Mutation
	setAccounts(accounts: IAccount[]): void {
		this.accounts = accounts;
	}

	@Action({ commit: 'setAccounts' })
	async refreshAccounts(): Promise<IAccount[]> {
		return await GetAllAccounts();
	}
}
