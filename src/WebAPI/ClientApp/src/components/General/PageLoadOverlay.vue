<template>
	<div>
		<div
			v-if="loading"
			class="q-pa-md">
			<div class="logo-square">
				<Logo :size="256" />
			</div>
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
.logo-square {
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
