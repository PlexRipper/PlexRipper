<script lang="ts">
import { defineComponent, h, mergeProps } from 'vue';
import { IBaseButtonProps } from '@props';
import { baseBtnPropsDefault } from '@composables/baseBtnProps';
import BaseButton from '@components/Buttons/BaseButton.vue';

export default defineComponent({
	name: 'VerticalButton',
	props: baseBtnPropsDefault(),
	render() {
		return h(
			BaseButton,
			{
				...mergeProps(this.$props, {
					vertical: true,
					outline: false,
					iconSize: 36,
					flat: true,
					size: 'md',
				} as IBaseButtonProps),
			},
			this.$slots.default,
		);
	},
});
</script>
