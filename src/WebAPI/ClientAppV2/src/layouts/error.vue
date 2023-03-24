<template>
	<q-layout view="hHh lpR fFf" dark class="no-background">
		<q-page-container>
			<q-page>
				<q-row justify="center">
					<q-col cols="auto">
						<h1>{{ $t('pages.error.header') }}</h1>
						<h1 class="text-left">
							<pre><code class="no-background">{{ errorFormatted }}</code></pre>
						</h1>
					</q-col>
				</q-row>
				<q-row justify="center">
					<q-col cols="auto">
						<h1>
							<NuxtLink to="/">
								{{ $t('pages.error.return-link') }}
							</NuxtLink>
						</h1>
					</q-col>
				</q-row>
			</q-page>
		</q-page-container>
	</q-layout>
</template>

<script setup lang="ts">
import { defineProps } from 'vue';

const props = defineProps<{ error: any }>();

const errorFormatted = computed(() => {
	return JSON.stringify(props.error, null, 4);
});
</script>
