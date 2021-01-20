import { ReplaySubject, Observable } from 'rxjs';
// import { distinctUntilChanged, switchMap } from 'rxjs/operators';
// import GlobalService from '@state/globalService';
// import { getHealthStatus } from '@api/healthApi';
export class HealthService {
	private _serverPing: ReplaySubject<boolean> = new ReplaySubject();

	public constructor() {
		// GlobalService.getAxiosReady()
		// 	.pipe(
		// 		switchMap(() => interval(20000)),
		// 		switchMap(() => getHealthStatus()),
		// 		distinctUntilChanged(),
		// 	)
		// 	.subscribe((status) => {
		// 		this._serverPing.next(status === 'Healthy');
		// 	});
	}

	public getServerStatus(): Observable<boolean> {
		return this._serverPing.asObservable();
	}
}

const healthService = new HealthService();
export default healthService;
