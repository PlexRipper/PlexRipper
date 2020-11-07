<template>
	<v-row justify="space-between" no-gutters>
		<v-col cols="auto">
			<p class="form-label">{{ label }}</p>
		</v-col>
		<v-col cols="auto">
			<v-btn v-if="id" icon @click="openDialog">
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

	@Prop({ required: false, type: String, default: '' })
	readonly id!: string;

	openDialog(): void {
		HelpService.openHelpDialog(this.id);
	}
}
</script>
