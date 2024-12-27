<template>
	<div>
		<div
			v-if="loading"
			class="page-load-overlay">
			<Logo
				:size="256"
				class="page-load-overlay-logo" />
		</div>
		<!-- Page Content -->
		<div v-show="!loading">
			<slot />
		</div>
	</div>
</template>

<script setup lang="ts">
withDefaults(defineProps<{ loading: boolean }>(), {
	loading: true,
});
</script>

<style>
.page-load-overlay {
  z-index: -1 !important;
  position: fixed;
  width: 100%;
  height: 100%;
  top: 0;
  display: flex;
  justify-content: center;
}

.page-load-overlay .page-load-overlay-logo {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  margin: auto;
  animation: rotate-animation 3s cubic-bezier(0.25, 0.46, 0.45, 0.94) infinite;
}

.page-load-overlay-logo {
  width: 256px;
  height: 256px;
  animation: rotate-animation 3s cubic-bezier(0.25, 0.46, 0.45, 0.94) infinite;
  margin: 0 auto;
}

@keyframes rotate-animation {
  from {
    transform: rotateY(0deg);
  }

  to {
    transform: rotateY(359deg);
  }
}
</style>
