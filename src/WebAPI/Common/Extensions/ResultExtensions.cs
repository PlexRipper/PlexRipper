using PlexRipper.WebAPI.Common.FluentResult;
using Riok.Mapperly.Abstractions;

namespace PlexRipper.WebAPI.Common.Extensions;

[Mapper]
public static partial class ResultExtensions
{
    public static partial ResultDTO ToResultDTO(this Result result);
    private static partial List<ReasonDTO> ToReasonDTOs(this List<IReason> reasons);
    private static partial List<ErrorDTO> ToErrorDTOs(this List<IError> reasons);
    private static partial List<SuccessDTO> ToSuccessDTOs(this List<ISuccess> reasons);

    public static ResultDTO<TTarget> ToResultDTO<TSource, TTarget>(this Result<TSource> result, TTarget Value) => new()
    {
        Value = Value,
        IsFailed = result.IsFailed,
        IsSuccess = result.IsSuccess,
        Reasons = result.Reasons.ToReasonDTOs(),
        Errors = result.Errors.ToErrorDTOs(),
        Successes = result.Successes.ToSuccessDTOs(),
    };
}