<template>
	<q-dialog v-model:model-value="showDialog">
		<q-card bordered :style="{ width, height, minWidth: width, minHeight: height }">
			<q-card-title>
				<slot name="title" :value="parentValue" />
			</q-card-title>
			<q-card-section class="q-pb-none">
				<slot name="top-row" />
			</q-card-section>
			<q-card-section style="height: 50vh" class="q-pt-none">
				<q-scroll-area class="fit">
					<slot :value="parentValue" />
				</q-scroll-area>
			</q-card-section>
			<q-card-actions align="right">
				<slot name="actions" :value="parentValue" />
			</q-card-actions>
			<QLoadingOverlay />
		</q-card>
	</q-dialog>
</template>

<script setup lang="ts">
import { ref, defineProps, defineEmits } from 'vue';
import { useControlDialog } from '#imports';

const controlDialog = useControlDialog();
const showDialog = ref(false);
const value = ref<any | null>(null);
const props = defineProps<{
	name: string;
	width: string;
	height: string;
	value?: any;
	loading: boolean;
}>();

const emit = defineEmits<{
	(e: 'opened', value: any): void;
	(e: 'closed', value: boolean): void;
}>();

const parentValue = computed(() => {
	if (props.value) {
		return props.value;
	}
	return value.value;
});

controlDialog.on((data) => {
	if (data.name === props.name) {
		showDialog.value = data.state;
		value.value = data.value;
		if (showDialog.value) {
			emit('opened', data.value);
		} else {
			emit('closed', showDialog.value);
		}
	}
});
</script>
