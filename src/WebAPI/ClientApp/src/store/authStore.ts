import Log from 'consola';
import { acceptHMRUpdate, defineStore } from 'pinia';
import type { ISetupResult } from '@interfaces';
import type { Observable } from 'rxjs';
import { authenticationApi } from '@api';
import { catchError, of } from 'rxjs';
import { tap, switchMap } from 'rxjs/operators';
import { useGlobalStore } from '@store';
import { useRouter } from '#imports';

export const useAuthenticationStore = defineStore('AuthenticationStore', () => {
	const state = reactive<{ isLoggedIn: boolean }>({
		isLoggedIn: false,
	});

	const globalStore = useGlobalStore();

	const actions = {
		setup(): Observable<ISetupResult> {
			return actions.status().pipe(switchMap(() => of({
				name: 'useAuthenticationStore',
				isSuccess: state.isLoggedIn ?? false,
			})));
		},
		login(username: string, password: string, rememberMe: boolean): Observable<number> {
			const router = useRouter();

			const data = new FormData();
			data.append('username', username);
			data.append('password', password);
			data.append('rememberMe', rememberMe + '');

			// @ts-expect-error - FormData is not assignable to type 'AppUserLoginEndpointRequest'
			return authenticationApi.appUserLoginEndpoint(data).pipe(switchMap((res) => {
				if (res.isSuccess) {
					Log.info('User logged in');
					state.isLoggedIn = true;
					return globalStore.setup().pipe(tap(() => router.push('/')), switchMap(() => of(res.statusCode)));
				}
				Log.error('User login failed');
				state.isLoggedIn = false;
				return of(res.statusCode);
			}));
		},
		logout() {
			const router = useRouter();

			return authenticationApi.appUserLogOutEndpoint().pipe(
				tap((res) => Log.info('User logged out', res)),
				tap(() => router.push('/login')),
				tap(() => globalStore.$reset()));
		},
		status: () => authenticationApi.authenticationStatusEndpoint().pipe(
			tap((res) => state.isLoggedIn = res.isSuccess),
			catchError((err) => {
				state.isLoggedIn = false;
				return of(err);
			})),
		$reset: () => {
		},
	};
	const getters = {};
	return {
		...toRefs(state),
		...actions,
		...getters,
	};
});

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useAuthenticationStore, import.meta.hot));
}
