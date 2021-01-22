<template>
	<span> {{ duration }}</span>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import { formatDuration } from 'date-fns';

@Component
export default class Duration extends Vue {
	@Prop({ required: true, type: Number })
	readonly value!: number;

	get duration(): string {
		const date = new Date(this.value * 1000);
		return formatDuration(
			{ hours: date.getUTCHours(), minutes: date.getUTCMinutes(), seconds: date.getSeconds() },
			{ delimiter: ' ', format: ['hours', 'minutes', 'seconds'] },
		);
	}
}
</script>
