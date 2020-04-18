import Severity from '@/enums/severity';

export interface ILogMessage {
	/** Client context information like useragent */
	Context: string;
	/** Message severity like warning, info and default error */
	Severity: Severity;
	/** Client module name */
	ModuleName: string;
	/** Other information */
	Message: string;
}

export default ILogMessage;
