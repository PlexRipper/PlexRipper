<template>
	<QPage>
		<QSection :header="$t('pages.settings.accounts.plex-accounts-header')">
			<AccountOverview />
		</QSection>
		<QSection :header="$t('pages.settings.accounts.app-accounts-header')">
			<AuthenticationOverview />
		</QSection>
	</QPage>
</template>
