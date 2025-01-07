<template>
	<QPage>
		<QSection>
			<template #header>
				{{ $t('pages.settings.accounts.plex-accounts-header') }}
			</template>
			<AccountOverview />
		</QSection>
		<QSection>
			<template #header>
				{{ $t('pages.settings.accounts.app-accounts-header') }}
			</template>
			<AuthenticationOverview />
		</QSection>
	</QPage>
</template>
