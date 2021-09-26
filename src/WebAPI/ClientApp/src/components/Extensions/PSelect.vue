<template>
	<v-select
		:dense="dense"
		:filled="filled"
		:outlined="outlined"
		:color="color"
		:hide-details="hideDetails"
		:menu-props="menuProps"
		v-bind="$attrs"
		v-on="$listeners"
	>
		<template #item="slotProps">
			<slot name="item" v-bind="slotProps"></slot>
		</template>
	</v-select>
</template>

<script lang="ts">
import { Component, Prop } from 'vue-property-decorator';
import { VSelect } from 'vuetify/lib';

@Component<PSelect>({})
export default class PSelect extends VSelect {
	@Prop({ required: false, type: Boolean, default: true })
	readonly dense!: boolean;

	@Prop({ required: false, type: Boolean, default: true })
	readonly filled!: boolean;

	@Prop({ required: false, type: Boolean, default: true })
	readonly outlined!: boolean;

	@Prop({ required: false, type: String, default: 'red' })
	readonly color!: string;

	@Prop({ required: false, type: String, default: 'auto' })
	readonly hideDetails!: string;

	@Prop({
		required: false,
		default: () => {
			return { offsetY: true, contentClass: 'menu-background' };
		},
	})
	readonly menuProps!: any;
}
</script>
