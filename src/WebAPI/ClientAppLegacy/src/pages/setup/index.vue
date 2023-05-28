<template>
	<v-container class="pa-0" style="max-width: 900px">
		<!-- Logo	-->
		<v-row justify="center" no-gutters>
			<v-col cols="auto">
				<logo :size="128" />
			</v-col>
		</v-row>
		<!--	Steppers	-->
		<v-row>
			<v-col>
				<v-stepper id="stepper-main" v-model="stepIndex" non-linear>
					<!-- Step headers	-->
					<v-stepper-header>
						<template v-for="(header, i) in headers">
							<v-stepper-step
								:key="i"
								:step="i + 1"
								:color="i + 1 === stepPagesCount ? 'green' : stepIndex > i + 1 ? 'green' : 'red'"
								:complete="i + 1 === stepPagesCount ? stepIndex > i : stepIndex > i + 1"
								editable
								edit-icon="$complete"
							>
								{{ $t(`pages.setup.${header}.header`) }}
							</v-stepper-step>
							<v-divider v-if="i < stepPagesCount - 1" :key="i + 100" />
						</template>
					</v-stepper-header>

					<!-- Step pages	-->
					<v-stepper-items>
						<!-- Introduction	-->
						<v-stepper-content class="stepper-content" :step="1">
							<v-container fluid>
								<v-row no-gutters>
									<v-col>
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
									</v-col>
								</v-row>
							</v-container>
						</v-stepper-content>

						<!-- Future plans!	-->
						<v-stepper-content class="stepper-content" :step="2">
							<v-container fluid>
								<v-row no-gutters>
									<v-col>
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
									</v-col>
								</v-row>
							</v-container>
						</v-stepper-content>

						<!-- Checking paths	-->
						<v-stepper-content class="stepper-content" :step="3">
							<h2 class="mt-2">{{ $t('pages.setup.paths.title') }}</h2>
							<paths-default-overview />
						</v-stepper-content>

						<!-- Plex Accounts	-->
						<v-stepper-content class="stepper-content" :step="4">
							<h2 class="mt-2">{{ $t('pages.setup.accounts.title') }}</h2>
							<account-overview />
						</v-stepper-content>

						<!-- Finished	-->
						<v-stepper-content class="stepper-content" :step="5">
							<v-container fluid>
								<v-row no-gutters>
									<v-col>
										<h2 class="mt-2">{{ $t('pages.setup.finished.title') }}</h2>
									</v-col>
								</v-row>
								<v-row no-gutters>
									<v-col>
										<p>{{ $t('pages.setup.finished.text.p-1') }}</p>
										<v-list dense class="no-background">
											<v-list-item v-for="(link, i) in links" :key="i" :href="link" target="_blank">
												<v-list-item-title>
													<ul>
														<li>
															<span v-if="messages" style="font-weight: normal">
																{{ messages['finished'].list['item-' + (i + 1)] }}
															</span>
														</li>
													</ul>
												</v-list-item-title>
												<v-list-item-action>
													<ExternalLinkButton :href="link" />
												</v-list-item-action>
											</v-list-item>
										</v-list>
									</v-col>
								</v-row>
							</v-container>
						</v-stepper-content>
					</v-stepper-items>
					<v-divider />
					<!-- Stepper navigation bar	-->
					<navigation-bar
						:disable-back="isBackDisabled"
						:disable-next="isNextDisabled"
						:is-last="isNextDisabled"
						@back="back"
						@next="next"
						@finish="finishSetup"
					/>
				</v-stepper>
			</v-col>
		</v-row>

		<!--	Skip button	-->
		<v-row justify="center">
			<v-col cols="3">
				<NavigationSkipSetupButton :disabled="isNextDisabled" :width="100" @click="skipDialogOpen = true" />
				<confirmation-dialog
					text-id="skip-setup"
					:dialog="skipDialogOpen"
					@confirm="finishSetup"
					@cancel="skipDialogOpen = false"
				/>
			</v-col>
		</v-row>
	</v-container>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import { useSubscription } from '@vueuse/rxjs';
import Log from 'consola';
import { SettingsService } from '@service';

@Component
export default class Setup extends Vue {
	stepIndex: number = 1;
	stepPagesCount: number = 5;
	skipDialogOpen: boolean = false;

	links: string[] = [
		'https://github.com/PlexRipper/PlexRipper/',
		'https://github.com/PlexRipper/PlexRipper/issues',
		'https://github.com/PlexRipper/PlexRipper#translate-plexripper',
		'https://github.com/PlexRipper/PlexRipper/',
	];

	back(): void {
		if (this.stepIndex > 1) {
			this.stepIndex--;
		}
	}

	next(): void {
		if (this.stepIndex < this.stepPagesCount) {
			this.stepIndex++;
		}
	}

	finishSetup(): void {
		useSubscription(
			SettingsService.updateGeneralSettings('firstTimeSetup', false).subscribe(() => {
				Log.info('Setup process is finished or skipped, redirecting to home page now and refreshing the page');
				this.$router.push('/', () => {
					// Refresh the page when we go to the home page to make sure we get all new data.
					location.reload();
				});
			}),
		);
	}

	get headers(): string[] {
		return ['intro', 'future-plans', 'paths', 'accounts', 'finished'];
	}

	get isBackDisabled(): boolean {
		return this.stepIndex === 1;
	}

	get isNextDisabled(): boolean {
		return this.stepIndex === this.stepPagesCount;
	}

	get messages(): any {
		return this.$getMessage('pages.setup');
	}
}
</script>
