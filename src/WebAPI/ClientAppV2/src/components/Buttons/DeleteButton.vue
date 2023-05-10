<script lang="ts">
import { defineComponent, h, mergeProps } from 'vue';
import BaseButton from './BaseButton.vue';
import { IBaseButtonProps } from '@props';
import { baseBtnPropsDefault } from '~/composables/baseBtnProps';

export default defineComponent({
	name: 'DeleteButton',
	props: baseBtnPropsDefault(),
	render() {
		return h(
			BaseButton,
			{
				...mergeProps(this.$props, {
					color: 'negative',
					textId: 'delete',
					icon: 'mdi-delete',
				} as IBaseButtonProps),
			},
			this.$slots.default,
		);
	},
});
</script>
