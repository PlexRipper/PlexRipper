<template>
	<q-dialog :model-value="showDialog">
		<q-card class="directory-browser-content">
			<q-card-section v-if="path" style="padding-bottom: 0 !important">
				<h2>
					{{ $t('components.directory-browser.select-path', { pathName: path.displayName }) }}
				</h2>
				<div>
					<p-text-field
						:model-value="path.directory"
						outlined
						color="red"
						debounce="500"
						placeholder="Start typing or select a path below"
						@update:model-value="requestDirectories" />
				</div>
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
			</q-card-section>
			<q-card-section style="height: 50vh; padding: 0 16px !important">
				<q-scroll-area class="fit">
					<!--	Loading screen	-->
					<template v-if="isLoading">
						<q-row style="height: 50vh; width: 100%" justify="center" align="center">
							<q-col cols="auto">
								<loading-spinner />
							</q-col>
						</q-row>
					</template>
					<!--	Directory Browser	-->
					<template v-else>
						<q-markup-table>
							<tbody class="scroll">
								<tr v-for="row in items" @click="directoryNavigate($event, row)">
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
				</q-scroll-area>
			</q-card-section>

			<q-card-actions class="justify-end" style="height: 60px">
				<CancelButton @click="cancel()" />
				<ConfirmButton @click="confirm()" />
			</q-card-actions>
		</q-card>
	</q-dialog>
</template>

<script setup lang="ts">
import Log from 'consola';
import { ref, defineProps, defineEmits } from 'vue';
import { useSubscription } from '@vueuse/rxjs';
import { cloneDeep } from 'lodash-es';
import { getDirectoryPath } from '@api/pathApi';
import type { FileSystemModelDTO, FolderPathDTO } from '@dto/mainApi';
import { FileSystemEntityType } from '@dto/mainApi';

const path = ref<FolderPathDTO | null>(null);
const showDialog = ref(false);
const parentPath = ref('');
const isLoading = ref(true);
const items = ref<FileSystemModelDTO[]>([]);

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
	showDialog.value = true;
};

function confirm(): void {
	if (!path.value) {
		Log.error('path was null when confirming DirectoryBrowser');
		return;
	}
	emit('confirm', path.value);
	showDialog.value = false;
}

function cancel(): void {
	emit('cancel');
	showDialog.value = false;
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

function directoryNavigate(evt, dataRow: FileSystemModelDTO): void {
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
<style lang="scss">
@import './src/assets/scss/_variables.scss';
@import './src/assets/scss/_mixins.scss';

.directory-browser-content {
	max-width: 80vw !important;
	min-width: 70vw !important;
}
</style>
