<template>
    <BaseButton v-bind="props"/>
</template>

<script setup lang="ts">
import {defineProps, withDefaults} from 'vue';


interface CancelBtnProps {
    width?: number;
    icon?: string;
    cy?: string;
    label?: string;
}

const props = withDefaults(defineProps<CancelBtnProps>(), {
    width: 130,
    icon: 'mdi-alert',
    cy: '',
    label: 'Warning',
    textId: 'cancel',
})
</script>
