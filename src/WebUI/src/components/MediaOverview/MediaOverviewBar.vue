<template>
	<v-toolbar class="media-overview-bar no-background" :height="barHeight">
		<v-toolbar-title>
			<h3>{{ server ? server.name : '?' }} - {{ library ? library.title : '?' }}</h3>
		</v-toolbar-title>

		<v-spacer></v-spacer>
		<!--	Refresh library button	-->
		<vertical-button icon="mdi-refresh" label="Refresh" :height="barHeight" @click="refreshLibrary" />

		<!--	View mode	-->
		<v-menu left bottom offset-y>
			<template v-slot:activator="{ on, attrs }">
				<vertical-button v-bind="attrs" icon="mdi-eye" label="View" :height="barHeight" v-on="on" />
			</template>
			<!-- View mode options -->
			<v-list>
				<template v-for="(viewOption, i) in viewOptions">
					<v-list-item :key="i" @click="changeView(viewOption.viewMode)">
						<v-list-item-content>
							<v-list-item-title>{{ viewOption.label }}</v-list-item-title>
						</v-list-item-content>
						<!--	Is selected icon	-->
						<v-list-item-icon>
							<v-icon v-if="isSelected(viewOption.viewMode)">mdi-check</v-icon>
						</v-list-item-icon>
					</v-list-item>
				</template>
			</v-list>
		</v-menu>
	</v-toolbar>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import type { PlexLibraryDTO, PlexServerDTO } from '@dto/mainApi';
import { ViewMode } from '@dto/mainApi';
import VerticalButton from '@components/General/VerticalButton.vue';

interface IViewOptions {
	label: string;
	viewMode: ViewMode;
}

@Component({
	components: { VerticalButton },
})
export default class MediaOverviewBar extends Vue {
	@Prop({ required: true, type: Object as () => PlexServerDTO })
	readonly server!: PlexServerDTO;

	@Prop({ required: true, type: Object as () => PlexLibraryDTO })
	readonly library!: PlexLibraryDTO;

	@Prop({ required: true, type: String })
	readonly viewMode!: ViewMode;

	readonly barHeight: number = 85;

	refreshLibrary(): void {
		this.$emit('refresh-library', this.library.id);
	}

	changeView(viewMode: ViewMode): void {
		this.$emit('view-change', viewMode);
	}

	isSelected(viewMode: ViewMode): boolean {
		return this.viewMode === viewMode;
	}

	get viewOptions(): IViewOptions[] {
		return [
			{
				label: 'Poster View',
				viewMode: ViewMode.Poster,
			},
			{
				label: 'Table View',
				viewMode: ViewMode.Table,
			},
			// {
			// 	label: 'Overview',
			// 	viewMode: ViewMode.Overview,
			// },
		];
	}
}
</script>
