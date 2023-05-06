<template>
	<div v-if="progress">
		<template v-if="!progress.completed">
			<span v-if="progress && progress.retryAttemptIndex > 0">
				{{
					$t('components.connection-progress-text.retry-connection', {
						attemptIndex: progress.retryAttemptIndex,
						attemptCount: progress.retryAttemptCount,
					})
				}}
			</span>
		</template>
		<!--	Completed -->
		<template v-else>
			<span v-if="progress.connectionSuccessful">
				{{ $t('components.connection-progress-text.connection-connectable') }}
			</span>
			<span v-else>
				{{ $t('components.connection-progress-text.connection-un-connectable') }}
			</span>
		</template>
	</div>
</template>

<script setup lang="ts">
import { withDefaults, defineProps } from 'vue';
import { ServerConnectionCheckStatusProgressDTO } from '@dto/mainApi';

withDefaults(
	defineProps<{
		progress: ServerConnectionCheckStatusProgressDTO | null;
	}>(),
	{
		progress: null,
	},
);
</script>
