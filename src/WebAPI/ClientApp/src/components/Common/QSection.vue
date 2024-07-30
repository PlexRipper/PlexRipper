<template>
	<q-row
		no-gutters
		style="width: 100%"
	>
		<q-col cols="12">
			<!-- Header	-->
			<q-row no-gutters>
				<q-col>
					<QText
						size="h4"
						align="center"
						bold="medium"
					>
						<slot name="header" />
					</QText>
					<q-separator />
				</q-col>
			</q-row>
			<!--	Section Content	-->
			<slot />
		</q-col>
	</q-row>
</template>

<script setup lang="ts"></script>
