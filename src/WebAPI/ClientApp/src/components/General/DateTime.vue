<template>
	<span> {{ dateTimeString }}</span>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import { format } from 'date-fns';
import { SettingsService } from '@state';

@Component
export default class DateTime extends Vue {
	@Prop({ required: true, type: String, default: '' })
	readonly text!: string;

	@Prop({ required: false, type: Boolean, default: false })
	readonly shortDate!: boolean;

	@Prop({ required: false, type: Boolean, default: false })
	readonly longDate!: boolean;

	@Prop({ required: false, type: Boolean, default: true })
	readonly time!: boolean;

	shortDateFormat: string = 'dd/MM/yyyy';
	longDateFormat: string = 'EEEE, dd MMMM yyyy';
	timeFormat: string = 'HH:mm:ss';

	get date(): Date {
		return new Date(this.text);
	}

	get dateTimeString(): string {
		if (!this.text) {
			return '';
		}
		let string = '';
		if (this.time) {
			string += format(this.date, this.timeFormat);
		}
		if (this.time && (this.shortDate || this.longDate)) {
			string += ' - ';
		}

		if (this.shortDate && this.shortDateFormat) {
			string += format(this.date, this.shortDateFormat);
		}

		if (this.longDate && this.longDateFormat) {
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
