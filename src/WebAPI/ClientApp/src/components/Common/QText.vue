<template>
	<div :class="['q-text-container', align]">
		<span :class="['q-text', size]">
			<slot />
		</span>
	</div>
</template>
<script lang="ts" setup>
withDefaults(
	defineProps<{
		size?: 'small' | 'normal' | 'large';
		type?: 'primary' | 'success' | 'warning' | 'danger' | 'info';
		align?: 'left' | 'center' | 'right';
	}>(),
	{
		size: 'normal',
		type: 'primary',
		align: 'left',
	},
);
</script>
<style lang="scss">
.q-text-container {
	width: 100%;
	padding: 0.5rem 0;

	&.left {
		text-align: left;
	}

	&.center {
		text-align: center;
	}

	&.right {
		text-align: right;
	}

	.q-text {
		&.small {
			font-size: 0.8rem;
		}

		&.normal {
			font-size: 1rem;
		}

		&.large {
			font-size: 1.2rem;
		}
	}
}
</style>
