<template>
	<v-checkbox :value="value" color="red" class="ma-3 pt-0" hide-details @click="handleInput($event)" />
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';

@Component
export default class PCheckbox extends Vue {
	@Prop({ required: true, type: Boolean })
	readonly value!: boolean;

	handleInput(event: any): void {
		this.$emit('input', event);
	}
}
</script>
