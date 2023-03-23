<template>
	<q-page>
		<q-section>
			<template #header>
				{{ $t('pages.settings.paths.main.header') }}
			</template>
			<paths-default-overview />
		</q-section>
		<q-section>
			<template #header>
				{{ $t('pages.settings.paths.movie.header') }}
			</template>
			<paths-custom-overview :folder-type="movieFolderType" />
		</q-section>
		<q-section>
			<template #header>
				{{ $t('pages.settings.paths.tv-show.header') }}
			</template>
			<paths-custom-overview :folder-type="tvShowFolderType" />
		</q-section>
	</q-page>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { FolderType } from '@dto/mainApi';

const movieFolderType = ref<FolderType>(FolderType.MovieFolder);
const tvShowFolderType = ref<FolderType>(FolderType.TvShowFolder);
</script>
