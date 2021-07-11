<template>
	<v-menu left bottom offset-y :close-on-content-click="false">
		<template #activator="{ on }">
			<v-btn icon v-on="on">
				<v-badge :content="getVisibleNotifications.length" :value="getVisibleNotifications.length > 0" color="green" overlap>
					<v-icon>mdi-bell</v-icon>
				</v-badge>
			</v-btn>
		</template>

		<template v-for="notification in getVisibleNotifications">
			<v-alert
				:key="notification.id"
				:min-width="200"
				:max-width="450"
				:type="notification.level.toLowerCase()"
				dismissible
				@click="hideNotification(notification.id)"
			>
				{{ notification.message }}
			</v-alert>
		</template>
	</v-menu>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import { NotificationService } from '@state';
import { NotificationDTO } from '@dto/mainApi';

@Component
export default class NotificationButton extends Vue {
	private notifications: NotificationDTO[] = [];

	get getVisibleNotifications(): NotificationDTO[] {
		return this.notifications.filter((x) => !x.hidden) ?? [];
	}

	hideNotification(id: number): void {
		NotificationService.hideNotification(id);
	}

	mounted(): void {
		this.$subscribeTo(NotificationService.getNotifications(), (value) => {
			this.notifications = value;
		});
	}
}
</script>
<style scoped lang="scss">
.v-alert:hover {
	cursor: pointer;
}
</style>
