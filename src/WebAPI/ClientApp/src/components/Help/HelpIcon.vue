<template>
	<QRow
		no-wrap
		align="center"
	>
		<QCol>
			<QSubHeader>
				{{ $t(`${helpId}.label`) }}
			</QSubHeader>
		</QCol>
		<QCol
			v-if="hasHelpPage"
			cols="auto"
		>
			<IconButton
				icon="mdi-help-circle-outline"
				class="q-ma-sm"
				@click="helpStore.openHelpDialog(props.helpId)"
			/>
		</QCol>
	</QRow>
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
