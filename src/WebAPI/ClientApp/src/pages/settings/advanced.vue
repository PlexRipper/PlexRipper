<template>
	<page>
		<download-manager-section />
		<database-section />
		<setup-section />
	</page>
</template>

<script lang="ts">
import { Vue, Component } from 'vue-property-decorator';
import DownloadManagerSection from './components/DownloadManagerSection.vue';
import DebugSection from './components/DebugSection.vue';
import SetupSection from './components/SetupSection.vue';

@Component<AdvancedSettings>({
	components: {
		DownloadManagerSection,
		DebugSection,
		SetupSection,
	},
})
export default class AdvancedSettings extends Vue {}
</script>
