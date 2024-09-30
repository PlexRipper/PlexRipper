<template>
	<QCardDialog
		:loading="loading"
		:name="name"
		:scroll="false"
		:content-height="tabIndex === 'overview' ? '60' : '100'"
		max-width="60vw"
		@closed="onClose"
		@opened="onOpen">
		<!-- Title -->
		<template #title>
			<template v-if="downloadTask">
				<QText
					:value="downloadTask.fullTitle"
					cy="download-details-dialog-title"
					size="h6">
					<template #prepend>
						<QMediaTypeIcon
							:media-type="downloadTask.mediaType"
							:size="36"
							class="q-mr-sm" />
					</template>
				</QText>
			</template>
			<template v-else>
				{{ $t('components.download-details-dialog.invalid-download-task.title') }}
			</template>
		</template>
		<template #default>
			<div class="layout-container">
				<div class="tabs">
					<q-tabs
						v-model="tabIndex"
						active-color="red"
						vertical>
						<!-- Overview Tab -->
						<q-tab
							class="q-mr-md"
							data-cy="download-details-dialog-tab-1"
							icon="mdi-chart-box-outline"

							label="Overview"
							name="overview" />

						<!-- Logs Tab -->
						<q-tab
							class="q-mr-md"
							data-cy="download-details-dialog-tab-2"
							icon="mdi-text-box-outline"
							label="Logs"
							name="logs" />
					</q-tabs>
				</div>
				<div class="panels">
					<q-tab-panels
						v-model="tabIndex"
						animated
						transition-next="slide-up"
						transition-prev="slide-down"
						vertical>
						<!-- Overview Panel -->
						<q-tab-panel
							data-cy="download-details-dialog-tab-content-1"
							name="overview">
							<q-markup-table
								v-if="downloadTask"
								class="section-table">
								<tbody>
									<tr>
										<td style="width: 25%">
											{{ t('components.download-details-dialog.overview.status') }}
										</td>
										<td data-cy="download-details-dialog-status">
											{{ downloadTask.status }}
										</td>
									</tr>
									<tr v-if="downloadTask.fileName">
										<td>{{ t('components.download-details-dialog.overview.file-name') }}</td>
										<td data-cy="download-details-dialog-file-name">
											{{ downloadTask.fileName }}
										</td>
									</tr>
									<tr>
										<td>{{ t('components.download-details-dialog.overview.download-path') }}</td>
										<td data-cy="download-details-dialog-download-path">
											{{ downloadTask.downloadDirectory }}
										</td>
									</tr>
									<tr>
										<td>{{ t('components.download-details-dialog.overview.destination-path') }}</td>
										<td data-cy="download-details-dialog-destination-path">
											{{ downloadTask.destinationDirectory }}
										</td>
									</tr>
									<tr v-if="downloadTask.downloadUrl">
										<td>{{ t('components.download-details-dialog.overview.download-url') }}</td>
										<td data-cy="download-details-dialog-download-url">
											<QRow
												class="no-wrap"
												no-gutters>
												<QCol>{{ downloadTask.downloadUrl }}</QCol>
												<QCol cols="auto">
													<ExternalLinkButton :href="downloadTask.downloadUrl" />
												</QCol>
											</QRow>
										</td>
									</tr>
								</tbody>
							</q-markup-table>
							<div v-else>
								<h4>{{ $t('components.download-details-dialog.invalid-download-task.body') }}</h4>
								<ul>
									<li
										v-for="(error, index) in errors"
										:key="index">
										{{ error.message }}
									</li>
								</ul>
							</div>
						</q-tab-panel>
						<!-- Logs Panel -->
						<q-tab-panel
							data-cy="download-details-dialog-tab-content-2"
							name="logs">
							<q-scroll-area style="height: 100%; max-width: 100%;">
								<q-list>
									<q-item
										v-for="(item, index) in logs"
										:key="index"
										v-ripple
										clickable>
										<q-item-section avatar>
											<q-icon
												:color="Convert.logLevelToColor(item.logLevel)"
												:name="Convert.logLevelToIcon(item.logLevel)" />
										</q-item-section>

										<q-item-section>
											<QDateTime
												:text="item.createdAt"
												long-date
												time />
											<QText :value="item.message" />
										</q-item-section>
									</q-item>
									<!-- Logs Loading -->
									<q-item>
										<QRow
											column
											align="center">
											<QCol cols="auto">
												<QSpinnerDots
													color="primary"
													size="40px" />
											</QCol>
										</QRow>
									</q-item>
								</q-list>
							</q-scroll-area>
						</q-tab-panel>
					</q-tab-panels>
				</div>
			</div>
		</template>
	</QCardDialog>
</template>

<script lang="ts" setup>
import { set, get } from '@vueuse/core';
import { type DownloadTaskDTO, type DownloadWorkerLogDTO, type ReasonDTO } from '@dto';
import { downloadApi } from '@api';
import Convert from '@class/Convert';

const { t } = useI18n();

const tabIndex = ref<string>('overview');

const loading = ref(true);
const logsLoading = ref(false);
const downloadTaskId = ref<string>('');
const downloadTask = ref<DownloadTaskDTO>();
const logs = ref<DownloadWorkerLogDTO[]>([]);
const logRefreshTimer = useIntervalFn(() => refreshLogs(), 1000);

const errors = ref<ReasonDTO[]>([]);
defineProps<{
	name: string;
}>();

function onOpen(event: unknown) {
	set(loading, true);
	set(downloadTaskId, event as string);

	useSubscription(downloadApi.getDownloadTaskByGuidEndpoint(get(downloadTaskId)).subscribe((data) => {
		if (data.isSuccess && data.value) {
			set(downloadTask, data.value);
		} else {
			set(errors, data?.reasons ?? []);
		}
		set(loading, false);
	}));

	logRefreshTimer.resume();
}

function refreshLogs() {
	if (!get(downloadTaskId)) {
		return;
	}

	set(logsLoading, true);
	logRefreshTimer.pause();

	useSubscription(
		downloadApi.getDownloadTaskLogsByDownloadTaskIdEndpoint(get(downloadTaskId)).subscribe((data) => {
			if (data.isSuccess && data.value) {
				set(logs, [...data.value]);
			} else {
				set(errors, data?.reasons ?? []);
			}
			set(logsLoading, true);
			logRefreshTimer.resume();
		}));
}

function onClose() {
	logRefreshTimer.pause();

	set(downloadTask, null);
	set(downloadTaskId, '');
	set(logs, []);
	set(errors, []);
	set(loading, true);

	set(tabIndex, 'overview');
}
</script>

<style lang="scss">
.layout-container {
  display: grid;
  grid-template-columns: repeat(8, 1fr);
  grid-template-rows: repeat(1, 1fr);
  gap: 0;
  flex-grow: 1;

  .tabs {
    grid-column: span 1 / span 1;
  }

  .panels {
    grid-column: span 7 / span 7;
    display: flex;

    .q-tab-panels {
      flex-grow: 1;
    }
  }
}
</style>
