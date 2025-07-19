<template>
	<q-menu
		:offset="[0, 12]"
		@hide="menuIndex = MediaMetaDataTypes.None">
		<q-list
			style="min-width: 300px">
			<!-- Categories -->
			<template v-if="menuIndex === MediaMetaDataTypes.None">
				<q-item
					v-close-popup
					clickable
					@click="clearMetadataFilter()">
					<q-item-section>{{ t('components.media-filter-menu.all') }}</q-item-section>
				</q-item>
				<q-separator />
				<q-item
					v-for="(item, index) in menuItems"
					:key="index"
					clickable
					@click="onMenuOpen(item.type)">
					<q-item-section>{{ item.text }}</q-item-section>
				</q-item>
			</template>
			<template v-else>
				<q-item
					clickable
					@click="goBackToMainMenu()">
					<q-item-section avatar>
						<q-icon
							size="2em"
							name="mdi-chevron-left" />
					</q-item-section>
					<q-item-section>
						{{ menuItems.find((item) => item.type === menuIndex)?.text }}
					</q-item-section>
					<q-item-section side>
						<IconButton
							icon="mdi-magnify"
							@click.stop="toggleMenuSearch" />
					</q-item-section>
				</q-item>
				<!-- Search Input -->
				<q-item
					v-if="showMenuSearch"
					clickable>
					<q-input
						v-model="menuFilterQuery"
						color="red"
						class="full-width"
						dense />
				</q-item>
				<q-separator />
				<QScroll
					:fit="false"
					:height="'300px'">
					<!-- Show Genres Sub-Menu -->
					<template v-if="menuIndex === MediaMetaDataTypes.Genres">
						<q-item
							v-for="genre in mediaOverviewStore.getGenres.filter(x => x.name.toLowerCase().includes(menuFilterQuery.toLowerCase()))"
							:key="genre.id"
							clickable
							@click="setMetadataFilter({ genreId: genre.id })">
							<q-item-section avatar>
								<q-icon
									v-if="genre.id === mediaOverviewStore.metadata.genreId"
									name="mdi-check" />
							</q-item-section>
							<q-item-section>
								{{ genre.name }}
							</q-item-section>
						</q-item>
					</template>

					<!-- Show Countries Sub-Menu -->
					<template v-if="menuIndex === MediaMetaDataTypes.Country">
						<q-item
							v-for="country in mediaOverviewStore.getCountries.filter(x => x.name.toLowerCase().includes(menuFilterQuery.toLowerCase()))"
							:key="country.id"
							clickable
							@click="setMetadataFilter({ countryId: country.id })">
							<q-item-section avatar>
								<q-icon
									v-if="country.id === mediaOverviewStore.metadata.countryId"
									name="mdi-check" />
							</q-item-section>
							<q-item-section>{{ country.name }}</q-item-section>
						</q-item>
					</template>

					<!-- Show Roles Sub-Menu -->
					<template v-if="menuIndex === MediaMetaDataTypes.Roles">
						<q-item
							v-for="role in mediaOverviewStore.getRoles.filter(x => x.name.toLowerCase().includes(menuFilterQuery.toLowerCase()))"
							:key="role.id"
							clickable
							@click="setMetadataFilter({ roleId: role.id })">
							<q-item-section avatar>
								<q-icon
									v-if="role.id === mediaOverviewStore.metadata.roleId"
									name="mdi-check" />
							</q-item-section>
							<q-item-section>{{ role.name }}</q-item-section>
						</q-item>
					</template>
				</QScroll>
			</template>
		</q-list>
	</q-menu>
</template>

<script setup lang="ts">
import { get, set } from '@vueuse/core';
import { useMediaOverviewStore } from '@store';
import { MediaMetaDataTypes } from '@enums';
import IconButton from '@components/Buttons/IconButton.vue';

const menuIndex = ref<MediaMetaDataTypes>(MediaMetaDataTypes.None);
const mediaOverviewStore = useMediaOverviewStore();
const { t } = useI18n();

const showMenuSearch = ref(false);
const menuFilterQuery = ref<string>('');

withDefaults(defineProps<{
	libraryId?: number;
}>(), {
	libraryId: 0,
});

const menuItems: { text: string; type: MediaMetaDataTypes }[] = [
	{
		text: t('components.media-filter-menu.meta-data.country'),
		type: MediaMetaDataTypes.Country,
	},
	{
		text: t('components.media-filter-menu.meta-data.genre'),
		type: MediaMetaDataTypes.Genres,
	},
	{
		text: t('components.media-filter-menu.meta-data.roles'),
		type: MediaMetaDataTypes.Roles,
	},
];

function onMenuOpen(category: MediaMetaDataTypes) {
	set(menuIndex, category);
}

function toggleMenuSearch() {
	set(showMenuSearch, !get(showMenuSearch));
	set(menuFilterQuery, '');
}

function goBackToMainMenu() {
	set(menuIndex, MediaMetaDataTypes.None);
	set(showMenuSearch, false);
	set(menuFilterQuery, '');
}

function setMetadataFilter({
	countryId,
	roleId,
	genreId,
}: {
	countryId?: number;
	roleId?: number;
	genreId?: number;
}) {
	useSubscription(
		mediaOverviewStore.setMetaData({
			countryId,
			roleId,
			genreId,
		}).subscribe());
}

function clearMetadataFilter() {
	useSubscription(mediaOverviewStore.clearMetaDataFilter().subscribe());
}
</script>
