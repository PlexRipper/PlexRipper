using PlexRipper.Settings.Annotations;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PlexRipper.Settings.Models
{
    public abstract class BaseModel : INotifyPropertyChanged
    {
        #region Methods

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Methods

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

    }
}
