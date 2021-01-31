<template>
	<v-checkbox :value="value" color="red" class="ma-3 pt-0" hide-details @click="handleInput()" />
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';

@Component
export default class PCheckbox extends Vue {
	@Prop({ required: true, type: Boolean })
	readonly value!: boolean;

	handleInput(): void {
		this.$emit('input', !this.value);
	}
}
</script>
