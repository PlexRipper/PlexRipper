<script lang="ts">
import { h, defineComponent, mergeProps } from 'vue';
import BaseButton from './BaseButton.vue';
import { IBaseButtonProps } from '@props';
import { baseBtnPropsDefault } from '~/composables/baseBtnProps';

export default defineComponent({
	name: 'NavigationNextButton',
	props: baseBtnPropsDefault(),
	render() {
		return h(BaseButton, {
			...mergeProps(this.$props, {
				block: true,
				textId: 'next',
				iconAlign: 'right',
				icon: 'mdi-arrow-right',
			}),
		} as Partial<IBaseButtonProps>);
	},
});
</script>
