<template>
    <q-btn v-bind="props" :style="{width: props.width + 'px'}" outline glossy/>
</template>

<script setup lang="ts">
import {withDefaults, defineProps} from 'vue';

interface AccountValidationBtnProps {
    width?: number;
    icon?: string;
    cy?: string;
    textId?: string;
    disabled?: boolean;
    loading?: boolean;

}

const props = defineProps<AccountValidationBtnProps>();

</script>
