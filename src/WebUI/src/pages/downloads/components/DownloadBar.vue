<template>
	<!-- Download Toolbar -->
	<v-row no-gutters>
		<v-col>
			<v-toolbar>
				<!--Prioritize buttons-->
				<v-btn-toggle borderless group tile :max="0">
					<v-btn>
						<v-icon large>mdi-arrow-collapse-up</v-icon>
					</v-btn>

					<v-btn>
						<v-icon large>mdi-arrow-up</v-icon>
					</v-btn>

					<v-btn>
						<v-icon large>mdi-arrow-down</v-icon>
					</v-btn>

					<v-btn>
						<v-icon large>mdi-arrow-collapse-down</v-icon>
					</v-btn>
				</v-btn-toggle>

				<v-spacer />

				<!--Command buttons-->
				<v-btn depressed tile @click="clearDownloadTasks">
					<v-icon large left>mdi-notification-clear-all</v-icon>
					<span class="hidden-sm-and-down">Clear Completed</span>
				</v-btn>
				<v-btn depressed tile>
					<v-icon large left>mdi-pause</v-icon>
					<span class="hidden-sm-and-down">Pause</span>
				</v-btn>

				<v-btn depressed tile>
					<v-icon large left>mdi-play</v-icon>
					<span class="hidden-sm-and-down">Start</span>
				</v-btn>

				<v-btn depressed tile>
					<v-icon large left>mdi-restart</v-icon>
					<span class="hidden-sm-and-down">Restart</span>
				</v-btn>

				<v-btn depressed tile @click="deleteDownloadTasks">
					<v-icon large left>mdi-delete</v-icon>
					<span class="hidden-sm-and-down">Delete</span>
				</v-btn>
			</v-toolbar>
		</v-col>
	</v-row>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';

@Component
export default class DownloadBar extends Vue {
	clearDownloadTasks(): void {
		this.$emit('clear');
	}

	deleteDownloadTasks(): void {
		this.$emit('delete');
	}
}
</script>
