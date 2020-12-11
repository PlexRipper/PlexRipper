import { ReplaySubject, Observable } from 'rxjs';

export class HealthService {
	private _helpDialog: ReplaySubject<string> = new ReplaySubject();

	public getHelpDialog(): Observable<string> {
		return this._helpDialog.asObservable();
	}

	public openHelpDialog(helpId: string): void {
		this._helpDialog.next(helpId);
	}
}

const healthService = new HealthService();
export default healthService;
