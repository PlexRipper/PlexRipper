<template>
	<q-section>
		<template #header>
			{{ $t('pages.settings.ui.language.header') }}
		</template>
		<help-row help-id="help.settings.ui.language.language-selection">
			<q-select
				v-model:model-value="language"
				:dense="false"
				:options="languageOptions"
				data-cy="language-selector"
			>
				<template #selected-item="scope">
					<q-item>
						<q-item-section avatar>
							<q-img
								:src="scope.opt.img"
								height="50"
								:max-width="80"
								:alt="scope.opt.text"
							/>
						</q-item-section>
						<q-item-section>
							<q-item-label> {{ scope.opt.text }}</q-item-label>
						</q-item-section>
					</q-item>
				</template>
				<template #option="scope">
					<q-item
						v-bind="scope.itemProps"
						:data-cy="`option-${scope.opt.code}`"
					>
						<q-item-section avatar>
							<q-img
								:src="scope.opt.img"
								height="50"
								:max-width="80"
								:alt="scope.opt.text"
							/>
						</q-item-section>
						<q-item-section>
							<q-item-label> {{ scope.opt.text }}</q-item-label>
						</q-item-section>
					</q-item>
				</template>
			</q-select>
		</help-row>
	</q-section>
</template>

<script setup lang="ts">
import { get } from '@vueuse/core';
import type { ILocaleConfig } from '@interfaces';
import { useLocalizationStore } from '~/store';

interface ILanguageOption extends ILocaleConfig {
	value: string;
	img: string;
}

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
