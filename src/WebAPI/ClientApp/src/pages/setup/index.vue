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
								{{ header }}
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
										<h2 class="mt-2">{{ $t('pages.setup.page-1.title') }}</h2>
										<p>{{ $t('pages.setup.page-1.text.p-1') }}</p>
										<ul>
											<li>
												{{ $t('pages.setup.page-1.list.item-1') }}
												<external-link href="https://github.com/PlexRipper/PlexRipper/issues" />
											</li>
											<li v-html="$t('pages.setup.page-1.list.item-2')"></li>
											<li v-html="$t('pages.setup.page-1.list.item-3')"></li>
											<li v-html="$t('pages.setup.page-1.list.item-4')"></li>
											<li v-html="$t('pages.setup.page-1.list.item-5')"></li>
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
										<h2>Future plans for Plex Ripper!</h2>
										<p>Here are some possible features:</p>
										<ul>
											<li>
												Radarr and Sonarr integration
												<ul>
													<li>
														Any searches for media can be redirected to Plex Ripper, and Plex Ripper will search all available
														servers, download the media and notify Radarr/Sonarr of the finished download.
													</li>
												</ul>
											</li>
											<li>Music and Photo's download support.</li>
											<li>Subtitles download support.</li>
											<li>Telegram notification support.</li>
											<li>Multi-language support.</li>
											<li>
												Better Plex integration
												<ul>
													<li>
														Download collections, add to own Plex server and automatically copy over all Plex related settings and
														configurations.
													</li>
												</ul>
											</li>
										</ul>
										<p>Any ideas are very welcome!</p>
									</v-col>
								</v-row>
							</v-container>
						</v-stepper-content>

						<!-- Checking paths	-->
						<v-stepper-content class="stepper-content" :step="3">
							<h2 class="mt-2">Ensure that all paths are valid!</h2>
							<paths-default-overview />
						</v-stepper-content>

						<!-- Plex Accounts	-->
						<v-stepper-content class="stepper-content" :step="4">
							<h2 class="mt-2">Add your Plex Accounts!</h2>
							<account-overview />
						</v-stepper-content>

						<!-- Finished	-->
						<v-stepper-content class="stepper-content" :step="5">
							<v-container fluid>
								<v-row no-gutters>
									<v-col>
										<h2 class="mt-2">{{ $t('pages.setup.page-5.title') }}</h2>
									</v-col>
								</v-row>
								<v-row no-gutters>
									<v-col>
										<p>{{ $t('pages.setup.page-5.text.p-1') }}</p>
										<v-list dense class="no-background">
											<v-list-item v-for="(link, i) in links" :key="i" :href="link" target="_blank">
												<v-list-item-title>
													<ul>
														<li>
															<span v-if="messages" style="font-weight: normal">
																{{ messages['page-5'].list['item-' + (i + 1)] }}
															</span>
														</li>
													</ul>
												</v-list-item-title>
												<v-list-item-action>
													<external-link :href="link" />
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
				<confirmation-dialog
					:disabled="isNextDisabled"
					:width="100"
					block
					text-id="skip-setup"
					button-text-id="skip-setup"
					@confirm="finishSetup"
				/>
			</v-col>
		</v-row>
	</v-container>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import PathsOverview from '@overviews/PathsCustomOverview.vue';
import AccountOverview from '@overviews/AccountOverview/AccountOverview.vue';
import ExternalLink from '@components/General/ExternalLink.vue';
import PBtn from '@components/Extensions/PButton.vue';
import ConfirmationDialog from '@components/General/ConfirmationDialog.vue';
import { SettingsService } from '@service';
import NavigationBar from './components/NavigationBar.vue';

@Component({
	components: { NavigationBar, PathsOverview, AccountOverview, ExternalLink, PBtn, ConfirmationDialog },
})
export default class Setup extends Vue {
	stepIndex: number = 1;
	stepPagesCount: number = 5;

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
		SettingsService.updateFirstTimeSetup(false);
		this.$router.push('/');
	}

	get headers(): string[] {
		if (!this.messages) {
			return [];
		}
		const headers: string[] = [];
		for (let i = 1; i <= this.stepPagesCount; i++) {
			if (this.messages['page-' + i]) {
				headers.push(this.messages['page-' + i].header);
			}
		}
		return headers;
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
