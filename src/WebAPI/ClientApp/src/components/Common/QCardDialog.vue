<template>
	<q-dialog
		v-model:model-value="showDialog"
		:no-route-dismiss="noRouteDismiss"
		:no-backdrop-dismiss="noBackdropDismiss"
		:persistent="persistent"
		:seamless="seamless"
		:maximized="maximized"
		:transition-show="transitionShow"
		:transition-hide="transitionHide"
		@before-show="$emit('opened', dataValue)"
		@before-hide="$emit('closed')"
	>
		<q-row
			column
			:data-cy="cy"
			:class="['q-card-dialog', 'q-card-dialog-background', noBackground ? 'no-background' : '']"
			:style="styles"
		>
			<!-- Dialog Title	-->
			<q-col
				v-if="$slots['title']"
				cols="auto"
				class="q-card-dialog-title"
			>
				<div v-if="!loading">
					<QCardTitle>
						<slot
							name="title"
							:value="parentValue"
						/>
					</QCardTitle>
				</div>
			</q-col>
			<!--	Dialog Top Row -->
			<q-col
				v-if="$slots['top-row']"
				cols="auto"
				class="q-card-dialog-top-row"
			>
				<div v-show="!loading">
					<slot name="top-row" />
				</div>
			</q-col>
			<q-col
				v-if="$slots['default']"
				:class="contentClasses"
				align-self="stretch"
			>
				<div v-if="!loading">
					<slot :value="parentValue" />
				</div>
			</q-col>
			<q-col
				v-if="$slots['actions']"
				cols="auto"
				align-self="stretch"
				class="q-card-dialog-actions q-pa-md"
			>
				<div v-if="!loading">
					<!--	Dialog Buttons		-->
					<q-card-actions :align="buttonAlign">
						<slot
							name="actions"
							:close="closeDialog"
							:open="openDialog"
							:value="parentValue"
						/>
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
const dataValue = ref<unknown | null>(null);
const props = withDefaults(
	defineProps<{
		name: string;
		width?: string;
		minWidth?: string;
		maxWidth?: string;
		allWidth?: string;
		contentHeight?: '100' | '80' | '60' | '40' | '20' | '0';
		value?: unknown;
		loading?: boolean;
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
		minWidth: '',
		maxWidth: '',
		allWidth: '',
		contentHeight: '0',
		value: null,
		loading: false,
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
	(e: 'opened', value: unknown): void;
	(e: 'closed'): void;
}>();

const parentValue = computed(() => {
	if (props.value) {
		return props.value;
	}
	return get(dataValue);
});

const contentClasses = computed(() => {
	return ['q-card-dialog-content', `q-card-dialog-content-${props.contentHeight}`, props.scroll ? 'scroll' : ''];
});

function openDialog(value: unknown) {
	// Data value should always be set first before opening, since that value is emitted on open
	set(dataValue, value);
	set(showDialog, true);
}

function closeDialog() {
	set(showDialog, false);
}

const styles = computed(() => {
	if (props.allWidth) {
		return Object.assign(
			{},
			props.allWidth !== '' ? { width: props.allWidth } : null,
			props.allWidth !== '' ? { minWidth: props.allWidth } : null,
			props.allWidth !== '' ? { maxWidth: props.allWidth } : null,
		);
	}

	return Object.assign(
		{},
		props.width !== '' ? { width: props.width } : null,
		props.minWidth !== '' ? { minWidth: props.minWidth } : null,
		props.maxWidth !== '' ? { maxWidth: props.maxWidth } : null,
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

			&-0 {
				min-height: inherit;
				height: inherit;
				max-height: inherit;
			}

			&-20 {
				min-height: calc(20vh - $q-card-dialog-title-height - $q-card-dialog-actions-height) !important;
				height: calc(20vh - $q-card-dialog-title-height - $q-card-dialog-actions-height) !important;
				max-height: calc(20vh - $q-card-dialog-title-height - $q-card-dialog-actions-height) !important;
			}

			&-40 {
				min-height: calc(40vh - $q-card-dialog-title-height - $q-card-dialog-actions-height) !important;
				height: calc(40vh - $q-card-dialog-title-height - $q-card-dialog-actions-height) !important;
				max-height: calc(40vh - $q-card-dialog-title-height - $q-card-dialog-actions-height) !important;
			}

			&-60 {
				min-height: calc(60vh - $q-card-dialog-title-height - $q-card-dialog-actions-height) !important;
				height: calc(60vh - $q-card-dialog-title-height - $q-card-dialog-actions-height) !important;
				max-height: calc(60vh - $q-card-dialog-title-height - $q-card-dialog-actions-height) !important;
			}

			&-80 {
				min-height: calc(80vh - $q-card-dialog-title-height - $q-card-dialog-actions-height) !important;
				height: calc(80vh - $q-card-dialog-title-height - $q-card-dialog-actions-height) !important;
				max-height: calc(80vh - $q-card-dialog-title-height - $q-card-dialog-actions-height) !important;
			}

			&-100 {
				min-height: calc(100vh - $q-card-dialog-title-height - $q-card-dialog-actions-height) !important;
				height: calc(100vh - $q-card-dialog-title-height - $q-card-dialog-actions-height) !important;
				max-height: calc(100vh - $q-card-dialog-title-height - $q-card-dialog-actions-height) !important;
			}
		}

		&-title,
		&-actions {
			height: auto;
			width: 100% !important;
		}

		&-title {
			max-height: $q-card-dialog-title-height;
		}

		&-actions {
			max-height: $q-card-dialog-actions-height;
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
