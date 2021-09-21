<template>
	<v-container :fluid="fluid" :class="['page', 'px-0', getThemeClass]">
		<template v-if="noScrollbar">
			<slot></slot>
		</template>
		<template v-else>
			<perfect-scrollbar>
				<slot></slot>
			</perfect-scrollbar>
		</template>
	</v-container>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';

@Component
export default class PageContainer extends Vue {
	@Prop({ required: false, type: Boolean, default: true })
	readonly fluid!: boolean;

	@Prop({ required: false, type: Boolean, default: false })
	readonly noScrollbar!: boolean;

	get getThemeClass(): string {
		return this.$getThemeClass();
	}
}
</script>
