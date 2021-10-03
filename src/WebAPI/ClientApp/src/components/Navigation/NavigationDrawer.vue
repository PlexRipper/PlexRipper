<template>
	<v-navigation-drawer
		ref="drawer"
		:value="showDrawer"
		:permanent="showDrawer"
		:width="width"
		app
		clipped
		class="navigation-drawer no-background"
	>
		<!-- Server drawer -->
		<server-drawer />
		<!-- Menu items -->
		<template #append>
			<v-list>
				<template v-for="(item, i) in getNavItems">
					<!-- Grouped nav items -->
					<v-list-group v-if="item.children && item.children.length > 0" :key="item.title" color="">
						<template #activator>
							<v-list-item-icon>
								<v-icon>{{ item.icon }}</v-icon>
							</v-list-item-icon>
							<v-list-item-title>{{ $t(item.title) }}</v-list-item-title>
						</template>
						<v-list-item v-for="(child, j) in item.children" :key="j" link nuxt :to="child.link">
							<v-list-item-icon>
								<v-icon>{{ child.icon }}</v-icon>
							</v-list-item-icon>

							<v-list-item-content>
								<v-list-item-title>{{ $t(child.title) }}</v-list-item-title>
							</v-list-item-content>
						</v-list-item>
					</v-list-group>
					<!-- Single nav item  -->
					<v-list-item v-else :key="i" link nuxt :to="item.link">
						<v-list-item-icon>
							<v-icon>{{ item.icon }}</v-icon>
						</v-list-item-icon>

						<v-list-item-content>
							<v-list-item-title>
								{{ $t(item.title) }}
							</v-list-item-title>
						</v-list-item-content>
						<v-list-item-action v-if="item.title.includes('downloads')">
							<v-avatar v-if="downloadTaskCount > 0" class="red" size="32">
								<b>{{ downloadTaskCount }}</b>
							</v-avatar>
						</v-list-item-action>
					</v-list-item>
				</template>
			</v-list>
		</template>
	</v-navigation-drawer>
</template>

<script lang="ts">
import { Component, Prop, Ref, Vue } from 'vue-property-decorator';
import { DownloadService } from '@service';
import VNavigationDrawer from 'vuetify/lib/components/VNavigationDrawer/VNavigationDrawer.js';

interface INavItem {
	title: string;
	icon: string;
	link: string;
	children?: INavItem[];
}

@Component
export default class NavigationDrawer extends Vue {
	items: object[] = [];
	downloadTaskCount = 0;
	width: Number = 350;
	borderSize: Number = 3;

	@Prop({ required: true, type: Boolean })
	readonly showDrawer!: boolean;

	@Ref('drawer')
	readonly drawer!: VNavigationDrawer;

	get getNavItems(): INavItem[] {
		return [
			{
				title: 'components.navigation-drawer.downloads',
				icon: 'mdi-download',
				link: '/downloads',
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
	}

	setBorderWidth(): void {
		const i = this.drawer.$el.querySelector<HTMLElement>('.v-navigation-drawer__border');
		if (i) {
			i.style.width = this.borderSize + 'px';
			i.style.cursor = 'ew-resize';
		}
	}

	setEvents() {
		// https://codepen.io/oze4/pen/mojrZM
		const minSize = this.borderSize;
		const el = this.drawer.$el;
		const drawerBorder = el.querySelector<HTMLElement>('.v-navigation-drawer__border');
		const vm = this;

		function resize(e) {
			document.body.style.cursor = 'ew-resize';
			if (e.clientX > 300) {
				el.style.width = e.clientX + 'px';
			}
		}

		if (drawerBorder) {
			drawerBorder.addEventListener(
				'mousedown',
				function (e) {
					if (e.offsetX < minSize) {
						// m_pos = e.x;
						el.style.transition = 'initial';
						document.addEventListener('mousemove', resize, false);
					}
				},
				false,
			);
		}

		document.addEventListener(
			'mouseup',
			() => {
				if (drawerBorder) {
					el.style.transition = '';
					vm.width = +el.style.width;
				}
				document.body.style.cursor = '';
				document.removeEventListener('mousemove', resize, false);
			},
			false,
		);
	}

	mounted(): void {
		// this.setBorderWidth();
		// this.setEvents();
		this.$subscribeTo(DownloadService.getDownloadList(), (downloadTasks) => {
			this.downloadTaskCount = downloadTasks?.length ?? -1;
		});
	}
}
</script>
