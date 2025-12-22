<template>
	<QCardDialog
		persistent
		:name="name"
		width="500px"
		button-align="between"
		cy="confirmation-dialog">
		<template #title>
			{{ confirmationText.title }}
		</template>
		<template #default>
			<div>
				<p>{{ confirmationText.text }}</p>
				<p
					v-if="confirmationText.warning"
					class="text-center">
					<b>{{ confirmationText.warning }}</b>
				</p>
			</div>
		</template>
		<template #actions>
			<CancelButton
				cy="confirmation-dialog-cancel-button"
				@click="cancel" />
			<ConfirmButton
				cy="confirmation-dialog-confirmation-button"
				:loading="confirmLoading"
				@click="confirm" />
		</template>
	</QCardDialog>
</template>

<script setup lang="ts">
import type { DialogType } from '@enums';
import { useI18n } from 'vue-i18n';
import { useDialogStore } from '@store';

const { t } = useI18n();

const dialogStore = useDialogStore();

const props = withDefaults(defineProps<{
	name: DialogType;
	title: string;
	text: string;
	warning?: string;
	confirmLoading?: boolean;
}>(), {
	warning: '',
	confirmLoading: false,
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
	dialogStore.closeDialog(props.name);
}

function confirm() {
	emit('confirm');
}
</script>
