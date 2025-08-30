<template>
	<q-tab-panel
		:data-cy="`setup-panel-${name}`"
		:name="name">
		<QSection
			:header="$t('pages.setup.intro.title')"
			align="center">
			<p>{{ $t('pages.setup.intro.text.p-1') }}</p>
			<QLinkList :list="list" />
		</QSection>
	</q-tab-panel>
</template>

<script lang="ts" setup>
import { discordInviteLink } from '@composables';
import type { SetupPanelType } from '@enums';

const { t } = useI18n();

defineProps<{
	name: SetupPanelType;
}>();

const list: { text: string; link?: string }[] = [
	{
		text: t('pages.setup.intro.list.item-1'),
		link: discordInviteLink(),
	},
	{
		text: t('pages.setup.intro.list.item-2'),
		link: 'https://plexripper.rocks/faq',
	},
	{
		text: t('pages.setup.intro.list.item-3'),
		link: 'https://github.com/Reaparr/Reaparr/issues',
	},
];
</script>
