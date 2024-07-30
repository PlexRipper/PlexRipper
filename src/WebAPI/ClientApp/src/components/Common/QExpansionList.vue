<template>
	<q-list>
		<template v-for="(item, index) in items">
			<!-- Grouped items -->
			<q-expansion-item
				v-if="item.children && item.children.length > 0"
				:key="`true-${index}`"
				group="expansion"
				:icon="item.icon"
				:label="item.noTranslate ? item.title : t(item.title ?? '')"
				expand-icon="mdi-chevron-down"
			>
				<q-item
					v-for="(child, j) in item.children"
					:key="j"
					v-ripple
					clickable
					:to="child.link"
					active-class="text-red"
				>
					<q-item-section avatar>
						<q-icon :name="child.icon" />
					</q-item-section>
					<q-item-section>{{ t(child.title ?? '') }}</q-item-section>
				</q-item>
			</q-expansion-item>
			<!-- Single item  -->
			<q-item
				v-else
				:key="`false-${index}`"
				v-ripple
				clickable
				:to="item.link"
			>
				<q-item-section avatar>
					<q-icon :name="item.icon" />
				</q-item-section>
				<q-item-section>{{ t(item.title ?? '') }}</q-item-section>
				<!-- Badge -->
				<q-item-section
					v-if="item && item.type === 'badge'"
					side
				>
					<q-chip
						v-if="item.count && item.count > 0"
						color="red"
						text-color="white"
						size="md"
					>
						{{ item.count }}
					</q-chip>
				</q-item-section>
			</q-item>
		</template>
	</q-list>
</template>

<script setup lang="ts">
import type { QExpansionListProps } from '@interfaces/components/QExpansionListProps';

const { t } = useI18n();

withDefaults(defineProps<{ items: QExpansionListProps[] }>(), {
	items: () => [],
});
</script>
