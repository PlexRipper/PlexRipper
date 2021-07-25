<template>
	<v-progress-circular indeterminate color="red" :size="size" />
</template>

<script lang="ts">
import { Component, Vue, Prop } from 'vue-property-decorator';

@Component
export default class LoadingSpinner extends Vue {
	@Prop({ required: false, type: Number, default: 32 })
	readonly size!: number;
}
</script>
