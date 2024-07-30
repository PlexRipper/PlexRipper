<template>
	<QPage>
		<!-- Logo	-->
		<QRow
			justify="center"
			no-gutters
			no-wrap
		>
			<QCol
				class="q-my-md"
				cols="auto"
			>
				<Logo :size="128" />
			</QCol>
		</QRow>
		<!--	Horizontal Container	-->
		<QRow justify="center">
			<QCol
				cols="12"
				lg="8"
			>
				<!--	Vertical Container	-->
				<QRow
					class="setup-card"
					column
				>
					<QCol align-self="stretch">
						<!-- Tabs -->
						<QRow align="start">
							<QCol cols="auto">
								<q-tabs
									v-model="stepIndex"
									active-color="primary"
									indicator-color="primary"
									vertical
								>
									<!-- Step headers	-->
									<template
										v-for="(header, index) in headers"
										:key="index"
									>
										<q-tab
											:color="
												index + 1 === stepPagesCount ? 'green' : stepIndex > index + 1 ? 'green' : 'red'
											"
											:complete="index + 1 === stepPagesCount ? stepIndex > index : stepIndex > index + 1"
											:data-cy="`setup-header-tab-${index + 1}`"
											:label="header.name"
											:name="index + 1"
											class="setup-tab"
											edit-icon="$complete"
										/>
										<q-separator
											v-if="index < stepPagesCount - 1"
											:key="index + 100"
										/>
									</template>
								</q-tabs>
							</QCol>
							<QCol>
								<q-tab-panels
									v-model="stepIndex"
									animated
									class="fit q-pa-md"
									transition-next="slide-up"
									transition-prev="slide-down"
								>
									<!-- Introduction	-->
									<q-tab-panel :name="1">
										<QRow no-gutters>
											<QCol>
												<h2>{{ t('pages.setup.intro.title') }}</h2>
												<p>{{ t('pages.setup.intro.text.p-1') }}</p>
												<ul>
													<li>
														{{ t('pages.setup.intro.list.item-1') }}
														<ExternalLinkButton
															href="https://github.com/PlexRipper/PlexRipper/issues"
														/>
													</li>
													<li>{{ t('pages.setup.intro.list.item-2') }}</li>
													<li>{{ t('pages.setup.intro.list.item-3') }}</li>
													<li>{{ t('pages.setup.intro.list.item-4') }}</li>
													<li>{{ t('pages.setup.intro.list.item-5') }}</li>
												</ul>
											</QCol>
										</QRow>
									</q-tab-panel>
									<!-- Future plans!	-->
									<q-tab-panel :name="2">
										<QRow no-gutters>
											<QCol>
												<h2 class="mt-2">
													{{ t('pages.setup.future-plans.title') }}
												</h2>
											</QCol>
										</QRow>
										<QRow no-gutters>
											<QCol>
												<p>{{ t('pages.setup.future-plans.text.p-1') }}</p>
												<ul>
													<li>
														{{ t('pages.setup.future-plans.list.item-1') }}
														<ul>
															<li>
																{{ t('pages.setup.future-plans.list.item-1-1') }}
															</li>
														</ul>
													</li>
													<li>{{ t('pages.setup.future-plans.list.item-2') }}</li>
													<li>{{ t('pages.setup.future-plans.list.item-3') }}</li>
													<li>{{ t('pages.setup.future-plans.list.item-4') }}</li>
													<li>
														{{ t('pages.setup.future-plans.list.item-6') }}
														<ul>
															<li>
																{{ t('pages.setup.future-plans.list.item-6-1') }}
															</li>
														</ul>
													</li>
												</ul>
												<h2 class="text-center">
													{{ t('pages.setup.future-plans.text.p-2') }}
												</h2>
											</QCol>
										</QRow>
									</q-tab-panel>
									<!-- Checking paths	-->
									<q-tab-panel :name="3">
										<h2>
											{{ t('pages.setup.paths.title') }}
										</h2>
										<QRow no-gutters>
											<QCol align-self="stretch">
												<FolderPathsOverview only-defaults />
											</QCol>
										</QRow>
									</q-tab-panel>
									<!-- Plex Accounts	-->
									<q-tab-panel :name="4">
										<h2 class="mt-2">
											{{ t('pages.setup.accounts.title') }}
										</h2>
										<AccountOverview />
									</q-tab-panel>
									<!-- Finished	-->
									<q-tab-panel :name="5">
										<QRow no-gutters>
											<QCol>
												<h2 class="mt-2">
													{{ t('pages.setup.finished.title') }}
												</h2>
											</QCol>
										</QRow>
										<QRow no-gutters>
											<QCol>
												<p>{{ t('pages.setup.finished.text.p-1') }}</p>
												<q-list
													class="no-background"
													dense
												>
													<q-item
														v-for="(link, i) in links"
														:key="i"
														:href="link.link"
														target="_blank"
													>
														<q-item-section avatar>
															<ul>
																<li>
																	<span style="font-weight: normal">
																		{{ link.text }}
																	</span>
																</li>
															</ul>
														</q-item-section>
														<q-item-section side>
															<ExternalLinkButton :href="link.link" />
														</q-item-section>
													</q-item>
												</q-list>
											</QCol>
										</QRow>
									</q-tab-panel>
								</q-tab-panels>
							</QCol>
						</QRow>
					</QCol>
					<!-- Stepper navigation bar	-->
					<QCol
						align-self="stretch"
						cols="12"
					>
						<q-separator class="q-mb-md" />
						<QRow align="center">
							<QCol>
								<QRow justify="center">
									<QCol
										v-if="!isNextDisabled"
										class="q-mx-md"
										cols="2"
									>
										<NavigationPreviousButton
											:disabled="isBackDisabled"
											cy="setup-page-previous-button"
											@click="back"
										/>
									</QCol>
									<QCol
										v-if="!isNextDisabled"
										class="q-mx-md"
										cols="2"
									>
										<NavigationNextButton
											:disabled="isNextDisabled"
											cy="setup-page-next-button"
											@click="next"
										/>
									</QCol>
									<QCol
										v-else
										class="q-mx-md q-mb-md"
										cols="auto"
									>
										<NavigationFinishSetupButton
											cy="setup-page-skip-setup-button"
											@click="finishSetup"
										/>
									</QCol>
								</QRow>
							</QCol>
							<!--	Skip button	-->
							<QCol
								v-if="!isNextDisabled"
								class="q-mx-md"
								cols="auto"
							>
								<NavigationSkipSetupButton
									:disabled="isNextDisabled"
									@click="useOpenControlDialog(confirmationDialogName)"
								/>
								<ConfirmationDialog
									:name="confirmationDialogName"
									:text="$t('confirmation.skip-setup.text')"
									:title="$t('confirmation.skip-setup.title')"
									@confirm="finishSetup"
								/>
							</QCol>
						</QRow>
					</QCol>
				</QRow>
			</QCol>
		</QRow>
	</QPage>
</template>

<script lang="ts" setup>
import Log from 'consola';
import { useRouter, useOpenControlDialog } from '#imports';
import { useSettingsStore } from '~/store';

const router = useRouter();
const { t } = useI18n();
const settingsStore = useSettingsStore();

const stepIndex = ref(1);
const stepPagesCount = ref(5);

const confirmationDialogName = 'skip-setup-confirmation';
const headers = ref([{ name: t(`pages.setup.intro.header`) },
	{ name: t(`pages.setup.future-plans.header`) },
	{ name: t(`pages.setup.paths.header`) },
	{ name: t(`pages.setup.accounts.header`) },
	{ name: t(`pages.setup.finished.header`) }]);

const links = ref([{
	link: 'https://github.com/PlexRipper/PlexRipper/',
	text: t('pages.setup.finished.list.item-1'),
},
{
	link: 'https://github.com/PlexRipper/PlexRipper/issues',
	text: t('pages.setup.finished.list.item-2'),
},
{
	link: 'https://www.plexripper.rocks/contributing/translating',
	text: t('pages.setup.finished.list.item-3'),
},
{
	link: 'https://github.com/PlexRipper/PlexRipper/',
	text: t('pages.setup.finished.list.item-4'),
},
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
	settingsStore.generalSettings.firstTimeSetup = false;
	Log.info('Setup process is finished or skipped, redirecting to home page now and refreshing the page');
	router.push('/').then(() => {
		// Refresh the page when we go to the home page to make sure we get all new data.
		location.reload();
	});
};
</script>

<style lang="scss">
@import '@/assets/scss/mixins.scss';

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
