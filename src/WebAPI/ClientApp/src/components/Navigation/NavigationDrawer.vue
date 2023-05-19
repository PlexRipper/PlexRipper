<template>
	<q-drawer
		class="navigation-drawer"
		:model-value="showDrawer"
		:width="400"
		bordered
		style="overflow-x: hidden"
		@before-show="onShow"
		@before-hide="onHide">
		<q-col class="server-drawer-container">
			<q-scroll>
				<!-- Server drawer -->
				<server-drawer />
			</q-scroll>
		</q-col>
		<q-col class="menu-items">
			<q-separator />
			<!-- Menu items -->
			<q-expansion-list :items="getNavItems" />
		</q-col>
	</q-drawer>
</template>

<script setup lang="ts">
import { useSubscription } from '@vueuse/rxjs';
import { get, set } from '@vueuse/core';
import { DownloadService, SettingsService } from '@service';
import { QExpansionListProps } from '@interfaces/components/QExpansionListProps';

withDefaults(defineProps<{ showDrawer: boolean }>(), {
	showDrawer: false,
});
const debugMode = ref(false);

const items = ref<object[]>([]);

const downloadTaskCount = ref(0);

const getNavItems = computed((): QExpansionListProps[] => {
	const mainItems: QExpansionListProps[] = [
		{
			title: 'components.navigation-drawer.downloads',
			icon: 'mdi-download',
			link: '/downloads',
			type: 'badge',
			count: downloadTaskCount.value,
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

	if (get(debugMode)) {
		mainItems.push({
			title: 'components.navigation-drawer.debug',
			icon: 'mdi-bug-outline',
			children: [
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

	useSubscription(
		DownloadService.getTotalDownloadsCount().subscribe((count) => {
			set(downloadTaskCount, count);
		}),
	);

	useSubscription(
		SettingsService.getDebugMode().subscribe((value) => {
			set(debugMode, value);
		}),
	);
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
