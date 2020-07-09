<template>
	<v-row>
		<v-col>
			<p>Downloads</p>
			<v-btn @click="submitCard">Test button</v-btn>
		</v-col>
	</v-row>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import Log from 'consola';
import LoadingSpinner from '@/components/LoadingSpinner.vue';
import { SignalrStore } from '@/store/';

@Component({
	components: {
		LoadingSpinner,
	},
})
export default class Downloads extends Vue {
	submitCard() {
		const progressConnection = SignalrStore.getDownloadProgressConnection;

		progressConnection.invoke('SendMessage', 'this.userName', 'this.userMessage').catch(function(err) {
			Log.error(err.toSting());
		});
	}

	mounted(): void {
		addEventListener('signalRSetup', () => {
			const progressConnection = SignalrStore.getDownloadProgressConnection;
			progressConnection.on('ReceiveMessage', function(user: string, message: string) {
				Log.debug(user);
				Log.debug(message);
			});
		});
	}
}
</script>
