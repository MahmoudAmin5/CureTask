namespace Cure.Application.DTOs.Clinical;

public sealed record PagedRequestDto(
    int Page = 1,
    int PageSize = 10);
