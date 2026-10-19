namespace Reaparr.Application.UnitTests;

public class PhotoSourcePathResolverUnitTests
{
    [Test]
    public void ShouldMatchPosixPathsCaseSensitivelyAndRequireCompleteComponents()
    {
        // Arrange
        var roots = new[] { new PhotoSourceRootPath("root", "/photos/Family") };

        // Act
        var matched = PhotoSourcePathResolver.TryResolve(roots, "/photos/Family/2026/photo.jpg", out var resolution, out var error);
        var wrongCase = PhotoSourcePathResolver.TryResolve(roots, "/photos/family/2026/photo.jpg", out _, out _);
        var prefixLookalike = PhotoSourcePathResolver.TryResolve(roots, "/photos/Family-old/photo.jpg", out _, out _);

        // Assert
        matched.ShouldBeTrue(error);
        resolution.ShouldNotBeNull();
        resolution.RelativePath.ShouldBe("2026/photo.jpg");
        resolution.RelativeDirectory.ShouldBe("2026");
        resolution.FileName.ShouldBe("photo.jpg");
        wrongCase.ShouldBeFalse();
        prefixLookalike.ShouldBeFalse();
    }

    [Test]
    public void ShouldSelectLongestWindowsRootAndCanonicalizeCaseAndSeparators()
    {
        // Arrange
        var roots = new[]
        {
            new PhotoSourceRootPath("broad", "D:/Pictures"),
            new PhotoSourceRootPath("specific", @"d:\Pictures\Family"),
        };

        // Act
        var matched = PhotoSourcePathResolver.TryResolve(roots, @"D:\pictures\family\Trips\IMG001.JPG", out var resolution, out var error);

        // Assert
        matched.ShouldBeTrue(error);
        resolution.ShouldNotBeNull();
        resolution.IdentityKey.ShouldBe("specific");
        resolution.RelativePath.ShouldBe("Trips/IMG001.JPG");
        resolution.CanonicalRelativePath.ShouldBe("TRIPS/IMG001.JPG");
        resolution.CanonicalRelativeDirectory.ShouldBe("TRIPS");
    }

    [Test]
    public void ShouldRejectEqualSpecificityRootsInsteadOfChoosingFirst()
    {
        // Arrange
        var roots = new[]
        {
            new PhotoSourceRootPath("first", "/photos/Family"),
            new PhotoSourceRootPath("second", "/photos/Family"),
        };

        // Act
        var matched = PhotoSourcePathResolver.TryResolve(roots, "/photos/Family/photo.jpg", out var resolution, out var error);

        // Assert
        matched.ShouldBeFalse();
        resolution.ShouldBeNull();
        error.ShouldContain("ambiguously");
    }

    [Test]
    public void ShouldSupportUncRootsOnAnyHost()
    {
        // Arrange
        var roots = new[] { new PhotoSourceRootPath("unc", @"\\server\Share\Photos") };

        // Act
        var matched = PhotoSourcePathResolver.TryResolve(roots, @"//SERVER/share/photos\2026\photo.jpg", out var resolution, out var error);

        // Assert
        matched.ShouldBeTrue(error);
        resolution.ShouldNotBeNull();
        resolution.Style.ShouldBe(PhotoSourcePathStyle.Unc);
        resolution.RelativeDirectory.ShouldBe("2026");
        resolution.CanonicalRelativePath.ShouldBe("2026/PHOTO.JPG");
    }

    [Test]
    public void ShouldIgnoreInactiveRootsButRejectInvalidActiveRoots()
    {
        // Arrange
        var inactiveOnly = new[] { new PhotoSourceRootPath("old", "relative/path", false) };
        var invalidActive = new[] { new PhotoSourceRootPath("bad", "relative/path") };

        // Act
        var inactiveResult = PhotoSourcePathResolver.TryResolve(inactiveOnly, "/photos/photo.jpg", out _, out _);
        var invalidResult = PhotoSourcePathResolver.TryResolve(invalidActive, "/photos/photo.jpg", out _, out var error);

        // Assert
        inactiveResult.ShouldBeFalse();
        invalidResult.ShouldBeFalse();
        error.ShouldContain("absolute");
    }

    [Test]
    public void ShouldRejectRelativeDriveRelativeTraversalNulAndMixedStyles()
    {
        // Arrange
        var roots = new[] { new PhotoSourceRootPath("posix", "/photos") };

        // Act
        var relative = PhotoSourcePathResolver.TryResolve(roots, "photos/photo.jpg", out _, out _);
        var driveRelative = PhotoSourcePathResolver.TryResolve(roots, "C:photos/photo.jpg", out _, out _);
        var traversal = PhotoSourcePathResolver.TryResolve(roots, "/photos/../secret.jpg", out _, out _);
        var nul = PhotoSourcePathResolver.TryResolve(roots, "/photos/photo\0.jpg", out _, out _);
        var mixedStyle = PhotoSourcePathResolver.TryResolve(roots, @"C:\photos\photo.jpg", out _, out _);

        // Assert
        relative.ShouldBeFalse();
        driveRelative.ShouldBeFalse();
        traversal.ShouldBeFalse();
        nul.ShouldBeFalse();
        mixedStyle.ShouldBeFalse();
    }

    [Test]
    public void ShouldCreateStablePathIdentityKeysWithSourceStyle()
    {
        // Arrange
        var posix = PhotoSourcePathResolver.TryCreateIdentityKey("/Photos/A", out var posixKey, out var posixError);
        var windows = PhotoSourcePathResolver.TryCreateIdentityKey(@"c:\Photos\A", out var windowsKey, out var windowsError);

        // Act
        var posixAgain = PhotoSourcePathResolver.TryCreateIdentityKey("/Photos/A", out var posixKeyAgain, out _);

        // Assert
        posix.ShouldBeTrue(posixError);
        windows.ShouldBeTrue(windowsError);
        posixAgain.ShouldBeTrue();
        posixKey.ShouldBe("path:posix:/Photos/A");
        windowsKey.ShouldBe("path:windows:C:/PHOTOS/A");
        posixKeyAgain.ShouldBe(posixKey);
        windowsKey.ShouldNotBe(posixKey);
    }
}
