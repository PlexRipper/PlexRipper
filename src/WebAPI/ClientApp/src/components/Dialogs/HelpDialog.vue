<template>
	<QCardDialog
		max-width="500px"
		:name="name"
		:loading="false"
		@opened="onOpen"
		@closed="onClose"
	>
		<template #title>
			{{ helpTitle ? helpTitle : missingHelpTitle }}
		</template>
		<!--	Help text	-->
		<template #default>
			<div class="i18n-formatting">
				{{ helpText ? helpText : missingHelpText }}
			</div>
		</template>
		<!--	Close action	-->
		<template #actions="{ close }">
			<q-space />
			<q-btn
				flat
				:label="t('general.commands.close')"
				@click="close"
			/>
		</template>
	</QCardDialog>
</template>

<script setup lang="ts">
import { get, set } from '@vueuse/core';
import type { IHelp } from '@interfaces';
import { useI18n } from '#imports';

defineProps<{ name: string }>();

const { t } = useI18n();
const helpTitle = ref('');
const helpText = ref('');

const missingHelpTitle = ref(t('help.default.title'));
const missingHelpText = ref(t('help.default.text'));

function onOpen(event: unknown): void {
	const value = event as IHelp;
	set(helpTitle, value.title ? value.title : get(missingHelpTitle));
	set(helpText, value.text ? value.text : get(missingHelpText));
}

function onClose() {
	set(helpTitle, '');
	set(helpText, '');
}
</script>
