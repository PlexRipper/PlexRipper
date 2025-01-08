<template>
	<q-tab-panel
		:name="name"
		:data-cy="`setup-panel-${name}`">
		<QSection
			:header="$t('pages.setup.finished.title')"
			align="center">
			<p>{{ $t('pages.setup.finished.text.p-1') }}</p>
			<q-list
				class="no-background"
				dense>
				<q-item
					v-for="(link, i) in links"
					:key="i"
					:href="link.link"
					target="_blank">
					<q-item-section avatar>
						<ul>
							<li>
								<span style="font-weight: normal">
									{{ link.text }}
								</span>
							</li>
						</ul>
					</q-item-section>
					<q-item-section side>
						<ExternalLinkButton :href="link.link" />
					</q-item-section>
				</q-item>
			</q-list>
		</QSection>
	</q-tab-panel>
</template>

<script setup lang="ts">
import type { SetupPanelType } from '@enums';
import { useI18n } from '#imports';

defineProps<{
	name: SetupPanelType;
}>();
const { t } = useI18n();

const links = ref([{
	link: 'https://github.com/PlexRipper/PlexRipper/',
	text: t('pages.setup.finished.list.item-1'),
},
{
	link: 'https://github.com/PlexRipper/PlexRipper/issues',
	text: t('pages.setup.finished.list.item-2'),
},
{
	link: 'https://www.plexripper.rocks/contributing/translating',
	text: t('pages.setup.finished.list.item-3'),
},
{
	link: 'https://github.com/PlexRipper/PlexRipper/',
	text: t('pages.setup.finished.list.item-4'),
},
]);
</script>
