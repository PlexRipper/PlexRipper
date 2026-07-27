<template>
	<QSection :header="$t('pages.debug.buttons.media-comparison-state-header')">
		<QRow class="q-col-gutter-xl">
			<QCol cols="3">
				<div class="column q-gutter-sm">
					<MediaComparisonStateButton
						v-for="comparisonState in comparisonStates"
						:key="`icon-${comparisonState}`"
						:comparison-state="comparisonState"
						show-tooltip />
				</div>
			</QCol>

			<QCol cols="6">
				<div class="column q-gutter-sm">
					<MediaComparisonStateButton
						v-for="comparisonState in comparisonStates"
						:key="`label-${comparisonState}`"
						:comparison-state="comparisonState"
						show-label />
				</div>
			</QCol>
		</QRow>
	</QSection>
</template>

<script setup lang="ts">
import { PlexMediaComparisonState } from '@dto';

const comparisonStates: PlexMediaComparisonState[] = [
	PlexMediaComparisonState.NotCompared,
	PlexMediaComparisonState.Owned,
	PlexMediaComparisonState.Missing,
	PlexMediaComparisonState.HigherQuality,
	PlexMediaComparisonState.Pending,
	PlexMediaComparisonState.Partial,
	PlexMediaComparisonState.PartialAndHigherQuality,
	PlexMediaComparisonState.Unknown,
];
</script>
