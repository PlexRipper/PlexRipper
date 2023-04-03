<template>
	<QCardDialog :name="name" :value="path" width="80vw" :loading="isLoading" @opened="open">
		<template #title>
			{{ $t('components.directory-browser.select-path', { pathName: path?.displayName ?? '' }) }}
		</template>
		<template #top-row>
			<q-row>
				<q-col>
					<p-text-field
						:model-value="path?.directory"
						outlined
						color="red"
						debounce="500"
						placeholder="Start typing or select a path below"
						@update:model-value="requestDirectories" />
				</q-col>
			</q-row>
			<q-row>
				<q-col>
					<q-markup-table>
						<thead>
							<tr>
								<th class="text-left" style="width: 100px">
									{{ $t('components.directory-browser.type') }}
								</th>
								<th class="text-left">
									{{ $t('components.directory-browser.path') }}
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
					<tr v-for="row in items" @click="directoryNavigate(row)">
						<td class="text-left" style="width: 100px">
							<q-icon size="md" :name="getIcon(row.type)" />
						</td>
						<td class="text-left">
							{{ row.path }}
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
import { ref, defineProps, defineEmits } from 'vue';
import { useSubscription } from '@vueuse/rxjs';
import { cloneDeep } from 'lodash-es';
import { getDirectoryPath } from '@api/pathApi';
import type { FileSystemModelDTO, FolderPathDTO } from '@dto/mainApi';
import { FileSystemEntityType } from '@dto/mainApi';
import { useCloseControlDialog } from '~/composables/event-bus';

const path = ref<FolderPathDTO>();
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
	path.value = selectedPath;
};

function confirm(): void {
	if (!path.value) {
		Log.error('path was null when confirming DirectoryBrowser');
		return;
	}
	emit('confirm', path.value);
	useCloseControlDialog(props.name);
}

function cancel(): void {
	emit('cancel');
	useCloseControlDialog(props.name);
}

function requestDirectories(newPath: string): void {
	if (newPath === '' || newPath === '/') {
		isLoading.value = true;
	}
	if (path.value) {
		path.value.directory = newPath;
	}

	useSubscription(
		getDirectoryPath(newPath).subscribe((data) => {
			if (data.isSuccess && data.value) {
				items.value = data.value?.directories;

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
				isLoading.value = false;
				parentPath.value = data.value?.parent;
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

defineExpose({
	open,
});
</script>
