<template>
	<q-list v-if="debugMode" bordered class="rounded-borders">
		<q-expansion-item switch-toggle-side expand-separator :label="title" icon="mdi-debug">
			<q-card :style="{ maxHeight: height + 'px' }" class="scroll">
				<q-card-section>
					<pre><slot /></pre>
				</q-card-section>
			</q-card>
		</q-expansion-item>
	</q-list>
</template>

<script setup lang="ts">
import { useSubscription } from '@vueuse/rxjs';
import { set } from '@vueuse/core';
import { SettingsService } from '@service';

const debugMode = ref(false);

withDefaults(
	defineProps<{
		title?: string;
		height?: number;
	}>(),
	{
		title: 'Print',
		height: 500,
	},
);

onMounted(() => {
	useSubscription(
		SettingsService.getDebugMode().subscribe((value) => {
			set(debugMode, value);
		}),
	);
});
</script>
