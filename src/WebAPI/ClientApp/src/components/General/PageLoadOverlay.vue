<template>
	<div class="q-pa-md">
		<div class="logo-square">
			<Logo :size="256" />
		</div>
	</div>
</template>

<style>
.logo-square {
	width: 256px;
	height: 256px;
	animation: rotate-animation 3s cubic-bezier(0.25, 0.46, 0.45, 0.94) infinite;
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
