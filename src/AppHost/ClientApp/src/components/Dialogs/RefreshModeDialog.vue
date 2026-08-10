<template>
	<QCardDialog
		persistent
		:name="DialogType.RefreshMediaDialog"
		width="600px"
		button-align="between"
		cy="refresh-mode-dialog">
		<template #title>
			{{ title }}
		</template>
		<template #default>
			<p>{{ text }}</p>
			<QAlert type="warning">
				{{ t('refresh-mode-dialog.force-warning') }}
			</QAlert>
		</template>
		<template #actions>
			<CancelButton
				cy="refresh-mode-cancel-button"
				@click="close" />
			<div class="row q-gutter-sm">
				<BaseButton
					color="positive"
					cy="refresh-mode-incremental-button"
					:label="t('refresh-mode-dialog.incremental')"
					@click="select(false)" />
				<BaseButton
					color="negative"
					cy="refresh-mode-force-button"
					:label="t('refresh-mode-dialog.force')"
					@click="select(true)" />
			</div>
		</template>
	</QCardDialog>
</template>

<script setup lang="ts">
import { DialogType } from '@enums';
import { useDialogStore, useI18n } from '#imports';

const props = withDefaults(defineProps<{
	scope?: 'library' | 'server';
}>(), {
	scope: 'library',
});

const emit = defineEmits<{
	(e: 'select', forceMediaRefresh: boolean): void;
}>();

const { t } = useI18n();
const dialogStore = useDialogStore();
const title = computed(() => props.scope === 'server'
	? t('refresh-mode-dialog.server.title')
	: t('refresh-mode-dialog.library.title'));
const text = computed(() => props.scope === 'server'
	? t('refresh-mode-dialog.server.text')
	: t('refresh-mode-dialog.library.text'));

function close(): void {
	dialogStore.closeDialog(DialogType.RefreshMediaDialog);
}

function select(forceMediaRefresh: boolean): void {
	close();
	emit('select', forceMediaRefresh);
}
</script>
