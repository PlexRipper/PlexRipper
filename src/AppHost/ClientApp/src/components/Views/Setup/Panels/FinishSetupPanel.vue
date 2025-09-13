<template>
	<q-tab-panel
		:name="name"
		:data-cy="`setup-panel-${name}`">
		<QSection
			:header="$t('pages.setup.finished.title')"
			align="center">
			<p>{{ $t('pages.setup.finished.text.p-1') }}</p>
			<QLinkList
				:list="list"
				cy="finish-setup-links" />
		</QSection>
	</q-tab-panel>
</template>

<script setup lang="ts">
import type { SetupPanelType } from '@enums';
import { useI18n } from '#imports';

const { t } = useI18n();

defineProps<{
	name: SetupPanelType;
}>();

const list = ref([{
	link: 'https://github.com/Reaparr/Reaparr/',
	text: t('pages.setup.finished.list.item-1'),
},
{
	link: 'https://hub.docker.com/r/reaparr/reaparr',
	text: t('pages.setup.finished.list.item-5'),
},
{
	link: 'https://github.com/Reaparr/Reaparr/issues',
	text: t('pages.setup.finished.list.item-2'),
},
{
	link: 'https://www.reaparr.rocks/contributing/translating',
	text: t('pages.setup.finished.list.item-3'),
},
{
	link: 'https://github.com/Reaparr/Reaparr/',
	text: t('pages.setup.finished.list.item-4'),
},
]);
</script>
