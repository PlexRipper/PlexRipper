<template>
	<q-row no-wrap class="no-gutters">
		<q-col cols="auto">
			<q-sub-header>
				{{ getLabel }}
			</q-sub-header>
		</q-col>
		<q-col v-if="hasHelpPage" cols="auto">
			<q-btn icon="mdi-help-circle-outline" style="margin: 6px" flat @click="openDialog()" />
		</q-col>
	</q-row>
</template>

<script setup lang="ts">
import Log from 'consola';
import { withDefaults, defineProps, computed } from 'vue';
import { useNuxtApp, useI18n } from '#imports';
import { HelpService } from '@service';

interface IHelp {
	label: string;
	title: string;
	text: string;
}

const { $getMessage } = useNuxtApp();

const props = withDefaults(
	defineProps<{
		labelId?: string;
		helpId: string;
	}>(),
	{
		labelId: '',
		helpId: '',
	},
);

const getLabel = computed(() => {
	return useI18n().t(`${props.helpId}.label`);
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
