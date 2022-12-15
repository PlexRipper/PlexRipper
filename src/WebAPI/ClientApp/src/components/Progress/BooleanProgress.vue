<template>
	<div>
		<v-progress-circular v-if="loading" indeterminate color="red" />
		<v-icon v-else-if="success">mdi-check</v-icon>
		<v-icon v-else>mdi-close</v-icon>
	</div>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';

@Component
export default class BooleanProgress extends Vue {
	@Prop({ required: false, type: Boolean, default: false })
	readonly loading!: boolean;

	@Prop({ required: false, type: Boolean, default: false })
	readonly success!: boolean;
}
</script>

<style scoped></style>
