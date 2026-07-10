namespace Reaparr.Data.UnitTests;

public class MediaSortNormalizerUnitTests : BaseUnitTest<MediaSortNormalizerUnitTests>
{
    [Test]
    public void ShouldReturnSortIndex_WhenDefaultSortWithSingleLibrary()
    {
        // Arrange
        const string? sort = null;
        const int libraryCount = 1;

        // Act
        var result = MediaSortNormalizer.Normalize(sort, libraryCount);

        // Assert
        result.ShouldNotBeNull();
        result.Field.ShouldBe("sortIndex");
        result.Descending.ShouldBeFalse();
    }

    [Test]
    public void ShouldReturnSearchTitle_WhenDefaultSortWithMultipleLibraries()
    {
        // Arrange
        const string? sort = null;
        const int libraryCount = 3;

        // Act
        var result = MediaSortNormalizer.Normalize(sort, libraryCount);

        // Assert
        result.ShouldNotBeNull();
        result.Field.ShouldBe("SearchTitle");
        result.Descending.ShouldBeFalse();
    }

    [Test]
    public void ShouldReturnNull_WhenUnsupportedSortField()
    {
        // Arrange
        const string sort = "invalidField:asc";
        const int libraryCount = 1;

        // Act
        var result = MediaSortNormalizer.Normalize(sort, libraryCount);

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public void ShouldReturnNull_WhenMultiSegmentSort()
    {
        // Arrange
        const string sort = "year:asc,title:desc";
        const int libraryCount = 2;

        // Act
        var result = MediaSortNormalizer.Normalize(sort, libraryCount);

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public void ShouldReturnNull_WhenInvalidDirection()
    {
        // Arrange
        const string sort = "sortIndex:up";
        const int libraryCount = 1;

        // Act
        var result = MediaSortNormalizer.Normalize(sort, libraryCount);

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public void ShouldReturnDescending_WhenDescDirection()
    {
        // Arrange
        const string sort = "year:desc";
        const int libraryCount = 2;

        // Act
        var result = MediaSortNormalizer.Normalize(sort, libraryCount);

        // Assert
        result.ShouldNotBeNull();
        result.Field.ShouldBe("Year");
        result.Descending.ShouldBeTrue();
    }

    [Test]
    [Arguments("addedAt")]
    [Arguments("AddedAt")]
    [Arguments("duration")]
    [Arguments("Duration")]
    [Arguments("mediaSize")]
    [Arguments("MediaSize")]
    [Arguments("quality")]
    public void ShouldNormalizeAlias_WhenUsingShorthandField(string sortField)
    {
        // Arrange
        var sort = $"{sortField}:asc";
        const int libraryCount = 2;

        // Act
        var result = MediaSortNormalizer.Normalize(sort, libraryCount);

        // Assert
        result.ShouldNotBeNull();
        result.Field.ToLowerInvariant().ShouldBe(sortField.ToLowerInvariant());
        result.Descending.ShouldBeFalse();
    }

    [Test]
    [Arguments("sortTitle")]
    [Arguments("title")]
    [Arguments("sortIndex")]
    [Arguments("SortIndex")]
    [Arguments("SearchTitle")]
    public void ShouldNormalizeTitleField_ToSortIndex_WhenSingleLibrary(string sortField)
    {
        // Arrange
        var sort = $"{sortField}:asc";
        const int libraryCount = 1;

        // Act
        var result = MediaSortNormalizer.Normalize(sort, libraryCount);

        // Assert
        result.ShouldNotBeNull();
        result.Field.ShouldBe("sortIndex");
        result.Descending.ShouldBeFalse();
    }

    [Test]
    [Arguments("sortTitle")]
    [Arguments("title")]
    [Arguments("sortIndex")]
    [Arguments("SearchTitle")]
    public void ShouldNormalizeTitleField_ToSearchTitle_WhenMultipleLibraries(string sortField)
    {
        // Arrange
        var sort = $"{sortField}:asc";
        const int libraryCount = 5;

        // Act
        var result = MediaSortNormalizer.Normalize(sort, libraryCount);

        // Assert
        result.ShouldNotBeNull();
        result.Field.ShouldBe("SearchTitle");
        result.Descending.ShouldBeFalse();
    }

    [Test]
    public void ShouldReturnAscending_WhenNoDirectionSpecified()
    {
        // Arrange
        const string sort = "year";
        const int libraryCount = 1;

        // Act
        var result = MediaSortNormalizer.Normalize(sort, libraryCount);

        // Assert
        result.ShouldNotBeNull();
        result.Field.ShouldBe("Year");
        result.Descending.ShouldBeFalse();
    }

    [Test]
    public void ShouldReturnAscending_WhenEmptyDirection()
    {
        // Arrange
        const string sort = "year:";
        const int libraryCount = 1;

        // Act
        var result = MediaSortNormalizer.Normalize(sort, libraryCount);

        // Assert
        result.ShouldNotBeNull();
        result.Field.ShouldBe("Year");
        result.Descending.ShouldBeFalse();
    }
}
