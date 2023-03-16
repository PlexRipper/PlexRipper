<template>
    <q-btn v-bind="$props" outline color="amber" glossy text-color="black"/>
</template>

<script setup lang="ts">
import {QBtn, QBtnProps} from 'quasar';
import {withDefaults, defineProps} from 'vue'
import type {IBaseButtonProps} from "@buttons/IBaseButtonProps";

// const props = withDefaults(defineProps<IBaseButtonProps>(), {
//     width: 160,
//     icon: 'mdi-alert',
//     cy: '',
// })

interface WarningBtnProps extends IBaseButtonProps {
}

withDefaults(defineProps<WarningBtnProps>(), {
    width: 1600,
    icon: 'mdi-alert',
    cy: '',
    label: 'Warning',
});


// export default Vue.extend({
//
//     render(h: CreateElement, context: RenderContext): VNode {
//         return h(BaseButton, {
//             ...context.data,
//             props: {
//                 ...context.props,
//                 color: 'yellow darken-1',
//                 outlined: true,
//                 icon: 'mdi-alert',
//             } as Partial<IBaseButtonProps>,
//         });
//     },
// });
</script>
