using System.Net;
using System.Security.Claims;
using Litee.Contracts.Common;
using Litee.Contracts.Reports;
using Litee.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Litee.Application.Services.Reports;

public class ReportsService(IHttpContextAccessor httpContextAccessor, DatabaseContext databaseContext) : IReportsService
{
  private readonly DatabaseContext _databaseContext = databaseContext;
  private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

  public async Task<ServicesResult<FinanceResult>> GetReportsAsync(string? from, string? to, int accountId)
  {
    var userId = GetUserId();

    DateOnly startDate;
    if (!string.IsNullOrWhiteSpace(from) && DateOnly.TryParseExact(from, "yyyy-MM-dd", out var parsed))
      startDate = parsed;
    else
      startDate = DateOnly.FromDateTime(DateTime.Today).AddDays(-30);

    DateOnly endDate;
    if (!string.IsNullOrWhiteSpace(to) && DateOnly.TryParseExact(to, "yyyy-MM-dd", out var anotherParse))
      endDate = anotherParse;
    else
      endDate = DateOnly.FromDateTime(DateTime.Today);

    if (startDate > endDate)
      return new ServicesResult<FinanceResult>(false, HttpStatusCode.BadRequest, "Start date should not be greater than end date", null);


    // * with 1 day offset
    var periodLength = (endDate.ToDateTime(TimeOnly.MinValue)
                   - startDate.ToDateTime(TimeOnly.MinValue)).Days;
    var previousPeriodEnd = startDate.AddDays(-1);
    var previousPeriodStart = previousPeriodEnd.AddDays(-periodLength);

    Console.WriteLine(startDate.ToString());
    Console.WriteLine(endDate.ToShortDateString());
    Console.WriteLine(previousPeriodStart.ToShortDateString());
    Console.WriteLine(previousPeriodEnd.ToShortDateString());

    // *
    var currentPeriodSummary = await _databaseContext.Transactions
    .Where(t =>
        t.Date >= startDate &&
        t.Date <= endDate &&
        t.UserId == userId &&
        t.AccountId == accountId
    )
    .GroupBy(_ => 1)
    .Select(g => new FinanceSummary()
    {
      Income = g.Where(t => t.Amount >= 0).Sum(t => t.Amount),
      Expense = g.Where(t => t.Amount < 0).Sum(t => t.Amount),
      Remain = g.Sum(t => t.Amount)
    })
    .FirstOrDefaultAsync();
    // *

    // *
    var previousPeriodSummary = await _databaseContext.Transactions
    .Where(t =>
        t.Date >= previousPeriodStart &&
        t.Date <= previousPeriodEnd &&
        t.UserId == userId &&
        t.AccountId == accountId
    )
    .GroupBy(_ => 1)
    .Select(g => new FinanceSummary()
    {
      Income = g.Where(t => t.Amount >= 0).Sum(t => t.Amount),
      Expense = g.Where(t => t.Amount < 0).Sum(t => t.Amount),
      Remain = g.Sum(t => t.Amount)
    })
    .FirstOrDefaultAsync();
    // *

    // *
    var categoriesSummary = await _databaseContext.Transactions
    .Where(t =>
        t.Date >= startDate &&
        t.Date <= endDate &&
        t.UserId == userId &&
        t.AccountId == accountId &&
        t.Amount < 0
    )
    .Include(t => t.Category)
    .GroupBy(t => t.Category == null ? t.CategoryId.ToString() : t.Category.Name)
    .Select(g => new FinanceSummary
    {
      CategoryKey = g.Key,
      Expense = g.Where(t => t.Amount < 0).Sum(t => t.Amount),
    })
    .OrderBy(t => t.Expense)
    .ToListAsync();
    // *


    // *
    var totalDays = (endDate.ToDateTime(TimeOnly.MinValue)
                - startDate.ToDateTime(TimeOnly.MinValue)).Days;

    // * with 1 day offset
    var allDates = Enumerable.Range(0, totalDays + 1)
        .Select(offset => startDate.AddDays(offset))
        .ToList();

    var transactionsSummary = await _databaseContext.Transactions
        .Where(t =>
            t.Date >= startDate &&
            t.Date <= endDate &&
            t.UserId == userId &&
            t.AccountId == accountId
        )
        .GroupBy(t => t.Date)
        .Select(g => new FinanceSummary()
        {
          DateKey = g.Key,
          Income = g.Where(t => t.Amount >= 0).Sum(t => t.Amount),
          Expense = g.Where(t => t.Amount < 0).Sum(t => t.Amount)
        })
        .ToListAsync();

    var datesSummary = allDates
        .Select(date =>
        {
          var match = transactionsSummary.FirstOrDefault(t => t.DateKey == date);
          return new FinanceSummary
          {
            DateKey = date,
            Income = match?.Income ?? 0,
            Expense = match?.Expense ?? 0,
          };
        })
        .OrderBy(s => s.DateKey)
        .ToList();
    // *

    return new ServicesResult<FinanceResult>(true, null, $"Last {periodLength} days report", new FinanceResult()
    {
      Income = currentPeriodSummary?.Income ?? 0,
      Expense = currentPeriodSummary?.Expense ?? 0,
      Remain = currentPeriodSummary?.Remain ?? 0,
      IncomeDifference = GetDifference(currentPeriodSummary?.Income ?? 0, previousPeriodSummary?.Income ?? 0),
      ExpenseDifference = GetDifference(currentPeriodSummary?.Expense ?? 0, previousPeriodSummary?.Expense ?? 0),
      RemainDifference = GetDifference(currentPeriodSummary?.Remain ?? 0, previousPeriodSummary?.Remain ?? 0),
      CategoriesSummary = categoriesSummary,
      DatesSummary = datesSummary
    });

  }

  // * helpers
  public int? GetUserId()
  {
    var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
    return int.TryParse(claim?.Value, out var id) ? id : null;
  }

  public int GetDifference(int current, int previous)
  {
    if (previous == 0) return previous == current ? 0 : 100;
    return (current - previous) / previous * 100;
  }
}
