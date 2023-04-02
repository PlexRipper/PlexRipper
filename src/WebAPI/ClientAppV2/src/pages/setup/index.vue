<template>
	<q-page>
		<!-- Logo	-->
		<q-row justify="center" no-gutters>
			<q-col cols="auto">
				<logo :size="128" />
			</q-col>
		</q-row>
		<!--	Steppers	-->
		<q-row>
			<q-col>
				<!-- Step headers	-->
				<q-stepper id="stepper-main" v-model="stepIndex" non-linear>
					<!-- Introduction	-->
					<q-step :name="1" :title="$t(`pages.setup.intro.header`)">
						<q-row no-gutters>
							<q-col>
								<h2 class="mt-2">{{ $t('pages.setup.intro.title') }}</h2>
								<p>{{ $t('pages.setup.intro.text.p-1') }}</p>
								<ul>
									<li>
										{{ $t('pages.setup.intro.list.item-1') }}
										<ExternalLinkButton href="https://github.com/PlexRipper/PlexRipper/issues" />
									</li>
									<li>{{ $t('pages.setup.intro.list.item-2') }}</li>
									<li>{{ $t('pages.setup.intro.list.item-3') }}</li>
									<li>{{ $t('pages.setup.intro.list.item-4') }}</li>
									<li>{{ $t('pages.setup.intro.list.item-5') }}</li>
								</ul>
							</q-col>
						</q-row>
					</q-step>
					<!-- Future plans!	-->
					<q-step :name="2" :title="$t(`pages.setup.future-plans.header`)">
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
									<li>{{ $t('pages.setup.future-plans.list.item-5') }}</li>
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
					</q-step>
					<!-- Checking paths	-->
					<q-step :name="3" :title="$t(`pages.setup.paths.header`)">
						<h2 class="mt-2">{{ $t('pages.setup.paths.title') }}</h2>
						<paths-default-overview />
					</q-step>
					<!-- Plex Accounts	-->
					<q-step :name="4" :title="$t(`pages.setup.accounts.header`)">
						<h2 class="mt-2">{{ $t('pages.setup.accounts.title') }}</h2>
						<account-overview />
					</q-step>
					<!-- Finished	-->
					<q-step :name="5" :title="$t(`pages.setup.finished.header`)">
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
													<span v-if="messages" style="font-weight: normal">
														{{ messages['finished'].list['item-' + (i + 1)] }}
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
					</q-step>
					<!--					<q-stepper-header>-->
					<!--						<template v-for="(header, i) in headers">-->
					<!--							<v-stepper-step-->
					<!--								:key="i"-->
					<!--								:step="i + 1"-->
					<!--								:color="i + 1 === stepPagesCount ? 'green' : stepIndex > i + 1 ? 'green' : 'red'"-->
					<!--								:complete="i + 1 === stepPagesCount ? stepIndex > i : stepIndex > i + 1"-->
					<!--								editable-->
					<!--								edit-icon="$complete">-->
					<!--								{{ $t(`pages.setup.${header}.header`) }}-->
					<!--							</v-stepper-step>-->
					<!--							<q-separator v-if="i < stepPagesCount - 1" :key="i + 100" />-->
					<!--						</template>-->
					<!--					</q-stepper-header>-->

					<!-- Step pages	-->
				</q-stepper>
			</q-col>
		</q-row>
		<q-row>
			<q-col>
				<q-separator />
				<!-- Stepper navigation bar	-->
				<navigation-bar
					:disable-back="isBackDisabled"
					:disable-next="isNextDisabled"
					:is-last="isNextDisabled"
					@back="back"
					@next="next"
					@finish="finishSetup" />
			</q-col>
		</q-row>
		<!--	Skip button	-->
		<q-row justify="center">
			<q-col cols="3">
				<NavigationSkipSetupButton :disabled="isNextDisabled" :width="100" @click="skipDialogOpen = true" />
				<confirmation-dialog
					text-id="skip-setup"
					:dialog="skipDialogOpen"
					@confirm="finishSetup"
					@cancel="skipDialogOpen = false" />
			</q-col>
		</q-row>
	</q-page>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import { useSubscription } from '@vueuse/rxjs';
import Log from 'consola';
import { SettingsService } from '@service';
import { getMessage, useRouter } from '#imports';

const router = useRouter();

const stepIndex = ref(1);
const stepPagesCount = ref(5);
const skipDialogOpen = ref(false);

const links = ref([
	'https://github.com/PlexRipper/PlexRipper/',
	'https://github.com/PlexRipper/PlexRipper/issues',
	'https://github.com/PlexRipper/PlexRipper#translate-plexripper',
	'https://github.com/PlexRipper/PlexRipper/',
]);

const isBackDisabled = computed(() => {
	return stepIndex.value === 1;
});

const isNextDisabled = computed(() => {
	return stepIndex.value === stepPagesCount.value;
});

const messages = computed(() => {
	return getMessage('pages.setup');
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
				// location.reload();
			});
		}),
	);
};
</script>
