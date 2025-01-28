<template>
	<QCardDialog
		full-height
		:name="DialogType.RefreshAccountAccessDialog"
		:cy="DialogType.RefreshAccountAccessDialog"
		:type="[] as RefreshPlexAccountAccessRapportDTO[]"
		@opened="onOpened"
		@closed="onClosed">
		<template #default>
			<div>
				<Print force-show>
					{{ refreshRapports }}
				</Print>
				<Print force-show>
					{{ plexAccessNodes }}
				</Print>
				<div
					v-for="plexAccount in plexAccessNodes"
					:key="plexAccount.plexAccountId">
					<q-tree
						v-model:expanded="expanded"
						:nodes="plexAccount.servers"
						:node-key="'id' as keyof IAccessServer"
						default-expand-all>
						<template #default-header="{ node }: { node: IAccessNode }">
							<QRow
								justify="between"
								class="q-mr-lg"
								align="center">
								<QCol>
									<div :class="{ 'text-weight-bold': true }">
										<!--	Row Icon -->
										<q-icon
											v-if="node.type === 'server'"
											name="mdi-server"
											size="28px"
											class="q-mr-sm" />
										<QMediaTypeIcon
											v-else
											:media-type="node.libraryType" />
										<!-- Row Title	-->
										<span
											class="q-ml-sm"
											:data-cy="
												node.type === 'server'
													? 'refresh-account-access-dialog-server-title'
													: 'refresh-account-access-dialog-library-title'
											">
											{{ node.name }}
										</span>
									</div>
								</QCol>
							</QRow>
						</template>
					</q-tree>
				</div>
			</div>
		</template>
		<!-- Actions -->
		<template #actions="{ close }">
			<QRow justify="end">
				<QCol cols="auto">
					<HideButton
						cy="sync-server-media-dialog-hide-btn"
						@click="close" />
				</QCol>
			</QRow>
		</template>
	</QCardDialog>
</template>

<script setup lang="ts">
import { get, set } from '@vueuse/core';
import type {
	PlexMediaType,
	RefreshPlexAccountAccessRapportDTO } from '@dto';
import { DialogType } from '@enums';
import { isEmpty } from 'lodash-es';
import {
	useServerStore,
	useLibraryStore,
	useAccountStore,
} from '#imports';

const accountStore = useAccountStore();
const serverStore = useServerStore();
const libraryStore = useLibraryStore();
const refreshRapports = ref<RefreshPlexAccountAccessRapportDTO[]>([]);

const expanded = ref<number[]>([]);

const plexAccessNodes = computed((): IPlexAccountAccessRapportNode[] => {
	return get(refreshRapports).map((rapport): IPlexAccountAccessRapportNode => {
		const account = accountStore.getAccount(rapport.plexAccountId);
		if (!account) {
			throw new Error(`Could not find account with id ${rapport.plexAccountId}`);
		}

		const libraries = (rapport: RefreshPlexAccountAccessRapportDTO, plexServerId: number) => {
			const libraryNodes: IAccessNode[] = [];

			const created = rapport.libraryAccessRapport.filter((x) => x.plexServerId == plexServerId).flatMap((x) => x.created);
			if (!isEmpty(created)) {
				libraryNodes.push(...libraryStore.getLibraries(created)
					.map((x): IAccessNode => ({ id: x.id, name: x.title, access: 'added', type: 'library', libraryType: x.type })));
			}

			const updated = rapport.libraryAccessRapport.filter((x) => x.plexServerId == plexServerId).flatMap((x) => x.updated);
			if (!isEmpty(updated)) {
				const x = libraryStore.getLibraries(updated);
				console.log(x);
				libraryNodes.push(...libraryStore.getLibraries(updated)
					.map((x): IAccessNode => ({ id: x.id, name: x.title, access: 'remained', type: 'library', libraryType: x.type })));
			}

			const deleted = rapport.libraryAccessRapport.filter((x) => x.plexServerId == plexServerId).flatMap((x) => x.deleted);
			if (!isEmpty(deleted)) {
				libraryNodes.push(...libraryStore.getLibraries(deleted)
					.map((x): IAccessNode => ({ id: x.id, name: x.title, access: 'removed', type: 'library', libraryType: x.type })));
			}

			return libraryNodes;
		};

		const result: IPlexAccountAccessRapportNode = {
			plexAccountName: account.displayName,
			plexAccountId: account.id,
			servers: [],
		};

		if (!isEmpty(rapport.serverAccessRapport.created)) {
			result.servers.push(...serverStore.getServers(rapport.serverAccessRapport.created)
				.map((x): IAccessServer => ({
					id: x.id,
					name: x.name,
					access: 'added',
					type: 'server',
					children: libraries(rapport, x.id),
				})));
		}

		if (!isEmpty(rapport.serverAccessRapport.updated)) {
			result.servers.push(...serverStore.getServers(rapport.serverAccessRapport.updated)
				.map((x): IAccessServer => ({
					id: x.id,
					name: x.name,
					access: 'remained',
					type: 'server',
					children: libraries(rapport, x.id),
				})));
		}

		if (!isEmpty(rapport.serverAccessRapport.deleted)) {
			result.servers.push(...serverStore.getServers(rapport.serverAccessRapport.deleted)
				.map((x): IAccessServer => ({
					id: x.id,
					name: x.name,
					access: 'removed',
					type: 'server',
					children: libraries(rapport, x.id),
				})));
		}

		return result;
	});
},
);

function onOpened(data: RefreshPlexAccountAccessRapportDTO[]): void {
	set(refreshRapports, data);
}

function onClosed(): void {
	set(refreshRapports, []);
}
interface IAccessServer extends IAccessNode {
	children: IAccessNode[];
}

interface IAccessNode {
	id: number;
	access: 'added' | 'remained' | 'removed';
	name: string;
	type: 'server' | 'library';
	libraryType: PlexMediaType;
}

interface IPlexAccountAccessRapportNode {
	plexAccountId: number;
	plexAccountName: string;
	servers: IAccessServer[];
}
</script>
