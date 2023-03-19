<template>
	<tr>
		<!--	Server name and status	-->
		<td style="width: 30%">
			<q-status :value="progress ? progress.connectionSuccessful && progress.completed : false"/>
			{{ plexServerName }}
		</td>
		<!--	Status icon	-->
		<td style="width: 10%">
			<!--	Plex Connection Status Progress Icon -->
			<BooleanProgress :loading="!progress || !progress.completed" :success="progress.connectionSuccessful"/>
		</td>
		<!--	Current Action	-->
		<td style="width: 30%">
			<ConnectionProgressText :progress="progress"/>
		</td>
		<!--	Error message	-->
		<td style="width: 30%">
			<template v-if="progress">
				<span v-if="progress && progress.message">
					{{ progress.value.message }}
				</span>
			</template>
		</td>
	</tr>
</template>

<script setup lang="ts">
import {ref, defineProps, defineEmits, onMounted} from 'vue';
import {useSubscription} from '@vueuse/rxjs';
import {SignalrService} from '@service';
import {InspectServerProgressDTO} from '@dto/mainApi';
import ConnectionProgressText from '@components/Progress/ConnectionProgressText.vue';


const props = defineProps<{
	plexServerId: number;
	plexServerName: string;
}>();

const progress = ref<InspectServerProgressDTO | null>(null);

const emit = defineEmits<{
	(e: 'completed', plexServerId: number);
}>();

onMounted(() => {
	useSubscription(
		SignalrService.getInspectServerProgress(props.plexServerId).subscribe((data) => {
			progress.value = data;
			if (data?.completed) {
				emit('completed', props.plexServerId);
			}
		}),
	);
});

</script>
