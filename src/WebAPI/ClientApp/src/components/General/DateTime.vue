<template>
	<span> {{ dateTimeString }}</span>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import { format } from 'date-fns';
import SettingsService from '@state/settingsService';
@Component
export default class DateTime extends Vue {
	@Prop({ required: true, type: String })
	readonly text!: string;

	@Prop({ required: false, type: Boolean, default: false })
	readonly shortDate!: boolean;

	@Prop({ required: false, type: Boolean, default: false })
	readonly longDate!: boolean;

	@Prop({ required: false, type: Boolean, default: true })
	readonly time!: boolean;

	shortDateFormat: string = '';
	longDateFormat: string = '';
	timeFormat: string = '';

	get date(): Date {
		return new Date(this.text);
	}

	get dateTimeString(): string {
		let string = '';
		if (this.time) {
			string += format(this.date, this.timeFormat);
		}
		if (this.time && (this.shortDate || this.longDate)) {
			string += ' - ';
		}

		if (this.shortDate) {
			string += format(this.date, this.shortDateFormat);
		}

		if (this.longDate) {
			string += format(this.date, this.longDateFormat);
		}

		return string;
	}

	mounted(): void {
		this.$subscribeTo(SettingsService.getDateTimeSettings(), (dateTimeSettings) => {
			if (dateTimeSettings) {
				this.shortDateFormat = dateTimeSettings.shortDateFormat;
				this.longDateFormat = dateTimeSettings.longDateFormat;
				this.timeFormat = dateTimeSettings.timeFormat;
			}
		});
	}
}
</script>
