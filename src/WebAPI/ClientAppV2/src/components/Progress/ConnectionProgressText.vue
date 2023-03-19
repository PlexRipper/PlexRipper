<template>
	<div v-if="progress">
		<template v-if="!progress.completed">
			<span v-if="progress && progress.retryAttemptIndex > 0">
				{{
					$t('components.account-setup-progress.retry-connection', {
						attemptIndex: progress.retryAttemptIndex,
						attemptCount: progress.retryAttemptCount,
					})
				}}
			</span>
		</template>
		<!--	Completed -->
		<template v-else>
			<span v-if="progress.connectionSuccessful">
				{{ $t('components.account-setup-progress.server-connectable') }}
			</span>
			<span v-else>
				{{ $t('components.account-setup-progress.server-un-connectable') }}
			</span>
		</template>
	</div>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import { ServerConnectionCheckStatusProgressDTO } from '@dto/mainApi';

@Component
export default class ConnectionProgressText extends Vue {
	@Prop({ required: true, type: Object as () => ServerConnectionCheckStatusProgressDTO })
	progress!: ServerConnectionCheckStatusProgressDTO;
}
</script>
