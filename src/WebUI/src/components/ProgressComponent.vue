<template>
	<v-row justify="center">
		<v-col cols="10">
			<v-layout row justify-center align-center>
				<v-progress-circular :size="70" :width="7" color="red" indeterminate></v-progress-circular>
			</v-layout>
			<h1>{{ text }}</h1>
			<!-- Library progress bar -->
			<v-progress-linear :value="percentage" height="20" class="mt-3" striped color="red">
				<template v-slot="{ value }">
					<strong>{{ value }}%</strong>
				</template>
			</v-progress-linear>
		</v-col>
	</v-row>
</template>

<script lang="ts">
import { Component, Vue, Prop } from 'vue-property-decorator';

@Component
export default class ProgressComponent extends Vue {
	@Prop({ required: true, type: String })
	readonly text!: string[];

	@Prop({ required: true, type: Number })
	readonly percentage: number = 0;
}
</script>
