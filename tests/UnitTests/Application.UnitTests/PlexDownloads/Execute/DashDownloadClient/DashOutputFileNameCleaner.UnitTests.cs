namespace Reaparr.Application.UnitTests;

public class DashOutputFileNameCleanerUnitTests
{
    [Test]
    [Arguments("4 for Texas (1963).avi", VideoQuality.FullHD, "4.for.Texas.1963.WEB-DL.1080p.mkv")]
    [Arguments(
        "Nine.and.a.Half.Weeks.1986.1080p.BluRay.DTS.x264-HDMaNiAcS.mkv",
        VideoQuality.FullHD,
        "Nine.and.a.Half.Weeks.1986.WEB-DL.1080p.mkv"
    )]
    [Arguments("22 July (2018) WEBDL-2160p.mkv", VideoQuality.UHD_4K, "22.July.2018.WEB-DL.2160p.mkv")]
    [Arguments(
        "The Amityville Horror (1979) [imdb-tt0078767][tmdb-11449][Bluray-2160p][HDR][HDR10][AC3 5.1][x265].mp4",
        VideoQuality.UHD_4K,
        "The.Amityville.Horror.1979.imdb-tt0078767.tmdb-11449.WEB-DL.2160p.mkv"
    )]
    [Arguments(
        "风味原产地·潮汕.Flavorful.Origins.S01E06.1080p.NF.WEB-DL.DDP2.0.x264-Ao.mkv",
        VideoQuality.FullHD,
        "风味原产地·潮汕.Flavorful.Origins.S01E06.NF.Ao.WEB-DL.1080p.mkv"
    )]
    [Arguments(
        "Ōoku - The Inner Chambers - S01E04 - Episode 4 WEBDL-1080p.mkv",
        VideoQuality.FullHD,
        "Ōoku.The.Inner.Chambers.S01E04.Episode.4.WEB-DL.1080p.mkv"
    )]
    [Arguments(
        "Øen (2019) - S01E05 [DANiSH WEBRip-1080p h264 8bit 2.0ch AAC].mkv",
        VideoQuality.FullHD,
        "Øen.2019.S01E05.WEB-DL.1080p.mkv"
    )]
    [Arguments(
        "Årgang 20 - S05E03 - Episode 3 WEBDL-1080p.mkv",
        VideoQuality.FullHD,
        "Årgang.20.S05E03.Episode.3.WEB-DL.1080p.mkv"
    )]
    [Arguments(
        "¿A Qué Estás Esperando! (2024) - S01E04 [WEBDL-1080p 8-bit h264 EAC3 2.0][ES]-NTb.mkv",
        VideoQuality.FullHD,
        "¿A.Qué.Estás.Esperando!.2024.S01E04.WEB-DL.1080p.mkv"
    )]
    [Arguments(
        "wtFOCK (2018) - S01E09 [WEBDL-1080p 8-bit h264 AAC 2.0][NL]-BTN.mkv",
        VideoQuality.FullHD,
        "wtFOCK.2018.S01E09.WEB-DL.1080p.mkv"
    )]
    [Arguments("title_t11.mkv", VideoQuality.FullHD, "title.t11.WEB-DL.1080p.mkv")]
    [Arguments(
        "the.road.trip.s01e04.repack.dv.2160p.web.h265-nhtfs.mkv",
        VideoQuality.UHD_4K,
        "the.road.trip.s01e04.web.nhtfs.WEB-DL.2160p.mkv"
    )]
    [Arguments(
        "ted - S01E02 - My Two Dads Bluray-1080p.mkv",
        VideoQuality.FullHD,
        "ted.S01E02.My.Two.Dads.WEB-DL.1080p.mkv"
    )]
    [Arguments(
        "south.park.s22e04.1080p.bluray.x264-turmoil.mkv",
        VideoQuality.FullHD,
        "south.park.s22e04.turmoil.WEB-DL.1080p.mkv"
    )]
    [Arguments(
        "sMothered - S01E05 - Make Room For Mom WEBDL-1080p.mkv",
        VideoQuality.FullHD,
        "sMothered.S01E05.Make.Room.For.Mom.WEB-DL.1080p.mkv"
    )]
    [Arguments("rick and morty S04 E03.mkv", VideoQuality.FullHD, "rick.and.morty.S04.E03.WEB-DL.1080p.mkv")]
    [Arguments(
        "pet (2020) - S01E06.006 - BACK DOOR [Bluray-1080p Remux][8bit][h264][FLAC 2.0][JA]-npz.mkv",
        VideoQuality.FullHD,
        "pet.2020.S01E06.WEB-DL.1080p.mkv"
    )]
    [Arguments(
        "lol-) (2011) - S01E08 [HDTV-720p 8-bit x264 AC3 5.1]-BAWLS.mkv",
        VideoQuality.HD,
        "lol.2011.S01E08.WEB-DL.720p.mkv"
    )]
    [Arguments(
        "Show.Name.2024.S01E01.E02.1080p.WEB-DL.DDP5.1.x265-Group.mkv",
        VideoQuality.FullHD,
        "Show.Name.2024.S01E01.E02.WEB-DL.1080p.mkv"
    )]
    public void ShouldNormalizeToSceneStyleWebDlAndMkv_WhenInputContainsMixedReleaseTokens(
        string input,
        VideoQuality quality,
        string expected
    )
    {
        var result = DashOutputFileNameCleaner.NormalizeForDashOutput(input, quality);

        result.ShouldBe(expected);
    }

    [Test]
    public void ShouldUseProvidedQualityAndNotKeepOriginalQualityTokens_WhenOriginalContainsDifferentQuality()
    {
        var result = DashOutputFileNameCleaner.NormalizeForDashOutput(
            "All Quiet on the Western Front (2022) 4K.mkv",
            VideoQuality.HD
        );

        result.ShouldBe("All.Quiet.on.the.Western.Front.2022.WEB-DL.720p.mkv");
    }

    [Test]
    public void ShouldNotDuplicateWebDlTokens_WhenNormalizingAnAlreadyNormalizedName()
    {
        var firstPass = DashOutputFileNameCleaner.NormalizeForDashOutput("AEGIS - 1x01 - Ep1.mov", VideoQuality.FullHD);

        var secondPass = DashOutputFileNameCleaner.NormalizeForDashOutput(firstPass, VideoQuality.FullHD);

        secondPass.ShouldBe("AEGIS.1x01.Ep1.WEB-DL.1080p.mkv");
    }

    [Test]
    public void ShouldStripFilesystemInvalidCharacters_WhenNormalizingFileNameTokens()
    {
        var result = DashOutputFileNameCleaner.NormalizeForDashOutput(
            "Show: Name? * \"<test>|.mkv",
            VideoQuality.FullHD
        );

        result.ShouldBe("Show.Name.test.WEB-DL.1080p.mkv");
    }

    [Test]
    [Arguments("", VideoQuality.FullHD, "download.WEB-DL.1080p.mkv")]
    [Arguments("   ", VideoQuality.HD, "download.WEB-DL.720p.mkv")]
    public void ShouldUseDefaultDownloadName_WhenInputIsEmptyOrWhitespace(
        string input,
        VideoQuality quality,
        string expected
    )
    {
        var result = DashOutputFileNameCleaner.NormalizeForDashOutput(input, quality);

        result.ShouldBe(expected);
    }

    [Test]
    public void ShouldUseDefaultDownloadName_WhenInputIsNull()
    {
        var result = DashOutputFileNameCleaner.NormalizeForDashOutput(null!, VideoQuality.HD);

        result.ShouldBe("download.WEB-DL.720p.mkv");
    }
}
