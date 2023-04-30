<template>
	<q-card-dialog max-width="500px" :name="name" :loading="false" @opened="onOpen($event)" @closed="onClose">
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
			<q-btn flat :label="$t('general.commands.close')" @click="close" />
		</template>
	</q-card-dialog>
</template>

<script setup lang="ts">
import { ref, defineProps } from 'vue';
import { get, set } from '@vueuse/core';
import { useI18n } from '#imports';

defineProps<{ name: string }>();

const { t } = useI18n();
const helpId = ref('');
const helpTitle = ref('');
const helpText = ref('');

const missingHelpTitle = ref(t('help.default.title'));
const missingHelpText = ref(t('help.default.text'));

function onOpen(value: string): void {
	set(helpId, value);
	if (get(helpId) === '') {
		set(helpTitle, t('help.default.title'));
		set(helpText, t('help.default.text'));
	} else {
		set(helpTitle, t(`${get(helpId)}.title`));
		set(helpText, t(`${get(helpId)}.text`));
	}
}

function onClose() {
	set(helpTitle, '');
	set(helpText, '');
}
</script>
