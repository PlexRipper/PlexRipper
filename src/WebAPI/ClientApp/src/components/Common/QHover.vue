<template>
	<div
		@mouseover="hover = true"
		@mouseleave="hover = false"
	>
		<slot :hover="hover" />
	</div>
</template>

<script setup lang="ts">
const hover = ref(false);
</script>
