<template>
	<QPage>
		<DebugSection />
		<DownloadManagerSection />
		<TorznabSection />
		<SetupSection />
	</QPage>
</template>

<script setup lang="ts">
import DebugSection from '@components/Views/Settings/DebugSection.vue';
import DownloadManagerSection from '@components/Views/Settings/DownloadManagerSection.vue';
import TorznabSection from '@components/Views/Settings/TorznabSection.vue';
import SetupSection from '@components/Views/Settings/SetupSection.vue';
</script>
