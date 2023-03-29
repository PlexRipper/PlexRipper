<template>
	<q-section>
		<template #header>
			{{ $t('pages.settings.ui.date-and-time.header') }}
		</template>
		<q-row no-gutters>
			<q-col>
				<q-markup-table flat>
					<tbody>
						<!--	Short Date Format Setting	-->
						<tr>
							<td style="width: 30%">
								<help-icon help-id="help.settings.ui.date-and-time.short-date-format" />
							</td>
							<td>
								<q-select
									:model-value="shortDateFormat"
									:options="shortDateOptions"
									@update:model-value="updateSettings('shortDateFormat', $event.value)" />
							</td>
						</tr>
						<!--	Long Date Format Setting	-->
						<tr>
							<td>
								<help-icon help-id="help.settings.ui.date-and-time.long-date-format" />
							</td>
							<td>
								<q-select
									:model-value="longDateFormat"
									:options="longDateOptions"
									@update:model-value="updateSettings('longDateFormat', $event.value)" />
							</td>
						</tr>
						<!--	Time Format Setting	-->
						<tr>
							<td>
								<help-icon help-id="help.settings.ui.date-and-time.time-format" />
							</td>
							<td>
								<q-select
									:model-value="timeFormat"
									:options="timeFormatOptions"
									@update:model-value="updateSettings('timeFormat', $event.value)" />
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
						<!--									:options="timeZoneOptions"-->
						<!--								/>-->
						<!--							</td>-->
						<!--						</tr>-->

						<!--	Show Relative Dates Setting	-->
						<tr>
							<td>
								<help-icon help-id="help.settings.ui.date-and-time.show-relative-dates" />
							</td>
							<td>
								<q-toggle
									:model-value="showRelativeDates"
									size="lg"
									color="red"
									@update:model-value="updateSettings('showRelativeDates', $event)" />
							</td>
						</tr>
					</tbody>
				</q-markup-table>
			</q-col>
		</q-row>
	</q-section>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
// eslint-disable-next-line import/no-duplicates
import { format } from 'date-fns';
// eslint-disable-next-line import/no-duplicates
import { enUS, fr } from 'date-fns/locale';

import { useSubscription } from '@vueuse/rxjs';
import { SettingsService } from '@service';
import { DateTimeSettingsDTO } from '@dto/mainApi';

const i18n = useI18n();

interface ISelectOption {
	value: string;
	label: string;
}

// region Settings
const shortDateFormat = ref<ISelectOption | null>(null);
const longDateFormat = ref<ISelectOption | null>(null);
const timeFormat = ref<ISelectOption | null>(null);
const timeZone = ref<ISelectOption | null>(null);
const showRelativeDates = ref(false);

// endregion

const getLocale = computed(() => {
	switch (i18n.locale.value) {
		case 'en-US':
			return { locale: enUS };
		case 'fr-FR':
			return { locale: fr };
		default:
			return { locale: enUS };
	}
});

const shortDateOptions = computed(() => {
	const values: string[] = ['MMM dd yyyy', 'dd MMM yyyy', 'MM/dd/yyyy', 'dd/MM/yyyy', 'yyyy-MM-dd'];
	const date = Date.now();

	return values.map((x) => {
		return {
			value: x,
			label: format(date, x, getLocale.value),
		};
	});
});

const longDateOptions = computed(() => {
	const values: string[] = ['EEEE, MMMM dd, yyyy', 'EEEE, dd MMMM yyyy'];
	const date = Date.now();

	return values.map((x) => {
		return {
			value: x,
			label: format(date, x, getLocale.value),
		};
	});
});

const timeFormatOptions = computed(() => {
	const values: string[] = ['HH:mm:ss', 'pp'];
	const date = Date.now();
	return values.map((x) => {
		return {
			value: x,
			label: format(date, x, getLocale.value),
		};
	});
});

const timeZoneOptions = computed(() => {
	const currentTZ = Intl.DateTimeFormat().resolvedOptions().timeZone;
	const offSet = new Date().getTimezoneOffset() / 60;
	return [{ label: `${offSet} ${currentTZ}`, value: currentTZ }];
});

const updateSettings = (key: keyof DateTimeSettingsDTO, state: any): void => {
	useSubscription(SettingsService.updateDateTimeSetting(key, state).subscribe());
};

onMounted(() => {
	useSubscription(
		SettingsService.getShortDateFormat().subscribe((data) => {
			shortDateFormat.value = shortDateOptions.value.find((x) => x.value === data) ?? shortDateOptions.value[0];
		}),
	);
	useSubscription(
		SettingsService.getLongDateFormat().subscribe((data) => {
			longDateFormat.value = longDateOptions.value.find((x) => x.value === data) ?? longDateOptions.value[0];
		}),
	);
	useSubscription(
		SettingsService.getTimeFormat().subscribe((data) => {
			timeFormat.value = timeFormatOptions.value.find((x) => x.value === data) ?? timeFormatOptions.value[0];
		}),
	);
	useSubscription(
		SettingsService.getTimeZone().subscribe((data) => {
			timeZone.value = timeZoneOptions.value.find((x) => x.value === data) ?? timeZoneOptions.value[0];
		}),
	);
	useSubscription(
		SettingsService.getShowRelativeDates().subscribe((data) => {
			showRelativeDates.value = data;
		}),
	);
});
</script>
