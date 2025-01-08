<template>
	<q-tab-panel
		:name="name"
		:data-cy="`setup-panel-${name}`">
		<QSection
			align="center"
			:header="$t('pages.setup.disclaimer.header')">
			<QText class="q-my-md">
				{{ $t('pages.setup.disclaimer.text.heading') }}
			</QText>
			<QText
				bold
				class="q-my-md">
				{{ $t('pages.setup.disclaimer.text.section1.title') }}
			</QText>
			<QText class="q-my-md">
				{{ $t('pages.setup.disclaimer.text.section1.content') }}
			</QText>
			<QText bold>
				{{ $t('pages.setup.disclaimer.text.section2.title') }}
			</QText>
			<QText class="q-my-md">
				{{ $t('pages.setup.disclaimer.text.section2.content') }}
			</QText>
			<QText bold>
				{{ $t('pages.setup.disclaimer.text.section3.title') }}
			</QText>
			<QText class="q-my-md">
				{{ $t('pages.setup.disclaimer.text.section3.content') }}
			</QText>
			<QText bold>
				{{ $t('pages.setup.disclaimer.text.section4.title') }}
			</QText>
			<QText class="q-my-md">
				{{ $t('pages.setup.disclaimer.text.section4.content') }}
			</QText>
			<QText
				class="q-mt-xl"
				size="h5"
				align="center">
				{{ $t('pages.setup.disclaimer.text.confirmation') }}
			</QText>
		</QSection>
	</q-tab-panel>
</template>

<script setup lang="ts">
import type { SetupPanelType } from '@enums';

defineProps<{
	name: SetupPanelType;
}>();
</script>
