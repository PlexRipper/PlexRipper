<template>
	<page>
		<download-manager-section />
		<database-section />
	</page>
</template>

<script lang="ts">
import { Vue, Component } from 'vue-property-decorator';
import DownloadManagerSection from '~/pages/settings/components/DownloadManagerSection.vue';
import DatabaseSection from '~/pages/settings/components/DatabaseSection.vue';

@Component({
	components: {
		DownloadManagerSection,
		DatabaseSection,
	},
})
export default class AdvancedSettings extends Vue {}
</script>
