<template>
	<q-expansion-item switch-toggle-side expand-separator :label="title">
		<q-card :style="{ height: object ? height + 'px' : '0px' }">
			<q-card-section>
				<pre>{{ object }}</pre>
			</q-card-section>
		</q-card>
	</q-expansion-item>
</template>

<script setup lang="ts">
import { withDefaults, defineProps } from 'vue';

withDefaults(
	defineProps<{
		title?: string;
		object: any;
		height?: number;
	}>(),
	{
		title: 'Print',
		height: 500,
	},
);
</script>
