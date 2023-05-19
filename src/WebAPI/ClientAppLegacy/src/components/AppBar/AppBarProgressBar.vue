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
import { useSubscription } from '@vueuse/rxjs';
import { SignalrService } from '@service';
import { SyncServerProgress } from '@dto/mainApi';

@Component
export default class AppBarProgressBar extends Vue {
	progressList: SyncServerProgress[] = [];
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
		useSubscription(
			SignalrService.getAllSyncServerProgress().subscribe((progress) => {
				this.progressList = progress;
			}),
		);
	}
}
</script>
