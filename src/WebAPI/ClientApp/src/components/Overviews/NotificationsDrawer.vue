<template>
	<q-drawer :model-value="showDrawer" :width="450" side="right" overlay class="no-background notification-drawer">
		<q-col class="notification-container">
			<q-scroll>
				<!-- Render All Notifications	-->
				<template v-if="notifications.length > 0">
					<q-alert
						v-for="notification in getVisibleNotifications"
						:key="notification.id"
						:min-width="200"
						:max-width="450"
						:type="notification.level.toLowerCase()"
						dense
						dismissible
						outlined
						elevation="10"
						@click="hideNotification(notification.id)">
						<span class="text-wrap" style="overflow-wrap: anywhere">
							{{ notification.message }}
						</span>
					</q-alert>
				</template>
				<!-- No Notifications	-->
				<template v-else>
					<q-list>
						<q-item @click="clearAllNotifications">
							<q-item-section avatar>
								<q-icon name="mdi-check-circle-outline" />
							</q-item-section>
							<q-item-section>
								{{ t('components.notifications-drawer.no-notifications') }}
							</q-item-section>
						</q-item>
					</q-list>
				</template>
			</q-scroll>
		</q-col>
		<!-- Menu items -->
		<q-col v-if="notifications.length > 0" class="clear-notifications-container">
			<q-list>
				<q-item clickable @click="clearAllNotifications">
					<q-item-section avatar>
						<q-icon name="mdi-close-circle" />
					</q-item-section>
					<q-item-section>
						{{ t('components.notifications-drawer.clear-notifications') }}
					</q-item-section>
				</q-item>
			</q-list>
		</q-col>
	</q-drawer>
</template>

<script setup lang="ts">
import { useSubscription } from '@vueuse/rxjs';
import { NotificationService } from '@service';
import { NotificationDTO } from '@dto/mainApi';
import notificationService from '~/service/notificationService';

const notifications = ref<NotificationDTO[]>([]);
const { t } = useI18n();

defineProps<{
	showDrawer: boolean;
}>();

const emit = defineEmits<{
	(cleared: 'cleared'): void;
}>();

const getVisibleNotifications = computed(() => notifications.value?.filter((x) => !x.hidden) ?? []);

function hideNotification(id: number): void {
	NotificationService.hideNotification(id);
}

function clearAllNotifications() {
	NotificationService.clearAllNotifications();
	emit('cleared');
}

onMounted(() => {
	useSubscription(
		notificationService.getNotifications().subscribe((value) => {
			notifications.value = value;
		}),
	);
});
</script>

<style lang="scss">
@import '@/assets/scss/variables.scss';

.notification-drawer {
	height: 100vh;
	display: flex;
	flex-direction: column;
	justify-content: space-between;

	.notification-container {
		overflow-y: auto;
		overflow-x: hidden;

		flex-grow: 3;
	}

	.clear-notifications-container {
		flex-grow: 0;
	}
}

.q-drawer {
	background-color: transparent;
}
</style>
