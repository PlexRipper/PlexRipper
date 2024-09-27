<template>
	<q-item>
		<!--	Status icon	-->
		<q-item-section avatar>
			<BooleanProgress
				:loading="!progress || !progress.completed"
				:success="progress && progress.connectionSuccessful" />
		</q-item-section>
		<!--	Current Action	-->
		<q-item-section>
			<ConnectionProgressText :progress="progress" />
			<!--	Error message	-->
			<span v-if="progress && !progress.connectionSuccessful">
				{{ progress.message }}
			</span>
		</q-item-section>
	</q-item>
</template>

<script setup lang="ts">
import type { PlexServerDTO, ServerConnectionCheckStatusProgressDTO } from '@dto';

defineProps<{
	plexServer: PlexServerDTO | null;
	progress: ServerConnectionCheckStatusProgressDTO | null;
}>();
</script>
