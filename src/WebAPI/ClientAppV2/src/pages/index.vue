<template>
    <div>
        <h1>Text</h1>
        <div class="q-pa-md q-gutter-sm">
            <q-btn color="white" text-color="black" label="Standard"/>
            <q-btn color="primary" label="Primary"/>
            <q-btn color="secondary" label="Secondary"/>
            <q-btn color="amber" glossy label="Amber"/>
            <q-btn color="brown-5" label="Brown 5"/>
            <q-btn color="deep-orange" glossy label="Deep Orange"/>
            <q-btn color="purple" label="Purple"/>
            <q-btn color="black" label="Black"/>
        </div>
        <div>
            <WarningButton :width="500"/>
        </div>
    </div>
</template>

<script setup lang="ts">

import WarningButton from "@components/Buttons/WarningButton.vue";
</script>