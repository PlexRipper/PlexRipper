<template>
	<QCardDialog
		max-width="1000px"
		:name="name"
		@closed="alertStore.removeAlert(alert.id)"
	>
		<template #title>
			{{ alert.title }}
		</template>
		<template #default>
			<pre style="white-space: break-spaces">{{ alert.text }}</pre>
			<QText>{{ $t('components.alert-dialog.request-data-sent') }}</QText>
			<pre
				v-if="alert.result"
				style="white-space: break-spaces"
			>{{ alert.result }}</pre>
			<pre
				v-if="errors"
				style="white-space: break-spaces"
			>{{ errors }}</pre>
		</template>
		<template #actions="{ close }">
			<q-space />
			<!--	Close action	-->
			<BaseButton
				text-id="close"
				@click="close"
			/>
		</template>
	</QCardDialog>
</template>

<script setup lang="ts">
import type IAlert from '@interfaces/IAlert';
import { useAlertStore } from '~/store';

const alertStore = useAlertStore();

const props = defineProps<{ name: string; alert: IAlert }>();

const errors = computed(() => {
	if (props.alert?.result?.errors) {
		return props.alert.result.errors;
	}
	return null;
});
</script>
