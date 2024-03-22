<template>
	<QCardDialog
		:name="name"
		:value="path"
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
			<q-row>
				<q-col>
					<p-text-field
						v-model:model-value="currentPath"
						outlined
						color="red"
						@update:model-value="requestDirectories" />
					<q-text v-if="!(path?.directory ?? '')" size="large" align="center">
						{{ t('components.directory-browser.no-path') }}
					</q-text>
				</q-col>
			</q-row>
			<q-row>
				<q-col>
					<q-markup-table>
						<thead>
							<tr>
								<th class="text-left" style="width: 100px">
									{{ t('components.directory-browser.type') }}
								</th>
								<th class="text-left">
									{{ t('components.directory-browser.path') }}
								</th>
							</tr>
						</thead>
					</q-markup-table>
				</q-col>
			</q-row>
		</template>
		<template #default>
			<!--	Directory Browser	-->
			<q-markup-table>
				<tbody class="scroll">
					<tr v-for="(row, index) in items" :key="index" @click="directoryNavigate(row)">
						<td class="text-left" style="width: 100px">
							<q-icon size="md" :name="getIcon(row.type)" />
						</td>
						<td class="text-left">
							{{ row.name }}
						</td>
					</tr>
				</tbody>
			</q-markup-table>
		</template>
		<template #actions>
			<CancelButton @click="cancel()" />
			<ConfirmButton @click="confirm()" />
		</template>
	</QCardDialog>
</template>

<script setup lang="ts">
import Log from 'consola';
import { useSubscription } from '@vueuse/rxjs';
import { get, set } from '@vueuse/core';
import { getDirectoryPath } from '@api/pathApi';
import type { FileSystemModelDTO, FolderPathDTO } from '@dto/mainApi';
import { FileSystemEntityType } from '@dto/mainApi';
import { useCloseControlDialog } from '~/composables/event-bus';

const { t } = useI18n();
const path = ref<FolderPathDTO | null>(null);
const currentPath = ref('');
const parentPath = ref('');
const isLoading = ref(true);
const items = ref<FileSystemModelDTO[]>([]);

const props = defineProps<{
	name: string;
}>();

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

const emit = defineEmits<{
	(e: 'confirm', path: FolderPathDTO): void;
	(e: 'cancel'): void;
}>();

const open = (selectedPath: FolderPathDTO): void => {
	if (!selectedPath) {
		Log.error('parameter was null when opening DirectoryBrowser');
		return;
	}
	selectedPath = cloneDeep(selectedPath);
	requestDirectories(selectedPath.directory);
	set(path, selectedPath);
	set(currentPath, selectedPath.directory);
};
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
	if (newPath === '' || newPath === '/') {
		set(isLoading, true);
	}
	if (path.value) {
		// @ts-ignore
		path.value.directory = newPath;
	}

	useSubscription(
		getDirectoryPath(newPath).subscribe(({ isSuccess, value }) => {
			if (isSuccess && value) {
				set(items, value?.directories);

				// Don't add return row if in the root folder
				if (newPath !== '') {
					items.value.unshift({
						name: '...',
						path: '..',
						type: FileSystemEntityType.Parent,
						extension: '',
						size: 0,
						lastModified: '',
					});
				}
				set(isLoading, false);
				set(parentPath, value?.parent);
			}
		}),
	);
}

function directoryNavigate(dataRow: FileSystemModelDTO): void {
	if (dataRow.path === '..') {
		requestDirectories(parentPath.value);
	} else {
		requestDirectories(dataRow.path);
	}
}
</script>
