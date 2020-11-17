<template>
	<p-section>
		<template #header> {{ $t('pages.settings.advanced.header') }} </template>
		<!--	Max segmented downloads	-->
		<v-row>
			<v-col cols="4">
				<help-icon help-id="help.download-manager-section.download-segments" />
			</v-col>
			<v-col cols="8">
				<v-slider v-model="downloadSegments" min="1" max="8">
					<template #append>
						<p>{{ downloadSegments }}</p>
					</template>
				</v-slider>
			</v-col>
		</v-row>
	</p-section>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import { settingsStore } from '~/store';

@Component
export default class DownloadManagerSection extends Vue {
	get downloadSegments(): number {
		return settingsStore.downloadSegments;
	}

	set downloadSegments(value: number) {
		settingsStore.setDownloadSegments(value);
	}
}
</script>
