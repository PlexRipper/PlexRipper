<script setup lang="ts">
import Vue, {CreateElement, VNode} from 'vue';
import {RenderContext} from 'vue/types/options';
import BaseButton, {IBaseButtonProps} from './BaseButton.vue';

export default Vue.extend({
    name: 'CancelButton',
    functional: true,
    props: {
        cy: {
            type: String,
            default: '',
        },
        width: {
            type: Number,
            default: 130,
        },
    },
    render(h: CreateElement, context: RenderContext): VNode {
        return h(BaseButton, {
            ...context.data,
            props: {
                ...context.props,
                outlined: true,
                textId: 'cancel',
                icon: 'mdi-cancel',
            } as Partial<IBaseButtonProps>,
        });
    },
});
</script>
