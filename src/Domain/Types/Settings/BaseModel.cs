using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PlexRipper.Domain.Settings
{
    /// <summary>
    /// Used to implement the base properties for the <see cref="SettingsModel"/>.
    /// </summary>
    public abstract class BaseModel : INotifyPropertyChanged
    {
        #region Events

        /// <summary>
        /// The event which signals when a property has changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Methods

        /// <summary>
        /// Used to signal that a property in the property has changed.
        /// </summary>
        /// <param name="propertyName">The property name that has changed.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}