<template>
	<q-btn dense round flat icon="mdi-bell" @click="toggleNotificationDrawer">
		<q-badge
			v-if="getVisibleNotifications.length > 0"
			color="green"
			floating
			transparent
			:label="getVisibleNotifications.length"
		/>
	</q-btn>
</template>

<script setup lang="ts">
import { useSubscription } from '@vueuse/rxjs';
import { NotificationDTO } from '@dto/mainApi';
import { NotificationService } from '@service';

const notifications = ref<NotificationDTO[]>([]);

const emit = defineEmits<{ (e: 'toggle'): void }>();

const getVisibleNotifications = computed(() => {
	return notifications.value?.filter((x) => !x.hidden) ?? [];
});

function toggleNotificationDrawer() {
	emit('toggle');
}

onMounted(() => {
	useSubscription(
		NotificationService.getNotifications().subscribe((value) => {
			notifications.value = value;
		}),
	);
});
</script>
