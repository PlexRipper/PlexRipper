<template>
	<q-card-dialog max-width="1000px" :name="name" @closed="onClose">
		<template #title>
			{{ alert.title }}
		</template>
		<template #default>
			<p>{{ alert.text }}</p>
			<pre style="white-space: break-spaces">{{ errors }}</pre>
		</template>
		<template #actions="{ close }">
			<q-space />
			<!--	Close action	-->
			<base-button text-id="close" @click="close" />
		</template>
	</q-card-dialog>
</template>

<script setup lang="ts">
import { defineProps, computed } from 'vue';
import type IAlert from '@interfaces/IAlert';
import { AlertService } from '@service';

const props = defineProps<{ name: string; alert: IAlert }>();

const errors = computed(() => {
	if (props.alert?.result?.errors) {
		return props.alert.result.errors;
	}
	return [];
});

function onClose(): void {
	AlertService.removeAlert(props.alert.id);
}
</script>
