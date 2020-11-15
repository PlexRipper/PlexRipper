<template>
	<span> {{ getFormattedDateTime }}</span>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import { format } from 'date-fns';

@Component
export default class DateTime extends Vue {
	@Prop({ required: true, type: String })
	readonly text!: string;

	get getFormattedDateTime(): string {
		return format(new Date(this.text), 'MM/dd/yyyy');
	}
}
</script>
