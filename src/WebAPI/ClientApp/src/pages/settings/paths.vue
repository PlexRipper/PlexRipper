<template>
	<page>
		<p-section>
			<template #header> {{ $t('pages.settings.paths.header') }} </template>
			<paths-overview />
		</p-section>
	</page>
</template>

<script lang="ts">
import { Vue, Component } from 'vue-property-decorator';
import PathsOverview from '@overviews/PathsOverview.vue';
import Page from '@components/General/Page.vue';

@Component({
	components: {
		PathsOverview,
		Page,
	},
})
export default class SettingsPaths extends Vue {
	moduleName: string = 'Settings Paths';
}
</script>
