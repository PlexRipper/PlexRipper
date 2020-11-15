<template>
	<page>
		<v-row>
			<v-col>
				<v-sheet width="100%" class="pa-4">
					<v-row>
						<v-col>
							<h1>Download Manager</h1>
							<v-divider />
						</v-col>
					</v-row>
					<!--	Max segmented downloads	-->
					<v-row>
						<v-col cols="4">
							<help-icon help-id="help.advanced-1"></help-icon>
						</v-col>
						<v-col cols="8">
							<v-slider v-model="SegmentedDownloads" min="1" max="8">
								<template #append>
									<p>{{ SegmentedDownloads }}</p>
								</template>
							</v-slider>
						</v-col>
					</v-row>
				</v-sheet>
			</v-col>
		</v-row>
	</page>
</template>

<script lang="ts">
import { Vue, Component } from 'vue-property-decorator';
import { FolderPathDTO, SettingsModelDTO } from '@dto/mainApi';
import SettingsService from '@service/settingsService';
import HelpIcon from '@components/Help/HelpIcon.vue';
import { Subject, timer } from 'rxjs';
import { debounce } from 'rxjs/operators';
import Page from '@components/General/Page.vue';

@Component({
	components: {
		HelpIcon,
		Page,
	},
})
export default class AdvancedSettings extends Vue {
	settings: SettingsModelDTO | null = null;

	isDirectoryBrowserOpen: boolean = false;

	selectedPath: FolderPathDTO | null = null;

	segmentedDownloadsSubject: Subject<number> = new Subject<number>();

	openDirectoryBrowser(path: FolderPathDTO): void {
		this.selectedPath = path;
		this.isDirectoryBrowserOpen = true;
	}

	get SegmentedDownloads(): number {
		return this.settings?.advancedSettings?.downloadManager?.downloadSegments ?? -1;
	}

	set SegmentedDownloads(value: number) {
		if (this.settings?.advancedSettings?.downloadManager) {
			if (this.settings.advancedSettings.downloadManager.downloadSegments !== value) {
				this.settings.advancedSettings.downloadManager.downloadSegments = value;
				this.segmentedDownloadsSubject.next(value);
			}
		}
	}

	updateSettings(): void {
		if (this.settings) {
			SettingsService.updateSettings(this.settings);
		}
	}

	created(): void {
		SettingsService.getSettings().subscribe((data) => {
			this.settings = data;
		});

		this.segmentedDownloadsSubject.pipe(debounce(() => timer(1000))).subscribe(() => this.updateSettings());
	}
}
</script>
