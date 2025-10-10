using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using GenerativeAI;
using GenerativeAI.Types;
using Litee.Application.Helpers;
using Litee.Contracts.Common;
using Litee.Contracts.Transactions;
using Litee.Domain;
using Litee.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Litee.Application.Services.Transactions;

public class TransactionService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration, DatabaseContext databaseContext) : ITransactionService
{
  private readonly DatabaseContext _databaseContext = databaseContext;
  private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
  private const int DaysBeforeToday = 29; // * Today included * //
  private string _geminiApiKey = configuration["Api_Keys:Gemini"]
                                   ?? throw new InvalidOperationException("API key not found.");

  public async Task<PaginatedServicesResult<List<Transaction>, EmptyMetadata>> GetTransactionsAsync(TransactionsPaginationAndFilteringRequest request)
  {
    var dateRange = Utils.GetDateRange(request.From, request.To, DaysBeforeToday);

    if (dateRange.StartDate > dateRange.EndDate)
      return new PaginatedServicesResult<List<Transaction>, EmptyMetadata>(false, HttpStatusCode.BadRequest, "Start date should not be greater than end date", null);


    var query = _databaseContext.Transactions
      .AsQueryable();

    // * Filter
    query = query.Where(
      t => t.UserId == GetUserId() &&
      t.Date >= dateRange.StartDate &&
      t.Date <= dateRange.EndDate &&
      (!request.AccountId.HasValue || t.AccountId == request.AccountId.Value) &&
      (!request.CategoryId.HasValue || t.CategoryId == request.CategoryId.Value)
    );
    // if (!string.IsNullOrEmpty(request.Search))
    //   query.Where(t => t.Description.ToLower().Contains(request.Search.Trim().ToLower()));

    // * Sort
    query = request.OrderBy switch
    {
      "amount" => query.OrderBy(t => t.Amount),
      "amountDesc" => query.OrderByDescending(t => t.Amount),
      _ => query.OrderByDescending(t => t.Date)
    };

    // * Pagination
    var count = await query.CountAsync();
    var transactions = await query
       .Select(t => new Transaction
       {
         Id = t.Id,
         Description = t.Description,
         Amount = t.Amount,
         Payee = t.Payee,
         Date = t.Date,
         UserId = t.UserId,
         AccountId = t.AccountId,
         CategoryId = t.CategoryId,
         ReceiptId = t.ReceiptId,
         Category = t.Category == null ? null : new Category
         {
           Id = t.Category.Id,
           Name = t.Category.Name
         },
         Account = new Account()
         {
           Id = t.Account.Id,
           Name = t.Account.Name
         },
         Receipt = t.Receipt == null ? null : new Receipt
         {
           Id = t.Receipt.Id,
           Base64Image = t.Receipt.Base64Image
         }
       })
      .Skip((request.Page - 1) * request.PageSize)
      .Take(request.PageSize)
      .ToListAsync();

    return new PaginatedServicesResult<List<Transaction>, EmptyMetadata>(true, null, null, transactions, count, null);
  }

  public Task<ServicesResult<Transaction>> GetTransactionAsync(int id)
  {
    throw new NotImplementedException();
  }


  public async Task<ServicesResult<Transaction>> CreateTransactionAsync(CreateTransactionRequest request)
  {
    var userId = GetUserId();
    var transaction = await _databaseContext.Transactions.FirstOrDefaultAsync(a => a.UserId == userId && a.AccountId == request.AccountId && a.Description.ToLower() == request.Description.ToLower());

    if (transaction is not null)
      return new ServicesResult<Transaction>(false, HttpStatusCode.BadRequest, "Transaction with the same description already exists", null);

    var newTransaction = new Transaction
    {
      Description = request.Description,
      Amount = request.Amount,
      Payee = request.Payee,
      Date = request.Date,
      AccountId = request.AccountId,
      UserId = userId ?? 0,
      CategoryId = request.CategoryId
    };

    if (request.ReceiptId.HasValue)
    {
      newTransaction.ReceiptId = request.ReceiptId.Value;
    }

    await _databaseContext.Transactions.AddAsync(newTransaction);
    await _databaseContext.SaveChangesAsync();
    return new ServicesResult<Transaction>(true, null, null, newTransaction);
  }

  public async Task<ServicesResult<Transaction>> UpdateTransactionAsync(int id, CreateTransactionRequest request)
  {
    var userId = GetUserId();
    var transaction = await _databaseContext.Transactions.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

    if (transaction is null)
      return new ServicesResult<Transaction>(false, HttpStatusCode.NotFound, "Transaction not found", null);

    if (transaction.Description.ToLower() != request.Description.ToLower())
    {
      var existingTransaction = await _databaseContext.Transactions.FirstOrDefaultAsync(a => a.UserId == userId && a.AccountId == request.AccountId && a.Description.ToLower() == request.Description.ToLower());
      if (existingTransaction is not null)
        return new ServicesResult<Transaction>(false, HttpStatusCode.BadRequest, "Transaction with this description already exists", null);
    }

    transaction.Description = request.Description;
    transaction.Amount = request.Amount;
    transaction.Payee = request.Payee;
    transaction.Date = request.Date;
    transaction.AccountId = request.AccountId;
    transaction.CategoryId = request.CategoryId;
    await _databaseContext.SaveChangesAsync();
    return new ServicesResult<Transaction>(true, null, null, transaction);
  }

  public async Task<ServicesResult<List<Transaction>>> BulkCreateAsync(BulkCreateTransactionRequest request)
  {
    var userId = GetUserId();

    var invalidItems = new List<string>();

    var transactions = new List<Transaction>();

    foreach (var t in request.Transactions)
    {
      if (!int.TryParse(t.Amount.ToString(), out var parsedAmount))
      {
        invalidItems.Add($"Invalid Amount for transaction: {t.Description}");
        continue;
      }

      if (!DateOnly.TryParse(t.Date.ToString(), out var parsedDate))
      {
        invalidItems.Add($"Invalid Date format for transaction: {t.Date}");
        continue;
      }

      transactions.Add(new Transaction
      {
        Amount = parsedAmount,
        Date = parsedDate,
        Description = t.Description,
        Payee = t.Payee,
        AccountId = t.AccountId,
        UserId = userId ?? 0
      });
    }

    if (invalidItems.Any())
    {
      return new ServicesResult<List<Transaction>>(
          false,
          null,
          "Some transactions have errors. Please verify the data you provided before continuing",
          null
      );
    }

    await _databaseContext.Transactions.AddRangeAsync(transactions);
    await _databaseContext.SaveChangesAsync();

    return new ServicesResult<List<Transaction>>(true, null, "Transactions created successfully", null);
  }

  public async Task<ServicesResult<List<Transaction>>> BulkDeleteAsync(BulkDeleteTransactionRequest request)
  {
    var userId = GetUserId();
    var transactions = await _databaseContext.Transactions
      .Where(t => request.TransactionIds!.Contains(t.Id) && t.UserId == userId)
      .Select(t => new Transaction()
      {
        Id = t.Id,
        Description = t.Description,
        Amount = t.Amount,
        Payee = t.Payee,
        Date = t.Date,
        AccountId = t.AccountId,
        UserId = t.UserId,
        Category = t.Category == null ? null : new Category
        {
          Id = t.Category.Id,
          Name = t.Category.Name
        },
        Account = new Account
        {
          Id = t.Account.Id,
          Name = t.Account.Name
        }
      })
      .ToListAsync();

    if (transactions.Count == 0)
      return new ServicesResult<List<Transaction>>(false, HttpStatusCode.NotFound, "No matching transactions found", null);

    _databaseContext.Transactions.RemoveRange(transactions);
    await _databaseContext.SaveChangesAsync();

    return new ServicesResult<List<Transaction>>(true, null, "Transactions deleted successfully", null);
  }

  public async Task<ServicesResult<Transaction>> DeleteTransactionAsync(int id)
  {
    var userId = GetUserId();
    var transaction = await _databaseContext.Transactions
      .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

    if (transaction is null)
      return new ServicesResult<Transaction>(false, HttpStatusCode.NotFound, "Transaction not found", null);

    _databaseContext.Transactions.Remove(transaction);
    await _databaseContext.SaveChangesAsync();

    return new ServicesResult<Transaction>(true, null, "Transaction deleted successfully", null);
  }

  public async Task<ServicesResult<ScanReceiptResponse>> ScanReceiptAsync(IFormFile file)
  {
    if (file.Length > 5 * 1024 * 1024) // * 5MB limit
      return new ServicesResult<ScanReceiptResponse>(false, HttpStatusCode.BadRequest, "Max image size is 5MB", null);

    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
    if (!allowedExtensions.Contains(Path.GetExtension(file.FileName).ToLower()))
      return new ServicesResult<ScanReceiptResponse>(false, HttpStatusCode.BadRequest, "Only .jpg, .jpeg, .png and .webp formats are supported", null);

    try
    {
      var googleAI = new GoogleAi(_geminiApiKey);
      var model = googleAI.CreateGenerativeModel("models/gemini-2.5-flash");

      // * Generate buffer for the model
      var memoryStream = new MemoryStream();
      await file.CopyToAsync(memoryStream);
      var buffer = memoryStream.ToArray();
      var base64Image = Convert.ToBase64String(buffer);
      var dataUri = $"data:{file.ContentType};base64,{base64Image}";

      var request = new GenerateContentRequest();
      string ReceiptScanPrompt = $@"
        Analyze the attached image. If it is a valid receipt, strictly extract the requested financial data.

        1.  **Total amount:** Extract the grand total number.
        2.  **Payee Name:** Extract the official name of the merchant or store.
        3.  **Date:** Extract the transaction date in YYYY-MM-DD format.
        4.  **Description:** Provide a concise summary (max 80 characters) of the items purchased or a brief description of the receipt content.

        If the image is not a receipt or lacks essential data, return an empty JSON object: {{}}.

        If the image is a receipt, follow this exact JSON format:
        {{
          ""amount"": ""number"",
          ""date"": ""YYYY-MM-DD date format as string"",
          ""description"": ""string"",
          ""payee"": ""string"",
        }}
        If some data is ambiguous, return the value as empty string but keep the JSON structure as it is.
      ";
      request.AddText(ReceiptScanPrompt);
      request.AddInlineData(base64Image, file.ContentType);
      var response = await model.GenerateContentAsync(request);

      if (response.Text() is null)
        return new ServicesResult<ScanReceiptResponse>(false, HttpStatusCode.ExpectationFailed, "Receipt could not be parsed", null);

      var match = GenAiTextRegex.JsonBlockRegex().Match(response.Text() ?? "");
      if (!match.Success)
        throw new Exception("No valid JSON block found");

      var json = match.Groups[1].Value;
      var textResponse = JsonSerializer.Deserialize<ScanReceiptResponse>(json.Trim());

      // * Saving the image to the database
      var receipt = await _databaseContext.Receipts.AddAsync(new Receipt { Base64Image = dataUri });
      await _databaseContext.SaveChangesAsync();

      return new ServicesResult<ScanReceiptResponse>(true, null, "Receipt parsed successfully", new ScanReceiptResponse
      {
        Amount = textResponse?.Amount ?? "",
        Date = textResponse?.Date ?? "",
        Description = textResponse?.Description ?? "",
        Payee = textResponse?.Payee ?? "",
        Base64Image = dataUri ?? "",
        ReceiptId = receipt.Entity.Id
      });
    }
    catch (Exception e)
    {
      return new ServicesResult<ScanReceiptResponse>(false, HttpStatusCode.ServiceUnavailable, e.Message, null);
    }
  }

  public async Task<ServicesResult<GenAiInsightTextResponse>> GetInsightAsync(GetInsightRequest request)
  {
    const int InsightDaysBeforeToday = 6;
    var dateRange = Utils.GetDateRange(request.From, request.To, InsightDaysBeforeToday);
    var query = _databaseContext.Transactions
      .AsQueryable();

    // * Filter
    query = query.Where(
      t => t.UserId == GetUserId() &&
      t.Date >= dateRange.StartDate &&
      t.Date <= dateRange.EndDate &&
      (!request.AccountId.HasValue || t.AccountId == request.AccountId.Value) &&
      (!request.CategoryId.HasValue || t.CategoryId == request.CategoryId.Value)
    );

    // * Map
    query = query.OrderByDescending(t => t.Date);
    var transactions = await query
       .Select(t => new GenAiInsightSeed
       {
         Description = t.Description,
         Amount = t.Amount / 1000, // * Convert to K
         Payee = t.Payee,
         Date = t.Date,
         Account = t.Account.Name,
         Category = t.Category != null ? t.Category.Name : "Uncathegorized"
       })
      .ToListAsync();

    if (transactions.Count < 10)
      return new ServicesResult<GenAiInsightTextResponse>(false, HttpStatusCode.BadRequest, "Not enough transactions to generate insight. Provide at least 10 transactions or more inside the date range", null);

    string seed = JsonSerializer.Serialize(transactions, new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
    string InsightPrompt = $@"
        You are a highly analytical, non-judgemental Personal Financial Advisor AI.
        Your task is to analyze the provided raw transaction data for a single user and generate a comprehensive financial insight report.

        **CRITICAL DATA INSTRUCTION:**
        * **Transaction Type:** An 'Amount' that is **positive** represents **Income**. An 'Amount' that is **negative** represents an **Expense**.
        * **Amount Currency** For each an every 'Amount' the currency is in Euro so use the Euro sign € to indicate the currency.
        * **Character Limit:** For each insight characters should be at around 400 characters or less **.

        RULES:
        1.  **Analyze Holistically:** Consider dates, categories, payees, and accounts to find meaningful trends and risks, *using the calculated true amount*.
        2.  **Actuarial Advice:** Provide meaningful, strategic, and actionable advice.
        3.  **Strict JSON Format:** Your entire response must be a single, valid JSON object following the provided schema, with insights populated for every section.
        4.  **Data Structure:** The input data is an array of Transaction objects, each including its associated Account and Category names.

        --- TRANSACTION DATA TO ANALYZE ---
        {seed} 
        --- END DATA ---

        For the ouput of your analytics, provide a JSON object with the following structure:
        {{
          ""spendingHabitsSummary"": ""string"",
          ""budgetaryAlertsAndRisks"": ""string"",
          ""incomeAndSavingsPotential"": ""string"",
          ""accountHealthAndCashFlow"": ""string"",
          ""strategicRecommendations"": ""string""
        }}

        Generate the financial insight report now.
      ";

    try
    {
      var googleAI = new GoogleAi(_geminiApiKey);
      var model = googleAI.CreateGenerativeModel("models/gemini-2.5-flash");
      var contentRequest = new GenerateContentRequest();
      contentRequest.AddText(InsightPrompt);
      var response = await model.GenerateContentAsync(contentRequest);

      if (response.Text() is null)
        return new ServicesResult<GenAiInsightTextResponse>(false, HttpStatusCode.ExpectationFailed, "Unable to generate insight", null);

      var match = GenAiTextRegex.JsonBlockRegex().Match(response.Text() ?? "");
      if (!match.Success)
        throw new Exception("No valid JSON block found");

      var json = match.Groups[1].Value;
      var textResponse = JsonSerializer.Deserialize<GenAiInsightTextResponse>(json.Trim());

      return new ServicesResult<GenAiInsightTextResponse>(true, null, "Insight generated successfully", textResponse);
    }
    catch (Exception e)
    {
      return new ServicesResult<GenAiInsightTextResponse>(false, HttpStatusCode.ServiceUnavailable, e.Message, null);
    }
  }


  // * helpers
  public int? GetUserId()
  {
    var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
    return int.TryParse(claim?.Value, out var id) ? id : null;
  }
}