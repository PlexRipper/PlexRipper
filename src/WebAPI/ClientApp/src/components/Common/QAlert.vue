<template>
	<q-item
		v-ripple
		clickable
		:class="classConfig"
		:data-cy="cy">
		<div class="q-alert-wrapper">
			<q-icon
				:name="alertIcon"
				size="24px" />
			<div class="q-alert-content">
				<slot />
			</div>
			<div
				v-if="dismissible"
				class="q-alert-dismissible">
				<q-icon
					name="mdi-close-circle"
					size="24px" />
			</div>
		</div>
	</q-item>
</template>

<script setup lang="ts">
const props = withDefaults(defineProps<{
	type?: 'error' | 'warning' | 'info' | string;
	dismissible?: boolean;
	cy?: string;
}>(), {
	type: '',
	dismissible: false,
	cy: '',
});

const classConfig = computed(() => {
	return {
		'q-alert': true,
		[`q-alert--${props.type}`]: true,
	};
});

const alertIcon = computed((): string => {
	switch (props.type) {
		case 'error':
			return 'mdi-alert-circle-outline';
		case 'warning':
			return 'mdi-alert-outline';
		case 'info':
			return 'mdi-information-outline';
		default:
			return 'mdi-close';
	}
});
</script>

<style lang="scss">
@use '@/assets/scss/variables.scss' as *;

.q-alert {
  display: block;
  font-size: 16px;
  margin: 0;
  padding: 16px;
  position: relative;
  transition: 0.3s cubic-bezier(0.25, 0.8, 0.5, 1);
  border: 1px solid;
  border-radius: 4px;

  &--error {
    border-color: $error-color;
    color: $error-color;
  }

  &--warning {
    border-color: #fff8e1;
    color: #ff6f00;
  }

  &--info {
    border-color: #e8f5e9;
    color: #1b5e20;
  }

  .q-alert-wrapper {
    align-items: center;
    display: flex;

    .q-alert-content {
      flex: 1 1 auto;
      padding: 0 1em;
    }

    .q-alert-dismissible {
      margin: -16px -8px -16px 8px;
    }
  }

  &:hover {
    cursor: pointer;
  }
}
</style>
