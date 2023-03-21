<template>
	<q-row justify="center">
		<q-col cols="11">
			<!-- Circular Mode progress -->
			<template v-if="circularMode">
				<q-row justify="center" no-gutters class="my-3">
					<q-col cols="auto">
						<q-circular-progress
							:size="100"
							:rotate="-90"
							:width="15"
							:value="getPercentage"
							:indeterminate="indeterminate"
							color="red">
							<template v-if="!indeterminate">
								<span v-if="percentage < 100">
									<b>{{ getPercentage }}%</b>
								</span>
								<q-icon v-else large> mdi-check </q-icon>
							</template>
						</q-circular-progress>
					</q-col>
				</q-row>
				<!-- Progress text -->
				<q-row v-if="text" justify="center" no-gutters>
					<q-col cols="auto">
						<h3>{{ text }}</h3>
					</q-col>
				</q-row>
			</template>
			<!-- Linear Mode Progress -->
			<template v-else>
				<!-- Progress text -->
				<q-row v-if="text" justify="center" no-gutters>
					<q-col cols="auto">
						<h2>{{ text }}</h2>
					</q-col>
				</q-row>
				<!-- Progress bar -->
				<q-row justify="center" class="my-3" no-gutters>
					<q-col>
						<q-linear-progress
							:value="Math.min(getPercentage, 100)"
							height="20"
							class="mx-1"
							striped
							stream
							color="red"
							v-bind="$attrs">
							<template #default="{}">
								<strong>{{ getPercentage }}%</strong>
							</template>
						</q-linear-progress>
					</q-col>
				</q-row>
			</template>
		</q-col>
	</q-row>
</template>

<script setup lang="ts">
import { defineProps, computed } from 'vue';

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
