<template>
	<q-list>
		<template v-for="item in items">
			<!-- Grouped items -->
			<q-expansion-item v-if="item.children && item.children.length > 0" :icon="item.icon"
												:label="item.noTranslate ? item.title : $t(item.title ?? '')"
												expand-icon="mdi-chevron-down">
				<q-item clickable v-ripple v-for="(child, j) in item.children" :to="child.link" active-class="text-orange">
					<q-item-section avatar>
						<q-icon :name="child.icon"/>
					</q-item-section>
					<q-item-section>{{ $t(child.title ?? '') }}</q-item-section>
				</q-item>
			</q-expansion-item>
			<!-- Single item  -->
			<q-item v-else clickable v-ripple :to="item.link" active-class="text-orange">
				<q-item-section avatar>
					<q-icon :name="item.icon"/>
				</q-item-section>
				<q-item-section>{{ $t(item.title ?? '') }}</q-item-section>
				<!-- Badge -->
				<q-item-section side v-if="item && item.type === 'badge'">
					<q-badge v-if="item.count && item.count > 0" color="red" floating :label="item.count"/>
				</q-item-section>
			</q-item>
		</template>
	</q-list>
</template>

<script setup lang="ts">
import {QExpansionListProps} from "@interfaces/components/QExpansionListProps";

const props = withDefaults(defineProps<{ items: QExpansionListProps[] }>(), {
	items: () => [],
});

</script>

