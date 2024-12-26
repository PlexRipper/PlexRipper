<template>
	<q-input
		v-model="model"
		:rules="getPasswordRules"
		color="red"
		full-width
		outlined
		required
		hide-bottom-space
		:data-cy="cy"
		:append-icon="showPassword ? 'mdi-eye' : 'mdi-eye-off'"
		:type="showPassword ? 'text' : 'password'"
		@click:append="showPassword = !showPassword">
		<template #append>
			<q-btn
				flat
				:icon="showPassword ? 'mdi-eye-off' : 'mdi-eye'"
				@click="showPassword = !showPassword" />
		</template>
	</q-input>
</template>

<script setup lang="ts">
const model = defineModel<string>();

const showPassword = ref(false);

withDefaults(defineProps<{
	cy: string;
}>(), {
	cy: 'password-input-field',
});

const getPasswordRules = computed(() => [
	(v: string): boolean | string => !!v || 'Password is required',
	(v: string): boolean | string => (v && v.length >= 8) || 'Password must be at least 8 characters',
]);
</script>

<style lang="scss">

</style>
