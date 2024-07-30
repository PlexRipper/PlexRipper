<template>
	<q-row
		no-wrap
		align="center"
	>
		<q-col>
			<q-sub-header>
				{{ $t(`${helpId}.label`) }}
			</q-sub-header>
		</q-col>
		<q-col
			v-if="hasHelpPage"
			cols="auto"
		>
			<IconButton
				icon="mdi-help-circle-outline"
				class="q-ma-sm"
				@click="helpStore.openHelpDialog(props.helpId)"
			/>
		</q-col>
	</q-row>
</template>

<script setup lang="ts">
import { useI18n } from '#imports';

const { t } = useI18n();
const helpStore = useHelpStore();
const props = defineProps<{
	helpId: string;
}>();

const hasHelpPage = computed(() => {
	return props.helpId && t(`${props.helpId}.title`) && t(`${props.helpId}.text`);
});
</script>
