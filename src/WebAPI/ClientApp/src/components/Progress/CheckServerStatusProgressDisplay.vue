<template>
	<q-item>
		<!--	Status icon	-->
		<q-item-section avatar>
			<BooleanProgress :loading="!progress || !progress.completed" :success="progress && progress.connectionSuccessful" />
		</q-item-section>
		<!--	Current Action	-->
		<q-item-section>
			<ConnectionProgressText :progress="progress" />
		</q-item-section>
		<!--	Error message	-->
		<q-item-section>
			<template v-if="progress && !progress.completed">
				<span v-if="progress && progress.message">
					{{ progress.message }}
				</span>
			</template>
		</q-item-section>
	</q-item>
</template>

<script setup lang="ts">
import type { PlexServerDTO, ServerConnectionCheckStatusProgressDTO } from '@dto/mainApi';

defineProps<{
	plexServer: PlexServerDTO | null;
	progress: ServerConnectionCheckStatusProgressDTO | null;
}>();
</script>
