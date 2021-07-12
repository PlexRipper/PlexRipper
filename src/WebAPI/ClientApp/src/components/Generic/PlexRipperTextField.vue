<template>
	<v-text-field
		:append-icon="appendIcon"
		:value="value"
		:readonly="readonly"
		:height="height"
		dense
		outlined
		solo
		@click:append="clickAppend"
		@input="$emit('input', $event)"
	/>
</template>

<script lang="ts">
import { Component, Vue, Prop } from 'vue-property-decorator';

@Component
export default class PTextField extends Vue {
	@Prop({ required: false, type: String })
	readonly appendIcon!: string;

	@Prop({ required: false, type: String })
	readonly value!: string;

	@Prop({ required: false, type: Boolean })
	readonly readonly!: boolean;

	@Prop({ required: false, type: Number, default: 50 })
	readonly height!: number;

	clickAppend(): void {
		this.$emit('click');
	}
}
</script>
