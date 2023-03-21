<template>
	<q-row justify="between" no-wrap>
		<q-col>
			<span class="form-label text-no-wrap">{{ getLabel }}</span>
		</q-col>
		<q-col v-if="hasHelpPage" cols="auto">
			<q-btn icon="mdi-help-circle-outline" style="margin: 6px" @click="openDialog" />
		</q-col>
	</q-row>
</template>

<script setup lang="ts">
import { useI18n } from '#imports';
import { HelpService } from '@service';

interface IHelp {
	label: string;
	title: string;
	text: string;
}

const { $getMessage } = useNuxtApp();

const props = defineProps<{
	labelId?: string;
	helpId: string;
}>();

const getLabel = computed(() => {
	return useI18n().t(`${props.labelId}.label`);
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
