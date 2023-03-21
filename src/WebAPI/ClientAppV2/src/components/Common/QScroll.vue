<template>
	<q-scroll-area class="fit" :thumb-style="thumbStyle" :bar-style="barStyle">
		<slot />
	</q-scroll-area>
</template>

<script setup lang="ts">
const thumbStyle = {
	right: '4px',
	borderRadius: '5px',
	backgroundColor: '#d20000',
	width: '5px',
	opacity: 0.75,
};

const barStyle = {
	right: '2px',
	borderRadius: '9px',
	backgroundColor: 'rgba(0,0,0,0)',
	width: '9px',
	opacity: 0.2,
};
</script>
