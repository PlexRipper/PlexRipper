<template>
	<q-dialog
		v-model:model-value="showDialog"
		:full-height="fullHeight"
		:no-route-dismiss="noRouteDismiss"
		:no-backdrop-dismiss="noBackdropDismiss"
		:persistent="persistent"
		:seamless="seamless"
		:maximized="maximized"
		:transition-show="transitionShow"
		:transition-hide="transitionHide"
		@before-show="$emit('opened', dataValue)"
		@before-hide="$emit('closed')">
		<q-row
			column
			:data-cy="cy"
			:class="['q-card-dialog', 'q-card-dialog-background', noBackground ? 'no-background' : '']"
			:style="styles">
			<!-- Dialog Title	-->
			<q-col v-if="$slots['title']" cols="auto" class="q-card-dialog-title">
				<div v-if="!loading">
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
				<div v-if="!loading">
					<slot :value="parentValue" />
				</div>
			</q-col>
			<q-col v-if="$slots['actions']" cols="auto" align-self="stretch" class="q-card-dialog-actions q-pa-md">
				<div v-if="!loading">
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
		seamless?: boolean;
		maximized?: boolean;
		noBackdropDismiss?: boolean;
		noBackground?: boolean;
		noRouteDismiss?: boolean;
		transitionShow?: string;
		transitionHide?: string;
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
		seamless: false,
		maximized: false,
		noBackground: false,
		noBackdropDismiss: false,
		noRouteDismiss: false,
		cy: 'q-card-dialog-cy',
		buttonAlign: 'right',
		transitionShow: 'fade',
		transitionHide: 'fade',
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
<style lang="scss">
@import '@/assets/scss/variables.scss';
@import '@/assets/scss/_mixins.scss';
@import 'quasar/src/css/core/size.sass';

body {
	.q-card-dialog {
		// Scrollbar is hidden because otherwise the header and footer are also scrolling
		overflow-y: hidden;

		&-background {
			@extend .default-border;
			@extend .default-border-radius;
			@extend .default-shadow;
			max-width: none;
			max-height: none;
		}

		&-top-row {
			@extend .q-pt-none;
			@extend .q-px-md;
			width: 100% !important;
		}

		&-content {
			@extend .q-pt-none;
			@extend .q-px-md;
		}

		&-title,
		&-actions {
			height: auto;
			width: 100% !important;
		}
	}

	&.body--dark {
		.q-card-dialog-background {
			@extend .blur;
			background-color: $dark-sm-background-color;
		}
	}

	&.body--light {
		.q-card-dialog-background {
			@extend .blur;
			background-color: $light-sm-background-color;
		}
	}
}
</style>
