<template>
	<p-section>
		<template #header>
			{{ $t('pages.settings.ui.language.header') }}
		</template>
		<v-row>
			<v-col>
				<v-simple-table class="section-table">
					<tbody>
						<!--	Short Date Format Setting	-->
						<tr>
							<td style="width: 30%">
								<help-icon help-id="help.settings.ui.language.language-selection" />
							</td>
							<td>
								<p-select :value="language" :items="languageOptions" @input="updateSettings">
									<!--suppress HtmlUnknownAttribute -->
									<template #item="{ item }">
										<v-row class="my-2 no-wrap" align="center">
											<v-col cols="auto">
												<v-icon v-if="item.icon">{{ item.icon }}</v-icon>
												<v-img v-if="item.img" :src="item.img" :height="50" :max-width="80" :alt="item.text" />
											</v-col>
											<v-col cols="10">
												<span class="pa-2">
													{{ item.text }}
												</span>
											</v-col>
										</v-row>
									</template>
								</p-select>
							</td>
						</tr>
					</tbody>
				</v-simple-table>
			</v-col>
		</v-row>
	</p-section>
</template>

<script lang="ts">
import Log from 'consola';
import { Component, Vue } from 'vue-property-decorator';
import { SettingsService } from '@service';

interface ILanguageOption {
	text: string;
	value: string;
	img: string;
}

@Component<LanguageSection>({})
export default class LanguageSection extends Vue {
	language: string = '';
	languageOptions: ILanguageOption[] = [];

	updateSettings(langCode: string): void {
		Log.debug('Changed language to: ', langCode);
		this.$nuxt.$i18n.setLocale(langCode);
		SettingsService.updateSetting('language', langCode);
	}

	mounted() {
		this.$subscribeTo(SettingsService.getLanguage(), (language) => {
			this.language = language;
			this.$nuxt.$i18n.setLocale(language);
		});

		const locals = this.$nuxt.$i18n.locales as any[];
		for (const localsKey of locals) {
			this.languageOptions.push({
				text: localsKey.text,
				value: localsKey.code,
				img: require(`~/assets/img/flags/${localsKey.code}.svg`),
			});
		}
	}
}
</script>
