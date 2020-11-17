<template>
	<page>
		<download-manager-section />
	</page>
</template>

<script lang="ts">
import { Vue, Component } from 'vue-property-decorator';
import DownloadManagerSection from '~/pages/settings/components/DownloadManagerSection.vue';

@Component({
	components: {
		DownloadManagerSection,
	},
})
export default class AdvancedSettings extends Vue {}
</script>
