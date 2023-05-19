<template>
	<page-container>
		<p-section>
			<template #header> {{ $t('pages.settings.paths.main.header') }} </template>
			<paths-default-overview />
		</p-section>
		<p-section>
			<template #header> {{ $t('pages.settings.paths.movie.header') }} </template>
			<paths-custom-overview :folder-type="movieFolderType" />
		</p-section>
		<p-section>
			<template #header> {{ $t('pages.settings.paths.tv-show.header') }} </template>
			<paths-custom-overview :folder-type="tvShowFolderType" />
		</p-section>
	</page-container>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import { FolderType } from '@dto/mainApi';

@Component
export default class SettingsPaths extends Vue {
	moduleName: string = 'Settings Paths';
	movieFolderType: FolderType = FolderType.MovieFolder;
	tvShowFolderType: FolderType = FolderType.TvShowFolder;
}
</script>
