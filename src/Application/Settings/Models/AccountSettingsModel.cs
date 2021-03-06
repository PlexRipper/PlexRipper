﻿using Newtonsoft.Json;
using PlexRipper.Application.Settings.Models.Base;

namespace PlexRipper.Application.Settings.Models
{
    public class AccountSettingsModel : BaseModel
    {
        #region Fields

        private int _activeAccountId;

        #endregion

        #region Properties

        [JsonProperty("activeAccountId", Required = Required.Always)]
        public int ActiveAccountId
        {
            get => _activeAccountId;
            set
            {
                if (value != _activeAccountId)
                {
                    _activeAccountId = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion Properties
    }
}