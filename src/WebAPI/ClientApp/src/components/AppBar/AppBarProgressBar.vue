<template>
	<v-fade-transition v-if="showProgressBar">
		<v-row>
			<v-col>
				<v-tooltip bottom :nudge-bottom="-20">
					<template #activator="{ on, attrs }">
						<progress-component :percentage="getPercentage" v-bind="attrs" v-on="on" />
					</template>
					<span>{{ getText }}</span>
				</v-tooltip>
			</v-col>
		</v-row>
	</v-fade-transition>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import { ServerService, SignalrService } from '@service';
import { PlexServerDTO, SyncServerProgress } from '@dto/mainApi';

@Component
export default class AppBarProgressBar extends Vue {
	progressList: SyncServerProgress[] = [];
	servers: PlexServerDTO[] = [];
	show: boolean = false;

	get getPercentage(): number {
		return this.progressList.map((x) => x.percentage).sum() / this.progressList.length;
	}

	get showProgressBar(): boolean {
		return this.progressList.length > 0 && this.getPercentage <= 100;
	}

	get getText(): string {
		const finishedCount = this.progressList.filter((x) => x.percentage >= 100).length;
		return `Syncing PlexServer ${finishedCount} of ${this.progressList.length}`;
	}

	mounted(): void {
		this.$subscribeTo(SignalrService.getAllSyncServerProgress(), (progress) => {
			this.progressList = progress;
		});

		this.$subscribeTo(ServerService.getServers(), (servers) => {
			this.servers = servers;
		});
	}
}
</script>
