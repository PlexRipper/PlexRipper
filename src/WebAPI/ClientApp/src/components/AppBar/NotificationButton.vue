<template>
	<v-btn icon @click="toggleNotificationDrawer">
		<v-badge :content="getVisibleNotifications.length" :value="getVisibleNotifications.length > 0" color="green" overlap>
			<v-icon>mdi-bell</v-icon>
		</v-badge>
	</v-btn>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import { NotificationService } from '@service';
import { NotificationDTO } from '@dto/mainApi';

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
		this.$subscribeTo(NotificationService.getNotifications(), (value) => {
			this.notifications = value ?? [];
		});
	}
}
</script>
