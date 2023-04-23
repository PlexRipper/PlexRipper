<template>
	<q-drawer class="navigation-drawer" :model-value="showDrawer" :width="400" bordered style="overflow-x: hidden">
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
import { defineProps, ref, computed, withDefaults, onMounted } from 'vue';
import { useSubscription } from '@vueuse/rxjs';
import { set } from '@vueuse/core';
import { DownloadService } from '@service';
import { QExpansionListProps } from '@interfaces/components/QExpansionListProps';

withDefaults(defineProps<{ showDrawer: boolean }>(), {
	showDrawer: false,
});

const items = ref<object[]>([]);

const downloadTaskCount = ref(0);

const getNavItems = computed((): QExpansionListProps[] => {
	return [
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
				{
					title: 'components.navigation-drawer.debug',
					icon: 'mdi-bug-outline',
					link: '/settings/debug',
				},
			],
		},
		{
			title: 'components.navigation-drawer.debug',
			icon: 'mdi-bug-outline',
			children: [
				{
					title: 'Dialogs',
					icon: 'mdi-dock-window',
					link: '/debug-pages/dialogs',
				},
				{
					title: 'Buttons',
					icon: 'mdi-button-pointer',
					link: '/debug-pages/buttons',
				},
			],
		},
	];
});

onMounted(() => {
	items.value = getNavItems.value;
	useSubscription(
		DownloadService.getTotalDownloadsCount().subscribe((count) => {
			set(downloadTaskCount, count);
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
