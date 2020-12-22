import { RuntimeConfig } from '~/type_definitions/vueTypes';

export default class AppConfig {
	private readonly _nodeEnv: string;

	constructor(config: RuntimeConfig) {
		this._nodeEnv = config.nodeEnv;
	}

	get isDevelopment(): boolean {
		return this._nodeEnv === 'development';
	}

	get isProduction(): boolean {
		return this._nodeEnv === 'production';
	}

	/**
	 * The baseURL for the application. It is assumed that front and back-end run in a docker container on the same URL when in production
	 * due to being deployed statically in the wwwroot of the .NET Core back-end. The url is retrieved dynamically as to work with different domains.
	 * When in development, the front-end runs on port 3000 and the back-end on port 5000.
	 */
	get baseURL(): string {
		return this.isProduction ? window.location.origin : 'http://localhost:5000';
	}

	get baseApiUrl(): string {
		return this.baseURL + '/api';
	}
}
