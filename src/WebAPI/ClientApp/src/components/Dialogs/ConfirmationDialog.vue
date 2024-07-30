<template>
	<QCardDialog
		persistent
		:name="name"
		max-width="800px"
		button-align="between"
		cy="confirmation-dialog"
		@opened="onOpen"
	>
		<template #title>
			{{ confirmationText.title }}
		</template>
		<template #default>
			<p>{{ confirmationText.text }}</p>
			<p
				v-if="confirmationText.warning"
				class="text-center"
			>
				<b>{{ confirmationText.warning }}</b>
			</p>
		</template>
		<template #actions>
			<CancelButton
				cy="confirmation-dialog-cancel-button"
				@click="cancel"
			/>
			<ConfirmButton
				cy="confirmation-dialog-confirmation-button"
				:loading="loading"
				@click="confirm"
			/>
		</template>
	</QCardDialog>
</template>

<script setup lang="ts">
import { set } from '@vueuse/core';
import { useI18n, useCloseControlDialog } from '#imports';

const { t, te } = useI18n();

const loading = ref(false);

const confirmationText = ref<{
	id?: string;
	title?: string;
	text?: string;
	warning?: string;
}>({});

const props = defineProps<{
	/**
	 * The Vue-i18n text id used for the confirmation window that pops-up.
	 * @type {string}
	 */
	textId: string;
	name: string;
	confirmLoading?: boolean;
}>();

const emit = defineEmits<{
	(e: 'confirm' | 'cancel'): void;
}>();

function onOpen() {
	const textId = props.textId;
	const result = {
		id: textId,
		warning: '',
	} as IConfirmationResult;

	if (te<string>(`confirmation.${textId}.title`)) {
		result.title = t(`confirmation.${textId}.title`);
	} else {
		result.title = t(`confirmation.not-found.title`);
	}

	if (te<string>(`confirmation.${textId}.text`)) {
		result.text = t(`confirmation.${textId}.text`);
	} else {
		result.text = t(`confirmation.not-found.text`);
	}

	if (te<string>(`confirmation.${textId}.warning`)) {
		result.warning = t(`confirmation.${textId}.warning`);
	}

	set(confirmationText, result);
}

function cancel() {
	emit('cancel');
	useCloseControlDialog(props.name);
	set(loading, false);
}

function confirm() {
	emit('confirm');
	if (props.confirmLoading) {
		set(loading, true);
	}
}

interface IConfirmationResult {
	id: string;
	title: string;
	text: string;
	warning: string;
}
</script>
