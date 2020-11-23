<template>
	<span> {{ getFormattedDateTime }}</span>
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

	get getFormat(): string {
		let format = '';
		if (this.time) {
			format += settingsStore.timeFormat;
			if (this.longDate || this.shortDate) {
				format += ' - ';
			}
		}

		if (this.shortDate || (!this.longDate && !this.shortDate)) {
			format += settingsStore.shortDateFormat;
		}

		if (this.longDate) {
			format += settingsStore.longDateFormat;
		}
		return format;
	}

	get getFormattedDateTime(): string {
		return format(new Date(this.text), this.getFormat);
	}
}
</script>
