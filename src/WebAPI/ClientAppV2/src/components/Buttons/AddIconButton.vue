<template>
    <BaseButton v-bind="props" class="warning-btn" label="Standard"/>
</template>

<script setup lang="ts">
import {withDefaults, defineProps} from 'vue';
import BaseButton from "@buttons/BaseButton.vue";


interface AddIconBtnProps {
    width?: number;
    icon?: string;
    cy?: string;
    disabled?: boolean;
}

const props = withDefaults(defineProps<AddIconBtnProps>(), {
    height: 50,
    icon: 'mdi-plus-box-outline',
    cy: '',
    label: 'Warning',
})

</script>
