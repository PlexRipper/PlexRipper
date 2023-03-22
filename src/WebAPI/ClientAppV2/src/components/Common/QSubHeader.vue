<template>
	<div class="q-sub-header">
		<slot />
	</div>
</template>

<script setup lang="ts">
import Log from 'consola';
</script>
<style>
.q-sub-header {
	align-items: center;
	display: flex;
	height: 48px;
	font-size: 0.875rem;
	font-weight: 400;
	padding: 0 16px 0 16px;
}
</style>
