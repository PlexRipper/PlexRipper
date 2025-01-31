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
						:node-key="'id' as keyof IAccessNode"
						:children-key="'libraries' as keyof IAccessNode"
						dense
						default-expand-all>
						<template #default-header="{ node }: { node: IAccessNode }">
							<QRow
								justify="between"
								class="q-mr-lg"
								align="center">
								<!-- Server Header Prepend -->
								<template v-if="node.isServer">
									<QCol cols="auto">
										<!--	Row Icon -->
										<q-icon
											name="mdi-server"
											size="28px" />
									</QCol>
									<QCol
										cols="auto"
										class="q-mx-sm">
										<QStatus
											:value="!node.isServerOffline" />
									</QCol>
								</template>
								<!-- Library Header  Prepend -->
								<template v-else-if="node.isLibrary">
									<QCol cols="auto">
										<QIconTooltip
											:value="node.state"
											:options="options" />
										<QMediaTypeIcon
											class="q-mx-sm"
											:media-type="node.libraryType" />
									</QCol>
								</template>
								<!-- Row Title -->
								<QCol>
									<QText
										:bold="node.isServer? 'bold' : 'regular'"
										class="q-ml-sm q-mt-auto"
										:value="node.name"
										size="body2"
										:cy="
											node.isServer
												? 'refresh-account-access-dialog-server-title'
												: 'refresh-account-access-dialog-library-title'
										" />
								</QCol>
							</QRow>
						</template>
						<template #default-body="{ node }: { node: IAccessNode }">
							<QRow
								v-if="node.isServer && node.isServerOffline"
								:full-width="false"
								class="q-ml-lg">
								<QCol>
									<QText>
										Could not retrieve the library access list from this server because it is offline, try again when the server is back online.
									</QText>
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
import type {	PlexLibraryAccessRapportDTO,
	PlexServerAccessRapportDTO,
	RefreshPlexAccountAccessRapportDTO } from '@dto';
import {	PlexAccessState,	PlexMediaType } from '@dto';
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

		const servers = rapport.access.map((x): IAccessNode => ({
			id: x.plexServerId,
			plexServerId: x.plexServerId,
			state: x.state,
			isServerOffline: x.isServerOffline,
			name: serverStore.getServerName(x.plexServerId),
			isServer: true,
			isLibrary: false,
			libraryType: PlexMediaType.None,
			plexLibraryId: 0,
			libraries: sortBy(x.libraryAccess.map((y): IAccessNode => {
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
					isServer: false,
					isLibrary: true,
					libraries: [],
					isServerOffline: false,
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
		}));

		return {
			plexAccountId: account.id,
			plexAccountName: account.displayName,
			servers: sortBy(servers, (x) => x.isServerOffline),
		};
	});
});

const options = computed((): QIconTooltipData[] =>
	[{
		value: PlexAccessState.Granted,
		icon: 'mdi-timeline-plus-outline',
		tooltip: t('components.refresh-account-access-dialog.access.granted'),
		color: 'positive',
	},
	{
		value: PlexAccessState.Revoked,
		icon: 'mdi-timeline-alert-outline',
		tooltip: t('components.refresh-account-access-dialog.access.revoked'),
		color: 'negative',
	},
	{
		value: PlexAccessState.Updated,
		icon: 'mdi-timeline-check-outline',
		tooltip: t('components.refresh-account-access-dialog.access.updated'),
		color: 'grey',
	},
	]);

function onOpened(data: RefreshPlexAccountAccessRapportDTO[]) {
	if (data && data.length > 0) {
		set(refreshRapports, data);
		set(tab, data[0].plexAccountId);
	}
}

function onClosed() {
	set(refreshRapports, []);
}

interface IAccessNode extends PlexLibraryAccessRapportDTO, Omit<PlexServerAccessRapportDTO, 'libraryAccess'> {
	id: number;
	name: string;
	isServer: boolean;
	isLibrary: boolean;
	libraries: IAccessNode[];
	libraryType: PlexMediaType;
}

interface IPlexAccountAccessRapportNode {
	plexAccountId: number;
	plexAccountName: string;
	servers: IAccessNode[];
}
</script>
