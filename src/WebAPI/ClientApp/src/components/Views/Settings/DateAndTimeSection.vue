<template>
	<q-section>
		<template #header>
			{{ $t('pages.settings.ui.date-and-time.header') }}
		</template>
		<!--	Short Date Format Setting	-->
		<help-row help-id="help.settings.ui.date-and-time.short-date-format">
			<q-select v-model:model-value="shortDateFormat" :options="shortDateOptions" data-cy="short-date-format">
				<template #option="scope">
					<q-item v-bind="scope.itemProps" :data-cy="`option-${scope.opt.value}`">
						<q-item-section>
							<q-item-label> {{ scope.opt.label }}</q-item-label>
						</q-item-section>
					</q-item>
				</template>
			</q-select>
		</help-row>
		<!--	Long Date Format Setting	-->
		<help-row help-id="help.settings.ui.date-and-time.long-date-format">
			<q-select v-model:model-value="longDateFormat" :options="longDateOptions" data-cy="long-date-format">
				<template #option="scope">
					<q-item v-bind="scope.itemProps" :data-cy="`option-${scope.opt.value}`">
						<q-item-section>
							<q-item-label> {{ scope.opt.label }}</q-item-label>
						</q-item-section>
					</q-item>
				</template>
			</q-select>
		</help-row>
		<!--	Time Format Setting	-->
		<help-row help-id="help.settings.ui.date-and-time.time-format">
			<q-select v-model:model-value="timeFormat" :options="timeFormatOptions" data-cy="time-format">
				<template #option="scope">
					<q-item v-bind="scope.itemProps" :data-cy="`option-${scope.opt.value}`">
						<q-item-section>
							<q-item-label> {{ scope.opt.label }}</q-item-label>
						</q-item-section>
					</q-item>
				</template>
			</q-select>
		</help-row>
		<!--	Show Relative Dates Setting	-->
		<help-row help-id="help.settings.ui.date-and-time.show-relative-dates">
			<q-toggle
				v-model:model-value="settingsStore.dateTimeSettings.showRelativeDates"
				size="lg"
				color="red"
				data-cy="relative-date" />
		</help-row>

		<!--	TODO: Dealing with Timezones is 1 big cluster fuck, will go back to try again later-->
		<!--	Time Zone Setting	-->
		<!--		<help-row help-id="help.settings.ui.date-and-time.time-zone">-->
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
		<!--		</help-row>-->
	</q-section>
</template>

<script setup lang="ts">
// eslint-disable-next-line import/no-duplicates
import { format } from 'date-fns';
// eslint-disable-next-line import/no-duplicates
import { enUS, fr } from 'date-fns/locale';

import { get } from '@vueuse/core';
import { useSettingsStore } from '~/store';

const i18n = useI18n();
const settingsStore = useSettingsStore();

interface ISelectOption {
	value: string;
	label: string;
}

// region Settings
const defaultSelectOption: ISelectOption = { value: '', label: '' };
const shortDateFormat = computed({
	get: (): ISelectOption =>
		get(shortDateOptions).find((x) => x.value === settingsStore.dateTimeSettings.shortDateFormat) ?? defaultSelectOption,
	set: (value: ISelectOption) => (settingsStore.dateTimeSettings.shortDateFormat = value.value),
});
const longDateFormat = computed({
	get: (): ISelectOption =>
		get(longDateOptions).find((x) => x.value === settingsStore.dateTimeSettings.longDateFormat) ?? defaultSelectOption,
	set: (value: ISelectOption) => (settingsStore.dateTimeSettings.longDateFormat = value.value),
});
const timeFormat = computed({
	get: (): ISelectOption =>
		get(timeFormatOptions).find((x) => x.value === settingsStore.dateTimeSettings.timeFormat) ?? defaultSelectOption,
	set: (value: ISelectOption) => (settingsStore.dateTimeSettings.timeFormat = value.value),
});

// const timeZone = computed({
// 	get: () => get(timeZoneOptions).find((x) => x.value === settingsStore.dateTimeSettings.timeZone),
// 	set: (value: ISelectOption) => (settingsStore.dateTimeSettings.timeZone = value.value),
// });

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

	return values.map((dateFormat) => {
		return {
			value: dateFormat,
			label: format(date, dateFormat, getLocale.value),
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

// const timeZoneOptions = computed(() => {
// 	const currentTZ = Intl.DateTimeFormat().resolvedOptions().timeZone;
// 	const offSet = new Date().getTimezoneOffset() / 60;
// 	return [{ label: `${offSet} ${currentTZ}`, value: currentTZ }];
// });
</script>
