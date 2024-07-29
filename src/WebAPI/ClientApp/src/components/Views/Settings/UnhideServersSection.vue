<template>
	<q-section>
		<template #header> {{ t('pages.settings.ui.un-hide-servers-section.header') }}</template>

		<q-list bordered>
			<q-item v-for="server in serverStore.getHiddenServers" :key="server.id" v-ripple class="q-mb-sm" clickable>
				<q-item-section avatar>
					<IconSquareButton icon="mdi-eye-check-outline" @click="onServerUnHide(server.id)" />
				</q-item-section>

				<q-item-section>
					<q-item-label>{{ serverStore.getServerName(server.id) }}</q-item-label>
				</q-item-section>
			</q-item>
		</q-list>
	</q-section>
</template>

<script setup lang="ts">
import { tap } from 'rxjs/operators';

const { t } = useI18n();
const serverStore = useServerStore();

function onServerUnHide(plexServerId: number): void {
	useSubscription(
		serverStore
			.setServerHidden(plexServerId, false)
			.pipe(tap(() => close()))
			.subscribe(),
	);
}
</script>
