<template>
	<q-page>
		<!-- Logo	-->
		<q-row justify="center" no-gutters class="q-my-md" no-wrap>
			<q-col cols="auto">
				<logo :size="128" />
			</q-col>
		</q-row>
		<q-row justify="center">
			<q-col cols="auto" align-self="stretch">
				<q-row class="setup-card" column>
					<q-col align-self="stretch" cols="12">
						<!-- Tabs -->
						<q-row>
							<q-col cols="auto">
								<q-tabs v-model="stepIndex" vertical active-color="primary" indicator-color="primary">
									<!-- Step headers	-->
									<template v-for="(header, index) in headers" :key="index">
										<q-tab
											class="setup-tab"
											:name="index + 1"
											:color="
												index + 1 === stepPagesCount ? 'green' : stepIndex > index + 1 ? 'green' : 'red'
											"
											:complete="index + 1 === stepPagesCount ? stepIndex > index : stepIndex > index + 1"
											:label="$t(`pages.setup.${header}.header`)"
											edit-icon="$complete" />
										<q-separator v-if="index < stepPagesCount - 1" :key="index + 100" />
									</template>
								</q-tabs>
							</q-col>
							<q-col>
								<q-tab-panels
									v-model="stepIndex"
									animated
									class="fit q-pa-md"
									transition-prev="slide-down"
									transition-next="slide-up">
									<!-- Introduction	-->
									<q-tab-panel :name="1">
										<q-row no-gutters>
											<q-col>
												<h2 class="mt-2">{{ $t('pages.setup.intro.title') }}</h2>
												<p>{{ $t('pages.setup.intro.text.p-1') }}</p>
												<ul>
													<li>
														{{ $t('pages.setup.intro.list.item-1') }}
														<ExternalLinkButton
															href="https://github.com/PlexRipper/PlexRipper/issues" />
													</li>
													<li>{{ $t('pages.setup.intro.list.item-2') }}</li>
													<li>{{ $t('pages.setup.intro.list.item-3') }}</li>
													<li>{{ $t('pages.setup.intro.list.item-4') }}</li>
													<li>{{ $t('pages.setup.intro.list.item-5') }}</li>
												</ul>
											</q-col>
										</q-row>
									</q-tab-panel>
									<!-- Future plans!	-->
									<q-tab-panel :name="2">
										<q-row no-gutters>
											<q-col>
												<h2>{{ $t('pages.setup.future-plans.title') }}</h2>
												<p>{{ $t('pages.setup.future-plans.text.p-1') }}</p>
												<ul>
													<li>
														{{ $t('pages.setup.future-plans.list.item-1') }}
														<ul>
															<li>
																{{ $t('pages.setup.future-plans.list.item-1-1') }}
															</li>
														</ul>
													</li>
													<li>{{ $t('pages.setup.future-plans.list.item-2') }}</li>
													<li>{{ $t('pages.setup.future-plans.list.item-3') }}</li>
													<li>{{ $t('pages.setup.future-plans.list.item-4') }}</li>
													<li>
														{{ $t('pages.setup.future-plans.list.item-6') }}
														<ul>
															<li>
																{{ $t('pages.setup.future-plans.list.item-6-1') }}
															</li>
														</ul>
													</li>
												</ul>
												<h2 class="text-center">{{ $t('pages.setup.future-plans.text.p-2') }}</h2>
											</q-col>
										</q-row>
									</q-tab-panel>
									<!-- Checking paths	-->
									<q-tab-panel :name="3">
										<h2 class="mt-2">{{ $t('pages.setup.paths.title') }}</h2>
										<paths-default-overview />
									</q-tab-panel>
									<!-- Plex Accounts	-->
									<q-tab-panel :name="4">
										<h2 class="mt-2">{{ $t('pages.setup.accounts.title') }}</h2>
										<account-overview />
									</q-tab-panel>
									<!-- Finished	-->
									<q-tab-panel :name="5">
										<q-row no-gutters>
											<q-col>
												<h2 class="mt-2">{{ $t('pages.setup.finished.title') }}</h2>
											</q-col>
										</q-row>
										<q-row no-gutters>
											<q-col>
												<p>{{ $t('pages.setup.finished.text.p-1') }}</p>
												<q-list dense class="no-background">
													<q-item v-for="(link, i) in links" :key="i" :href="link" target="_blank">
														<q-item-section avatar>
															<ul>
																<li>
																	<span style="font-weight: normal">
																		{{ $t(`pages.setup.finished.list.item-${i + 1}`) }}
																	</span>
																</li>
															</ul>
														</q-item-section>
														<q-item-section side>
															<ExternalLinkButton :href="link" />
														</q-item-section>
													</q-item>
												</q-list>
											</q-col>
										</q-row>
									</q-tab-panel>
								</q-tab-panels>
							</q-col>
						</q-row>
					</q-col>
					<!-- Stepper navigation bar	-->
					<q-col align-self="stretch" cols="12">
						<q-separator class="q-mb-md" />
						<q-row align="center">
							<q-col cols="auto" align-self="start" class="q-mx-md">
								<DarkModeToggleButton />
							</q-col>
							<q-col>
								<q-row justify="center">
									<q-col v-if="!isNextDisabled" align-self="center" class="q-mx-md" cols="2">
										<NavigationPreviousButton
											:disabled="isBackDisabled"
											cy="setup-page-previous-button"
											@click="back" />
									</q-col>
									<q-col v-if="!isNextDisabled" align-self="center" class="q-mx-md" cols="2">
										<NavigationNextButton
											:disabled="isNextDisabled"
											cy="setup-page-next-button"
											@click="next" />
									</q-col>
									<q-col v-else align-self="center" cols="3">
										<NavigationFinishSetupButton cy="setup-page-skip-setup-button" @click="finishSetup" />
									</q-col>
								</q-row>
							</q-col>
							<!--	Skip button	-->
							<q-col v-if="!isNextDisabled" cols="auto" align-self="end" class="q-mx-md">
								<NavigationSkipSetupButton :disabled="isNextDisabled" @click="skipDialogOpen = true" />
								<confirmation-dialog
									text-id="skip-setup"
									:dialog="skipDialogOpen"
									@confirm="finishSetup"
									@cancel="skipDialogOpen = false" />
							</q-col>
						</q-row>
					</q-col>
				</q-row>
			</q-col>
		</q-row>
	</q-page>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import { useSubscription } from '@vueuse/rxjs';
import Log from 'consola';
import { SettingsService } from '@service';
import { useRouter } from '#imports';

const router = useRouter();

const stepIndex = ref(1);
const stepPagesCount = ref(5);
const skipDialogOpen = ref(false);

const headers = ref(['intro', 'future-plans', 'paths', 'accounts', 'finished']);

const links = ref([
	'https://github.com/PlexRipper/PlexRipper/',
	'https://github.com/PlexRipper/PlexRipper/issues',
	'https://www.plexripper.rocks/contributing/translating',
	'https://github.com/PlexRipper/PlexRipper/',
]);

const isBackDisabled = computed(() => {
	return stepIndex.value === 1;
});

const isNextDisabled = computed(() => {
	return stepIndex.value === stepPagesCount.value;
});

const next = () => {
	if (stepIndex.value < stepPagesCount.value) {
		stepIndex.value++;
	}
};

const back = () => {
	if (stepIndex.value > 1) {
		stepIndex.value--;
	}
};

const finishSetup = () => {
	useSubscription(
		SettingsService.updateGeneralSettings('firstTimeSetup', false).subscribe(() => {
			Log.info('Setup process is finished or skipped, redirecting to home page now and refreshing the page');
			router.push('/').then(() => {
				// Refresh the page when we go to the home page to make sure we get all new data.
				location.reload();
			});
		}),
	);
};
</script>
<style lang="scss">
@import '@/assets/scss/mixins.scss';

.setup-card {
	// min-height: 80vh;
	width: 60vw;
	border-radius: 15px;
	@extend .default-border;
}

.setup-tab {
	height: 12vh;
}

body {
	&.body--dark {
		.setup-card {
			background-color: $dark-background-color;
		}
	}

	&.body--light {
		.setup-card {
			background-color: $light-background-color;
		}
	}
}
</style>
