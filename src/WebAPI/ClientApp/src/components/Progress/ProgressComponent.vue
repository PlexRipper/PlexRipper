<template>
	<q-row justify="center">
		<q-col cols="11">
			<!-- Circular Mode progress -->
			<template v-if="circularMode">
				<q-row
					justify="center"
					no-gutters
					class="my-3"
				>
					<q-col cols="auto">
						<q-circular-progress
							show-value
							size="100px"
							:rotate="-90"
							:width="15"
							:value="getPercentage"
							:indeterminate="indeterminate"
							color="red"
						>
							<template v-if="!indeterminate">
								<span
									v-if="getPercentage < 100"
									class="text-bold"
								> {{ getPercentage }}% </span>
								<q-icon
									v-else
									large
									name="mdi-check"
								/>
							</template>
						</q-circular-progress>
					</q-col>
				</q-row>
				<!-- Progress text -->
				<q-row
					v-if="text"
					justify="center"
					no-gutters
				>
					<q-col>
						<h3>{{ text }}</h3>
					</q-col>
				</q-row>
			</template>
			<!-- Linear Mode Progress -->
			<template v-else>
				<!-- Progress text -->
				<q-row
					v-if="text"
					justify="center"
					no-gutters
				>
					<q-col cols="auto">
						<h3>{{ text }}</h3>
					</q-col>
				</q-row>
				<!-- Progress bar -->
				<q-row
					justify="center"
					class="my-3"
					no-gutters
				>
					<q-col>
						<q-linear-progress
							:value="Math.min(getPercentage, 100)"
							show-value
							height="20px"
							class="q-mx-sm"
							striped
							stream
							color="red"
						>
							<strong>{{ getPercentage }}%</strong>
						</q-linear-progress>
					</q-col>
				</q-row>
			</template>
		</q-col>
	</q-row>
</template>

<script setup lang="ts">
const props = defineProps<{
	text?: string;
	percentage: number;
	circularMode?: boolean;
	completed?: boolean;
	indeterminate?: boolean;
}>();

const getPercentage = computed((): number => {
	return Math.round(props.percentage * 100) / 100;
});
</script>
