<template>
	<QPage>
		<!-- Logo	-->
		<QRow
			justify="center"
			no-gutters
			no-wrap>
			<QCol
				class="q-my-md"
				cols="auto">
				<Logo :size="128" />
			</QCol>
		</QRow>
		<!--	Horizontal Container	-->
		<QRow justify="center">
			<QCol
				cols="12"
				lg="8">
				<!--	Vertical Container	-->
				<QRow
					class="setup-card"
					column>
					<QCol align-self="stretch">
						<QRow align="start">
							<!-- Tabs -->
							<QCol cols="2">
								<SetupTabs
									v-model="stepIndex"
									:headers="headers" />
							</QCol>
							<QCol>
								<!-- Panels -->
								<q-tab-panels
									v-model="stepIndex"
									animated
									class="fit q-pa-md"
									transition-next="slide-up"
									transition-prev="slide-down">
									<DisclaimerSetupPanel :name="1" />
									<!-- Introduction	-->
									<IntroductionSetupPanel :name="2" />
									<!-- Authorization	-->
									<AuthorizationSetupPanel :name="3" />
									<!-- Checking paths	-->
									<FolderOverviewSetupPanel :name="4" />
									<!-- Plex Accounts	-->
									<PlexAccountsSetupPanel :name="5" />
									<!-- Finished	-->
									<FinishSetupPanel :name="6" />
								</q-tab-panels>
							</QCol>
						</QRow>
					</QCol>
					<!-- Stepper navigation bar	-->
					<SetupFooter
						v-model="stepIndex"
						:max-pages="headers.length"
						@finish="finishSetup" />
				</QRow>
			</QCol>
		</QRow>
	</QPage>
</template>

<script lang="ts" setup>
import Log from 'consola';
import { useSettingsStore, useRouter, useI18n } from '#imports';

const { t } = useI18n();
const router = useRouter();
const settingsStore = useSettingsStore();

const stepIndex = ref(1);

const headers = ref([
	{ name: t('pages.setup.disclaimer.header') },
	{ name: t('pages.setup.intro.header') },
	{ name: t('pages.setup.authorization.header') },
	{ name: t('pages.setup.paths.header') },
	{ name: t('pages.setup.accounts.header') },
	{ name: t('pages.setup.finished.header') }]);

const finishSetup = () => {
	settingsStore.generalSettings.firstTimeSetup = false;
	Log.info('Setup process is finished or skipped, redirecting to home page now and refreshing the page');
	router.push('/').then(() => {
		// Refresh the page when we go to the home page to make sure we get all new data.
		location.reload();
	});
};
</script>

<style lang="scss">
@use '@/assets/scss/variables' as *;
@use '@/assets/scss/_mixins.scss';

.setup-card {
  @extend .default-border;
  @extend .default-border-radius;
}

.setup-tab {
  height: 12vh;
}

body {
  &.body--dark {
    .setup-card {
      background-color: $dark-xl-background-color;
    }
  }

  &.body--light {
    .setup-card {
      background-color: $light-xl-background-color;
    }
  }
}
</style>
