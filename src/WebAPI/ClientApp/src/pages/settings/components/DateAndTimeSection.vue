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
								<v-select
									v-model="shortDateFormat"
									color="red"
									filled
									outlined
									dense
									class="my-3"
									hide-details="auto"
									:menu-props="getMenuProps"
									:items="shortDateOptions"
								/>
							</td>
						</tr>
						<!--	Long Date Format Setting	-->
						<tr>
							<td>
								<help-icon help-id="help.settings.ui.date-and-time.long-date-format" />
							</td>
							<td>
								<v-select
									v-model="longDateFormat"
									color="red"
									filled
									outlined
									dense
									class="my-3"
									hide-details="auto"
									:menu-props="getMenuProps"
									:items="longDateOptions"
								/>
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
						<!--	Time Format Setting	-->
						<tr>
							<td>
								<help-icon help-id="help.settings.ui.date-and-time.time-format" />
							</td>
							<td>
								<v-select
									v-model="timeFormat"
									color="red"
									filled
									outlined
									dense
									class="my-3"
									hide-details="auto"
									:menu-props="getMenuProps"
									:items="timeFormatOptions"
								/>
							</td>
						</tr>
						<!--	Show Relative Dates Setting	-->
						<tr>
							<td>
								<help-icon help-id="help.settings.ui.date-and-time.show-relative-dates" />
							</td>
							<td>
								<p-checkbox v-model="showRelativeDates" />
							</td>
						</tr>
					</tbody>
				</v-simple-table>
			</v-col>
		</v-row>
	</p-section>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import { format } from 'date-fns';
import { settingsStore } from '~/store';

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

	get shortDateFormat(): string {
		return settingsStore.shortDateFormat;
	}

	set shortDateFormat(value: string) {
		settingsStore.setShortDateFormat(value);
	}

	get longDateFormat(): string {
		return settingsStore.longDateFormat;
	}

	set longDateFormat(value: string) {
		settingsStore.setLongDateFormat(value);
	}

	get timeFormat(): string {
		return settingsStore.timeFormat;
	}

	set timeFormat(value: string) {
		settingsStore.setTimeFormat(value);
	}

	get timeZone(): string {
		return settingsStore.timeZone;
	}

	set timeZone(value: string) {
		settingsStore.setTimeZone(value);
	}

	get showRelativeDates(): boolean {
		return settingsStore.showRelativeDates;
	}

	set showRelativeDates(value: boolean) {
		settingsStore.setShowRelativeDates(value);
	}
	// endregion

	get getMenuProps(): any {
		return {
			offsetY: true,
			contentClass: 'menu-background',
		};
	}

	get shortDateOptions(): ISelectItem[] {
		const values: string[] = ['MMM dd yyyy', 'dd MMM yyyy', 'MM/dd/yyyy', 'dd/MM/yyyy', 'yyyy-MM-dd'];
		const options: ISelectItem[] = [];
		const date = Date.now();
		values.forEach((x) => {
			options.push({
				value: x,
				text: format(date, x),
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
				text: format(date, x),
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
				text: format(date, x),
			});
		});

		return options;
	}

	get timeZoneOptions(): ISelectItem[] {
		const currentTZ = Intl.DateTimeFormat().resolvedOptions().timeZone;
		const offSet = new Date().getTimezoneOffset() / 60;
		return [{ text: `${offSet} ${currentTZ}`, value: currentTZ }];
	}
}
</script>
