<template>
	<div class="return-overview">
		<v-btn text @click="returnToOverview">
			<v-icon>mdi-arrow-left</v-icon>
			{{ text || $t('common.returnToOverview') }}
		</v-btn>
	</div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from 'vue-property-decorator';

@Component
export default class ReturnToOverviewButton extends Vue {
	@Prop(String)
	readonly text?: string;

	@Prop(Function)
	readonly onClick?: () => void;

	returnToOverview(): void {
		if (this.onClick) {
			this.onClick();
		} else {
			this.$router.push({
				// Replace the id part to go back to the overview
				// e.g. /edi418/123 becomes /edi418
				path: this.$route.path.replace(/\/[a-z|0-9]+(?:$|\/$|\?.*$)/, ''),
				params: {},
			});
		}
	}
}
</script>

<style lang="scss" scoped>
@import './style.scss';
</style>
