<template>
	<q-dialog
		v-model:model-value="showDialog"
		:full-height="fullHeight"
		:no-route-dismiss="noRouteDismiss"
		:no-backdrop-dismiss="noBackdropDismiss"
		@before-show="$emit('opened', dataValue)"
		@before-hide="$emit('closed')">
		<q-row column :data-cy="cy" class="q-card-dialog q-card-dialog-background" :style="styles">
			<!-- Dialog Title	-->
			<q-col v-if="$slots['title']" cols="auto" class="q-card-dialog-title">
				<div v-show="!loading">
					<QCardTitle>
						<slot name="title" :value="parentValue" />
					</QCardTitle>
				</div>
			</q-col>
			<!--	Dialog Top Row -->
			<q-col v-if="$slots['top-row']" cols="auto" class="q-card-dialog-top-row">
				<div v-show="!loading">
					<slot name="top-row" />
				</div>
			</q-col>
			<q-col v-if="$slots['default']" class="q-card-dialog-content" :class="{ scroll: scroll }" align-self="stretch">
				<div v-show="!loading">
					<slot :value="parentValue" />
				</div>
			</q-col>
			<q-col v-if="$slots['actions']" cols="auto" align-self="stretch" class="q-card-dialog-actions q-pa-md">
				<div v-show="!loading">
					<!--	Dialog Buttons		-->
					<q-card-actions :align="buttonAlign">
						<slot name="actions" :close="closeDialog" :open="openDialog" :value="parentValue" />
					</q-card-actions>
				</div>
			</q-col>
			<!--	Loading overlay	-->
			<QLoadingOverlay :loading="loading" />
		</q-row>
	</q-dialog>
</template>

<script setup lang="ts">
import { withDefaults, defineEmits, defineProps, ref } from 'vue';
import { get, set } from '@vueuse/core';
import { useControlDialog } from '#imports';

const controlDialog = useControlDialog();

const showDialog = ref(false);
const dataValue = ref<any | null>(null);
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
		loading?: boolean;
		fullHeight?: boolean;
		scroll?: boolean;
		persistent?: boolean;
		noBackdropDismiss?: boolean;
		noRouteDismiss?: boolean;
		buttonAlign?: 'left' | 'center' | 'right' | 'between' | 'around' | 'evenly' | 'stretch';
		cy?: string;
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
		fullHeight: false,
		scroll: true,
		persistent: false,
		noBackdropDismiss: false,
		noRouteDismiss: false,
		cy: 'q-card-dialog-cy',
		buttonAlign: 'right',
	},
);

defineEmits<{
	(e: 'opened', value: any): void;
	(e: 'closed'): void;
}>();

const parentValue = computed(() => {
	if (props.value) {
		return props.value;
	}
	return get(dataValue);
});

function openDialog(value: any) {
	// Data value should always be set first before opening, since that value is emitted on open
	set(dataValue, value);
	set(showDialog, true);
}

function closeDialog() {
	set(showDialog, false);
}

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

// Dialog control listener
controlDialog.on((data) => {
	if (data.name !== props.name) {
		return;
	}
	if (data.state) {
		openDialog(data.value ?? null);
	} else {
		closeDialog();
	}
});
</script>
