<template>
	<QRow>
		<QCol
			v-for="(item, index) in requirements"
			:key="index"
			cols="6">
			<QText :value="item.text">
				<template #prepend>
					<ValidIcon :valid="item.valid ? ValidationLevel.Valid : ValidationLevel.Invalid" />
				</template>
			</QText>
		</QCol>
	</QRow>
</template>

<script setup lang="ts">
import { get, set } from '@vueuse/core';
import { passwordStrength as passwordCheck } from 'check-password-strength';
import { ValidationLevel } from '@enums';

const { t } = useI18n();

const isValid = defineModel<boolean>('isValid', {
	default: false,
});

const props = withDefaults(defineProps<{
	value?: string;
	minPasswordLength?: number;
}>(), {
	value: '',
	minPasswordLength: 8,
});

const passwordStrength = computed((): {
	contains: ('lowercase' | 'uppercase' | 'number' | 'symbol')[];
	length: number;
	id: number;
	value: string;
} => passwordCheck(props.value));

const requirements = computed((): { text: string; valid: boolean }[] => {
	return [
		{
			// 'Has a capital letter'
			text: t('components.password-strength.validation.uppercase'),
			valid: get(passwordStrength).contains.some((x) => x === 'uppercase'),
		},
		{
			// 'Has a lowercase letter'
			text: t('components.password-strength.validation.lowercase'),
			valid: get(passwordStrength).contains.some((x) => x === 'lowercase'),
		},
		{
			// 'Has a number'
			text: t('components.password-strength.validation.number'),
			valid: get(passwordStrength).contains.some((x) => x === 'number'),
		},
		{
			// 'Has a special character'
			text: t('components.password-strength.validation.symbol'),
			valid: get(passwordStrength).contains.some((x) => x === 'symbol'),
		},
		{
			// 'Longer than 7 characters'
			text: t('components.password-strength.validation.length'),
			valid: get(passwordStrength).length >= props.minPasswordLength,
		},
	];
});

watchImmediate(() => get(requirements), () => {
	set(isValid, get(requirements).every((x) => x.valid));
});
</script>
