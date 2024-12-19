<template>
	<q-header class="app-bar">
		<QRow no-wrap>
			<q-toolbar class="app-bar">
				<q-toolbar-title>
					<q-btn
						flat
						round
						dense
						icon="mdi-menu"
						class="q-mr-sm"
						@click.stop="showNavigationDrawer" />
					<q-btn
						to="/"
						flat>
						<Logo
							:size="24"
							class="q-mr-md" />
						{{ t('general.name-version', { version: globalStore.getAppVersion }) }}
					</q-btn>
				</q-toolbar-title>

				<q-btn
					icon="mdi-github"
					flat
					rounded
					style="padding: 0.5rem"
					href="https://github.com/PlexRipper/PlexRipper"
					target="_blank" />

				<q-btn
					flat
					rounded
					style="padding: 0.5rem"
					@click="dialogStore.openDialog(DialogType.DiscordServerInviteDialog)">
					<DiscordIcon />
				</q-btn>

				<!-- Background Activity Toggle -->
				<BackgroundActivityToggleButton />

				<!-- Account Selector -->
				<AccountSelector />

				<!-- Notifications Selector -->
				<NotificationButton @toggle="showNotificationsDrawer" />
			</q-toolbar>
		</QRow>
	</q-header>
</template>

<script setup lang="ts">
import { useGlobalStore } from '@store';
import { DialogType } from '@enums';

const { t } = useI18n();
const globalStore = useGlobalStore();
const dialogStore = useDialogStore();

const emit = defineEmits<{
	(e: 'show-navigation' | 'show-notifications'): void;
}>();

function showNavigationDrawer(): void {
	emit('show-navigation');
}

function showNotificationsDrawer(): void {
	emit('show-notifications');
}
</script>

<style lang="scss">
@use '@/assets/scss/variables' as *;

.app-bar {
  height: $app-bar-height;
}

body {
  &.body--dark {
    .app-bar {
      background: rgba(255, 0, 0, 0.2) !important;
    }
  }

  &.body--light {
    .app-bar {
      background: rgba(255, 0, 0, 1) !important;
    }
  }
}
</style>
