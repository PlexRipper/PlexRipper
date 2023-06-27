<template>
	<q-btn dense round flat icon="mdi-bell" @click="toggleNotificationDrawer">
		<q-badge
			v-if="notificationsStore.getVisibleNotifications.length > 0"
			color="green"
			floating
			transparent
			:label="notificationsStore.getVisibleNotifications.length" />
	</q-btn>
</template>

<script setup lang="ts">
import { useNotificationsStore } from '~/store';
const notificationsStore = useNotificationsStore();

const emit = defineEmits<{ (e: 'toggle'): void }>();

function toggleNotificationDrawer() {
	emit('toggle');
}
</script>
