<template>
	<v-row justify="start">
		<v-col>
			<v-dialog :value="showDialog" persistent max-width="900px">
				<v-card>
					<v-card-title v-if="path">
						<v-row>
							<v-col cols="12">
								<label>
									{{ $t('components.directory-browser.select-path', { pathName: path.displayName }) }}
								</label>
							</v-col>
							<v-col cols="12" style="max-height: 75px">
								<v-text-field
									v-model="path.directory"
									outlined
									color="red"
									placeholder="Start typing or select a path below"
								/>
							</v-col>
						</v-row>
					</v-card-title>
					<v-divider />

					<v-card-text style="height: 100%; overflow-y: hidden">
						<!--	Loading screen	-->
						<template v-if="isLoading">
							<v-row style="height: 50vh; width: 100%" justify="center" align="center">
								<v-col cols="auto">
									<loading-spinner />
								</v-col>
							</v-row>
						</template>
						<!--	Directory Browser	-->
						<template v-else>
							<!--	Directory browser table header -->
							<v-simple-table>
								<template #default>
									<thead>
										<tr>
											<th class="text-left" :width="100">{{ $t('components.directory-browser.type') }}:</th>
											<th class="text-left">{{ $t('components.directory-browser.path') }}:</th>
										</tr>
									</thead>
								</template>
							</v-simple-table>
							<!--	Directory browser table content -->
							<vue-scroll>
								<div style="height: 50vh; width: 100%">
									<v-simple-table>
										<template #default>
											<tbody>
												<tr
													v-for="(item, i) in items"
													:key="i"
													style="cursor: pointer"
													@click="directoryNavigate(item)"
												>
													<td :width="100">
														<v-icon>{{ getIcon(item.type) }}</v-icon>
													</td>
													<td>{{ item.name }}</td>
												</tr>
											</tbody>
										</template>
									</v-simple-table>
								</div>
							</vue-scroll>
						</template>
					</v-card-text>
					<v-card-actions class="justify-end" style="height: 60px">
						<CancelButton @click="cancel()" />
						<ConfirmButton @click="confirm()" />
					</v-card-actions>
				</v-card>
			</v-dialog>
		</v-col>
	</v-row>
</template>

<script lang="ts">
import Log from 'consola';
import { Component, Vue, Watch } from 'vue-property-decorator';
import { useSubscription } from '@vueuse/rxjs';
import { cloneDeep, debounce, DebouncedFunc } from 'lodash-es';
import { getDirectoryPath } from '@api/pathApi';
import type { FileSystemModelDTO, FolderPathDTO } from '@dto/mainApi';
import { FileSystemEntityType } from '@dto/mainApi';

@Component
export default class DirectoryBrowser extends Vue {
	private path: FolderPathDTO | null = null;
	private showDialog: boolean = false;
	private parentPath: string = '';
	private isLoading: boolean = true;
	private items: FileSystemModelDTO[] = [];
	private debouncedWatch!: DebouncedFunc<(newValue: any, oldValue: any) => void>;

	getIcon(type: FileSystemEntityType): string {
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
	}

	open(path: FolderPathDTO): void {
		if (!path) {
			Log.error('parameter was null when opening DirectoryBrowser');
			return;
		}
		this.path = cloneDeep(path);
		this.requestDirectories(path.directory);
		this.showDialog = true;
	}

	@Watch('newDirectory')
	onNewDirectory(val: string, oldVal: string) {
		this.debouncedWatch(val, oldVal);
	}

	confirm(): void {
		this.$emit('confirm', this.path);
		this.showDialog = false;
	}

	cancel(): void {
		this.$emit('cancel');
		this.showDialog = false;
	}

	directoryNavigate(dataRow: FileSystemModelDTO): void {
		if (dataRow.path === '..') {
			this.requestDirectories(this.parentPath);
		} else {
			this.requestDirectories(dataRow.path);
		}
	}

	requestDirectories(path: string): void {
		if (path === '' || path === '/') {
			this.isLoading = true;
		}

		useSubscription(
			getDirectoryPath(path).subscribe((data) => {
				if (data.isSuccess && data.value) {
					this.items = data.value?.directories;

					// Don't add return row if in the root folder
					if (path !== '') {
						this.items.unshift({
							name: '...',
							path: '..',
							type: FileSystemEntityType.Parent,
							extension: '',
							size: 0,
							lastModified: '',
						});
					}
					this.parentPath = data.value?.parent;
					if (this.path) {
						this.path.directory = path;
					}
					this.isLoading = false;
				}
			}),
		);
	}

	created(): void {
		this.debouncedWatch = debounce((newValue) => {
			this.requestDirectories(newValue);
		}, 1000);
	}

	beforeUnmount() {
		this.debouncedWatch.cancel();
	}
}
</script>
