<template>
	<q-input
		v-model="model"
		lazy-rules
		:rules="disableValidation ? [(v: string): boolean | string =>
			!!v || t('components.api-key-input-field.validation.api-key-is-required')] : getApiKeyRules"
		color="red"
		full-width
		outlined
		required
		hide-bottom-space
		name="password"
		:data-cy="cy"
		:append-icon="showApiKey ? 'mdi-eye' : 'mdi-eye-off'"
		:type="showApiKey ? 'text' : 'password'"
		@focus="hasFocus = true"
		@blur="onBlur"
		@click:append="showApiKey = !showApiKey">
		<template
			v-if="!hideMaskButton"
			#append>
			<q-btn
				flat
				:icon="showApiKey ? 'mdi-eye-off' : 'mdi-eye'"
				@click="showApiKey = !showApiKey" />
		</template>
	</q-input>
</template>

<script setup lang="ts">
import { set } from '@vueuse/core';

const { t } = useI18n();
const model = defineModel<string>({
	default: '',
});

const hasFocus = defineModel<boolean>('hasFocus');

const showApiKey = ref(false);

withDefaults(defineProps<{
	hideMaskButton?: boolean;
	cy: string;
	showStrength?: boolean;
	minPasswordLength?: number;
	disableValidation?: boolean;
}>(), {
	hideMaskButton: false,
	cy: 'api-key-input-field',
	showStrength: false,
	minPasswordLength: 8,
	disableValidation: false,
});

const emits = defineEmits<{
	(e: 'blur'): void;
}>();

const getApiKeyRules = computed(() => [
	// Rule: API key is required
	(v: string): boolean | string =>
		!!v || t('components.api-key-input-field.validation.api-key-is-required'),

	// Rule: API key must be 32 characters
	(v: string): boolean | string =>
		(v && v.length === 32)
		|| t('components.api-key-input-field.validation.api-key-length'),

	// Rule: API key must be hexadecimal
	(v: string): boolean | string =>
		(v && /^[a-f0-9]+$/i.test(v))
		|| t('components.api-key-input-field.validation.api-key-format'),

	// Optional: prevent obvious placeholder or test values
	(v: string): boolean | string =>
		(!v || !/^(?:1234|abcd|test|api-key)$/i.test(v))
		|| t('components.api-key-input-field.validation.api-key-invalid-placeholder'),
]);

function onBlur() {
	set(hasFocus, false);
	emits('blur');
}
</script>
