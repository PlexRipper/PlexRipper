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
				<q-input
					v-model="username"
					color="red"
					full-width
					outlined
					required
					hide-bottom-space
					data-cy="login-username-input" />
				<PasswordInputField
					v-model="password"
					class="q-my-md"
					cy="login-password-input" />
			</q-card-section>

			<QCardActions align="center">
				<BaseButton
					block
					label="Login"
					class="login-button"
					@click="onLogin" />
			</QCardActions>
		</QCard>
	</QPage>
</template>

<script setup lang="ts">
import { get } from '@vueuse/core';
import { useAuthenticationStore } from '@store';
import { useSubscription } from '@vueuse/rxjs';

const authStore = useAuthenticationStore();
const username = ref('PlexRipperRocks');
const password = ref('Pl€XR!ℙℙ€R69');

function onLogin() {
	useSubscription(authStore.login(get(username), get(password)).subscribe());
}
</script>

<style lang="scss">
@use '@/assets/scss/_mixins.scss';

.login-card {
  @extend .background-md;
  margin: 50px auto 0;
  max-width: 400px;

  .login-button {
    margin: 0 0.5rem 0.5rem;
  }
}
</style>
