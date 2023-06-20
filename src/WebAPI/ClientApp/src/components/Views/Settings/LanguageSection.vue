<template>
	<q-section>
		<template #header>
			{{ $t('pages.settings.ui.language.header') }}
		</template>
		<help-row help-id="help.settings.ui.language.language-selection">
			<q-select v-model:model-value="language" :dense="false" :options="languageOptions" data-cy="language-selector">
				<template #selected-item="scope">
					<q-item>
						<q-item-section avatar>
							<q-img :src="scope.opt.img" height="50" :max-width="80" :alt="scope.opt.text" />
						</q-item-section>
						<q-item-section>
							<q-item-label> {{ scope.opt.text }}</q-item-label>
						</q-item-section>
					</q-item>
				</template>
				<template #option="scope">
					<q-item v-bind="scope.itemProps" :data-cy="`option-${scope.opt.code}`">
						<q-item-section avatar>
							<q-img :src="scope.opt.img" height="50" :max-width="80" :alt="scope.opt.text" />
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
import { get, set } from '@vueuse/core';
import { useI18n } from 'vue-i18n';
import { useSettingsStore } from '~/store';

interface ILanguageOption {
	code: string;
	file: string;
	iso: string;
	text: string;
	path: string;
	value: string;
	img: string;
}

const i18n = useI18n();
const settingsStore = useSettingsStore();

const languageOptions = ref<ILanguageOption[]>([]);

const language = computed({
	get: () => get(languageOptions).find((x) => x.value === settingsStore.languageSettings.language),
	set: (value: ILanguageOption) => (settingsStore.languageSettings.language = value.code),
});

onMounted(() => {
	// @ts-ignore
	set(
		languageOptions,
		i18n.locales.value.map((locale) => {
			return {
				...locale,
				value: locale.code,
				img: `/img/flags/${locale.code}.svg`,
			};
		}) as ILanguageOption[],
	);
});
</script>
