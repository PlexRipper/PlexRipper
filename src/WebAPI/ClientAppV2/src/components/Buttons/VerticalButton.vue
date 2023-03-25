<template>
	<q-btn push flat class="vertical-button">
		<div class="vertical-button-content">
			<q-icon :size="props.size" :name="props.icon" />
			<div class="text-center vertical-button-label">
				{{ props.label }}
			</div>
		</div>
		<slot />
	</q-btn>
</template>

<script setup lang="ts">
import { defineProps, withDefaults } from 'vue';
import { IBaseButtonProps } from '~/types/props/base-button/IBaseButtonProps';

const props = withDefaults(defineProps<IBaseButtonProps>(), {
	size: 'xl',
});
</script>
<style lang="scss">
.vertical-button {
	.vertical-button-content {
		display: flex;
		flex-direction: column;
		align-items: center;

		.vertical-button-label {
			transition-duration: 0.28s;
			height: 24px;
		}
	}
}
</style>
