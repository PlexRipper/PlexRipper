class WebGL {
	static isWebGLAvailable() {
		try {
			const canvas = document.createElement('canvas');
			return !!(window.WebGLRenderingContext && (canvas.getContext('webgl') || canvas.getContext('experimental-webgl')));
		} catch (e) {
			return false;
		}
	}

	static isWebGL2Available() {
		try {
			const canvas = document.createElement('canvas');
			return !!(window.WebGL2RenderingContext && canvas.getContext('webgl2'));
		} catch (e) {
			return false;
		}
	}

	static getWebGLErrorMessage() {
		return this.getErrorMessage(1);
	}

	static getWebGL2ErrorMessage() {
		return this.getErrorMessage(2);
	}

	static getErrorMessage(version) {
		const names = {
			1: 'WebGL',
			2: 'WebGL 2',
		};

		const contexts = {
			1: window.WebGLRenderingContext,
			2: window.WebGL2RenderingContext,
		};

		let message =
			'Your $0 does not seem to support <a href="http://khronos.org/webgl/wiki/Getting_a_WebGL_Implementation" style="color:#000">$1</a>';

		const element = document.createElement('div');
		element.id = 'webglmessage';
		element.style.fontFamily = 'monospace';
		element.style.fontSize = '13px';
		element.style.fontWeight = 'normal';
		element.style.textAlign = 'center';
		element.style.background = '#fff';
		element.style.color = '#000';
		element.style.padding = '1.5em';
		element.style.width = '400px';
		element.style.margin = '5em auto 0';

		if (contexts[version]) {
			message = message.replace('$0', 'graphics card');
		} else {
			message = message.replace('$0', 'browser');
		}

		message = message.replace('$1', names[version]);

		element.innerHTML = message;

		return element;
	}
}

window.addEventListener('load', function () {
	window.waveEffect = null;
	if (WebGL.isWebGLAvailable()) {
		window.waveEffect = VANTA.WAVES({
			el: 'body',
			mouseControls: true,
			touchControls: true,
			gyroControls: false,
			minHeight: 1000.0,
			minWidth: 1200.0,
			scale: 1.0,
			scaleMobile: 1.0,
			color: 0x880000,
			shininess: 43.0,
			waveHeight: 4.0,
			waveSpeed: 1.25,
			zoom: 0.65,
		});
	}
});
