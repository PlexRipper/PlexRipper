<template>
	<QRow no-gutters>
		<QCol :cols="12">
			<!-- Header	-->
			<QCol
				v-if="$slots['header'] || header"
				class="q-mx-md">
				<QText
					size="h5"
					full-width
					class="q-my-sm q-ml-md"
					:align="align"
					bold="medium">
					<slot name="header">
						{{ header }}
					</slot>
				</QText>
				<q-separator />
			</QCol>
			<!--	Section Content	-->
			<QCol class="q-pa-md">
				<slot />
			</QCol>
		</QCol>
	</QRow>
</template>

<script setup lang="ts">
withDefaults(defineProps<{
	header?: string;
	align?: 'left' | 'center' | 'right';
}>(), {
	header: '',
	align: 'left',
});
</script>
