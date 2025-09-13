<template>
	<div class="highlight-border-box">
		<slot />
		<!--	Highlight animation effect	-->
		<svg class="glow-container">
			<!-- suppress HtmlUnknownAttribute -->
			<rect
				pathLength="100"
				height="5"
				width="5"
				stroke-linecap="round"
				class="glow-blur" />
			<!-- suppress HtmlUnknownAttribute -->
			<rect
				pathLength="100"
				height="5"
				width="5"
				stroke-linecap="round"
				class="glow-line" />
		</svg>
	</div>
</template>

<script setup lang="ts">

</script>

<style lang="scss">

</style>
