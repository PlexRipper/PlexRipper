<template>
	<div id="background" style="height: 100%">
		<v-container style="max-width: 900px">
			<!-- Logo	-->
			<v-row justify="center">
				<v-col cols="auto">
					<v-img :height="128" :width="128" :src="require('@img/logo/full-logo-256.png')"></v-img>
				</v-col>
			</v-row>
			<!--	Steppers	-->
			<v-row>
				<v-col>
					<template>
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
												<h2 class="mt-2">{{ $t('setup.page-1.title') }}</h2>
												<p>{{ $t('setup.page-1.text.p-1') }}</p>
												<ul>
													<li>
														{{ $t('setup.page-1.list.item-1') }}
														<external-link href="https://github.com/PlexRipper/PlexRipper/issues" />
													</li>
													<li v-html="$t('setup.page-1.list.item-2')"></li>
													<li v-html="$t('setup.page-1.list.item-3')"></li>
													<li v-html="$t('setup.page-1.list.item-4')"></li>
													<li v-html="$t('setup.page-1.list.item-5')"></li>
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
																Download collections, add to own Plex server and automatically copy over all Plex related settings
																and configurations.
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
									<paths-overview />
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
												<h2 class="mt-2">{{ $t('setup.page-5.title') }}</h2>
											</v-col>
										</v-row>
										<v-row no-gutters>
											<v-col>
												<p>{{ $t('setup.page-5.text.p-1') }}</p>
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
							/>
						</v-stepper>
					</template>
				</v-col>
			</v-row>

			<!--	Skip button	-->
			<v-row justify="center">
				<v-col cols="3">
					<p-btn to="/" :disabled="isNextDisabled" block :width="100" text-id="skip-setup" />
				</v-col>
			</v-row>
		</v-container>
	</div>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import PathsOverview from '@overviews/PathsOverview.vue';
import * as THREE from 'three';
import WAVES from 'vanta/dist/vanta.waves.min';
import AccountOverview from '@overviews/AccountOverview/AccountOverview.vue';
import ExternalLink from '@components/General/ExternalLink.vue';
import PBtn from '@components/General/PlexRipperButton.vue';
import NavigationBar from './components/NavigationBar.vue';

@Component({
	layout: 'empty',
	components: { NavigationBar, PathsOverview, AccountOverview, ExternalLink, PBtn },
})
export default class Setup extends Vue {
	stepIndex: number = 1;
	stepPagesCount: number = 5;

	sliderHeight: number = 600;

	links: string[] = [
		'https://github.com/PlexRipper/PlexRipper/',
		'https://github.com/PlexRipper/PlexRipper/issues',
		'https://github.com/PlexRipper/PlexRipper#translate-plexripper',
		'https://github.com/PlexRipper/PlexRipper/',
	];

	vantaEffect: any;

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

	get headers(): string[] {
		const headers: string[] = [];
		for (let i = 1; i <= this.stepPagesCount; i++) {
			headers.push(this.messages['page-' + i].header);
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
		return this.$messages().setup;
	}

	mounted(): void {
		this.vantaEffect = WAVES({
			THREE,
			el: '#background',
			mouseControls: true,
			touchControls: true,
			gyroControls: false,
			minHeight: 200.0,
			minWidth: 200.0,
			scale: 1.0,
			scaleMobile: 1.0,
			color: 0x880000,
			shininess: 43.0,
			waveHeight: 4.0,
			waveSpeed: 1.25,
			zoom: 0.65,
		});
	}

	beforeDestroy(): void {
		if (this.vantaEffect) {
			this.vantaEffect.destroy();
		}
	}
}
</script>
