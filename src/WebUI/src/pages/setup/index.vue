<template>
	<div id="background" style="height: 100%">
		<v-container>
			<v-row>
				<v-col>
					<h1 class="text-center">Plex Ripper Setup</h1>
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
										:color="stepIndex > i + 1 ? 'green' : 'red'"
										:complete="stepIndex > i + 1"
										editable
										edit-icon="$complete"
									>
										{{ header }}
									</v-stepper-step>
									<v-divider v-if="i < headers.length - 1" :key="i + 100" />
								</template>
							</v-stepper-header>

							<!-- Step pages	-->
							<v-stepper-items>
								<!-- Introduction	-->
								<v-stepper-content step="1">
									<v-card class="stepper-page" :height="sliderHeight">
										<h2 class="text-center">You are awesome for trying out Plex Ripper!</h2>
										<p>Here is some useful info:</p>
										<ul>
											<li>
												Plex Ripper is currently under active development, so there might be some bugs here and there. Please make
												sure to report any you may find on the
												<a href="https://github.com/PlexRipper/PlexRipper/issues" target="_blank"> Github issues page</a> .
											</li>
											<li>
												Feedback or feature requests are really appreciated! Especially on the source code as this is a personal
												project meant to improve my coding skills.
											</li>
											<li>
												Plex Ripper has dark mode support! With that I mean that the light mode is "working" but not optimal so
												for now use the dark mode, which is enabled by default.
											</li>
											<li>
												Plex Ripper can download Movies and TvShows, other types such as music is currently not supported, but
												will in the future!
											</li>
											<li>
												Plex Ripper does not, and will not, collect any data about you or any of your activities. The only data
												that is sent to Plex is your username and password to authenticate your Plex account. After which, all
												communication is directly with the shared servers you have access to.
											</li>
											<li>
												A word of caution: Plex, and maybe Plex server owners, do not like you to download all "their" 100%
												legally acquired copies of media. Which is why there are some safe guards in place to protect Plex Ripper
												users from being banned by Plex. However, be respectful to Plex server owners by not taking up all their
												precious bandwidth and don't complain to me about being kicked of a server. In the future I would also
												like to protect Plex server owners from Plex Ripper abuse, any ideas or input are welcome.
											</li>
										</ul>
									</v-card>
								</v-stepper-content>

								<!-- Future plans!	-->
								<v-stepper-content step="2">
									<v-card class="stepper-page" :height="sliderHeight">
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
									</v-card>
								</v-stepper-content>

								<!-- Checking paths	-->
								<v-stepper-content step="3">
									<v-card class="stepper-page" :height="sliderHeight">
										<h2>Ensure that all paths are valid!</h2>
										<paths-overview />
									</v-card>
								</v-stepper-content>

								<!-- Plex Accounts	-->
								<v-stepper-content step="4">
									<v-card class="stepper-page" :height="sliderHeight">
										<account-overview />
									</v-card>
								</v-stepper-content>

								<!-- Finished	-->
								<v-stepper-content step="5">
									<v-card class="stepper-page" :height="sliderHeight"> </v-card>
								</v-stepper-content>
							</v-stepper-items>
							<!-- Stepper navigation bar	-->
							<navigation-bar @back="back" @next="next" />
						</v-stepper>
					</template>
				</v-col>
			</v-row>

			<!--	Skip button	-->
			<v-row justify="center">
				<v-col cols="auto">
					<v-btn nuxt to="/"> Skip setup </v-btn>
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
import NavigationBar from './components/NavigationBar.vue';

@Component({
	layout: 'empty',
	components: { NavigationBar, PathsOverview, AccountOverview },
})
export default class Setup extends Vue {
	stepIndex: number = 3;

	sliderHeight: number = 600;
	headers: string[] = ['Being called awesome!', 'Future plans!', 'Checking paths', 'Plex Accounts', 'Finished'];

	vantaEffect: any;

	back(): void {
		if (this.stepIndex > 0) {
			this.stepIndex--;
		}
	}

	next(): void {
		this.stepIndex++;
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
