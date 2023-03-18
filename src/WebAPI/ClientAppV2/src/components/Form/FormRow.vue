<template>
	<q-row class="flex-nowrap" no-gutters>
		<q-col cols="3">
			<q-sub-header class="form-label text-no-wrap">{{ getLabel }}</q-sub-header>
		</q-col>
		<q-col cols="1">
			<HelpButton v-if="hasHelpPage" @click="openDialog"/>
		</q-col>
		<q-col cols="8" align-self="end">
			<slot/>
		</q-col>
	</q-row>
</template>

<script setup lang="ts">
import {HelpService} from '@service';
import {useI18n} from "#imports";

interface IHelp {
	label: string;
	title: string;
	text: string;
}

const {$getMessage} = useNuxtApp()

const props = defineProps<{
	formId: string;
}>();

const getLabel = computed(() => {
	return props.formId ? useI18n().t(`${props.formId}.label`) : '';
});

const hasHelpPage = computed(() => {
	if (props.formId) {
		const msgObject = $getMessage(props.formId) as IHelp;
		// Complains about returning string if I return directly, instead of an if statement returning true
		if (msgObject && msgObject.title && msgObject.text) {
			return true;
		}
	}
	return false;
});


function openDialog(): void {
	HelpService.openHelpDialog(props.formId);
}

</script>
