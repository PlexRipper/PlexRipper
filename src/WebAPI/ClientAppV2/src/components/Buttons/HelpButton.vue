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
    icon: 'mdi-help-circle-outline',
    iconOnly: true,
    iconSize: 24,
})

</script>
