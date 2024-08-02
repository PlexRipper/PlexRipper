<template>
	<q-layout
		view="hHh lpR fFf"
		dark
		class="no-background">
		<q-page-container>
			<QPage>
				<QRow justify="center">
					<QCol cols="auto">
						<h1>{{ t('pages.error.header') }}</h1>
						<h1 class="text-left">
							<pre>
								<code class="no-background">{{ errorFormatted }}</code>
							</pre>
						</h1>
					</QCol>
				</QRow>
				<QRow justify="center">
					<QCol cols="auto">
						<h1>
							<NuxtLink to="/">
								{{ t('pages.error.return-link') }}
							</NuxtLink>
						</h1>
					</QCol>
				</QRow>
			</QPage>
		</q-page-container>
	</q-layout>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';

const { t } = useI18n();
const props = defineProps<{ error: Error }>();

const errorFormatted = computed(() => {
	return JSON.stringify(props.error, null, 4);
});
</script>
