
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using System.Security.Claims;
// using AgroMove.API.Data;
// using AgroMove.API.DTOs.Wallet;
// using AgroMove.API.Models;
// using Microsoft.EntityFrameworkCore;

// namespace AgroMove.API.Controllers
// {
//     [Route("api/wallet")]
//     [ApiController]
//     [Authorize]
//     public class WalletController : ControllerBase
//     {
//         private readonly AgroMoveDbContext _context;

//         public WalletController(AgroMoveDbContext context)
//         {
//             _context = context;
//         }

//         private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

//         // GET: api/wallet — Get full wallet details (balance + recent transactions)
//         [HttpGet]
//         public async Task<ActionResult<WalletResponse>> GetWallet()
//         {
//             var wallet = await _context.Wallets
//                 .Include(w => w.Transactions)
//                 .FirstOrDefaultAsync(w => w.UserId == CurrentUserId);

//             if (wallet == null)
//             {
//                 return NotFound(new { message = "Wallet not found for this user" });
//             }

//             var response = new WalletResponse
//             {
//                 Id = wallet.Id,
//                 Balance = wallet.Balance,
//                 CreatedAt = wallet.CreatedAt,
//                 Transactions = wallet.Transactions
//                     .OrderByDescending(t => t.Timestamp)
//                     .Take(50) // Limit recent transactions for performance
//                     .Select(t => new TransactionResponse
//                     {
//                         Id = t.Id,
//                         Amount = t.Amount,
//                         Type = t.Type,
//                         Description = t.Description,
//                         Status = t.Status,
//                         Timestamp = t.Timestamp
//                     })
//                     .ToList()
//             };

//             return Ok(response);
//         }

//         // GET: api/wallet/balance — Lightweight endpoint for just balance
//         [HttpGet("balance")]
//         public async Task<ActionResult<object>> GetBalanceOnly()
//         {
//             var wallet = await _context.Wallets
//                 .FirstOrDefaultAsync(w => w.UserId == CurrentUserId);

//             if (wallet == null)
//             {
//                 return NotFound(new { message = "Wallet not found for this user" });
//             }

//             return Ok(new { balance = wallet.Balance });
//         }

//         // GET: api/wallet/transactions — Full transaction history
//         [HttpGet("transactions")]
//         public async Task<ActionResult<List<TransactionResponse>>> GetTransactions()
//         {
//             var transactions = await _context.WalletTransactions
//                 .Where(t => t.Wallet.UserId == CurrentUserId)
//                 .OrderByDescending(t => t.Timestamp)
//                 .Select(t => new TransactionResponse
//                 {
//                     Id = t.Id,
//                     Amount = t.Amount,
//                     Type = t.Type,
//                     Description = t.Description,
//                     Status = t.Status,
//                     Timestamp = t.Timestamp
//                 })
//                 .ToListAsync();

//             return Ok(transactions);
//         }

//         // POST: api/wallet/fund — Add funds (credit)
//         [HttpPost("fund")]
//         public async Task<ActionResult<WalletResponse>> FundWallet([FromBody] FundWalletRequest request)
//         {
//             if (request.Amount <= 0)
//             {
//                 return BadRequest(new { message = "Amount must be greater than zero" });
//             }

//             var wallet = await _context.Wallets
//                 .Include(w => w.Transactions)
//                 .FirstOrDefaultAsync(w => w.UserId == CurrentUserId);

//             if (wallet == null)
//             {
//                 return NotFound(new { message = "Wallet not found for this user" });
//             }

//             wallet.Balance += request.Amount;

//             var transaction = new WalletTransaction
//             {
//                 WalletId = wallet.Id,
//                 Amount = request.Amount,
//                 Type = "CREDIT",
//                 Description = request.Method == "BANK_TRANSFER" 
//                     ? "Bank Transfer Funding" 
//                     : "Card Funding",
//                 Status = request.Method == "BANK_TRANSFER" ? "PENDING" : "SUCCESS",
//                 Timestamp = DateTime.UtcNow
//             };

//             _context.WalletTransactions.Add(transaction);
//             await _context.SaveChangesAsync();

//             // Return updated wallet
//             var response = new WalletResponse
//             {
//                 Id = wallet.Id,
//                 Balance = wallet.Balance,
//                 CreatedAt = wallet.CreatedAt,
//                 Transactions = wallet.Transactions
//                     .OrderByDescending(t => t.Timestamp)
//                     .Take(50)
//                     .Select(t => new TransactionResponse
//                     {
//                         Id = t.Id,
//                         Amount = t.Amount,
//                         Type = t.Type,
//                         Description = t.Description,
//                         Status = t.Status,
//                         Timestamp = t.Timestamp
//                     })
//                     .ToList()
//             };

//             return Ok(response);
//         }

//         // POST: api/wallet/debit — Deduct funds (e.g., for order payment)
//         [HttpPost("debit")]
//         public async Task<ActionResult<WalletResponse>> DebitWallet([FromBody] DebitWalletRequest request)
//         {
//             if (request.Amount <= 0)
//             {
//                 return BadRequest(new { message = "Amount must be greater than zero" });
//             }

//             if (string.IsNullOrWhiteSpace(request.Description))
//             {
//                 return BadRequest(new { message = "Transaction description is required" });
//             }

//             var wallet = await _context.Wallets
//                 .Include(w => w.Transactions)
//                 .FirstOrDefaultAsync(w => w.UserId == CurrentUserId);

//             if (wallet == null)
//             {
//                 return NotFound(new { message = "Wallet not found for this user" });
//             }

//             if (wallet.Balance < request.Amount)
//             {
//                 return BadRequest(new { message = "Insufficient wallet balance" });
//             }

//             wallet.Balance -= request.Amount;

//             var transaction = new WalletTransaction
//             {
//                 WalletId = wallet.Id,
//                 Amount = request.Amount,
//                 Type = "DEBIT",
//                 Description = request.Description,
//                 Status = "SUCCESS",
//                 Timestamp = DateTime.UtcNow
//             };

//             _context.WalletTransactions.Add(transaction);
//             await _context.SaveChangesAsync();

//             var response = new WalletResponse
//             {
//                 Id = wallet.Id,
//                 Balance = wallet.Balance,
//                 CreatedAt = wallet.CreatedAt,
//                 Transactions = wallet.Transactions
//                     .OrderByDescending(t => t.Timestamp)
//                     .Take(50)
//                     .Select(t => new TransactionResponse
//                     {
//                         Id = t.Id,
//                         Amount = t.Amount,
//                         Type = t.Type,
//                         Description = t.Description,
//                         Status = t.Status,
//                         Timestamp = t.Timestamp
//                     })
//                     .ToList()
//             };

//             return Ok(response);
//         }
//     }

//     // DTOs
//     public class FundWalletRequest
//     {
//         public decimal Amount { get; set; }
//         public string Method { get; set; } = "CARD"; // "CARD" or "BANK_TRANSFER"
//     }

//     public class DebitWalletRequest
//     {
//         public decimal Amount { get; set; }
//         public string Description { get; set; } = string.Empty;
//     }
// }





using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AgroMove.API.Data;
using AgroMove.API.DTOs.Wallet;
using AgroMove.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace AgroMove.API.Controllers
{
    [Route("api/wallet")]
    [ApiController]
    [Authorize]
    public class WalletController : ControllerBase
    {
        private readonly AgroMoveDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public WalletController(AgroMoveDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_configuration["Paystack:SecretKey"]}");
        }

        private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private async Task<User?> GetCurrentUser()
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == CurrentUserId);
        }

        // GET: api/wallet — Get full wallet details (balance + recent transactions)
        [HttpGet]
        public async Task<ActionResult<WalletResponse>> GetWallet()
        {
            var wallet = await _context.Wallets
                .Include(w => w.Transactions)
                .FirstOrDefaultAsync(w => w.UserId == CurrentUserId);

            if (wallet == null)
            {
                return NotFound(new { message = "Wallet not found for this user" });
            }

            var response = new WalletResponse
            {
                Id = wallet.Id,
                Balance = wallet.Balance,
                CreatedAt = wallet.CreatedAt,
                Transactions = wallet.Transactions
                    .OrderByDescending(t => t.Timestamp)
                    .Take(50)
                    .Select(t => new TransactionResponse
                    {
                        Id = t.Id,
                        Amount = t.Amount,
                        Type = t.Type,
                        Description = t.Description,
                        Status = t.Status,
                        Timestamp = t.Timestamp,
                        Reference = t.Reference
                    })
                    .ToList()
            };

            return Ok(response);
        }

        // GET: api/wallet/balance — Lightweight endpoint for just balance
        [HttpGet("balance")]
        public async Task<ActionResult<object>> GetBalanceOnly()
        {
            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.UserId == CurrentUserId);

            if (wallet == null)
            {
                return NotFound(new { message = "Wallet not found for this user" });
            }

            return Ok(new { balance = wallet.Balance });
        }

        // GET: api/wallet/transactions — Full transaction history
        [HttpGet("transactions")]
        public async Task<ActionResult<List<TransactionResponse>>> GetTransactions()
        {
            var transactions = await _context.WalletTransactions
                .Where(t => t.Wallet.UserId == CurrentUserId)
                .OrderByDescending(t => t.Timestamp)
                .Select(t => new TransactionResponse
                {
                    Id = t.Id,
                    Amount = t.Amount,
                    Type = t.Type,
                    Description = t.Description,
                    Status = t.Status,
                    Timestamp = t.Timestamp,
                    Reference = t.Reference
                })
                .ToListAsync();

            return Ok(transactions);
        }

        // POST: api/wallet/paystack/initialize — Initialize Paystack payment (card + bank)
        [HttpPost("paystack/initialize")]
        public async Task<ActionResult<object>> InitializePaystackPayment([FromBody] FundWalletRequest request)
        {
            if (request.Amount <= 0)
            {
                return BadRequest(new { message = "Amount must be greater than zero" });
            }

            var user = await GetCurrentUser();
            if (user == null || string.IsNullOrEmpty(user.Email))
            {
                return BadRequest(new { message = "User email required for payment" });
            }

            var reference = $"agromove_{Guid.NewGuid():N}";
            var amountKobo = (long)(request.Amount * 100); // Paystack uses kobo

            var payload = new
            {
                email = user.Email,
                amount = amountKobo,
                reference,
                callback_url = "https://yourapp.com/payment-callback", // Or deep link for RN
                metadata = new { userId = CurrentUserId.ToString(), method = request.Method ?? "CARD" }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.paystack.co/transaction/initialize", content);

            if (!response.IsSuccessStatusCode)
            {
                return BadRequest(new { message = "Failed to initialize payment with Paystack" });
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseJson);

            if (result.GetProperty("status").GetBoolean())
            {
                var data = result.GetProperty("data");
                var authorizationUrl = data.GetProperty("authorization_url").GetString();
                var accessCode = data.GetProperty("access_code").GetString();

                // Save pending transaction for reconciliation
                var wallet = await _context.Wallets.FirstAsync(w => w.UserId == CurrentUserId);
                var pendingTx = new WalletTransaction
                {
                    WalletId = wallet.Id,
                    Amount = request.Amount,
                    Type = "CREDIT",
                    Description = $"{request.Method ?? "CARD"} Funding via Paystack",
                    Status = "PENDING",
                    Reference = reference,
                    Timestamp = DateTime.UtcNow
                };
                _context.WalletTransactions.Add(pendingTx);
                await _context.SaveChangesAsync();

                return Ok(new 
                { 
                    authorizationUrl, 
                    accessCode, 
                    reference 
                });
            }

            return BadRequest(new { message = "Paystack initialization failed" });
        }

        // POST: api/wallet/paystack/verify — Verify and credit wallet
        [HttpPost("paystack/verify")]
        public async Task<IActionResult> VerifyPaystackPayment([FromBody] VerifyPaymentRequest request)
        {
            var response = await _httpClient.GetAsync($"https://api.paystack.co/transaction/verify/{request.Reference}");

            if (!response.IsSuccessStatusCode)
            {
                return BadRequest(new { message = "Verification failed" });
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseJson);

            if (result.GetProperty("status").GetBoolean() && 
                result.GetProperty("data").GetProperty("status").GetString() == "success")
            {
                var data = result.GetProperty("data");
                var amountNgn = data.GetProperty("amount").GetInt64() / 100m;
                var reference = data.GetProperty("reference").GetString();

                var pendingTx = await _context.WalletTransactions
                    .Include(t => t.Wallet)
                    .FirstOrDefaultAsync(t => t.Reference == reference && t.Status == "PENDING");

                if (pendingTx != null && Math.Abs(pendingTx.Amount - amountNgn) < 0.01m)
                {
                    pendingTx.Status = "SUCCESS";
                    pendingTx.Wallet.Balance += amountNgn;
                    await _context.SaveChangesAsync();

                    return Ok(new { message = "Payment verified and wallet credited" });
                }
            }

            return BadRequest(new { message = "Payment verification failed or already processed" });
        }

        // POST: api/wallet/fund — Record pending bank transfer (manual)
        [HttpPost("fund")]
        public async Task<ActionResult<object>> RecordPendingFund([FromBody] FundWalletRequest request)
        {
            if (request.Amount <= 0)
            {
                return BadRequest(new { message = "Amount must be greater than zero" });
            }

            if (request.Method != "BANK_TRANSFER")
            {
                return BadRequest(new { message = "Use /paystack/initialize for card or bank payments" });
            }

            var wallet = await _context.Wallets
                .Include(w => w.Transactions)
                .FirstOrDefaultAsync(w => w.UserId == CurrentUserId);

            if (wallet == null)
            {
                return NotFound(new { message = "Wallet not found" });
            }

            var transaction = new WalletTransaction
            {
                WalletId = wallet.Id,
                Amount = request.Amount,
                Type = "CREDIT",
                Description = "Bank Transfer Funding (Pending Confirmation)",
                Status = "PENDING",
                Timestamp = DateTime.UtcNow
            };

            _context.WalletTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Bank transfer recorded. Awaiting confirmation." });
        }

        // POST: api/wallet/debit — Deduct funds (e.g., for order payment)
        [HttpPost("debit")]
        public async Task<ActionResult<WalletResponse>> DebitWallet([FromBody] DebitWalletRequest request)
        {
            if (request.Amount <= 0)
            {
                return BadRequest(new { message = "Amount must be greater than zero" });
            }

            if (string.IsNullOrWhiteSpace(request.Description))
            {
                return BadRequest(new { message = "Transaction description is required" });
            }

            var wallet = await _context.Wallets
                .Include(w => w.Transactions)
                .FirstOrDefaultAsync(w => w.UserId == CurrentUserId);

            if (wallet == null)
            {
                return NotFound(new { message = "Wallet not found for this user" });
            }

            if (wallet.Balance < request.Amount)
            {
                return BadRequest(new { message = "Insufficient wallet balance" });
            }

            wallet.Balance -= request.Amount;

            var transaction = new WalletTransaction
            {
                WalletId = wallet.Id,
                Amount = request.Amount,
                Type = "DEBIT",
                Description = request.Description,
                Status = "SUCCESS",
                Timestamp = DateTime.UtcNow
            };

            _context.WalletTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            var response = new WalletResponse
            {
                Id = wallet.Id,
                Balance = wallet.Balance,
                CreatedAt = wallet.CreatedAt,
                Transactions = wallet.Transactions
                    .OrderByDescending(t => t.Timestamp)
                    .Take(50)
                    .Select(t => new TransactionResponse
                    {
                        Id = t.Id,
                        Amount = t.Amount,
                        Type = t.Type,
                        Description = t.Description,
                        Status = t.Status,
                        Timestamp = t.Timestamp,
                        Reference = t.Reference
                    })
                    .ToList()
            };

            return Ok(response);
        }
    }

    // DTOs
    public class WalletResponse
    {
        public Guid Id { get; set; }
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<TransactionResponse> Transactions { get; set; } = new();
    }

    public class TransactionResponse
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; } = string.Empty; // "CREDIT" or "DEBIT"
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // "SUCCESS", "PENDING"
        public DateTime Timestamp { get; set; }
        public string? Reference { get; set; }
    }

    public class FundWalletRequest
    {
        public decimal Amount { get; set; }
        public string Method { get; set; } = "CARD"; // "CARD" or "BANK_TRANSFER"
    }

    public class DebitWalletRequest
    {
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class VerifyPaymentRequest
    {
        public string Reference { get; set; } = string.Empty;
    }
}