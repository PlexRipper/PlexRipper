<template>
	<q-tab-panel
		:data-cy="`setup-panel-${name}`"
		:name="name">
		<QSection
			:header="$t('pages.setup.intro.title')"
			align="center">
			<p>{{ $t('pages.setup.intro.text.p-1') }}</p>
			<q-list
				bordered
				separator>
				<q-item
					v-for="(item, index) in list"
					:key="index"
					:href="item.link"
					target="_blank">
					<q-item-section>
						<q-item-label>
							<QText
								:value="item.text" />
						</q-item-label>
					</q-item-section>
					<q-item-section
						v-if="item.link"
						avatar>
						<ExternalLinkButton :href="item.link" />
					</q-item-section>
				</q-item>
			</q-list>
		</QSection>
	</q-tab-panel>
</template>

<script lang="ts" setup>
import { discordInviteLink } from '@composables';

defineProps<{
	name: number;
}>();
const { t } = useI18n();
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
		link: 'https://github.com/PlexRipper/PlexRipper/issues',
	},
];
</script>
