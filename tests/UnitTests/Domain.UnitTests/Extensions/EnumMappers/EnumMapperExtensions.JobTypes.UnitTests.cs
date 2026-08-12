namespace Reaparr.Domain.UnitTests;

public class EnumMapperExtensionsJobTypesUnitTests : BaseUnitTest
{
    [Test]
    public void ShouldMapAllJobTypesEnumValues_ToStringAndBack()
    {
        foreach (var jobType in Enum.GetValues<JobTypes>())
        {
            var jobTypeString = jobType.ToJobTypesString();

            jobTypeString.ShouldBe(
                jobType.ToString(),
                $"Missing {jobType.ToString()} from {nameof(EnumMapperExtensions)}"
            );

            jobTypeString.ToJobTypes().ShouldBe(jobType);
        }
    }

    [Test]
    public void ShouldMapAllJobTypeNames_ToEnumAndBack()
    {
        foreach (var name in Enum.GetNames<JobTypes>())
        {
            var parsedJobType = name.ToJobTypes();

            parsedJobType.ToString().ShouldBe(name, $"Missing {name} from {nameof(EnumMapperExtensions)}");
            parsedJobType.ToJobTypesString().ShouldBe(name);
        }
    }
}