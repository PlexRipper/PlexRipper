<template>
	<v-container :fluid="fluid" :class="['page', 'px-0', $getThemeClass()]">
		<perfect-scrollbar>
			<slot></slot>
		</perfect-scrollbar>
	</v-container>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';

@Component
export default class Page extends Vue {
	@Prop({ required: false, type: Boolean, default: true })
	readonly fluid!: boolean;
}
</script>
