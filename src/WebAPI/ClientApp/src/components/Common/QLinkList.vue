<template>
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
</template>

<script setup lang="ts">
withDefaults(defineProps<{
	list: { text: string; link?: string }[];
}>(), {
	list: () => [],
});
</script>
