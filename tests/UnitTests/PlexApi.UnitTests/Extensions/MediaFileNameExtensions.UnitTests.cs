namespace Reaparr.PlexApi.UnitTests;

public class MediaFileNameExtensionsUnitTests : BaseUnitTest
{
    public MediaFileNameExtensionsUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Theory]
    [InlineData("Dragon Ball Z Battle of Gods (2013) Remux-1080p.mkv", true)]
    [InlineData("Dragon Ball Z Resurrection 'F' (2015) Bluray-1080p.mkv", true)]
    [InlineData("1000 Men and Me - The Bonnie Blue Story (2025).mp4", false)]
    [InlineData("Beyond Boiling Point.mp4", false)]
    [InlineData("Boiling Point (1999) WEBDL-2160p.mkv", true)]
    [InlineData("Feels Good Man (2020) WEBDL-2160p {tmdb-653578}.mkv", true)]
    [InlineData("Free Solo (2018) Bluray-2160p.mp4", true)]
    [InlineData(
        "Pathological The Lies of Joran van der Sloot (2024) [imdb-tt31416709][tmdb-1248739][WEBDL-2160p][EAC3 5.1][h265].mkv",
        true
    )]
    [InlineData(
        "Polar Bear (2022) [imdb-tt17048330][tmdb-927070][WEBDL-2160p][HDR][HDR10][EAC3 Atmos 5.1][HEVC].mkv",
        true
    )]
    [InlineData("Seaspiracy (2021) [imdb-tt14152756][tmdb-801058][WEBDL-2160p][EAC3 5.1][HEVC].mkv", true)]
    [InlineData("Sound of Freedom (2023) Bluray-2160p.mkv", true)]
    [InlineData("Trainwreck Poop Cruise (2025) WEBDL-1080p.mkv", true)]
    [InlineData("What is a Woman! (2022) WEBRip-1080p.mp4", true)]
    [InlineData("What the Health (2017) [WEBRip-2160p x264 8-bit EAC3 2.0]-TrollUHD.mkv", true)]
    [InlineData("The Aristocats (1970) Bluray-1080p.mkv", true)]
    [InlineData("Bambi (1942) Remux-1080p.mkv", true)]
    [InlineData("Bedtime Stories (2008) WEBDL-1080p.mkv", true)]
    [InlineData("Bedtime.Stories.DVDRip.XViD-PUKKA.avi", false)]
    [InlineData("Beverly Hills Chihuahua (2008) Remux-1080p.mkv", true)]
    [InlineData("Beverly Hills Chihuahua.avi", false)]
    [InlineData("Brave (2012) Remux-2160p.mkv", true)]
    [InlineData("Bruce Almighty (2003) WEBDL-2160p.mkv", true)]
    [InlineData("BURN-E.mkv", false)]
    [InlineData("The Chronicles of Narnia Prince Caspian (2008) Remux-1080p.mkv", true)]
    [InlineData("The.Chronicles.Of.Narnia.Prince.Caspian.DVDRip.XViD.CD1-PUKKA.avi", true)]
    [InlineData("City of Ember (2008) Bluray-1080p.mkv", true)]
    [InlineData("Coraline (2009) Bluray-2160p.mkv", true)]
    [InlineData("arw-coraline-dvdrip.avi", false)]
    [InlineData("The Croods (2013) Bluray-2160p.m2ts", true)]
    [InlineData("DC League of Super-Pets (2022) Bluray-2160p.mkv", true)]
    [InlineData("Dead.Space.Downfall.2008.DVDRip.XViD-WPi.avi", true)]
    [InlineData("Despicable Me (2010) Remux-2160p Proper.mp4", true)]
    [InlineData("Dragonball Evolution (2009) Bluray-1080p.mkv", true)]
    [InlineData("done-dragonevo.avi", false)]
    [InlineData("Drillbit Taylor (2008) WEBDL-2160p.mkv", true)]
    [InlineData("dmd-drillbittaylor.avi", false)]
    [InlineData("Encanto (2021) Bluray-2160p.mkv", true)]
    [InlineData("Evan Almighty (2007) Remux-1080p.mkv", true)]
    [InlineData("Fantastic Four Rise of the Silver Surfer (2007) Bluray-2160p.mkv", true)]
    [InlineData("F4 Rise of the Silver Surfer.avi", false)]
    [InlineData("Finding Nemo (2003) Remux-2160p Proper.mkv", true)]
    [InlineData("The Forbidden Kingdom (2008) Bluray-2160p.mp4", true)]
    [InlineData("The Forbidden Kingdom.avi", false)]
    [InlineData("The Good Dinosaur (2015) Remux-2160p.mkv", true)]
    [InlineData("Happy Feet Two (2011) Remux-1080p.mkv", true)]
    [InlineData("Harry Potter and the Chamber of Secrets (2002) Remux-2160p Proper.mkv", true)]
    [InlineData("Hellboy II The Golden Army (2008) WEBDL-2160p.mkv", true)]
    [InlineData("Hellboy.II.The.Golden.Army.DVDRip.XviD.CD1-DiAMOND.avi", true)]
    [InlineData("High School Musical 3 Senior Year (2008) Remux-1080p.mkv", true)]
    [InlineData("High School Musical 3 DVDRip XViD-PUKKA cd1.avi", true)]
    [InlineData("Hocus Pocus 2 (2022) WEBDL-2160p.mkv", true)]
    [InlineData("Home Alone (1990) Bluray-2160p.mkv", true)]
    [InlineData("Home Alone 3 (1997) WEBRip-1080p.mp4", true)]
    [InlineData("How the Grinch Stole Christmas! (1966) Bluray-1080p.mkv", true)]
    [InlineData("How.to.Train.Your.Dragon.2010.UHD.BluRay.2160p.DTS-X.7.1.HEVC.REMUX-FraMeSToR.mkv", true)]
    [InlineData("How to Train Your Dragon 2 (2014) Bluray-2160p.mkv", true)]
    [InlineData("Ice Age A Mammoth Christmas (2011) Bluray-1080p.mkv", true)]
    [InlineData("The Incredible Hulk (2008) Bluray-2160p.mkv", true)]
    [InlineData("The.Incredible.Hulk.DVDRip.XviD.CD1-DoNE.avi", true)]
    [InlineData("The Incredibles (2004) Remux-2160p.mkv", true)]
    [InlineData("Inside Out (2015) Remux-2160p.mkv", true)]
    [InlineData("It's a SpongeBob Christmas! (2012) Bluray-720p.mkv", true)]
    [InlineData("It's a Wonderful Life (1946) Bluray-1080p.mkv", true)]
    [InlineData("Journey to the Center of the Earth (2008) Remux-1080p.mkv", true)]
    [InlineData("Journey.To.The.Center.Of.The.Earth.DVDRip.x264-era.mp4", false)]
    [InlineData("The.Last.Airbender.2010.1080p.BluRay.AC3.DL.x264-HDC.mkv", true)]
    [InlineData("Marry Me For Christmas (2013) WEBDL-1080p.mkv", true)]
    [InlineData("Meet Dave (2008) Bluray-1080p.mkv", true)]
    [InlineData("Meet Dave DVDRip ARROW.avi", false)]
    [InlineData("Meet Me in St. Louis (1944) Bluray-1080p.mp4", true)]
    [InlineData("Mickey's Christmas Carol (1983) Bluray-1080p.mp4", true)]
    [InlineData("Minions (2015) Remux-2160p.mkv", true)]
    [InlineData("Miracle on 34th Street (1947) Bluray-1080p.mp4", true)]
    [InlineData("Noelle (2019) WEBRip-1080p.mkv", true)]
    [InlineData("Olaf's Frozen Adventure (2017) Bluray-1080p.mkv", true)]
    [InlineData("Onward (2020) Bluray-1080p.mkv", true)]
    [InlineData("The Pink Panther 2 (2009) WEBDL-1080p.mkv", true)]
    [InlineData("Pink Panther 2.avi", false)]
    [InlineData("Puss in Boots (2011) Bluray-2160p.mkv", true)]
    [InlineData("Shark Tale (2004) Remux-1080p.mkv", true)]
    [InlineData("Shrek (2001) Bluray-2160p.mkv", true)]
    [InlineData("Shrek.the.Third.2007.1080p.BluRay.REMUX.AVC.TrueHD.7.1-EPSiLON.mkv", true)]
    [InlineData("Speed Racer (2008) Bluray-1080p.mkv", true)]
    [InlineData("Speed.Racer.DVDRip.XviD.CD1-ARROW.avi", true)]
    [InlineData(
        "The SpongeBob Movie Sponge on the Run (2020) [imdb-tt4823776][tmdb-400160][WEBDL-2160p][HDR][HDR10][EAC3 Atmos 5.1][h265].mkv",
        true
    )]
    [InlineData("vmt-spyool-xvid.avi", false)]
    [InlineData("The Super Mario Bros. Movie (2023) Bluray-2160p Proper.mkv", true)]
    [InlineData("Toy.Story.1995.1080p.BluRay.HEBDUB.Also.English.DTS-ES.x264-ZionHD.mkv", true)]
    [InlineData("A Year And A Half In The Life of Metallica part I.avi", true)]
    [InlineData("A Year And A Half In The Life of Metallica part II.avi", true)]
    [InlineData("239 100 Totally Accurate Accents From Countries All Around The World Part 2.mp4", true)]
    [InlineData("Some Movie part 1.mp4", true)]
    [InlineData("Another Movie Part 3.avi", true)]
    [InlineData("Test Movie part IV.mkv", true)]
    [InlineData("Ghost in the Shell (ARISE - A.A) - Pyrophoric Cult, Part 1 of 2 (2015 - 1080p JAP Audio).mkv", true)]
    [InlineData("Harry Potter and the Deathly Hallows Part 2 (2011) WEBDL-1080p.mkv", true)]
    [InlineData("The Hunger Games Mockingjay - Part 2 (2015) Bluray-1080p.mkv", true)]
    [InlineData("Rambo First Blood Part II (1985) Bluray-2160p.mkv", true)]
    [InlineData("A Quiet Place Part II (2021) Remux-2160p Proper.mkv", true)]
    public void ShouldReturnExpectedResult_WhenValidatingMovieFileName(string fileName, bool expected)
    {
        // Act
        var result = fileName.IsValidMediaFileName();

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("The.X-Files.S06E10.Tithonus.x265.HEVC-Qman[UTR].mkv", true)]
    [InlineData("The.X-Files.S06E20.Three.of.a.Kind.x265.HEVC-Qman[UTR].mkv", true)]
    [InlineData("The.X-Files.S08E16.Three.Words.x265.HEVC-Qman[UTR].mkv", true)]
    [InlineData("The.X-Files.S08E14.This.is.Not.Happening.x265.HEVC-Qman[UTR].mkv", true)]
    [InlineData("The.X-Files.S07E14.Theef.x265.HEVC-Qman[UTR].mkv", true)]
    [InlineData("The.X-Files.S03E07.The.Walk.x265.HEVC-Qman[UTR].mkv", true)]
    [InlineData("The.X-Files.S06E19.The.Unnatural.x265.HEVC-Qman[UTR].mkv", true)]
    [InlineData("The.X-Files.S09E19.The.Truth.x265.HEVC-Qman[UTR].mkv", true)]
    [InlineData("Spartacus - S01E04 - The Thing in the Pit.mkv", true)]
    [InlineData("The.X-Files.S07E02.The.Sixth.Extinction.II.Amor.Fati.x265.HEVC-Qman[UTR].mkv", true)]
    [InlineData("The.X-Files.S07E01.The.Sixth.Extinction.x265.HEVC-Qman[UTR].mkv", true)]
    [InlineData("The.X-Files.S05E14.The.Red.And.The.Black.x265.HEVC-Qman[UTR].mkv", true)]
    [InlineData("Spartacus - S01E02 - Sacramentum Gladiatorum.mkv", true)]
    [InlineData("Spartacus - S02E07 - Sacramentum.mkv", true)]
    public void ShouldReturnExpectedResult_WhenValidatingTvEpisodeFileName(string fileName, bool expected)
    {
        // Act
        var result = fileName.IsValidMediaFileName();

        // Assert
        result.ShouldBe(expected);
    }
}
