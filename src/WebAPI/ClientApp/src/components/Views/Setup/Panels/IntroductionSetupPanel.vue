<template>
	<q-tab-panel
		:name="name"
		:data-cy="`setup-panel-${name}`">
		<QSection
			align="center"
			:header="$t('pages.setup.intro.title')">
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
</template>

<script setup lang="ts">
defineProps<{
	name: number;
}>();
</script>
