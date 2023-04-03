<template>
	<q-section>
		<template #header> {{ $t('pages.settings.advanced.setup.header') }}</template>
		<!--	Reset Database	-->
		<q-row>
			<q-col cols="4" align-self="center">
				<help-icon help-id="help.settings.advanced.setup-section.go-to-setup-page" />
			</q-col>
			<q-col cols="4" align-self="center">
				<GoToButton :width="400" text-id="go-to-setup-page" to="/setup" block />
			</q-col>
		</q-row>
	</q-section>
</template>
