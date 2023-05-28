using Bogus.Premium;

namespace PlexRipper.BaseTests;

public static class BogusExtensions
{
    public static void Setup()
    {
        License.LicenseTo = "jasonlandbridge@protonmail.com";
        License.LicenseKey =
            "Qbvmc25QS+lrtERpYCoLJCneyw2uEioqEc/FAJTp3rerLCiEXdkzzOt7wQhI1lluxPJMjw+um6SvsWWH/Dq/78GOgPBLbsWOrynSuJ8qBaUx6YXc/pLEhBib0ca+o5xqfTxTyImGbD2GZtLAzHmtqvXX8gmqu3C98GkChVd4H40=";
    }
}