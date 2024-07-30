<template>
	<QRow
		no-gutters
		style="width: 100%"
	>
		<QCol cols="12">
			<!-- Header	-->
			<QRow no-gutters>
				<QCol>
					<QText
						size="h4"
						align="center"
						bold="medium"
					>
						<slot name="header" />
					</QText>
					<q-separator />
				</QCol>
			</QRow>
			<!--	Section Content	-->
			<slot />
		</QCol>
	</QRow>
</template>

<script setup lang="ts"></script>
