<template>
	<q-drawer
		class="navigation-drawer"
		:modelValue="showDrawer"
		:width="400"
		bordered
		style="overflow-x: hidden;"
	>
		<q-col class="server-drawer-container">
			<q-scroll>
				<!-- Server drawer -->
				<server-drawer/>
			</q-scroll>
		</q-col>
		<q-col class="menu-items">
			<q-separator/>
			<!-- Menu items -->
			<q-expansion-list :items="getNavItems"/>
		</q-col>
	</q-drawer>
</template>

<script setup lang="ts">
import {useSubscription} from '@vueuse/rxjs';
import {DownloadService} from '@service';
import {QExpansionListProps} from "@interfaces/components/QExpansionListProps";
import {computed} from "#imports";

interface INavItem {
	title: string;
	icon: string;
	link: string;
	children?: INavItem[];
}


const items = ref<object[]>([]);

const downloadTaskCount = ref(0);

const width = ref(350);

const borderSize = ref(3);

const props = withDefaults(defineProps<{ showDrawer: boolean }>(), {
	showDrawer: false,
});

// @Ref('drawer')
// readonly drawer!: VNavigationDrawer;

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
	];
});

onMounted(() => {
	items.value = getNavItems.value;
	// this.setBorderWidth();
	// this.setEvents();
	useSubscription(
		DownloadService.getDownloadList().subscribe((downloadTasks) => {
			downloadTaskCount.value = downloadTasks.flat(4)?.length ?? -1;
		}),
	);
});


</script>

<style lang="scss">
@import "./src/assets/scss/_variables.scss";

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

.q-drawer {
	background-color: transparent;
}

</style>
