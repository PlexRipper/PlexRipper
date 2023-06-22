<template>
	<q-row no-wrap align="center">
		<q-col>
			<q-sub-header>
				{{ $t(`${helpId}.label`) }}
			</q-sub-header>
		</q-col>
		<q-col v-if="hasHelpPage" cols="auto">
			<IconButton icon="mdi-help-circle-outline" class="q-ma-sm" @click="openDialog" />
		</q-col>
	</q-row>
</template>

<script setup lang="ts">
import { useI18n } from '#imports';
import { HelpService } from '@service';

const { t } = useI18n();

const props = defineProps<{
	helpId: string;
}>();

const hasHelpPage = computed(() => {
	return props.helpId && t(`${props.helpId}.title`) && t(`${props.helpId}.text`);
});

function openDialog(): void {
	HelpService.openHelpDialog(props.helpId);
}
</script>
