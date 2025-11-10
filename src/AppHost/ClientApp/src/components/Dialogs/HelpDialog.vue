<template>
	<QCardDialog
		:name="DialogType.HelpInfoDialog"
		:loading="false"
		:type="{} as IHelp"
		width="500px"
		@opened="onOpen"
		@closed="onClose">
		<template #title>
			{{ helpTitle ? helpTitle : missingHelpTitle }}
		</template>
		<!--	Help text	-->
		<template #default>
			<div class="i18n-formatting">
				<VueMarkdown :source="helpText ? helpText : missingHelpText" />
			</div>
		</template>
		<!--	Close action	-->
		<template #actions="{ close }">
			<q-space />
			<q-btn
				flat
				:label="t('general.commands.close')"
				@click="close" />
		</template>
	</QCardDialog>
</template>

<script setup lang="ts">
import { get, set } from '@vueuse/core';
import type { IHelp } from '@interfaces';
import { DialogType } from '@enums';
import { useI18n } from 'vue-i18n';
import VueMarkdown from 'vue-markdown-render';

const { t } = useI18n();
const helpTitle = ref('');
const helpText = ref('');

const missingHelpTitle = ref(t('help.default.title'));
const missingHelpText = ref(t('help.default.text'));

function onOpen(event: IHelp): void {
	set(helpTitle, event.title ? event.title : get(missingHelpTitle));
	set(helpText, event.text ? event.text : get(missingHelpText));
}

function onClose() {
	set(helpTitle, '');
	set(helpText, '');
}
</script>
