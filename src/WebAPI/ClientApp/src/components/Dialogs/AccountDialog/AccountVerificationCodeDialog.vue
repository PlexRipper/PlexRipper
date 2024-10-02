<template>
	<QCardDialog
		:name="name"
		persistent
		max-width="700px"
		cy="2fa-code-verification-dialog">
		<template #title>
			{{ t('components.account-verification-code-dialog.title') }}
		</template>
		<template #top-row>
			<QSubHeader>{{ t('components.account-verification-code-dialog.sub-title') }}</QSubHeader>
		</template>
		<template #default>
			<div>
				<!--	Verification Code input	-->
				<QRow justify="center">
					<QCol cols="auto">
						<VOtpInput
							v-model:value="codeValue"
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
				<QRow
					v-if="errors.length > 0"
					justify="center">
					<QCol cols="auto">
						<span style="color: red; font-weight: bold">{{
							t('components.account-verification-code-dialog.error')
						}}</span>
					</QCol>
				</QRow>
			</div>
		</template>
		<template #actions="{ close }">
			<QRow justify="between">
				<!-- Close	-->
				<QCol cols="auto">
					<CancelButton @click="close" />
				</QCol>
				<!--	Confirm	-->
				<QCol cols="auto">
					<ConfirmButton
						:loading="loading"
						:disabled="codeValue.length < 6"
						@click="onComplete" />
				</QCol>
			</QRow>
		</template>
	</QCardDialog>
</template>

<script setup lang="ts">
import Log from 'consola';
import VOtpInput from 'vue3-otp-input';
import { get, set } from '@vueuse/core';
import { useSubscription } from '@vueuse/rxjs';
import type { IError, PlexAccountDTO } from '@dto';
import { plexAccountApi } from '@api';
import { useCloseControlDialog } from '#imports';

const { t } = useI18n();

const props = defineProps<{
	name: string;
	account: PlexAccountDTO;
}>();

const codeValue = ref('0');
const loading = ref(false);
const errors = ref<IError[]>([]);
const emit = defineEmits<{
	(e: 'close'): void;
	(e: 'confirm', account: PlexAccountDTO): void;
}>();

function onComplete() {
	set(loading, true);
	const accountWithCode: PlexAccountDTO = {
		...props.account,
		verificationCode: get(codeValue),
	};

	useSubscription(
		plexAccountApi.validatePlexAccountEndpoint(accountWithCode).subscribe({
			next: (data) => {
				if (data && data.isSuccess && data.value) {
					emit('confirm', get(data.value));
					useCloseControlDialog(props.name);
				} else {
					Log.error('Validate Error', data);
				}
			},
			complete: () => {
				set(loading, false);
			},
		}),
	);
}
</script>

<style lang="scss">
@import '@/assets/scss/mixins.scss';

.otp-input {
	@extend .default-border;
	@extend .background-sm;
	width: 80px;
	height: 80px;
	padding: 5px;
	margin: 10px;
	font-size: 40px;
	border-radius: 4px;
	text-align: center;
	/* Background colour of an input field with value */
	&.is-complete {
		@extend .success-border;
		background-color: #ff0000;
	}
}

.otp-input::-webkit-inner-spin-button,
.otp-input::-webkit-outer-spin-button {
	-webkit-appearance: none;
	margin: 0;
}

input::placeholder {
	font-size: 35px;
	text-align: center;
	font-weight: 600;
}
</style>
