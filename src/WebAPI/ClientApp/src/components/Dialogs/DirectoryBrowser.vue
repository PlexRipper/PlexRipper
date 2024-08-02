<template>
	<QCardDialog
		:name="name"
		min-width="70vw"
		max-width="70vw"
		content-height="80"
		:loading="isLoading"
		button-align="between"
		@opened="open">
		<template #title>
			{{ t('components.directory-browser.select-path', { pathName: path?.displayName ?? '' }) }}
		</template>
		<template #top-row>
			<QRow>
				<QCol
					v-if="!isCurrentWritable"
					cols="auto"
					class="q-px-md">
					<q-icon
						color="red"
						size="sm"
						name="mdi-alert-circle">
						<q-tooltip
							anchor="top middle"
							self="center middle">
							<span>{{ t('components.directory-browser.has-no-write-permission') }}</span>
						</q-tooltip>
					</q-icon>
				</QCol>
				<QCol>
					<PTextField
						v-model="currentPath"
						outlined
						color="red"
						:placeholder="t('components.directory-browser.no-path')"
						@update:model-value="requestDirectories" />
				</QCol>
			</QRow>
			<q-markup-table class="q-pr-md">
				<thead>
					<tr>
						<th
							class="text-left"
							style="width: 100px">
							{{ t('components.directory-browser.type') }}
						</th>
						<th class="text-left">
							{{ t('components.directory-browser.path') }}
						</th>
					</tr>
				</thead>
				<!-- The return row -->
				<tbody v-if="currentPathModel != null">
					<tr @click="directoryNavigate(returnRow)">
						<td
							class="text-left"
							style="width: 100px">
							<q-icon
								size="md"
								:name="getIcon(returnRow.type)" />
						</td>
						<td class="text-left">
							{{ returnRow.name }}
						</td>
					</tr>
				</tbody>
			</q-markup-table>
		</template>
		<template #default>
			<!--	Directory Browser	-->
			<q-markup-table>
				<tbody class="scroll">
					<tr
						v-for="(row, index) in items"
						:key="index"
						:class="rowClass(!row.hasReadPermission)"
						@click="directoryNavigate(row)">
						<td
							class="text-left"
							style="width: 100px">
							<q-icon
								size="md"
								:name="getIcon(row.type)" />
						</td>
						<td class="text-left">
							<q-icon
								v-if="!row.hasReadPermission"
								color="red"
								size="sm"
								name="mdi-alert-circle">
								<q-tooltip
									anchor="top middle"
									self="center middle">
									<span>{{ t('components.directory-browser.has-no-read-permission') }}</span>
								</q-tooltip>
							</q-icon>

							{{ row.name }}
						</td>
					</tr>
				</tbody>
			</q-markup-table>
		</template>
		<template #actions>
			<CancelButton @click="cancel()" />
			<ConfirmButton
				:disabled="!isCurrentWritable"
				:tooltip-text="!isCurrentWritable ? $t('components.directory-browser.current-folder-has-no-write-permission') : ''"
				@click="confirm()" />
		</template>
	</QCardDialog>
</template>

<script setup lang="ts">
import Log from 'consola';
import { useSubscription } from '@vueuse/rxjs';
import { get, set } from '@vueuse/core';
import type { FileSystemModelDTO, FolderPathDTO } from '@dto';
import { FileSystemEntityType } from '@dto';
import { folderPathApi } from '@api';
import { useCloseControlDialog } from '~/composables/event-bus';

const { t } = useI18n();
const path = ref<FolderPathDTO | null>(null);
const currentPath = ref('');
const currentPathModel = ref<FileSystemModelDTO | null>(null);
const parentPath = ref('');
const isLoading = ref(true);
const items = ref<FileSystemModelDTO[]>([]);
const returnRow = ref<FileSystemModelDTO>({
	name: '...',
	path: '..',
	type: FileSystemEntityType.Parent,
	extension: '',
	size: 0,
	lastModified: '',
	hasReadPermission: true,
	hasWritePermission: false,
});

const props = defineProps<{
	name: string;
}>();

const emit = defineEmits<{
	(e: 'confirm', path: FolderPathDTO): void;
	(e: 'cancel'): void;
}>();

const isCurrentWritable = computed(() => {
	return get(currentPathModel)?.hasWritePermission ?? false;
});

function rowClass(hasReadPermission: boolean) {
	return {
		'cursor-pointer': !hasReadPermission,
		'q-tr--no-hover': hasReadPermission,
	};
}

const getIcon = (type: FileSystemEntityType): string => {
	switch (type) {
		case FileSystemEntityType.Parent:
			return 'mdi-arrow-left';
		case FileSystemEntityType.Drive:
			return 'mdi-harddisk';
		case FileSystemEntityType.Folder:
			return 'mdi-folder';
		case FileSystemEntityType.File:
			return 'mdi-file';
		default:
			return 'mdi-crosshairs-question';
	}
};

function open(event: unknown): void {
	let selectedPath = event as FolderPathDTO;
	if (!selectedPath) {
		Log.error('parameter was null when opening DirectoryBrowser');
		return;
	}
	selectedPath = cloneDeep(selectedPath);
	requestDirectories(selectedPath.directory);
	set(path, selectedPath);
	set(currentPath, selectedPath.directory);
}

function cancel(): void {
	emit('cancel');
	useCloseControlDialog(props.name);
}

function confirm(): void {
	if (!get(path)) {
		Log.error('path was null when confirming DirectoryBrowser');
		return;
	}

	emit('confirm', get(path) as FolderPathDTO);
	useCloseControlDialog(props.name);
}

function requestDirectories(newPath: string): void {
	if (path.value) {
		path.value.directory = newPath;
	}

	useSubscription(
		folderPathApi
			.getFolderPathDirectoryEndpoint({
				path: newPath,
			})
			.subscribe(({ isSuccess, value }) => {
				if (isSuccess && value) {
					set(items, value?.directories);
					set(currentPathModel, value?.current ?? null);
					set(parentPath, value?.parent);
					set(isLoading, false);
				}
			}),
	);
}

function directoryNavigate(dataRow: FileSystemModelDTO): void {
	if (!dataRow.hasReadPermission) {
		return;
	}

	if (dataRow.path === '..') {
		requestDirectories(parentPath.value);
		set(currentPath, parentPath.value);
	} else {
		requestDirectories(dataRow.path);
		set(currentPath, dataRow.path);
	}
}
</script>
