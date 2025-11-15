<template>
	<QCardDialog
		:loading="false"
		:name="DialogType.HelpInfoDialog"
		:type="{} as IHelp"
		width="500px"
		@closed="onClose"
		@opened="onOpen">
		<template #title>
			{{ helpTitle ? helpTitle : missingHelpTitle }}
		</template>
		<!--	Help text	-->
		<template #default>
			<div class="i18n-formatting">
				<VueMarkdown
					:plugins="[markdownItTargetBlank]"
					:source="helpText ? helpText : missingHelpText" />
			</div>
		</template>
		<!--	Close action	-->
		<template #actions="{ close }">
			<q-space />
			<q-btn
				:label="t('general.commands.close')"
				flat
				@click="close" />
		</template>
	</QCardDialog>
</template>

<script lang="ts" setup>
import { get, set } from '@vueuse/core';
import type { IHelp } from '@interfaces';
import { DialogType } from '@enums';
import { useI18n } from 'vue-i18n';
import VueMarkdown from 'vue-markdown-render';
import type MarkdownIt from 'markdown-it';

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

// Markdown-it plugin to add target="_blank" and rel="noopener noreferrer" to all links
function markdownItTargetBlank(md: MarkdownIt) {
	const d = md.renderer.rules.link_open || ((t, i, o, _, s) => s.renderToken(t, i, o));
	md.renderer.rules.link_open = (t, i, o, e, s) => {
		const a = t[i].attrIndex('target');
		if (a < 0) t[i].attrPush(['target', '_blank']);
		else t[i].attrs[a][1] = '_blank';
		t[i].attrPush(['rel', 'noopener noreferrer']);
		return d(t, i, o, e, s);
	};
}
</script>
