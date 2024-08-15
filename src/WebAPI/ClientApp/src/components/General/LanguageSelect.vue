<template>
	<q-select
		v-model:model-value="language"
		:dense="dense"
		:options="languageOptions"
		data-cy="language-selector">
		<template #selected-item="scope">
			<q-item :dense="dense">
				<q-item-section avatar>
					<q-img
						:src="scope.opt.img"
						:height="dense ? '30' : '50'"
						:max-width="80"
						fit="fill"
						:alt="scope.opt.text" />
				</q-item-section>
				<q-item-section>
					<q-item-label> {{ scope.opt.text }}</q-item-label>
				</q-item-section>
			</q-item>
		</template>
		<template #option="scope">
			<q-item
				:dense="dense"
				v-bind="scope.itemProps"
				:data-cy="`option-${scope.opt.code}`">
				<q-item-section avatar>
					<q-img
						:src="scope.opt.img"
						:height="dense ? '30' : '50'"
						:max-width="80"
						fit="fill"
						:alt="scope.opt.text" />
				</q-item-section>
				<q-item-section>
					<q-item-label> {{ scope.opt.text }}</q-item-label>
				</q-item-section>
			</q-item>
		</template>
	</q-select>
</template>

<script setup lang="ts">
import { get } from '@vueuse/core';
import type { ILocaleConfig } from '@interfaces';
import { useLocalizationStore } from '~/store';

interface ILanguageOption extends ILocaleConfig {
	value: string;
	img: string;
}
withDefaults(defineProps<{ dense: boolean }>(), {
	dense: false,
});

const localizationStore = useLocalizationStore();

const language = computed({
	get: (): ILanguageOption =>
		get(languageOptions).find((x) => x.value === localizationStore.getLanguageLocale.code) ?? ({} as ILanguageOption),
	set: (value: ILanguageOption) => localizationStore.changeLanguageLocale(value.code),
});

const languageOptions = computed((): ILanguageOption[] =>
	localizationStore.getLanguageLocaleOptions.map((locale) => ({
		...locale,
		value: locale.code,
		img: `/img/flags/${locale.code}.svg`,
	})),
);
</script>
