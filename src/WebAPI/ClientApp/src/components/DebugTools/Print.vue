<template>
	<q-list v-if="settingsStore.debugMode" bordered class="rounded-borders">
		<q-expansion-item switch-toggle-side expand-separator :label="title" icon="mdi-debug" :default-opened="expanded">
			<q-card :style="{ maxHeight: height + 'px' }" class="scroll">
				<q-card-section>
					<pre><slot /></pre>
				</q-card-section>
			</q-card>
		</q-expansion-item>
	</q-list>
</template>

<script setup lang="ts">
import { useSettingsStore } from '~/store';

const settingsStore = useSettingsStore();

withDefaults(
	defineProps<{
		title?: string;
		height?: number;
		expanded?: boolean;
	}>(),
	{
		title: 'Print',
		height: 500,
		expanded: false,
	},
);
</script>
