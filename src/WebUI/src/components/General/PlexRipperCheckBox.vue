<template>
	<v-checkbox :value="value" color="red" class="ma-3 pt-0" hide-details @input="handleInput" />
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';

@Component
export default class PCheckBox extends Vue {
	@Prop({ required: true, type: Boolean })
	readonly value!: boolean;

	handleInput(e): void {
		this.$emit('input', e);
	}
}
</script>
