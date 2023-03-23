<template>
	<q-row>
		<q-col>
			<div class="cursor-pointer">
				<q-sub-header>
					{{ newValue }}
				</q-sub-header>
				<QPopupEdit ref="popup" v-slot="scope" :model-value="newValue" auto-save @save="save">
					<q-input v-model="scope.value" dense autofocus counter @keyup.enter="scope.set" />
				</QPopupEdit>
			</div>
		</q-col>
	</q-row>
</template>

<script setup lang="ts">
import { QPopupEdit } from 'quasar';

const props = defineProps<{
	label: string;
	value?: string;
	disabled?: boolean;
}>();

const emit = defineEmits<{
	(e: 'save', save: string): void;
}>();

const popup = ref<QPopupEdit>(null);

const newValue = ref(props.value);

function save(event: string): void {
	emit('save', event);
}
</script>
