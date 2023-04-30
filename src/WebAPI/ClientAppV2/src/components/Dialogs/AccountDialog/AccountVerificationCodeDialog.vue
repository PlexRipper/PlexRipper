<template>
	<q-dialog :model-value="dialog" persistent>
		<q-card :max-width="500">
			<q-card-section>
				<h3>{{ $t('components.account-verification-code-dialog.title') }}</h3>
				<q-sub-header>{{ $t('components.account-verification-code-dialog.sub-title') }}</q-sub-header>
			</q-card-section>
			<q-card-section>
				<!--	Verification Code input	-->
				<q-row justify="center">
					<q-col cols="auto">
						<!--						<QVerificationCodeInput-->
						<!--							:loading="false"-->
						<!--							@change="onChange"-->
						<!--							@complete="onComplete"-->
						<!--							@keyup.enter="onEnter" />-->
					</q-col>
				</q-row>
				<q-row v-if="errors.length > 0" justify="center">
					<q-col cols="auto">
						<span style="color: red; font-weight: bold">{{
							$t('components.account-verification-code-dialog.error')
						}}</span>
					</q-col>
				</q-row>
			</q-card-section>
			<q-card-actions>
				<!--	Submit button	-->
				<q-row justify="center">
					<q-col cols="auto">
						<CancelButton @click="closeDialog" />
					</q-col>
					<q-space />
					<q-col cols="auto">
						<ConfirmButton :disabled="code.length < 6" @click="submitCode" />
					</q-col>
				</q-row>
			</q-card-actions>
		</q-card>
	</q-dialog>
</template>

<script setup lang="ts">
import Log from 'consola';
import { withDefaults, defineProps } from 'vue';
import type { IError } from '@dto/mainApi';

withDefaults(
	defineProps<{
		dialog: boolean;
		errors: IError[];
	}>(),
	{
		dialog: false,
		errors: () => [],
	},
);

const code = ref('0');

const emit = defineEmits<{
	(e: 'close'): void;
	(e: 'submit', code: string): void;
}>();

// @Watch('errors')
// onChildChanged(val: string) {
// 	// If an error appears, then clear the code
// 	if (val.length > 0) {
// 		this.code = '0';
// 	}
// }

const onChange = (v: string) => {
	code.value = v;
};

const onComplete = (v: string) => {
	Log.info('onComplete ', v);
};

const onEnter = () => {
	Log.info('Enter pressed');
};

const closeDialog = () => {
	emit('close');
};

const submitCode = () => {
	emit('submit', code.value);
};
</script>

<style>
.react-code-input > input[data-v-e1087700]:focus {
	border: 1px solid #f00 !important;
	caret-color: #f00 !important;
}
</style>
