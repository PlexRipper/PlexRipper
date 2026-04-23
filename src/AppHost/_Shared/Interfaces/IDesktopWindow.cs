namespace Reaparr.AppHost;

/// <summary>
/// Abstraction over the native desktop window to keep desktop lifecycle logic testable.
/// </summary>
public interface IDesktopWindow
{
    /// <summary>
    /// Creates and configures the native window instance.
    /// </summary>
    void ConfigureWindow();

    /// <summary>
    /// Registers a native window-closing callback.
    /// </summary>
    void RegisterWindowClosingHandler(Func<object?, EventArgs, bool> handler);

    /// <summary>
    /// Registers a callback for messages sent by the embedded browser when an external link is requested.
    /// </summary>
    void RegisterDesktopMessageHandler(Action<DesktopMessageDTO> handler);

    /// <summary>
    /// Opens the supplied URI with the operating system default browser.
    /// </summary>
    void OpenExternalBrowser(Uri uri);

    /// <summary>
    /// Hides or minimizes the visible window without ending desktop runtime.
    /// </summary>
    void CloseToBackground();

    /// <summary>
    /// Restores a previously backgrounded window.
    /// </summary>
    void RestoreFromBackground();

    /// <summary>
    /// Requests native window closure.
    /// </summary>
    void CloseNativeWindow();

    /// <summary>
    /// Disposes native window resources owned by the wrapper.
    /// </summary>
    void DisposeWindow();

    /// <summary>
    /// Gets whether the native window was initialized.
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Starts the native window message loop when needed.
    /// </summary>
    void WaitForClose();
}
