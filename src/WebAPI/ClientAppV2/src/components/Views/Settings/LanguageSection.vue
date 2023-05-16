<template>
	<q-section>
		<template #header>
			{{ $t('pages.settings.ui.language.header') }}
		</template>
		<q-row>
			<q-col>
				<q-markup-table flat>
					<tbody>
						<!--	Short Date Format Setting	-->
						<tr>
							<td style="width: 30%">
								<help-icon help-id="help.settings.ui.language.language-selection" />
							</td>
							<td>
								<q-select
									:model-value="language"
									:dense="false"
									:options="languageOptions"
									@update:model-value="updateSettings">
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
										<q-item v-bind="scope.itemProps">
											<q-item-section avatar>
												<q-img :src="scope.opt.img" height="50" :max-width="80" :alt="scope.opt.text" />
											</q-item-section>
											<q-item-section>
												<q-item-label> {{ scope.opt.text }}</q-item-label>
											</q-item-section>
										</q-item>
									</template>
								</q-select>
							</td>
						</tr>
					</tbody>
				</q-markup-table>
			</q-col>
		</q-row>
	</q-section>
</template>

<script setup lang="ts">
import Log from 'consola';

import { set } from '@vueuse/core';
import { useSubscription } from '@vueuse/rxjs';
import { useI18n } from 'vue-i18n';
import { SettingsService } from '@service';

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
const language = ref<ILanguageOption | null>(null);
const languageOptions = ref<ILanguageOption[]>([]);

const updateSettings = (languageOption: ILanguageOption): void => {
	Log.debug('Changed language to: ', languageOption);
	useSubscription(SettingsService.updateLanguageSettings('language', languageOption.code).subscribe());
};

onMounted(() => {
	// @ts-ignore
	set(
		languageOptions,
		i18n.locales.value.map((x) => {
			return {
				...x,
				value: x.code,
				img: `/img/flags/${x.code}.svg`,
			};
		}) as ILanguageOption[],
	);

	useSubscription(
		SettingsService.getLanguage().subscribe((data) => {
			language.value = languageOptions.value.find((x) => x.code === data) ?? null;
			i18n.setLocale(data);
		}),
	);
});
</script>
