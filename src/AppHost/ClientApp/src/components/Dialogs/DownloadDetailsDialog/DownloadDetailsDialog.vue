<template>
	<QCardDialog
		:loading="loading"
		:name="DialogType.DownloadDetailsDialog"
		:type="'' as string"
		full-height
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
							<OverviewTabContent
								:download-task="downloadTask"
								:errors="errors" />
						</q-tab-panel>

						<!-- Logs Panel -->
						<q-tab-panel
							data-cy="download-details-dialog-tab-content-2"
							name="logs">
							<DownloadDetailsDialogLogsTabContent :download-task-id="downloadTaskId" />
						</q-tab-panel>
					</q-tab-panels>
				</div>
			</div>
		</template>
	</QCardDialog>
</template>

<script lang="ts" setup>
import { set, get } from '@vueuse/core';
import type { DownloadTaskDTO, ErrorDTO } from '@dto';
import { downloadApi } from '@api';
import { DialogType } from '@enums';

const tabIndex = ref<string>('overview');

const loading = ref(true);

const downloadTaskId = ref<string>('');
const downloadTask = ref<DownloadTaskDTO>();

const errors = ref<ErrorDTO[]>([]);

function onOpen(event: string) {
	set(loading, true);
	set(downloadTaskId, event);

	useSubscription(downloadApi.getDownloadTaskByGuidEndpoint(get(downloadTaskId)).subscribe((data) => {
		if (data.isSuccess && data.value) {
			set(downloadTask, data.value);
		} else {
			set(errors, data?.errors ?? []);
		}
		set(loading, false);
	}));
}

function onClose() {
	set(downloadTask, null);
	set(downloadTaskId, '');
	set(errors, []);
	set(loading, true);
	set(tabIndex, 'overview');
}
</script>

<style lang="scss">
.layout-container {
  height: 100%;
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
