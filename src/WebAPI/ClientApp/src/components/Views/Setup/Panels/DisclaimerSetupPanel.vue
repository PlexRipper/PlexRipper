<template>
	<q-tab-panel
		:name="name"
		:data-cy="`setup-panel-${name}`">
		<QSection
			align="center"
			:header="$t('pages.setup.disclaimer.header')">
			<p>Please read and agree to the terms below before continuing:</p>
			<p><strong>1. Legal and Ethical Use</strong></p>
			<p>
				You are responsible for ensuring that your use of PlexRipper complies with all applicable laws,
				including copyright laws, and the Terms of Service of any platforms you interact with. PlexRipper
				is designed solely for personal media management and backups. It must not be used for unauthorized
				downloading, copyright infringement, or any other unlawful activity.
			</p>
			<p><strong>2. Ownership and Permissions</strong></p>
			<p>
				You must have explicit ownership or permission to access the media content and servers you interact
				with using PlexRipper. Unauthorized access or use of third-party servers or media is strictly
				prohibited.
			</p>
			<p><strong>3. No Affiliation</strong></p>
			<p>
				PlexRipper is an independent tool and is not affiliated with, endorsed by, or associated with Plex
				Inc. or any other platform.
			</p>
			<p><strong>4. Limitation of Liability</strong></p>
			<p>
				The developers of PlexRipper are not responsible for any misuse of the tool or for consequences
				arising from unauthorized or illegal use.
			</p>
			<br>
			<br>
			<QText
				class="q-mt-md"
				size="h5"
				align="center">
				By proceeding, you confirm that you understand and accept these terms, and that you will use
				PlexRipper
				responsibly and ethically.
			</QText>
		</QSection>
	</q-tab-panel>
</template>

<script setup lang="ts">
defineProps<{
	name: number;
}>();
</script>
