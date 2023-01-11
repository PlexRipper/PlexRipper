using Quartz;

namespace BackgroundServices.Base;

public interface IBaseJob : IJob
{
    // TODO   IBaseJob.cs(7, 35): [CA2252] Using both 'static' and 'abstract' modifiers requires opting into preview features. See https://aka.ms/dotnet-warnings/preview-features for more information.
    //  public static abstract JobKey GetJobKey(int id);
}