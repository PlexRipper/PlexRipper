import { EMPTY, Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Context } from '@nuxt/types';
import { BaseService } from '@service';
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

	public setup(nuxtContext: Context): Observable<ISetupResult> {
		super.setup(nuxtContext);
		return EMPTY;
	}

	// region Alerts
	public getAlerts(): Observable<IAlert[]> {
		return this.stateChanged.pipe(map((state: IStoreState) => state?.alerts ?? []));
	}

	public showAlert(alert: IAlert): void {
		const alerts = this.getState().alerts;
		// Add unique id
		const id = (alerts.last()?.id ?? 0) + 1;
		this.setState({ alerts: [...alerts, ...[{ ...alert, id }]] });
	}

	public removeAlert(id: number): void {
		this.setState({ alerts: this.getState().alerts.filter((x) => x.id !== id) });
	}

	// endregion
}

const alertService = new AlertService();
export default alertService;
