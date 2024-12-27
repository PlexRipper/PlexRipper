<template>
	<QPage>
		<QCard
			dark
			class="login-card">
			<QCardSection>
				<!-- Logo -->
				<QRow justify="center">
					<QCol cols="auto">
						<Logo :size="64" />
					</QCol>
				</QRow>
				<!-- Header -->
				<QRow justify="center">
					<QCol cols="auto">
						<QText
							size="h5"
							class="q-mt-md"
							:value="$t('pages.login.header')" />
					</QCol>
				</QRow>
			</QCardSection>
			<q-card-section>
				<!-- Username Login Field -->
				<q-input
					v-model="username"
					color="red"
					full-width
					outlined
					required
					hide-bottom-space
					data-cy="login-username-input" />
				<!-- Password Login Field -->
				<PasswordInputField
					v-model="password"
					class="q-my-md"
					cy="login-password-input" />
				<!-- Invalid Credentials Error -->
				<QAlert
					v-if="invalidCredentials"
					type="error"
					cy="login-invalid-credentials-alert"
					class="q-mx-none">
					{{ $t('pages.login.invalid-credentials') }}
				</QAlert>
			</q-card-section>

			<QCardActions align="center">
				<BaseButton
					block
					cy="login-submit-button"
					label="Login"
					class="login-button"
					@click="onLogin" />
			</QCardActions>
		</QCard>
	</QPage>
</template>

<script setup lang="ts">
import { get, set } from '@vueuse/core';
import { useAuthenticationStore } from '@store';
import { useSubscription } from '@vueuse/rxjs';

const authStore = useAuthenticationStore();
const username = ref('');
const password = ref('');
const invalidCredentials = ref(false);

function onLogin() {
	useSubscription(authStore.login(get(username), get(password)).subscribe((isLoggedIn) => {
		set(invalidCredentials, !isLoggedIn);
	}));
}
</script>

<style lang="scss">
@use '@/assets/scss/_mixins.scss';

.login-card {
  @extend .background-md;
  margin: 6rem auto 0;
  max-width: 400px;

  .login-button {
    margin: 0 0.5rem 0.5rem;
  }
}
</style>
