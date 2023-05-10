<script lang="ts">
import { defineComponent, h, mergeProps } from 'vue';
import BaseButton from './BaseButton.vue';
import { IBaseButtonProps } from '@props';
import { baseBtnPropsDefault } from '~/composables/baseBtnProps';

export default defineComponent({
	name: 'ResetButton',
	props: baseBtnPropsDefault(),
	render() {
		return h(
			BaseButton,
			{
				...mergeProps(this.$props, {
					textId: 'reset',
					icon: 'mdi-restore',
				} as IBaseButtonProps),
			},
			this.$slots.default,
		);
	},
});
</script>
