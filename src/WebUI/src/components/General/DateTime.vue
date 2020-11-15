<template>
	<span> {{ getFormattedDateTime }}</span>
</template>

<script lang="ts">
import Log from 'consola';
import { Component, Prop, Vue } from 'vue-property-decorator';

@Component
export default class DateTime extends Vue {
	@Prop({ required: true, type: String })
	readonly text!: string;

	get getFormattedDateTime(): string {
		Log.warn(this.$moment(this.text));
		return this.$moment(this.text);
	}
}
</script>
