<template>
	<v-select
		:dense="true"
		:filled="true"
		:outlined="true"
		color="red"
		hide-details="auto"
		:menu-props="{ offsetY: true, contentClass: 'menu-background' }"
		:items="items"
		:value="value"
		v-bind="$attrs"
		v-on="$listeners"
	>
		<template #item="{ item }">
			<v-row class="my-2 no-wrap" align="center">
				<v-col cols="auto">
					<v-icon v-if="item.icon">{{ item.icon }}</v-icon>
					<v-img v-if="item.img" :src="item.img" :height="50" :max-width="80" :alt="item.text" />
				</v-col>
				<v-col cols="10">
					<span class="pa-2">
						{{ item.text }}
					</span>
				</v-col>
			</v-row>
		</template>
	</v-select>
</template>

<script>
import { VSelect } from 'vuetify/lib';

export default {
	name: 'PSelect',
	extends: VSelect,
	props: {
		items: {
			type: Array,
			default: () => [],
		},
		value: {
			type: String,
			default: '',
		},
	},
};
</script>
