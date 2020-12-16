<template>
	<v-row justify="center">
		<v-col cols="11">
			<!-- Circular progress -->
			<v-row v-if="showCircular" justify="center" no-gutters class="my-3">
				<v-col cols="auto">
					<v-progress-circular :size="70" :width="7" color="red" indeterminate></v-progress-circular>
				</v-col>
			</v-row>
			<!-- Progress text -->
			<v-row v-if="text" justify="center" no-gutters>
				<v-col cols="auto">
					<h2>{{ text }}</h2>
				</v-col>
			</v-row>
			<!-- Library progress bar -->
			<v-row justify="center" class="my-3" no-gutters>
				<v-col>
					<v-progress-linear :value="percentage" height="20" class="mx-1" striped stream color="red">
						<template #default="{ value }">
							<strong>{{ value }}%</strong>
						</template>
					</v-progress-linear>
				</v-col>
			</v-row>
		</v-col>
	</v-row>
</template>

<script lang="ts">
import { Component, Vue, Prop } from 'vue-property-decorator';

@Component
export default class ProgressComponent extends Vue {
	@Prop({ required: false, type: String, default: '' })
	readonly text!: string;

	@Prop({ required: true, type: Number })
	readonly percentage!: number;

	@Prop({ required: false, type: Boolean, default: true })
	readonly showCircular!: boolean;
}
</script>
