namespace CaptainPayment.Core.Models;

public class PagedResult<T>
{
    public List<T> Data { get; set; } = new ();
    public bool HasMore { get; set; }
    public int? TotalCount { get; set; }
}