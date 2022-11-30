<template>
	<p-section>
		<template #header>
			{{ $t('pages.settings.ui.date-and-time.header') }}
		</template>
		<v-row no-gutters>
			<v-col>
				<v-simple-table class="section-table">
					<tbody>
						<!--	Short Date Format Setting	-->
						<tr>
							<td style="width: 30%">
								<help-icon help-id="help.settings.ui.date-and-time.short-date-format" />
							</td>
							<td>
								<p-select :value="shortDateFormat" :items="shortDateOptions" @input="updateSettings(0, $event)" />
							</td>
						</tr>
						<!--	Long Date Format Setting	-->
						<tr>
							<td>
								<help-icon help-id="help.settings.ui.date-and-time.long-date-format" />
							</td>
							<td>
								<p-select :value="longDateFormat" :items="longDateOptions" @input="updateSettings(1, $event)" />
							</td>
						</tr>
						<!--	Time Format Setting	-->
						<tr>
							<td>
								<help-icon help-id="help.settings.ui.date-and-time.time-format" />
							</td>
							<td>
								<p-select :value="timeFormat" :items="timeFormatOptions" @input="updateSettings(2, $event)" />
							</td>
						</tr>
						<!--	Time Zone Setting	-->
						<!--	Dealing with Timezones is 1 big clusterfuck, will go back to try again later-->
						<!--						<tr>-->
						<!--							<td>-->
						<!--								<help-icon help-id="help.settings.ui.date-and-time.time-zone" />-->
						<!--							</td>-->
						<!--							<td>-->
						<!--								<v-select-->
						<!--									v-model="timeZone"-->
						<!--									color="red"-->
						<!--									filled-->
						<!--									outlined-->
						<!--									dense-->
						<!--									class="my-3"-->
						<!--									hide-details="auto"-->
						<!--									:menu-props="getMenuProps"-->
						<!--									:items="timeZoneOptions"-->
						<!--								/>-->
						<!--							</td>-->
						<!--						</tr>-->

						<!--	Show Relative Dates Setting	-->
						<tr>
							<td>
								<help-icon help-id="help.settings.ui.date-and-time.show-relative-dates" />
							</td>
							<td>
								<p-checkbox :value="showRelativeDates" @input="updateSettings(4, $event)" />
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
// eslint-disable-next-line import/no-duplicates
import { format } from 'date-fns';
// eslint-disable-next-line import/no-duplicates
import { enUS, fr } from 'date-fns/locale';

import { useSubscription } from '@vueuse/rxjs';
import { SettingsService } from '@service';
import { DateTimeSettingsDTO } from '@dto/mainApi';

interface ISelectItem {
	text: string | number | object;
	value: string | number | object;
	disabled?: boolean;
	divider?: boolean;
	header?: string;
}

@Component
export default class DateAndTimeSection extends Vue {
	// region Settings

	shortDateFormat: string = '';
	longDateFormat: string = '';
	timeFormat: string = '';
	timeZone: string = '';
	showRelativeDates: boolean = false;

	// endregion

	get getMenuProps(): any {
		return {
			offsetY: true,
			contentClass: 'menu-background',
		};
	}

	get getLocale(): { locale?: Locale } {
		switch (this.$i18n.locale) {
			case 'en-US':
				return { locale: enUS };
			case 'fr-FR':
				return { locale: fr };
			default:
				return { locale: enUS };
		}
	}

	get shortDateOptions(): ISelectItem[] {
		const values: string[] = ['MMM dd yyyy', 'dd MMM yyyy', 'MM/dd/yyyy', 'dd/MM/yyyy', 'yyyy-MM-dd'];
		const options: ISelectItem[] = [];
		const date = Date.now();
		values.forEach((x) => {
			options.push({
				value: x,
				text: format(date, x, this.getLocale),
			});
		});

		return options;
	}

	get longDateOptions(): ISelectItem[] {
		const values: string[] = ['EEEE, MMMM dd, yyyy', 'EEEE, dd MMMM yyyy'];
		const options: ISelectItem[] = [];
		const date = Date.now();
		values.forEach((x) => {
			options.push({
				value: x,
				text: format(date, x, this.getLocale),
			});
		});

		return options;
	}

	get timeFormatOptions(): ISelectItem[] {
		const values: string[] = ['HH:mm:ss', 'pp'];
		const options: ISelectItem[] = [];
		const date = Date.now();
		values.forEach((x) => {
			options.push({
				value: x,
				text: format(date, x, this.getLocale),
			});
		});

		return options;
	}

	get timeZoneOptions(): ISelectItem[] {
		const currentTZ = Intl.DateTimeFormat().resolvedOptions().timeZone;
		const offSet = new Date().getTimezoneOffset() / 60;
		return [{ text: `${offSet} ${currentTZ}`, value: currentTZ }];
	}

	updateSettings(index: number, state: any): void {
		let key: keyof DateTimeSettingsDTO | null = null;
		switch (index) {
			case 0:
				key = 'shortDateFormat';
				break;
			case 1:
				key = 'longDateFormat';
				break;
			case 2:
				key = 'timeFormat';
				break;
			case 3:
				key = 'timeZone';
				break;
			case 4:
				key = 'showRelativeDates';
				break;
			default:
				Log.error(`Failed to update settings with index ${index} and value ${state}`);
				key = null;
		}
		if (key) {
			useSubscription(SettingsService.updateDateTimeSetting(key, state).subscribe());
		}
	}

	mounted(): void {
		useSubscription(
			SettingsService.getShortDateFormat().subscribe((value) => {
				this.shortDateFormat = value;
			}),
		);
		useSubscription(
			SettingsService.getLongDateFormat().subscribe((value) => {
				this.longDateFormat = value;
			}),
		);
		useSubscription(
			SettingsService.getTimeFormat().subscribe((value) => {
				this.timeFormat = value;
			}),
		);
		useSubscription(
			SettingsService.getTimeZone().subscribe((value) => {
				this.timeZone = value;
			}),
		);
		useSubscription(
			SettingsService.getShowRelativeDates().subscribe((value) => {
				this.showRelativeDates = value;
			}),
		);
	}
}
</script>
