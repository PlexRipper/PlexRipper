<script lang="ts">
import { defineComponent, h, mergeProps } from 'vue';
import BaseButton from './BaseButton.vue';
import { IBaseButtonProps } from '@props';
import { baseBtnPropsDefault } from '~/composables/baseBtnProps';

export default defineComponent({
	name: 'NavigationNextButton',
	props: baseBtnPropsDefault(),
	render() {
		return h(
			BaseButton,
			{
				...this.$attrs,
				...mergeProps(this.$props, {
					block: true,
					textId: 'next',
					iconAlign: 'right',
					icon: 'mdi-arrow-right',
				} as IBaseButtonProps),
			},
			this.$slots.default,
		);
	},
});
</script>
