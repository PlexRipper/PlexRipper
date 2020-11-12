<template>
	<v-container>
		<paths-overview />
	</v-container>
</template>

<script lang="ts">
import { Vue, Component } from 'vue-property-decorator';
import PathsOverview from '@overviews/PathsOverview.vue';

@Component({
	components: {
		PathsOverview,
	},
})
export default class SettingsPaths extends Vue {
	moduleName: string = 'Settings Paths';
}
</script>
