<template>
	<q-row justify="between" class="flex-nowrap" no-gutters>
		<q-col cols="9">
			<q-sub-header v-if="!editMode" class="form-label text-no-wrap">
				{{ newValue }}
			</q-sub-header>
			<q-input v-else v-model="newValue" />
		</q-col>
		<q-col justify="right">
			<EditIconButton v-if="!editMode" :disabled="disabled" :height="50" @click="edit" />
			<SaveIconButton v-else :disabled="disabled" :height="50" @click="save" />
		</q-col>
	</q-row>
</template>

<script setup lang="ts">
const props = defineProps<{
	label: string;
	value?: string;
	disabled?: boolean;
}>();

const emit = defineEmits<{
	(e: 'save', save: string): void;
}>();

const editMode = ref(false);
const newValue = ref(props.value);

function edit(): void {
	editMode.value = true;
}

function save(): void {
	editMode.value = false;
	emit('save', newValue?.value ?? '');
}
</script>
