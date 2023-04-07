<template>
	<QCardDialog persistent :name="name" button-align="between" @opened="onOpen">
		<template #title>
			{{ confirmationText.title }}
		</template>
		<template #default>
			<p>{{ confirmationText.text }}</p>
			<p v-if="confirmationText.warning" class="text-center">
				<b>{{ confirmationText.warning }}</b>
			</p>
		</template>
		<template #actions>
			<CancelButton @click="cancel" />
			<ConfirmButton :loading="loading" @click="confirm" />
		</template>
	</QCardDialog>
</template>

<script setup lang="ts">
import { defineProps, defineEmits, ref } from 'vue';
import { useI18n } from '#imports';

const { t, te } = useI18n();

const loading = ref(false);
const confirmationText = ref<{
	id?: string;
	title?: string;
	text?: string;
	warning?: string;
}>({});
const props = defineProps<{
	/**
	 * The Vue-i18n text id used for the confirmation window that pops-up.
	 * @type {string}
	 */
	textId: string;
	name: string;
	dialog: boolean;
	confirmLoading?: boolean;
}>();

const emit = defineEmits<{
	(e: 'confirm'): void;
	(e: 'cancel'): void;
}>();

const onOpen = () => {
	const textId = props.textId;
	const result = {
		id: textId,
		warning: '',
	} as IConfirmationResult;

	if (te<string>(`confirmation.${textId}.title`)) {
		result.title = t(`confirmation.${textId}.title`);
	} else {
		result.title = t(`confirmation.not-found.title`);
	}

	if (te<string>(`confirmation.${textId}.text`)) {
		result.text = t(`confirmation.${textId}.text`);
	} else {
		result.text = t(`confirmation.not-found.text`);
	}

	if (te<string>(`confirmation.${textId}.warning`)) {
		result.warning = t(`confirmation.${textId}.warning`);
	}

	confirmationText.value = result;
};

const cancel = () => {
	emit('cancel');
	useCloseControlDialog(props.name);
	loading.value = false;
};

const confirm = () => {
	emit('confirm');
	if (props.confirmLoading) {
		loading.value = true;
	}
};

interface IConfirmationResult {
	id: string;
	title: string;
	text: string;
	warning: string;
}
</script>
