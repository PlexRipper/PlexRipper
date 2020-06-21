namespace PlexRipper.Settings
{
    public interface IUserSettings
    {
        bool Save();
        bool Load();
        string ApiKey { get; set; }
        bool ConfirmExit { get; set; }
        void Reset();
    }
}