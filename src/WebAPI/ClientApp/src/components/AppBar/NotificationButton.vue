<template>
	<v-btn icon @click="toggleNotificationDrawer">
		<v-badge :content="getVisibleNotifications.length" :value="getVisibleNotifications.length > 0" color="green" overlap>
			<v-icon>mdi-bell</v-icon>
		</v-badge>
	</v-btn>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import { NotificationDTO } from '@dto/mainApi';
import notificationService from '~/service/notificationService';

@Component
export default class NotificationButton extends Vue {
	private notifications: NotificationDTO[] = [];

	get getVisibleNotifications(): NotificationDTO[] {
		return this.notifications?.filter((x) => !x.hidden) ?? [];
	}

	toggleNotificationDrawer() {
		this.$emit('toggle');
	}

	mounted(): void {
		this.$subscribeTo(notificationService.getNotifications(), (value) => {
			this.notifications = value;
		});
	}
}
</script>
