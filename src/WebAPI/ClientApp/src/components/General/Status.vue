<template>
	<status-indicator class="mx-2" :status="value ? 'positive' : 'negative'" :pulse="pulse" />
</template>

<script lang="ts">
import { Component, Vue, Prop } from 'vue-property-decorator';

@Component
export default class Status extends Vue {
	@Prop({ required: true, type: Boolean })
	readonly value!: boolean;

	get pulse(): boolean {
		return this.value;
	}
}
</script>
