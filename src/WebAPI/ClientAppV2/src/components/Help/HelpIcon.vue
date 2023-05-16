<template>
	<q-row no-wrap align="center">
		<q-col>
			<q-sub-header>
				{{ getLabel }}
			</q-sub-header>
		</q-col>
		<q-col v-if="hasHelpPage" cols="auto">
			<BaseButton icon="mdi-help-circle-outline" class="q-ma-sm" icon-only @click="openDialog" />
		</q-col>
	</q-row>
</template>

<script setup lang="ts">
import { useNuxtApp, useI18n } from '#imports';
import { HelpService } from '@service';

interface IHelp {
	label: string;
	title: string;
	text: string;
}

const { $getMessage } = useNuxtApp();
const { t } = useI18n();

const props = withDefaults(
	defineProps<{
		labelId?: string;
		helpId?: string;
	}>(),
	{
		labelId: '',
		helpId: '',
	},
);

const getLabel = computed(() => {
	return t(`${props.helpId}.label`);
});

const hasHelpPage = computed(() => {
	if (props.helpId) {
		const msgObject = $getMessage(props.helpId) as IHelp;
		// Complains about returning string if I return directly, instead of an if statement returning true
		if (msgObject && msgObject.title && msgObject.text) {
			return true;
		}
	}
	return false;
});

function openDialog(): void {
	HelpService.openHelpDialog(props.helpId);
}
</script>
