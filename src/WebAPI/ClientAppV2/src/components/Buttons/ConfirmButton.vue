<template>
    <BaseButton v-bind="props"/>
</template>

<script setup lang="ts">
import {withDefaults, defineProps} from 'vue';
import BaseButton from "@buttons/BaseButton.vue";

interface ConfirmBtnProps {
    disabled?: boolean;
    loading?: boolean;
    icon?: string;
    cy?: string;
    label?: string;
}

const props = withDefaults(defineProps<ConfirmBtnProps>(), {
    width: 130,
    cy: '',
    outlined: true,
    color: 'green',
    textId: 'confirm',
    icon: 'mdi-check',
})

</script>
