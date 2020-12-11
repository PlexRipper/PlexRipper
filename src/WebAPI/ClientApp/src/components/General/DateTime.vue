<template>
	<span> {{ dateTimeString }}</span>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import { format } from 'date-fns';
import { settingsStore } from '~/store';
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

	get date(): Date {
		return new Date(this.text);
	}

	get timeFormatted(): string {
		return format(this.date, settingsStore.timeFormat);
	}

	get shortDateFormatted(): string {
		return format(this.date, settingsStore.shortDateFormat);
	}

	get longDateFormatted(): string {
		return format(this.date, settingsStore.longDateFormat);
	}

	get dateTimeString(): string {
		let string = '';
		if (this.time) {
			string += this.timeFormatted;
		}
		if (this.time && (this.shortDate || this.longDate)) {
			string += ' - ';
		}

		if (this.shortDate) {
			string += this.shortDateFormatted;
		}

		if (this.longDate) {
			string += this.longDateFormatted;
		}

		return string;
	}
}
</script>
