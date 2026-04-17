<template>
	<QCardDialog
		:name="DialogType.UpdateAvailableDialog"
		content-height="80"
		cy="updatedialog-cy">
		<template #title>
			{{ t('components.update-available-dialog.title') }}
		</template>

		<template #default>
			<QRow
				align="start"
				full-height>
				<QCol
					cols="auto"
					align-self="stretch">
					<!-- Tab Index -->
					<q-tabs
						v-model="releaseIndex"
						vertical
						active-color="primary"
						indicator-color="primary"
						dense>
						<q-tab
							v-for="(release, index) in updateStore.releaseNotes"
							:key="index"
							:name="index"
							:label="release.version">
							<QDateTime
								:text="release.releaseDate"
								short-date />
							<q-badge
								v-if="release.isDevRelease"
								color="warning"
								text-color="black"
								:label="t('components.update-available-dialog.dev-badge')" />
						</q-tab>
					</q-tabs>
				</QCol>
				<QCol
					align-self="stretch"
					class="tab-content inherit-all-height scroll">
					<div
						v-if="!updateStore.hasUpdateAvailable"
						class="update-dialog__empty">
						{{ t('components.update-available-dialog.no-releases') }}
					</div>
					<q-tab-panels
						v-model="releaseIndex"
						animated>
						<q-tab-panel
							v-for="(release, index) in updateStore.releaseNotes"
							:key="index"
							:name="index"
							class="update-dialog__tab-panel">
							<div class="update-dialog__panel-header q-mb-md">
								<div class="column">
									<span class="update-dialog__panel-version">{{ release.version }}</span>
									<QDateTime
										:text="release.releaseDate"
										long-date
										time />
								</div>
								<q-badge
									v-if="release.isDevRelease"
									color="warning"
									text-color="black"
									:label="t('components.update-available-dialog.dev-badge')" />
							</div>

							<div class="i18n-formatting update-dialog__notes">
								<VueMarkdown
									v-if="release.notes"
									:markdown="release.notes" />
							</div>
						</q-tab-panel>
					</q-tab-panels>
				</QCol>
			</QRow>
		</template>

		<template #actions="{ close }">
			<q-btn
				v-if="globalStore.isDockerMode"
				unelevated
				color="primary"
				icon="mdi-docker"
				target="_blank"
				href="https://hub.docker.com/r/reaparr/reaparr"
				:label="t('components.update-available-dialog.docker-action')" />

			<q-btn
				v-else
				unelevated
				color="primary"
				icon="mdi-update"
				:label="t('components.update-available-dialog.update-now')"
				@click="runDesktopUpdate" />

			<q-btn
				flat
				:label="t('general.commands.close')"
				@click="close" />
		</template>
	</QCardDialog>
</template>

<script setup lang="ts">
import { useUpdateStore } from '@store';
import { DialogType } from '@enums';
import { useI18n } from 'vue-i18n';
import { VueMarkdown } from '@crazydos/vue-markdown';

const { t } = useI18n();

const globalStore = useGlobalStore();
const updateStore = useUpdateStore();

const releaseIndex = ref(0);

function runDesktopUpdate() {
	useSubscription(updateStore.downloadUpdate().subscribe());
}
</script>
