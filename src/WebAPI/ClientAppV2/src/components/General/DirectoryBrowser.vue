<template>
	<q-row justify="start">
		<q-col>
			<q-dialog :model-value="showDialog" max-width="900px">
				<q-card>
					<q-card-section v-if="path">
						<q-sub-header>
							{{ $t('components.directory-browser.select-path', { pathName: path.displayName }) }}
						</q-sub-header>
						<div>
							<p-text-field
								v-model="path.directory"
								outlined
								color="red"
								placeholder="Start typing or select a path below" />
						</div>
					</q-card-section>
					<q-separator inset />

					<q-card-section style="height: 100%; overflow-y: hidden">
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
								<!--	Directory browser table header -->
								<q-tr>
									<q-th> {{ $t('components.directory-browser.type') }}:</q-th>
									<q-th> {{ $t('components.directory-browser.path') }}:</q-th>
								</q-tr>
							</q-markup-table>
							<div style="height: 50vh; width: 100%">
								<!--	Directory browser table content -->
								<q-markup-table>
									<q-tr
										v-for="(item, i) in items"
										:key="i"
										style="cursor: pointer"
										@click="directoryNavigate(item)">
										<q-td>
											<q-icon :name="getIcon(item.type)" />
										</q-td>
										<q-td>
											{{ item.name }}
										</q-td>
									</q-tr>
								</q-markup-table>
							</div>
						</template>
					</q-card-section>

					<q-card-actions class="justify-end" style="height: 60px">
						<CancelButton @click="cancel()" />
						<ConfirmButton @click="confirm()" />
					</q-card-actions>
				</q-card>
			</q-dialog>
		</q-col>
	</q-row>
</template>

<script setup lang="ts">
import Log from 'consola';
import { ref, defineProps, defineEmits } from 'vue';
import { useSubscription } from '@vueuse/rxjs';
import { cloneDeep, debounce, DebouncedFunc } from 'lodash-es';
import { getDirectoryPath } from '@api/pathApi';
import type { FileSystemModelDTO, FolderPathDTO } from '@dto/mainApi';
import { FileSystemEntityType } from '@dto/mainApi';

const path = ref<FolderPathDTO | null>(null);
const showDialog = ref(false);
const parentPath = ref('');
const isLoading = ref(true);
const items = ref<FileSystemModelDTO[]>([]);
let debouncedWatch = debounce((newValue: any, oldValue: any) => {
	if (newValue !== oldValue) {
		path.value!.directory = newValue;
	}
}, 500);

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

// @Watch('newDirectory')
// onNewDirectory(val: string, oldVal: string) {
// 	this.debouncedWatch(val, oldVal);
// }

const open = (path: FolderPathDTO): void => {
	if (!path) {
		Log.error('parameter was null when opening DirectoryBrowser');
		return;
	}
	path = cloneDeep(path);
	requestDirectories(path.directory);
	showDialog.value = true;
};

function confirm(): void {
	emit('confirm', path.value);
	showDialog.value = false;
}

function cancel(): void {
	emit('cancel');
	showDialog.value = false;
}

function requestDirectories(path: string): void {
	if (path === '' || path === '/') {
		isLoading.value = true;
	}

	useSubscription(
		getDirectoryPath(path).subscribe((data) => {
			if (data.isSuccess && data.value) {
				items.value = data.value?.directories;

				// Don't add return row if in the root folder
				if (path !== '') {
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

onMounted(() => {
	debouncedWatch = debounce((newValue: any, oldValue: any) => {
		if (newValue !== oldValue) {
			path.value!.directory = newValue;
		}
	}, 1000);
});

onUnmounted(() => {
	debouncedWatch.cancel();
});

defineExpose({
	open,
});
</script>
