<template>
	<q-tabs
		v-model="model"
		active-color="primary"
		indicator-color="primary"
		vertical>
		<!-- Step headers	-->
		<template
			v-for="(header, index) in headers"
			:key="index">
			<q-tab
				:color="color"
				:complete="index + 1 === headers.length ? model > index : model > index + 1"
				:data-cy="`setup-header-tab-${index + 1}`"
				:label="header.name"
				:name="index + 1"
				:disable="isDisabled"
				class="setup-tab"
				edit-icon="$complete" />
			<q-separator
				v-if="index < headers.length - 1"
				:key="index + 100" />
		</template>
	</q-tabs>
</template>

<script setup lang="ts">
import { get } from '@vueuse/core';

const model = defineModel<number>({
	default: 1,
});

const props = defineProps<{ headers: { name: string }[] }>();

const color = computed(() => {
	return get(model) === props.headers.length ? 'green' : get(model) > props.headers.length ? 'green' : 'red';
});

const isDisabled = computed(() => {
	return get(model) == 1;
});
</script>
