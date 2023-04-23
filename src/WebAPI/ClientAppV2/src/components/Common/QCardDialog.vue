<template>
	<q-dialog v-model:model-value="showDialog">
		<div class="q-card-dialog q-card-dialog-background">
			<!-- Dialog Title	-->
			<q-col cols="12" class="q-card-dialog-title">
				<QCardTitle>
					<slot name="title" :value="parentValue" />
				</QCardTitle>
			</q-col>
			<!--	Dialog Top Row		-->
			<q-col v-if="$slots['top-row']" cols="auto" class="q-card-dialog-top-row">
				<slot name="top-row" />
			</q-col>
			<q-col v-if="$slots['default']" class="q-card-dialog-content" :class="{ scroll: scroll }" align-self="stretch">
				<slot :value="parentValue" />
			</q-col>
			<q-col v-if="$slots['actions']" cols="auto" align-self="stretch" class="q-card-dialog-actions q-pa-md">
				<!--	Dialog Buttons		-->
				<q-card-actions :align="buttonAlign">
					<slot name="actions" :value="parentValue" />
				</q-card-actions>
			</q-col>
			<QLoadingOverlay :loading="loading" />
		</div>
	</q-dialog>
</template>

<script setup lang="ts">
import { defineEmits, defineProps, ref } from 'vue';
import { useControlDialog } from '#imports';

const controlDialog = useControlDialog();

const showDialog = ref(false);
const value = ref<any | null>(null);
const props = withDefaults(
	defineProps<{
		name: string;
		width?: string;
		height?: string;
		minWidth?: string;
		minHeight?: string;
		maxWidth?: string;
		maxHeight?: string;
		allWidth?: string;
		allHeight?: string;
		viewHeight?: string;
		value?: any;
		loading: boolean;
		scroll: boolean;
		persistent?: boolean;
		buttonAlign?: 'left' | 'center' | 'right' | 'between' | 'around' | 'evenly' | 'stretch';
	}>(),
	{
		name: '',
		width: '',
		height: '',
		minWidth: '',
		minHeight: '',
		maxWidth: '',
		maxHeight: '',
		allWidth: '',
		allHeight: '',
		viewHeight: '50vh',
		value: null,
		loading: false,
		scroll: true,
		persistent: false,
		buttonAlign: 'right',
	},
);

const emit = defineEmits<{
	(e: 'opened', value: any): void;
	(e: 'closed'): void;
}>();

const parentValue = computed(() => {
	if (props.value) {
		return props.value;
	}
	return value.value;
});

const styles = computed(() => {
	if (props.allWidth && props.allHeight) {
		return Object.assign(
			{},
			props.allWidth !== '' ? { width: props.allWidth } : null,
			props.allHeight !== '' ? { height: props.allHeight } : null,
			props.allWidth !== '' ? { minWidth: props.allWidth } : null,
			props.allHeight !== '' ? { minHeight: props.allHeight } : null,
			props.allWidth !== '' ? { maxWidth: props.allWidth } : null,
			props.allHeight !== '' ? { maxHeight: props.allHeight } : null,
		);
	}

	if (props.allWidth && !props.allHeight) {
		return Object.assign(
			{},
			props.allWidth !== '' ? { width: props.allWidth } : null,
			props.height !== '' ? { height: props.height } : null,
			props.allWidth !== '' ? { minWidth: props.allWidth } : null,
			props.minHeight !== '' ? { minHeight: props.minHeight } : null,
			props.allWidth !== '' ? { maxWidth: props.allWidth } : null,
			props.maxHeight !== '' ? { maxHeight: props.maxHeight } : null,
		);
	}

	if (!props.allWidth && props.allHeight) {
		return Object.assign(
			{},
			props.width !== '' ? { width: props.width } : null,
			props.allHeight !== '' ? { height: props.allHeight } : null,
			props.minWidth !== '' ? { minWidth: props.minWidth } : null,
			props.allHeight !== '' ? { minHeight: props.allHeight } : null,
			props.maxWidth !== '' ? { maxWidth: props.maxWidth } : null,
			props.allHeight !== '' ? { maxHeight: props.allHeight } : null,
		);
	}

	return Object.assign(
		{},
		props.width !== '' ? { width: props.width } : null,
		props.height !== '' ? { height: props.height } : null,
		props.minWidth !== '' ? { minWidth: props.minWidth } : null,
		props.minHeight !== '' ? { minHeight: props.minHeight } : null,
		props.maxWidth !== '' ? { maxWidth: props.maxWidth } : null,
		props.maxHeight !== '' ? { maxHeight: props.maxHeight } : null,
	);
});

controlDialog.on((data) => {
	if (data.name === props.name) {
		showDialog.value = data.state;
		value.value = data.value;
		if (showDialog.value) {
			emit('opened', data.value);
		} else {
			emit('closed');
		}
	}
});
</script>
