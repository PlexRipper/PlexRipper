<template>
	<QCardDialog
		persistent
		:name="name"
		max-width="800px"
		button-align="between"
		cy="confirmation-dialog"
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

const { t } = useI18n();

const loading = ref(false);

const props = withDefaults(defineProps<{
	title: string;
	text: string;
	warning?: string;
	name: string;
	confirmLoading?: boolean;
}>(), {
	warning: '',
});

const emit = defineEmits<{
	(e: 'confirm' | 'cancel'): void;
}>();

const confirmationText = computed(() => ({
	title: props.title != '' ? props.title : t('confirmation.not-found.title'),
	text: props.text != '' ? props.text : t('confirmation.not-found.text'),
	warning: props.warning,
}));

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
</script>
