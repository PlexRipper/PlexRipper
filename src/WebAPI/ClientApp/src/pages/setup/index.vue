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
						<!-- Tabs -->
						<QRow align="start">
							<QCol cols="auto">
								<q-tabs
									v-model="stepIndex"
									active-color="primary"
									indicator-color="primary"
									vertical>
									<!-- Step headers	-->
									<template
										v-for="(header, index) in headers"
										:key="index">
										<q-tab
											:color="
												index + 1 === stepPagesCount ? 'green' : stepIndex > index + 1 ? 'green' : 'red'
											"
											:complete="index + 1 === stepPagesCount ? stepIndex > index : stepIndex > index + 1"
											:data-cy="`setup-header-tab-${index + 1}`"
											:label="header.name"
											:name="index + 1"
											class="setup-tab"
											edit-icon="$complete" />
										<q-separator
											v-if="index < stepPagesCount - 1"
											:key="index + 100" />
									</template>
								</q-tabs>
							</QCol>
							<QCol>
								<q-tab-panels
									v-model="stepIndex"
									animated
									class="fit q-pa-md"
									transition-next="slide-up"
									transition-prev="slide-down">
									<!-- Introduction	-->
									<q-tab-panel :name="1">
										<QSection>
											<template #header>
												<QText
													size="h4"
													align="center">
													{{ $t('pages.setup.intro.title') }}
												</QText>
											</template>
											<div class="q-pa-md">
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
											</div>
										</QSection>
									</q-tab-panel>
									<!-- Future plans!	-->
									<q-tab-panel :name="2">
										<QSection>
											<template #header>
												<QText
													size="h4"
													align="center"
													class="mt-2"
													:value="$t('pages.setup.future-plans.title')" />
											</template>
											<div class="q-pa-md">
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
												<QText
													size="h4"
													align="center"
													:value="$t('pages.setup.future-plans.text.p-2')" />
											</div>
										</QSection>
									</q-tab-panel>
									<!-- Checking paths	-->
									<q-tab-panel :name="3">
										<QSection>
											<template #header>
												<QText
													size="h4"
													align="center">
													{{ $t('pages.setup.paths.title') }}
												</QText>
											</template>
											<div class="q-pa-md">
												<FolderPathsOverview only-defaults />
											</div>
										</QSection>
									</q-tab-panel>
									<!-- Plex Accounts	-->
									<q-tab-panel :name="4">
										<QSection>
											<template #header>
												<QText
													size="h4"
													align="center"
													class="mt-2">
													{{ $t('pages.setup.accounts.title') }}
												</QText>
											</template>
											<div class="q-pa-md">
												<AccountOverview />
											</div>
										</QSection>
									</q-tab-panel>
									<!-- Finished	-->
									<q-tab-panel :name="5">
										<QSection>
											<template #header>
												<QText
													size="h4"
													align="center"
													class="mt-2"
													:value="$t('pages.setup.finished.title')" />
											</template>
											<div class="q-pa-md">
												<p>{{ $t('pages.setup.finished.text.p-1') }}</p>
												<q-list
													class="no-background"
													dense>
													<q-item
														v-for="(link, i) in links"
														:key="i"
														:href="link.link"
														target="_blank">
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
											</div>
										</QSection>
									</q-tab-panel>
								</q-tab-panels>
							</QCol>
						</QRow>
					</QCol>
					<!-- Stepper navigation bar	-->
					<QCol
						align-self="stretch"
						cols="12"
						style="max-height: 76px;">
						<q-separator class="q-mb-md" />
						<QRow
							justify="between"
							align="center"
							class="q-my-md">
							<!-- Language Selector -->
							<QCol cols="2">
								<LanguageSelect
									class="q-ml-md"
									dense />
							</QCol>
							<!-- Navigation buttons -->
							<QCol>
								<QRow
									justify="center"
									align="center">
									<QCol
										v-if="!isNextDisabled"
										class="q-mx-md"
										cols="3">
										<NavigationPreviousButton
											:disabled="isBackDisabled"
											cy="setup-page-previous-button"
											@click="back" />
									</QCol>
									<QCol
										v-if="!isNextDisabled"
										class="q-mx-md"
										cols="3">
										<NavigationNextButton
											:disabled="isNextDisabled"
											cy="setup-page-next-button"
											@click="next" />
									</QCol>
									<QCol
										v-else
										class="q-mx-md"
										cols="auto">
										<NavigationFinishSetupButton
											cy="setup-page-skip-setup-button"
											@click="finishSetup" />
									</QCol>
								</QRow>
							</QCol>
							<!--	Skip button	-->
							<QCol
								v-if="!isNextDisabled"
								class="q-mx-md"
								cols="auto">
								<NavigationSkipSetupButton
									:disabled="isNextDisabled"
									@click="dialogStore.openDialog(DialogType.SetupSkipConfirmationDialog)" />
								<ConfirmationDialog
									:name="DialogType.SetupSkipConfirmationDialog"
									:text="$t('confirmation.skip-setup.text')"
									:title="$t('confirmation.skip-setup.title')"
									@confirm="finishSetup" />
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
import { DialogType } from '@enums';
import QSection from '@components/Common/QSection.vue';
import { useSettingsStore, useDialogStore, useRouter, useI18n } from '#imports';

const { t } = useI18n();
const router = useRouter();
const settingsStore = useSettingsStore();
const dialogStore = useDialogStore();

const stepIndex = ref(1);
const stepPagesCount = ref(5);

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
