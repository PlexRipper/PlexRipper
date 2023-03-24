<template>
	<q-dialog :model-value="dialog" persistent @click:outside="cancel">
		<q-card>
			<q-card-section class="row items-center">
				{{ getText.title }}
			</q-card-section>
			<q-card-section>
				<p>{{ getText.text }}</p>
				<p v-if="getText.warning" class="text-center">
					<b>{{ getText.warning }}</b>
				</p>
			</q-card-section>
			<q-separator />
			<q-card-actions align="right">
				<CancelButton @click="cancel" />
				<q-space />
				<ConfirmButton :loading="loading" @click="confirm" />
			</q-card-actions>
		</q-card>
	</q-dialog>
</template>

<script setup lang="ts">
import { defineProps, defineEmits, ref, computed } from 'vue';
import { useNuxtApp } from '#imports';

const { $getMessage } = useNuxtApp();

const loading = ref(false);

const props = defineProps<{
	/**
	 * The Vue-i18n text id used for the confirmation window that pops-up.
	 * @type {string}
	 */
	textId: string;
	dialog: boolean;
	confirmLoading?: boolean;
}>();

const emit = defineEmits<{
	(e: 'confirm'): void;
	(e: 'cancel'): void;
}>();

const getText = computed(() => {
	if (props.textId === '') {
		return {
			id: 'null',
			title: 'Could not find the correct confirmation text..',
			text: 'Could not find the correct confirmation text..',
			warning: '',
		};
	}
	const msg: any = $getMessage(`confirmation.${props.textId}`);
	return {
		id: props.textId,
		title: msg?.title ?? '',
		text: msg?.text ?? '',
		warning: msg?.warning ?? '',
	};
});

const cancel = () => {
	emit('cancel');
	loading.value = false;
};

const confirm = () => {
	emit('confirm');
	if (props.confirmLoading) {
		loading.value = true;
	}
};
</script>
