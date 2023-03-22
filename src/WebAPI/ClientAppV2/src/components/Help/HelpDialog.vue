<template>
	<q-dialog :model-value="show" max-width="500" @click:outside="close">
		<q-card>
			<q-card-section>
				<div class="headline i18n-formatting">
					{{ $t(getHelpText.title) }}
				</div>
			</q-card-section>

			<!--	Help text	-->
			<q-card-section>
				<div class="i18n-formatting">
					{{ $t(getHelpText.text) }}
				</div>
			</q-card-section>

			<q-separator />

			<!--	Close action	-->
			<q-card-actions>
				<q-space />
				<q-btn flat :label="$t('general.commands.close')" @click="close" />
			</q-card-actions>
		</q-card>
	</q-dialog>
</template>

<script setup lang="ts">
import { computed, defineProps, defineEmits } from 'vue';

const props = defineProps<{
	show: boolean;
	id: string;
}>();

const emit = defineEmits<{
	(e: 'close'): void;
}>();

const getHelpText = computed(() => {
	if (props.id === '') {
		return {
			id: 'help.default',
			title: 'help.default.title',
			text: 'help.default.text',
		};
	}

	return {
		id: props.id,
		title: `${props.id}.title`,
		text: `${props.id}.text`,
	};
});

const close = () => {
	emit('close');
};
</script>
