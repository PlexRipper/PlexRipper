import { Module, Mutation, Action, VuexModule } from 'vuex-module-decorators';
import Log from 'consola';
import { PlexAccountDTO, PlexServerDTO } from '@dto/mainApi';

// Doc: https://typescript.nuxtjs.org/cookbook/store.html#class-based
@Module({ name: 'userStore', namespaced: true, stateFactory: true })
export default class UserStore extends VuexModule {
	activeAccountId: number = 0;

	accounts: PlexAccountDTO[] = [];

	get getActiveAccount(): PlexAccountDTO | undefined {
		if (this.activeAccountId <= 0) {
			Log.error(`activeAccountId is invalid: ${this.activeAccountId}`);
			return undefined;
		}
		return this.accounts.find((x) => x.id === this.activeAccountId);
	}

	get getAccountId(): number {
		return this.getActiveAccount?.id ?? this.activeAccountId;
	}

	get getAccounts(): PlexAccountDTO[] {
		return this.accounts;
	}

	get getEnabledAccounts(): PlexAccountDTO[] {
		return this.accounts?.filter((x) => x.isEnabled);
	}

	get getServers(): PlexServerDTO[] {
		return this.accounts?.find((x) => x.id === this.activeAccountId)?.plexServers ?? [];
	}

	selectAccount(): void {}

	@Mutation
	setAccounts(accounts: PlexAccountDTO[]): void {
		this.accounts = accounts;

		// Set the default selected account to the first instance
		if (this.accounts.length > 0 && this.activeAccountId <= 0) {
			this.activeAccountId = this.accounts[0].id;
			Log.debug(`No account was selected so the default is: ${this.activeAccountId}`);
		}
	}

	@Action
	nuxtServerInit(): void {
		Log.debug('ServerInit');
	}
}
