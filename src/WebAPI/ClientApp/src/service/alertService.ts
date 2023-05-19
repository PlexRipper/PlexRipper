import { Observable, of } from 'rxjs';
import { map, take } from 'rxjs/operators';

import BaseService from '@service/baseService';
import IStoreState from '@interfaces/service/IStoreState';
import IAlert from '@interfaces/IAlert';
import ISetupResult from '@interfaces/service/ISetupResult';

export class AlertService extends BaseService {
	public constructor() {
		super('AlertService', {
			// Note: Each service file can only have "unique" state slices which are not also used in other service files
			stateSliceSelector: (state: IStoreState) => {
				return {
					alerts: state.alerts,
				};
			},
		});
	}

	public setup(): Observable<ISetupResult> {
		super.setup();
		return of({ name: this._name, isSuccess: true }).pipe(take(1));
	}

	// region Alerts
	public getAlerts(): Observable<IAlert[]> {
		return this.stateChanged.pipe(map((state: IStoreState) => state?.alerts ?? []));
	}

	public showAlert(alert: IAlert): void {
		const alerts = this.getState().alerts;
		// Add unique id
		const id = Date.now();
		this.setState({ alerts: [...alerts, ...[{ ...alert, id }]] });
	}

	public removeAlert(id: number): void {
		this.setState({ alerts: this.getState().alerts.filter((x) => x.id !== id) });
	}

	// endregion
}

export default new AlertService();
