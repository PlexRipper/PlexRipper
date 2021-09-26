<template>
	<v-navigation-drawer :value="showDrawer" :width="450" right app clipped class="no-background">
		<vue-scroll>
			<!-- Render All Notifications	-->
			<template v-if="notifications.length > 0">
				<v-alert
					v-for="notification in notifications"
					:key="notification.id"
					:min-width="200"
					:max-width="450"
					:type="notification.level.toLowerCase()"
					dense
					dismissible
					outlined
					elevation="10"
					@click="hideNotification(notification.id)"
				>
					<span class="text-wrap" style="overflow-wrap: anywhere">
						{{ notification.message }}
					</span>
				</v-alert>
			</template>
			<!-- No Notifications	-->
			<template v-else>
				<v-list>
					<v-list-item>
						<v-list-item-icon>
							<v-icon>mdi-check-circle-outline</v-icon>
						</v-list-item-icon>
						<v-list-item-content>
							<v-list-item-title> {{ $t('components.notifications-drawer.no-notifications') }}</v-list-item-title>
						</v-list-item-content>
					</v-list-item>
				</v-list>
			</template>
		</vue-scroll>
		<!-- Menu items -->
		<template v-if="notifications.length > 0" #append>
			<v-list>
				<v-list-item @click="clearAllNotifications">
					<v-list-item-icon>
						<v-icon>mdi-close-circle</v-icon>
					</v-list-item-icon>
					<v-list-item-content>
						<v-list-item-title>
							{{ $t('components.notifications-drawer.clear-notifications') }}
						</v-list-item-title>
					</v-list-item-content>
				</v-list-item>
			</v-list>
		</template>
	</v-navigation-drawer>
</template>

<script lang="ts">
import { Component, Vue, Prop } from 'vue-property-decorator';
import { NotificationService } from '@service';
import { NotificationDTO } from '@dto/mainApi';

@Component<NotificationsDrawer>({})
export default class NotificationsDrawer extends Vue {
	private notifications: NotificationDTO[] = [];

	@Prop({ required: true, type: Boolean })
	readonly showDrawer!: boolean;

	get getVisibleNotifications(): NotificationDTO[] {
		return this.notifications?.filter((x) => !x.hidden) ?? [];
	}

	hideNotification(id: number): void {
		NotificationService.hideNotification(id);
	}

	clearAllNotifications() {
		NotificationService.clearAllNotifications();
		this.$emit('cleared');
	}

	mounted(): void {
		this.$subscribeTo(NotificationService.getNotifications(), (value) => {
			this.notifications = value ?? [];
		});
	}
}
</script>
<style scoped lang="scss">
.v-alert:hover {
	cursor: pointer;
}
</style>
