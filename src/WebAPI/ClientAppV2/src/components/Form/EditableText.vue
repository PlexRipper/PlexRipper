<template>
	<q-list>
		<q-item v-ripple clickable>
			<q-item-section>
				<q-sub-header>
					{{ value }}
				</q-sub-header>
			</q-item-section>
			<QPopupEdit v-slot="scope" :model-value="value" auto-save @save="$emit('save', $event)">
				<q-input v-model="scope.value" dense autofocus counter @keyup.enter="scope.set" />
			</QPopupEdit>
		</q-item>
	</q-list>
</template>

<script setup lang="ts">
import { defineProps, defineEmits } from 'vue';

defineProps<{
	label: string;
	value?: string;
	disabled?: boolean;
}>();

defineEmits<{
	(e: 'save', save: string): void;
}>();
</script>
