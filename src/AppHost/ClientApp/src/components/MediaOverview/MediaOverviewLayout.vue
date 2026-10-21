<template>
	<div class="media-overview-layout">
		<div class="media-overview-header">
			<slot name="header" />
		</div>
		<div class="media-overview-content">
			<slot />
		</div>
	</div>
</template>

<style lang="scss">
.media-overview-layout {
  position: absolute;
  inset: 0;
  display: flex;
  flex-direction: column;
  min-height: 0;
  overflow: hidden;
}

.media-overview-header {
  flex: 0 0 auto;
  min-width: 0;
}

.media-overview-content {
  position: relative;
  flex: 1 1 auto;
  min-height: 0;
  overflow: hidden;
}
</style>
