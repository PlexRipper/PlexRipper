<template>
	<div>
		<FormRow form-id="help.server-dialog.server-commands.inspect-server">
			<BaseButton :disabled="syncLoading" :loading="inspectLoading" text-id="inspect-server" @click="inspectServer"/>
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

<script setup lang="ts">
import {ref, onUnmounted} from '#imports'
import {useSubscription} from '@vueuse/rxjs';
import {inspectPlexServer, syncPlexServer} from '@api/plexServerApi';
import type {PlexServerDTO} from '@dto/mainApi';

const props = defineProps<{
	plexServer: PlexServerDTO;
	isVisible: boolean;
}>();

const syncLoading = ref(false);
const inspectLoading = ref(false);


function syncServerLibraries(): void {
	syncLoading.value = true;
	useSubscription(
		syncPlexServer(props.plexServer.id, true).subscribe(() => {
			syncLoading.value = false;
		}),
	);
}

function inspectServer(): void {
	inspectLoading.value = true;
	useSubscription(
		inspectPlexServer(props.plexServer.id).subscribe(() => {
			inspectLoading.value = false;
		}),
	);
}

onUnmounted(() => {
	syncLoading.value = false;
	inspectLoading.value = false;
})

</script>
