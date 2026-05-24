import type { DesktopMessageDTO } from '@dto';

type PhotinoMessageHandler = (message: string) => void;

declare const window: Window & {
	external: {
		sendMessage(message: string): void;
		receiveMessage(handler: PhotinoMessageHandler): void;
	};
};

export function sendDesktopMessage(message: DesktopMessageDTO): void {
	window.external.sendMessage(JSON.stringify(message));
}

export function canSendDesktopMessage(): boolean {
	return typeof window !== 'undefined' && typeof window.external?.sendMessage === 'function';
}
