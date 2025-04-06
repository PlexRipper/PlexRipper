export function urlBuilder(relativeUrl: string) {
	try {
		return new URL(relativeUrl, `http://localhost:${Cypress.env('API_PORT')}`);
	} catch (error) {
		console.error('Error parsing URL:', error);
		throw error;
	}
}
