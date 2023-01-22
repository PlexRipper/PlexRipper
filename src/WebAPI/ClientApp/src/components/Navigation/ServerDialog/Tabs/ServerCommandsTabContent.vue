<template>
	<div>
		<FormRow form-id="help.server-dialog.server-commands.inspect-server">
			<BaseButton :disabled="syncLoading" :loading="inspectLoading" text-id="inspect-server" @click="inspectServer" />
		</FormRow>
		<FormRow form-id="help.server-dialog.server-commands.sync-server-libraries">
			<BaseButton
				:disabled="inspectLoading"
				:loading="syncLoading"
				text-id="sync-server-libraries"
				@click="syncServerLibraries"
			/>
		</FormRow>
	</div>
</template>

<script lang="ts">
import { Component, Prop, Vue, Watch } from 'vue-property-decorator';
import { useSubscription } from '@vueuse/rxjs';
import { inspectPlexServer, syncPlexServer } from '@api/plexServerApi';
import type { PlexServerDTO } from '@dto/mainApi';

@Component
export default class ServerCommandsTabContent extends Vue {
	@Prop({ required: true, type: Object as () => PlexServerDTO })
	readonly plexServer!: PlexServerDTO;

	@Prop({ required: true, type: Boolean })
	readonly isVisible!: boolean;

	syncLoading: boolean = false;
	inspectLoading: boolean = false;

	syncServerLibraries(): void {
		this.syncLoading = true;
		useSubscription(
			syncPlexServer(this.plexServer.id, true).subscribe(() => {
				this.syncLoading = false;
			}),
		);
	}

	inspectServer(): void {
		this.inspectLoading = true;
		useSubscription(
			inspectPlexServer(this.plexServer.id).subscribe(() => {
				this.inspectLoading = false;
			}),
		);
	}

	@Watch('isVisible')
	onIsVisible(isVisible): void {
		if (!isVisible) {
			this.syncLoading = false;
			this.inspectLoading = false;
		}
	}
}
</script>
