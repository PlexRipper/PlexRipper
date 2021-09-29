<template>
	<v-row justify="center">
		<v-col cols="11">
			<!-- Circular Mode progress -->
			<template v-if="circularMode">
				<v-row justify="center" no-gutters class="my-3">
					<v-col cols="auto">
						<v-progress-circular
							:size="100"
							:rotate="-90"
							:width="15"
							:value="getPercentage"
							:indeterminate="indeterminate"
							color="red"
						>
							<template v-if="!indeterminate">
								<span v-if="percentage < 100">
									<b>{{ getPercentage }}%</b>
								</span>
								<v-icon v-else large>mdi-check</v-icon>
							</template>
						</v-progress-circular>
					</v-col>
				</v-row>
				<!-- Progress text -->
				<v-row v-if="text" justify="center" no-gutters>
					<v-col cols="auto">
						<h2>{{ text }}</h2>
					</v-col>
				</v-row>
			</template>
			<!-- Linear Mode Progress -->
			<template v-else>
				<!-- Progress text -->
				<v-row v-if="text" justify="center" no-gutters>
					<v-col cols="auto">
						<h2>{{ text }}</h2>
					</v-col>
				</v-row>
				<!-- Progress bar -->
				<v-row justify="center" class="my-3" no-gutters>
					<v-col>
						<v-progress-linear
							:value="Math.min(getPercentage, 100)"
							height="20"
							class="mx-1"
							striped
							stream
							color="red"
							v-bind="$attrs"
							v-on="$listeners"
						>
							<template #default="{}">
								<strong>{{ getPercentage }}%</strong>
							</template>
						</v-progress-linear>
					</v-col>
				</v-row>
			</template>
		</v-col>
	</v-row>
</template>

<script lang="ts">
import { Component, Vue, Prop } from 'vue-property-decorator';

@Component<ProgressComponent>({})
export default class ProgressComponent extends Vue {
	@Prop({ required: false, type: String, default: '' })
	readonly text!: string;

	@Prop({ required: true, type: Number })
	readonly percentage!: number;

	@Prop({ required: false, type: Boolean, default: false })
	readonly circularMode!: boolean;

	@Prop({ required: false, type: Boolean, default: false })
	readonly completed!: boolean;

	@Prop({ required: false, type: Boolean, default: false })
	readonly indeterminate!: boolean;

	get getPercentage(): number {
		return Math.round(this.percentage * 100) / 100;
	}
}
</script>
