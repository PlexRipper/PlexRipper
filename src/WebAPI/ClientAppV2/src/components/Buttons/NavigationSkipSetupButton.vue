<template>
    <BaseButton v-bind="props"/>
</template>

<script setup lang="ts">
import {withDefaults, defineProps} from 'vue';
import BaseButton from "@buttons/BaseButton.vue";

interface CheckConnectionBtnProps {
    disabled?: boolean;
    loading?: boolean;
    icon?: string;
    cy?: string;
    label?: string;
}

const props = withDefaults(defineProps<CheckConnectionBtnProps>(), {
    width: 130,
    cy: '',
    textId: 'skip-setup',
    icon: 'mdi-debug-step-over',
    iconAlign: 'Right',
})

</script>
