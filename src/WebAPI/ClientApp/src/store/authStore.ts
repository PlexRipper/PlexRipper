import Log from 'consola';
import { acceptHMRUpdate, defineStore } from 'pinia';
import type { ISetupResult } from '@interfaces';
import type { Observable } from 'rxjs';
import { authenticationApi } from '@api';
import { of } from 'rxjs';
import { tap } from 'rxjs/operators';
import { cloneDeep } from 'lodash-es';

export const useAuthenticationStore = defineStore('AuthenticationStore', () => {
	const actions = {
		setup(): Observable<ISetupResult> {
			return of({ name: useAuthenticationStore.name, isSuccess: true });
		},
		login(username: string, password: string) {
			const data = new FormData();
			data.append('username', username);
			data.append('password', password);

			// @ts-expect-error - FormData is not assignable to type 'AppUserLoginEndpointRequest'
			return authenticationApi.appUserLoginEndpoint(data).pipe(tap((res) => {
				Log.info('User logged in', res);
			}));
		},
		logout() {
			return authenticationApi.appUserLogOutEndpoint().pipe(tap((res) => {
				Log.info('User logged out', res);
			}));
		},
		$reset() {
			Object.assign({}, cloneDeep({}));
		},
	};
	const getters = {};
	return {
		...actions,
		...getters,
	};
});

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useAuthenticationStore, import.meta.hot));
}
