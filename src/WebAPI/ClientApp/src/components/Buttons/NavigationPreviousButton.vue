<script lang="ts">
import { defineComponent, h, mergeProps } from 'vue';
import BaseButton from './BaseButton.vue';
import { IBaseButtonProps } from '@props';
import { baseBtnPropsDefault } from '~/composables/baseBtnProps';

export default defineComponent({
	name: 'NavigationPreviousButton',
	props: baseBtnPropsDefault(),
	render() {
		return h(
			BaseButton,
			{
				...mergeProps(this.$props, {
					block: true,
					textId: 'back',
					icon: 'mdi-arrow-left',
				} as IBaseButtonProps),
			},
			this.$slots.default,
		);
	},
});
</script>
