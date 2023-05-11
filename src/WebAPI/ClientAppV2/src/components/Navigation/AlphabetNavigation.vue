<template>
	<div class="alphabet-navigation-container">
		<div class="alphabet-navigation">
			<q-btn
				v-for="letter in alphabet"
				:key="letter"
				class="navigation-btn"
				:label="letter"
				flat
				square
				:data-cy="`letter-${letter}-alphabet-navigation-btn`"
				@click="sendMediaOverviewScrollToCommand(letter)" />
		</div>
	</div>
</template>

<script setup lang="ts">
import { defineProps, computed } from 'vue';
import { sendMediaOverviewScrollToCommand } from '@composables/event-bus';

const props = defineProps<{
	scrollDict: Record<string, number>;
}>();

const alphabet = computed((): string[] => {
	return Object.keys(props.scrollDict);
});
</script>
<style lang="scss">
@import '@/assets/scss/_mixins.scss';

.alphabet-navigation-container {
	display: flex;
	align-content: stretch;
	align-items: stretch;
	align-self: stretch;
	justify-content: center;
	flex: 0 0 30px;

	.alphabet-navigation {
		display: flex;
		justify-content: space-around;
		flex: 0 0 100%;
		flex-direction: column;
		overflow: hidden;

		.navigation-btn {
			@extend .fade-out-border;
			flex: 1 1 25px;
			text-align: center;
			font-weight: bold;
			background: transparent !important;

			&:hover {
				&::before {
					opacity: 0.2 !important;
				}
			}
		}
	}
}

body {
	&.body--dark {
		.navigation-btn {
			color: red;
		}
	}

	&.body--light {
		.navigation-btn {
			color: darkred;
		}
	}
}
</style>
