<template>
	<QCardDialog
		:name="DialogType.AccountGenerateTokenDialog"
		persistent
		width="700px"
		cy="generate-token-dialog"
		@opened="onOpen">
		<template #title>
			{{ $t('components.account-token-validate-dialog.title') }}
		</template>
		<template #top-row>
			<QSubHeader v-if="accountDialogStore.isValidated">
				{{ $t('components.account-token-validate-dialog.sub-header',
					{ authToken: accountDialogStore.authenticationToken })
				}}
			</QSubHeader>
		</template>
		<template #default>
			<div>
				<!--	Verification Code input	-->
				<QRow
					v-if="needVerificationToken"
					justify="center">
					<QCol cols="auto">
						<VOtpInput
							v-model:value="verificationCodeValue"
							input-classes="otp-input"
							separator=""
							:num-inputs="6"
							:should-auto-focus="true"
							input-type="number"
							inputmode="numeric"
							data-cy="2fa-code-verification-input"
							:conditional-class="['one', 'two', 'three', 'four', 'five', 'six']"
							@on-complete="onComplete" />
					</QCol>
				</QRow>
				<q-input
					v-else
					:model-value="generatedToken"
					data-cy="generate-token-dialog-token-input"
					readonly>
					<template #append>
						<IconButton
							cy="generate-token-dialog-copy-button"
							icon="mdi-content-copy"
							@click="copy(generatedToken)" />
					</template>
				</q-input>
			</div>
		</template>
		<template #actions="{ close }">
			<QRow justify="end">
				<!--	Hide	-->
				<QCol cols="auto">
					<HideButton
						cy="generate-token-dialog-hide-button"
						@click="close" />
				</QCol>
			</QRow>
		</template>
	</QCardDialog>
</template>

<script setup lang="ts">
import { set, useClipboard } from '@vueuse/core';
import { DialogType } from '@enums';
import VOtpInput from 'vue3-otp-input';
import { useAccountDialogStore, useSubscription } from '#imports';

const accountDialogStore = useAccountDialogStore();
const { copy } = useClipboard({ legacy: true });

const generatedToken = ref<string>('');
const verificationCodeValue = ref<string>('');
const needVerificationToken = ref<boolean>(false);

function onOpen() {
	requestToken();
}

function onComplete(verificationCode: string) {
	requestToken(verificationCode);
	set(needVerificationToken, false);
	set(verificationCodeValue, '');
}

function requestToken(verificationCode: string = '') {
	useSubscription(accountDialogStore.generateToken(verificationCode).subscribe({
		next({ isSuccess, value, errors }) {
			if (errors.some((x) => x.message.includes('verification code'))) {
				set(needVerificationToken, true);
				return;
			}

			if (isSuccess) {
				set(generatedToken, value?.plexAuthToken ?? '');
			}
		},
	}));
}
</script>
