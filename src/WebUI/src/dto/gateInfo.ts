export default class GateInfo {
	id: number = 0;

	gateName: string = '';

	gateDirection: string = '';

	truckLicensePlate: string = '';

	containerCode: string = '';

	laneMessageTitle: string = '';

	laneMessage: string = '';

	state: string = '';

	barrierClosed: boolean = false;

	barrierLocked: boolean = false;

	laneClosed: boolean = false;

	gateCategory: string = '';

	public constructor(init?: Partial<GateInfo>) {
		Object.assign(this, init);
	}
}
