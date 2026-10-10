<template>
	<q-card class="media-info-container">
		<q-card-section>
			<q-markup-table wrap-cells>
				<tbody>
					<tr class="q-tr--no-hover">
						<td
							colspan="2"
							class="media-info-column media-title">
							{{ props.title }}
						</td>
					</tr>
					<slot />
					<template v-if="props.showSummary">
						<slot name="summary">
							<tr>
								<td class="media-info-column">
									{{ props.summaryLabel }}
								</td>
								<td class="media-info-column">
									{{ props.summary ?? '' }}
								</td>
							</tr>
						</slot>
					</template>
				</tbody>
			</q-markup-table>
		</q-card-section>
	</q-card>
</template>

<script setup lang="ts">
const props = withDefaults(defineProps<{
	title: string;
	summaryLabel: string;
	summary?: string | null;
	showSummary?: boolean;
}>(), {
	summary: '',
	showSummary: true,
});
</script>

<style lang="scss">
@use '@/assets/scss/_mixins.scss';

.media-info-container {
	@extend .background-sm;
	box-shadow: 0px 3px 1px -2px rgba(0, 0, 0, 0.2), 0px 2px 2px 0px rgba(0, 0, 0, 0.14),
	0px 1px 5px 0px rgba(0, 0, 0, 0.12);
	min-height: 300px;
	margin-right: 1rem;

	.media-title {
		font-size: 30px;
		font-weight: bold;
	}

	.media-info-column {
		min-width: 150px;
		text-align: left;
		white-space: pre-wrap;
	}
}

@media (max-width: $breakpoint-xs-max) {
	.media-info-container {
		width: calc(100% - 1rem) !important;
		max-width: calc(100% - 1rem) !important;
		height: auto !important;
		margin: 0 0.5rem 1rem;

		.q-markup-table,
		.q-table__container,
		.q-table {
			width: 100% !important;
			max-width: 100% !important;
			table-layout: fixed;
		}

		.media-title {
			font-size: clamp(1.35rem, 1.1rem + 1vw, 1.5rem);
		}

		.media-info-column {
			min-width: 0;
			padding: 0.5rem;
			overflow-wrap: anywhere;
		}
	}
}
</style>
