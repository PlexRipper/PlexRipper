<template>
	<page-container>
		<v-row v-if="firstTimeSetup">
			<v-col cols="12">
				<h2>{{ $t('pages.home.setup-question') }}</h2>
				<v-row justify="center">
					<v-col cols="3">
						<NavigationSkipSetupButton :block="true" @click="skipSetup()" />
					</v-col>
					<v-col cols="3">
						<GoToButton text-id="go-to-setup-page" :block="true" to="/setup" color="green" />
					</v-col>
				</v-row>
			</v-col>
		</v-row>
		<v-row v-else>
			<v-col>
				<h1>{{ $t('pages.home.header') }}</h1>
			</v-col>
		</v-row>
	</page-container>
</template>

<script lang="ts">
import Log from 'consola';
import { Component, Vue } from 'vue-property-decorator';
import { useSubscription } from '@vueuse/rxjs';
import { SettingsService } from '@service';

@Component<Home>({})
export default class Home extends Vue {
	firstTimeSetup: boolean = false;

	skipSetup(): void {
		useSubscription(
			SettingsService.updateGeneralSettings('firstTimeSetup', false).subscribe(() => {
				Log.info('Setup process skipped');
			}),
		);
	}

	mounted() {
		useSubscription(
			SettingsService.getFirstTimeSetup().subscribe((state) => {
				this.firstTimeSetup = state;
			}),
		);
	}
}
</script>
