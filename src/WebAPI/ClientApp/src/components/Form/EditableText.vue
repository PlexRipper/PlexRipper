<template>
	<q-list class="editable-text">
		<q-item
			clickable
			class="editable-text-item">
			<template v-if="!editMode">
				<q-item-section>
					<QText
						class="editable-text-display"
						:type="type"
						:size="size"
						:bold="bold"
						:value="displayText !== '' ? displayText : value"
						:align="align" />
				</q-item-section>
				<q-icon
					class="q-pt-sm"
					name="mdi-square-edit-outline"
					size="md" />
			</template>
			<QPopupEdit
				v-slot="scope"
				:model-value="value"
				square
				auto-save
				@before-show="editMode = true"
				@before-hide="editMode = false"
				@save="$emit('save', $event)">
				<q-input
					v-model="scope.value"
					dense
					autofocus
					:input-class="inputClasses"
					@keyup.enter="scope.set" />
			</QPopupEdit>
		</q-item>
	</q-list>
</template>

<script setup lang="ts">
import type { IQTextProps } from '@interfaces';

const editMode = ref(false);

const props = withDefaults(
	defineProps<
		IQTextProps & {
			displayText?: string;
			disabled?: boolean;
		}
	>(),
	{
		disabled: false,
		displayText: '',
		value: '',
		cy: '',
	},
);

const inputClasses = computed(() => ({
	[`text-${props.size}`]: true,
	[`text-weight-${props.bold}`]: true,
}));

defineEmits<{
	(e: 'save', save: string): void;
}>();
</script>

<style lang="scss">
.q-popup-edit {
	box-shadow: none !important;
	background: transparent !important;
	backdrop-filter: none !important;
}
</style>
