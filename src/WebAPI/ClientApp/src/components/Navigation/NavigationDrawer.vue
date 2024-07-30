<template>
	<q-drawer
		class="navigation-drawer"
		:model-value="showDrawer"
		:width="400"
		bordered
		style="overflow-x: hidden"
		@before-show="onShow"
		@before-hide="onHide"
	>
		<QCol class="server-drawer-container">
			<q-scroll>
				<!-- Server drawer -->
				<ServerDrawer />
			</q-scroll>
		</QCol>
		<QCol class="menu-items">
			<q-separator />
			<!-- Menu items -->
			<QExpansionList :items="getNavItems" />
		</QCol>
	</q-drawer>
</template>

<script setup lang="ts">
import type { QExpansionListProps } from '@interfaces/components/QExpansionListProps';
import { useSettingsStore } from '~/store';

withDefaults(defineProps<{ showDrawer: boolean }>(), {
	showDrawer: false,
});
const settingsStore = useSettingsStore();
const downloadStore = useDownloadStore();
const items = ref<object[]>([]);

const getNavItems = computed((): QExpansionListProps[] => {
	const mainItems: QExpansionListProps[] = [
		{
			title: 'components.navigation-drawer.downloads',
			icon: 'mdi-download',
			link: '/downloads',
			type: 'badge',
			count: downloadStore.getActiveDownloadList().length,
		},
		{
			title: 'components.navigation-drawer.settings',
			icon: 'mdi-cog',
			link: '/settings',
			children: [
				{
					title: 'components.navigation-drawer.accounts',
					icon: 'mdi-account',
					link: '/settings/accounts',
				},
				{
					title: 'components.navigation-drawer.paths',
					icon: 'mdi-folder',
					link: '/settings/paths',
				},
				{
					title: 'components.navigation-drawer.ui',
					icon: 'mdi-television-guide',
					link: '/settings/ui',
				},
				{
					title: 'components.navigation-drawer.advanced',
					icon: 'mdi-wrench',
					link: '/settings/advanced',
				},
			],
		},
	];

	if (settingsStore.debugMode) {
		mainItems.push({
			title: 'components.navigation-drawer.debug',
			icon: 'mdi-bug-outline',
			children: [
				{
					title: 'components.navigation-drawer.scratchpad',
					icon: 'mdi-note-edit',
					link: '/debug-pages/scratchpad',
				},
				{
					title: 'components.navigation-drawer.dialogs',
					icon: 'mdi-dock-window',
					link: '/debug-pages/dialogs',
				},
				{
					title: 'components.navigation-drawer.buttons',
					icon: 'mdi-button-pointer',
					link: '/debug-pages/buttons',
				},
			],
		});
	}
	return mainItems;
});

function onShow() {
	document.body.classList.remove('navigation-drawer-closed');
	document.body.classList.add('navigation-drawer-opened');
}

function onHide() {
	document.body.classList.remove('navigation-drawer-opened');
	document.body.classList.add('navigation-drawer-closed');
}

onMounted(() => {
	items.value = getNavItems.value;
	document.body.classList.add('navigation-drawer-opened');
});
</script>

<style lang="scss">
@import '@/assets/scss/variables.scss';

.navigation-drawer {
	height: 100vh;
	display: flex;
	flex-direction: column;
	justify-content: space-between;

	.server-drawer-container {
		overflow-y: auto;
		overflow-x: hidden;

		flex-grow: 3;
	}

	.menu-items {
		flex-grow: 0;
	}
}
</style>
