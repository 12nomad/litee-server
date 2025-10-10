namespace Litee.Application.Helpers;

public class DateRange
{
  public DateOnly StartDate { get; set; }
  public DateOnly EndDate { get; set; }
}


public class Utils
{
  public static DateRange GetDateRange(string? from, string? to, int daysBeforeToday)
  {
    DateOnly startDate;
    if (!string.IsNullOrWhiteSpace(from) && DateOnly.TryParseExact(from, "yyyy-MM-dd", out var parsed))
      startDate = parsed;
    else
      startDate = DateOnly.FromDateTime(DateTime.Today).AddDays(-daysBeforeToday);

    DateOnly endDate;
    if (!string.IsNullOrWhiteSpace(to) && DateOnly.TryParseExact(to, "yyyy-MM-dd", out var anotherParse))
      endDate = anotherParse;
    else
      endDate = DateOnly.FromDateTime(DateTime.Today);

    return new DateRange { StartDate = startDate, EndDate = endDate };
  }
}
