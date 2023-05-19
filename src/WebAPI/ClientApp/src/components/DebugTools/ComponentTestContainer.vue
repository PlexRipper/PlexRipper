<template>
	<div class="debug-container">
		<div class="debug-container-overlay">
			<slot />
		</div>
	</div>
</template>

<script setup lang="ts"></script>

<style lang="scss">
.debug-container-overlay {
	background-color: rgba(0, 0, 0, 0.5);
	height: 100%;
	width: 100%;
	display: flex;
	align-items: center;
	justify-content: center;
}

.debug-container {
	height: 100vh;
	width: 100vw;
	background-image: url('/img/background/background.png');
}
</style>
