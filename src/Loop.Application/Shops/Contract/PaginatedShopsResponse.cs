namespace Loop.Application.Shops.Contract;

public sealed class PaginatedShopsResponse
{
    public List<GetShopsResponse> Data { get; set; } = [];
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}
