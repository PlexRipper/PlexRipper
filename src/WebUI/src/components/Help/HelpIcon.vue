<template>
	<v-row justify="space-between" no-gutters>
		<v-col cols="auto">
			<p style="margin-top: 6px">{{ label }}</p>
		</v-col>
		<v-col cols="auto">
			<v-btn icon @click="openDialog">
				<v-icon> mdi-help-circle-outline </v-icon>
			</v-btn>
		</v-col>
	</v-row>
</template>

<script lang="ts">
import { Component, Vue, Prop } from 'vue-property-decorator';
import HelpService from '@service/helpService';

@Component
export default class HelpIcon extends Vue {
	@Prop({ required: true, type: String })
	readonly label!: string;

	@Prop({ required: true, type: String })
	readonly id!: string;

	openDialog(): void {
		HelpService.openHelpDialog(this.id);
	}
}
</script>
