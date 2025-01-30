<template>
	<QCardDialog
		full-height
		:name="DialogType.RefreshAccountAccessDialog"
		:cy="DialogType.RefreshAccountAccessDialog"
		:type="[] as RefreshPlexAccountAccessRapportDTO[]"
		@opened="onOpened"

		@closed="onClosed">
		<template #title>
			<QText
				align="center"
				size="h4"
				:value="'Plex Account Access Report'" />
		</template>
		<template #top-row>
			<q-separator />
			<q-tabs
				v-model="tab"
				dense
				class="text-grey"
				active-color="primary"
				indicator-color="primary"
				align="justify"
				narrow-indicator>
				<q-tab
					v-for="plexAccount in plexAccessNodes"
					:key="plexAccount.plexAccountId"
					:data-cy="`refresh-account-access-tab-${plexAccount.plexAccountId}`"
					:name="plexAccount.plexAccountId"
					:label="plexAccount.plexAccountName" />
			</q-tabs>
			<q-separator />
		</template>
		<template #default>
			<q-tab-panels
				v-model="tab"
				animated>
				<q-tab-panel
					v-for="plexAccount in plexAccessNodes"
					:key="plexAccount.plexAccountId"
					:name="plexAccount.plexAccountId">
					<q-tree
						v-model:expanded="expanded"
						:nodes="plexAccount.servers"
						:node-key="'id' as keyof IServerAccessNode"
						:children-key="'libraries' as keyof IServerAccessNode"
						default-expand-all>
						<template #default-header="{ node }: { node: ILibraryAccessNode }">
							<QRow
								justify="between"
								class="q-mr-lg"
								align="center"
								gutter="sm">
								<QCol
									v-if="node.nodeType === 'library'"
									cols="auto">
									<QIconTooltip
										:value="node.state"
										:options="options" />
								</QCol>
								<QCol cols="auto">
									<!--	Row Icon -->
									<q-icon
										v-if="node.nodeType === 'server'"
										name="mdi-server"
										size="28px"
										class="q-mr-sm" />
									<QMediaTypeIcon
										v-else
										:media-type="node.libraryType" />
								</QCol>
								<QCol>
									<div :class="{ 'text-weight-bold': true }">
										<!-- Row Title	-->
										<QText
											class="q-ml-sm"
											:value="node.name"
											:cy="
												node.nodeType === 'server'
													? 'refresh-account-access-dialog-server-title'
													: 'refresh-account-access-dialog-library-title'
											" />
									</div>
								</QCol>
							</QRow>
						</template>
					</q-tree>
				</q-tab-panel>
			</q-tab-panels>
		</template>
		<!-- Actions -->
		<template #actions="{ close }">
			<QRow justify="end">
				<QCol cols="auto">
					<HideButton
						cy="refresh-account-access-dialog-hide-btn"
						@click="close" />
				</QCol>
			</QRow>
		</template>
	</QCardDialog>
</template>

<script setup lang="ts">
import { get, set } from '@vueuse/core';
import {
	PlexAccessState,
	type PlexLibraryAccessRapportDTO,
	type PlexMediaType,
	type PlexServerAccessRapportDTO,
	type RefreshPlexAccountAccessRapportDTO,
} from '@dto';
import { DialogType } from '@enums';
import type { QIconTooltipData } from '@interfaces';
import { sortBy } from 'lodash-es';
import QCardDialog from '@components/Common/QCardDialog.vue';
import { useAccountStore, useLibraryStore, useServerStore } from '#imports';

const { t } = useI18n();
const accountStore = useAccountStore();
const serverStore = useServerStore();
const libraryStore = useLibraryStore();
const refreshRapports = ref<RefreshPlexAccountAccessRapportDTO[]>([]);

const expanded = ref<number[]>([]);
const tab = ref(0);

const plexAccessNodes = computed((): IPlexAccountAccessRapportNode[] => {
	return get(refreshRapports).map((rapport) => {
		const account = accountStore.getAccount(rapport.plexAccountId);
		if (!account) {
			throw new Error(`Could not find account with id ${rapport.plexAccountId}`);
		}

		return {
			plexAccountId: account.id,
			plexAccountName: account.displayName,
			servers: rapport.access.map((x) => ({
				id: x.plexServerId,
				plexServerId: x.plexServerId,
				state: x.state,
				name: serverStore.getServerName(x.plexServerId),
				nodeType: 'server',
				libraries: sortBy(x.libraryAccess.map((y) => {
					const library = libraryStore.getLibrary(y.plexLibraryId);
					if (!library) {
						throw new Error(`Could not find library with id ${rapport.plexAccountId}`);
					}
					return {
						id: library.id,
						libraryType: library.type,
						name: library.title,
						plexLibraryId: library.id,
						plexServerId: library.plexServerId,
						state: y.state,
						nodeType: 'library',
					};
				}),
				// Sort first by state and then by title
				(x) => {
					switch (x.state) {
						case PlexAccessState.Granted:
							return 0;
						case PlexAccessState.Revoked:
							return 1;
						case PlexAccessState.Updated:
							return 2;
						default:
							return 10;
					}
				}, (x) => x.name.toLowerCase()),
			})),
		};
	});
});

const options = computed((): QIconTooltipData[] =>
	[{
		value: PlexAccessState.Granted,
		icon: 'mdi-timeline-plus-outline',
		tooltip: t('components.refresh-account-access-dialog.access.granted'),
	}, {
		value: PlexAccessState.Updated,
		icon: 'mdi-timeline-check-outline',
		tooltip: t('components.refresh-account-access-dialog.access.updated'),
	}, {
		value: PlexAccessState.Revoked,
		icon: 'mdi-timeline-alert-outline',
		tooltip: t('components.refresh-account-access-dialog.access.revoked'),
	}]);

function onOpened(data: RefreshPlexAccountAccessRapportDTO[]) {
	if (data && data.length > 0) {
		set(refreshRapports, data);
		set(tab, data[0].plexAccountId);
	}
}

function onClosed() {
	set(refreshRapports, []);
}

interface IServerAccessNode extends Omit<PlexServerAccessRapportDTO, 'libraryAccess'> {
	id: number;
	name: string;
	nodeType: 'library' | 'server';
	libraries: ILibraryAccessNode[];
}

interface ILibraryAccessNode extends PlexLibraryAccessRapportDTO {
	id: number;
	name: string;
	nodeType: 'library' | 'server';
	libraryType: PlexMediaType;
}

interface IPlexAccountAccessRapportNode {
	plexAccountId: number;
	plexAccountName: string;
	servers: IServerAccessNode[];
}
</script>
