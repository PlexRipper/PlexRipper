import Log from 'consola';
import { acceptHMRUpdate, defineStore } from 'pinia';
import type { ISetupResult } from '@interfaces';
import type { Observable } from 'rxjs';
import { authenticationApi } from '@api';
import { catchError, of } from 'rxjs';
import { tap, switchMap } from 'rxjs/operators';
import { useGlobalStore } from '@store';
import { get } from '@vueuse/core';
import { useRouter } from '#imports';

export const useAuthenticationStore = defineStore('AuthenticationStore', () => {
	const state = reactive<{
		isLoggedIn: boolean;
		currentUsername: string;
		username: string;
		currentPassword: string;
		password: string;
		confirmPassword: string;
	}>({
		isLoggedIn: false,
		currentUsername: '',
		username: '',
		currentPassword: '',
		password: '',
		confirmPassword: '',
	});

	const globalStore = useGlobalStore();

	const actions = {
		setup(): Observable<ISetupResult> {
			return actions.status().pipe(switchMap(() => of({
				name: 'useAuthenticationStore',
				isSuccess: state.isLoggedIn ?? false,
			})));
		},
		refreshCredentials() {
			return authenticationApi.getAppCredentials().pipe(
				tap((res) => {
					if (res.isSuccess && res.value) {
						state.currentUsername = res.value.userName;
						state.username = res.value.userName;
						state.currentPassword = res.value.password;
						state.password = res.value.password;
						state.confirmPassword = '';
					}
				}));
		},
		updateCredentials() {
			const usernameChanged = get(getters.hasUsernameChanged);
			const passwordChanged = get(getters.hasPasswordChanged);

			if (!usernameChanged && !passwordChanged) {
				return of(null);
			}
			return authenticationApi.updateCredentialsEndpoint({
				username: usernameChanged ? state.username : null,
				password: passwordChanged ? state.password : null,
			}).pipe(
				tap((res) => Log.info('User credentials updated', res)),
				switchMap(() => actions.refreshCredentials()),
				catchError((err) => {
					Log.error('User credentials update failed', err);
					return of(err);
				}));
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
			state.currentUsername = '';
			state.username = '';
			state.currentPassword = '';
			state.password = '';
			state.confirmPassword = '';
		},
	};
	const getters = {
		hasUsernameChanged: computed(() => state.currentUsername !== state.username),
		hasPasswordChanged: computed(() => state.currentPassword !== state.password),
		canUpdateCredentials: computed(() => (get(getters.hasUsernameChanged) || get(getters.hasPasswordChanged)) && state.password == state.confirmPassword),
	};
	return {
		...toRefs(state),
		...actions,
		...getters,
	};
});

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useAuthenticationStore, import.meta.hot));
}
