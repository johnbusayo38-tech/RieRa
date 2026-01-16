
// // // Controllers/AdminOrderController.cs
// // using Microsoft.AspNetCore.Authorization;
// // using Microsoft.AspNetCore.Mvc;
// // using Microsoft.EntityFrameworkCore;
// // using System.Text.Json;
// // using FirebaseAdmin.Messaging;
// // using AgroMove.API.Data;
// // using AgroMove.API.Models;
// // using AgroMove.API.DTOs.Admin;

// // namespace AgroMove.API.Controllers
// // {
// //     [Route("api/admin/orders")]
// //     [ApiController]
// //     [Authorize(Roles = "ADMIN,SUPERADMIN")]
// //     public class AdminOrderController : ControllerBase
// //     {
// //         private readonly AgroMoveDbContext _context;

// //         public AdminOrderController(AgroMoveDbContext context)
// //         {
// //             _context = context;
// //         }

// //         // --- BACKEND CALCULATION HELPERS ---
// //         private decimal CalculateTotalPayable(Order order)
// //         {
// //             // If Marketplace items exist, sum them using PriceAtPurchase
// //             if (order.OrderItems != null && order.OrderItems.Any())
// //             {
// //                 return order.OrderItems.Sum(i => i.PriceAtPurchase * i.Quantity);
// //             }
// //             // Otherwise, fallback to the Admin-quoted EstimatedCost (for Logistics)
// //             return order.EstimatedCost;
// //         }

// //         private string GetMarketplaceSummary(Order order)
// //         {
// //             if (order.OrderItems != null && order.OrderItems.Any())
// //             {
// //                 return string.Join(", ", order.OrderItems.Select(i => $"{i.Quantity}x {i.ProductName}"));
// //             }
// //             return "Logistics Service";
// //         }

// //         private JsonElement SafeParseJson(string? jsonString)
// //         {
// //             try
// //             {
// //                 if (string.IsNullOrWhiteSpace(jsonString))
// //                     return JsonDocument.Parse("{}").RootElement;
// //                 return JsonSerializer.Deserialize<JsonElement>(jsonString);
// //             }
// //             catch
// //             {
// //                 return JsonDocument.Parse("{}").RootElement;
// //             }
// //         }



// // // NEW ENDPOINT: Get all active marketplace orders (Admin only)
// //         [HttpGet("admin/marketplace/orders")]
// //         [Authorize(Roles = "ADMIN,SUPERADMIN")]
// //         public async Task<ActionResult<IEnumerable<AdminMarketplaceOrderResponse>>> GetAllMarketplaceOrders(
// //             [FromQuery] string status = "Pending", // Default to active/pending
// //             [FromQuery] int page = 1,
// //             [FromQuery] int size = 20)
// //         {
// //             var query = _context.MarketplaceOrders
// //                 .Include(o => o.Items)
// //                 .Include(o => o.User)
// //                 .AsQueryable();

// //             // Filter by status (active = Pending, Processing, etc.)
// //             if (!string.IsNullOrEmpty(status))
// //             {
// //                 query = query.Where(o => o.Status == status);
// //             }

// //             var total = await query.CountAsync();

// //             var orders = await query
// //                 .OrderByDescending(o => o.CreatedAt)
// //                 .Skip((page - 1) * size)
// //                 .Take(size)
// //                 .ToListAsync();

// //             var response = orders.Select(o => new AdminMarketplaceOrderResponse
// //             {
// //                 Id = o.Id,
// //                 UserId = o.UserId,
// //                 UserName = o.User?.Name ?? "Unknown",
// //                 UserPhone = o.User?.Phone ?? "N/A",
// //                 TotalAmount = o.TotalAmount,
// //                 Status = o.Status,
// //                 DeliveryAddress = o.DeliveryAddress,
// //                 ReceiverName = o.ReceiverName,
// //                 ReceiverPhone = o.ReceiverPhone,
// //                 CreatedAt = o.CreatedAt,
// //                 Items = o.Items.Select(i => new AdminMarketplaceOrderItemResponse
// //                 {
// //                     ProductName = i.ProductName,
// //                     GaugeLabel = i.GaugeLabel,
// //                     Quantity = i.Quantity,
// //                     PriceAtPurchase = i.PriceAtPurchase,
// //                     Weight = i.Weight
// //                 }).ToList()
// //             }).ToList();

// //             return Ok(new PaginatedResponse<AdminMarketplaceOrderResponse>
// //             {
// //                 Data = response,
// //                 Total = total,
// //                 Page = page,
// //                 Size = size
// //             });
// //         }





// //         // GET: api/admin/orders/active
// //         [HttpGet("active")]
// //         public async Task<ActionResult<PaginatedResponse<AdminOrderResponse>>> GetActiveOrders(
// //             [FromQuery] int page = 1,
// //             [FromQuery] int size = 50)
// //         {
// //             var activeStatuses = new[] { 
// //                 OrderStatus.Pending, 
// //                 OrderStatus.Accepted, 
// //                 OrderStatus.InTransit, 
// //                 OrderStatus.Cleared 
// //             };

// //             var query = _context.Orders
// //                 .Where(o => activeStatuses.Contains(o.Status))
// //                 .Include(o => o.Shipper)
// //                 .Include(o => o.Driver)
// //                 .Include(o => o.OrderItems); 

// //             var total = await query.CountAsync();

// //             var ordersRaw = await query
// //                 .OrderByDescending(o => o.CreatedAt)
// //                 .Skip((page - 1) * size)
// //                 .Take(size)
// //                 .ToListAsync();

// //             var responseData = ordersRaw.Select(o => new AdminOrderResponse
// //             {
// //                 Id = o.Id,
// //                 Status = o.Status.ToString(),
// //                 IsInternational = o.IsInternational,
// //                 PickupLocation = o.PickupLocation,
// //                 Destination = o.Destination,
// //                 ProduceType = o.ProduceType,
// //               //  Quantity = o.Quantity,
// //                 Weight = o.Weight,
// //                 BoxSize = o.BoxSize,
// //                 SpecialInstructions = o.SpecialInstructions,
// //                 ReceiverName = o.ReceiverName,
// //                 ReceiverPhone = o.ReceiverPhone,
// //                 SenderName = o.SenderName ?? o.Shipper?.Name,
                
// //                 // BACKEND CALCULATED VALUES
// //                 EstimatedCost = o.EstimatedCost,
// //                 TotalPayable = CalculateTotalPayable(o),
// //                 MarketplaceSummary = GetMarketplaceSummary(o),

// //                 RecommendedVehicle = o.RecommendedVehicle,
// //                 SpecialAdvice = o.SpecialAdvice,
// //                 EstimatedTime = o.EstimatedTime,
// //                 CargoImageUrl = o.CargoImageUrl,
// //                 DriverName = o.Driver?.Name,
// //                 CreatedAt = o.CreatedAt,
// //                 Details = SafeParseJson(o.OrderDetailsJson)
// //             }).ToList();

// //             return Ok(new PaginatedResponse<AdminOrderResponse>
// //             {
// //                 Data = responseData,
// //                 Total = total,
// //                 Page = page,
// //                 Size = size
// //             });
// //         }

// //         // GET: api/admin/orders/{id}
// //         [HttpGet("{id}")]
// //         public async Task<ActionResult<AdminOrderDetailResponse>> GetOrderDetails(Guid id)
// //         {
// //             var order = await _context.Orders
// //                 .Include(o => o.Shipper)
// //                 .Include(o => o.Driver)
// //                 .Include(o => o.OrderItems)
// //                 .FirstOrDefaultAsync(o => o.Id == id);

// //             if (order == null) return NotFound(new { message = "Order not found" });

// //             return Ok(new AdminOrderDetailResponse
// //             {
// //                 Id = order.Id,
// //                 Shipper = new AdminUserSummary { Id = order.Shipper.Id, Name = order.Shipper.Name, Phone = order.Shipper.Phone },
// //                 Driver = order.Driver != null ? new AdminUserSummary { Id = order.Driver.Id, Name = order.Driver.Name, Phone = order.Driver.Phone } : null,
// //                 PickupLocation = order.PickupLocation,
// //                 Destination = order.Destination,
// //                 ProduceType = order.ProduceType,
// //             //    Quantity = order.Quantity,
// //                 Weight = order.Weight,
// //                 BoxSize = order.BoxSize,
// //                 ReceiverName = order.ReceiverName,
// //                 ReceiverPhone = order.ReceiverPhone,
// //                 SenderName = order.SenderName ?? order.Shipper.Name,
// //                 SpecialInstructions = order.SpecialInstructions,
                
// //                 // BACKEND CALCULATED VALUES
// //                 EstimatedCost = order.EstimatedCost,
// //                 TotalPayable = CalculateTotalPayable(order),
// //                 MarketplaceSummary = GetMarketplaceSummary(order),

// //                 RecommendedVehicle = order.RecommendedVehicle ?? string.Empty,
// //                 SpecialAdvice = order.SpecialAdvice ?? string.Empty,
// //                 EstimatedTime = order.EstimatedTime ?? string.Empty,
// //                 Status = order.Status.ToString(),
// //                 IsInternational = order.IsInternational,
// //                 CreatedAt = order.CreatedAt,
// //                 AcceptedAt = order.AcceptedAt,
// //                 CargoImageUrl = order.CargoImageUrl,
// //                 Details = SafeParseJson(order.OrderDetailsJson)
// //             });
// //         }

// // [HttpPut("{id}/status")]
// // public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
// // {
// //     if (!Enum.TryParse<OrderStatus>(request.Status, true, out var newStatus))
// //         return BadRequest(new { message = "Invalid status" });

// //     var order = await _context.Orders
// //         .Include(o => o.Shipper)
// //         .FirstOrDefaultAsync(o => o.Id == id);

// //     if (order == null) 
// //         return NotFound(new { message = "Order not found" });

// //     var oldStatus = order.Status;
// //     order.Status = newStatus;

// //     // Update relevant timestamps only if not already set
// //     switch (newStatus)
// //     {
// //         case OrderStatus.Accepted when order.AcceptedAt == null:
// //             order.AcceptedAt = DateTime.UtcNow;
// //             break;
// //         case OrderStatus.InTransit when order.InTransitAt == null:
// //             order.InTransitAt = DateTime.UtcNow;
// //             break;
// //         case OrderStatus.Cleared when order.ClearedAt == null:
// //             order.ClearedAt = DateTime.UtcNow;
// //             break;
// //         case OrderStatus.Delivered when order.DeliveredAt == null:
// //             order.DeliveredAt = DateTime.UtcNow;
// //             break;
// //         case OrderStatus.Cancelled when order.CancelledAt == null:
// //             order.CancelledAt = DateTime.UtcNow;
// //             break;
// //     }

// //     await _context.SaveChangesAsync();

// //     // Save notification to database (personal to shipper)
// //     if (order.ShipperId != null && oldStatus != newStatus)
// //     {
// //         var notification = new AgroMove.API.Models.Notification // Fully qualified to avoid any ambiguity
// //         {
// //             UserId = order.ShipperId,
// //             Title = "Order Status Update",
// //             Message = $"Your order for {order.ProduceType ?? "cargo"} is now {newStatus.ToString().Replace("_", " ")}.",
// //             Type = "ORDER_UPDATE",
// //             RelatedOrderId = order.Id,
// //             IsRead = false,
// //             CreatedAt = DateTime.UtcNow
// //         };

// //         _context.Notifications.Add(notification);
// //         await _context.SaveChangesAsync();

// //         Console.WriteLine($"DB Notification saved for Shipper {order.ShipperId} - Order {order.Id}");
// //     }

// //     return Ok(new 
// //     { 
// //         message = "Order status updated and notification saved to database",
// //         status = newStatus.ToString()
// //     });
// // }
// //     }


// //     // NEW DTOs for Admin Marketplace Orders
// //     public class AdminMarketplaceOrderResponse
// //     {
// //         public Guid Id { get; set; }
// //         public Guid UserId { get; set; }
// //         public string UserName { get; set; } = string.Empty;
// //         public string UserPhone { get; set; } = string.Empty;
// //         public decimal TotalAmount { get; set; }
// //         public string Status { get; set; } = "Pending";
// //         public string DeliveryAddress { get; set; } = string.Empty;
// //         public string ReceiverName { get; set; } = string.Empty;
// //         public string ReceiverPhone { get; set; } = string.Empty;
// //         public DateTime CreatedAt { get; set; }
// //         public List<AdminMarketplaceOrderItemResponse> Items { get; set; } = new();
// //     }

// //     public class AdminMarketplaceOrderItemResponse
// //     {
// //         public string ProductName { get; set; } = string.Empty;
// //         public string GaugeLabel { get; set; } = string.Empty;
// //         public int Quantity { get; set; }
// //         public decimal PriceAtPurchase { get; set; }
// //         public string Weight { get; set; } = string.Empty;
// //     }

// //     // Reusable Paginated Response (add to shared DTOs if needed)
// //     public class PaginatedResponse<T>
// //     {
// //         public List<T> Data { get; set; } = new();
// //         public int Total { get; set; }
// //         public int Page { get; set; }
// //         public int Size { get; set; }
// //     }
// // }






// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using System.Security.Claims;
// using System.Text.Json;
// using AgroMove.API.Data;
// using AgroMove.API.Models;
// using AgroMove.API.DTOs.Admin;

// namespace AgroMove.API.Controllers
// {
//     [Route("api/admin/orders")]
//     [ApiController]
//     [Authorize(Roles = "ADMIN,SUPERADMIN")]
//     public class AdminOrderController : ControllerBase
//     {
//         private readonly AgroMoveDbContext _context;

//         public AdminOrderController(AgroMoveDbContext context)
//         {
//             _context = context;
//         }

//         // --- BACKEND CALCULATION HELPERS ---
//         private decimal CalculateTotalPayable(Order order)
//         {
//             // If Marketplace items exist, sum them using PriceAtPurchase
//             if (order.OrderItems != null && order.OrderItems.Any())
//             {
//                 return order.OrderItems.Sum(i => i.PriceAtPurchase * i.Quantity);
//             }
//             // Otherwise, fallback to the Admin-quoted EstimatedCost (for Logistics)
//             return order.EstimatedCost;
//         }

//         private string GetMarketplaceSummary(Order order)
//         {
//             if (order.OrderItems != null && order.OrderItems.Any())
//             {
//                 return string.Join(", ", order.OrderItems.Select(i => $"{i.Quantity}x {i.ProductName}"));
//             }
//             return "Logistics Service";
//         }

//         private JsonElement SafeParseJson(string? jsonString)
//         {
//             try
//             {
//                 if (string.IsNullOrWhiteSpace(jsonString))
//                     return JsonDocument.Parse("{}").RootElement;
//                 return JsonSerializer.Deserialize<JsonElement>(jsonString);
//             }
//             catch
//             {
//                 return JsonDocument.Parse("{}").RootElement;
//             }
//         }

//         // GET: api/admin/orders/admin/marketplace/orders
//         // Admin view of all marketplace orders (paginated, filterable)
//         [HttpGet("admin/marketplace/orders")]
//         public async Task<ActionResult<PaginatedResponse<AdminMarketplaceOrderResponse>>> GetAllMarketplaceOrders(
//             [FromQuery] string status = "", // Optional filter
//             [FromQuery] int page = 1,
//             [FromQuery] int size = 20)
//         {
//             var query = _context.MarketplaceOrders
//                 .Include(o => o.Items)
//                 .Include(o => o.User)
//                 .AsQueryable();

//             if (!string.IsNullOrEmpty(status))
//             {
//                 query = query.Where(o => o.Status == status);
//             }

//             var total = await query.CountAsync();

//             var orders = await query
//                 .OrderByDescending(o => o.CreatedAt)
//                 .Skip((page - 1) * size)
//                 .Take(size)
//                 .ToListAsync();

//             var response = orders.Select(o => new AdminMarketplaceOrderResponse
//             {
//                 Id = o.Id,
//                 UserId = o.UserId,
//                 UserName = o.User?.Name ?? "Unknown",
//                 UserPhone = o.User?.Phone ?? "N/A",
//                 UserEmail = o.User?.Email ?? "N/A",
//                 TotalAmount = o.TotalAmount,
//                 Status = o.Status,
//                 DeliveryAddress = o.DeliveryAddress,
//                 ReceiverName = o.ReceiverName,
//                 ReceiverPhone = o.ReceiverPhone,
//                 CreatedAt = o.CreatedAt,
//                 Items = o.Items.Select(i => new AdminMarketplaceOrderItemResponse
//                 {
//                     ProductName = i.ProductName,
//                     GaugeLabel = i.GaugeLabel,
//                     Quantity = i.Quantity,
//                     PriceAtPurchase = i.PriceAtPurchase,
//                     Weight = i.Weight,
//                     ImageUrl = i.ImageUrl,
//                     Description = i.Description
//                 }).ToList()
//             }).ToList();

//             return Ok(new PaginatedResponse<AdminMarketplaceOrderResponse>
//             {
//                 Data = response,
//                 Total = total,
//                 Page = page,
//                 Size = size
//             });
//         }

//         // GET: api/admin/orders/active
//         [HttpGet("active")]
//         public async Task<ActionResult<PaginatedResponse<AdminOrderResponse>>> GetActiveOrders(
//             [FromQuery] int page = 1,
//             [FromQuery] int size = 50)
//         {
//             var activeStatuses = new[] { 
//                 OrderStatus.Pending, 
//                 OrderStatus.Accepted, 
//                 OrderStatus.InTransit, 
//                 OrderStatus.Cleared 
//             };

//             var query = _context.Orders
//                 .Where(o => activeStatuses.Contains(o.Status))
//                 .Include(o => o.Shipper)
//                 .Include(o => o.Driver)
//                 .Include(o => o.OrderItems); 

//             var total = await query.CountAsync();

//             var ordersRaw = await query
//                 .OrderByDescending(o => o.CreatedAt)
//                 .Skip((page - 1) * size)
//                 .Take(size)
//                 .ToListAsync();

//             var responseData = ordersRaw.Select(o => new AdminOrderResponse
//             {
//                 Id = o.Id,
//                 Status = o.Status.ToString(),
//                 IsInternational = o.IsInternational,
//                 PickupLocation = o.PickupLocation,
//                 Destination = o.Destination,
//                 ProduceType = o.ProduceType,
//                 Weight = o.Weight,
//                 BoxSize = o.BoxSize,
//                 SpecialInstructions = o.SpecialInstructions,
//                 ReceiverName = o.ReceiverName,
//                 ReceiverPhone = o.ReceiverPhone,
//                 SenderName = o.SenderName ?? o.Shipper?.Name,
                
//                 // BACKEND CALCULATED VALUES
//                 EstimatedCost = o.EstimatedCost,
//                 TotalPayable = CalculateTotalPayable(o),
//                 MarketplaceSummary = GetMarketplaceSummary(o),

//                 RecommendedVehicle = o.RecommendedVehicle,
//                 SpecialAdvice = o.SpecialAdvice,
//                 EstimatedTime = o.EstimatedTime,
//                 CargoImageUrl = o.CargoImageUrl,
//                 DriverName = o.Driver?.Name,
//                 CreatedAt = o.CreatedAt,
//                 Details = SafeParseJson(o.OrderDetailsJson)
//             }).ToList();

//             return Ok(new PaginatedResponse<AdminOrderResponse>
//             {
//                 Data = responseData,
//                 Total = total,
//                 Page = page,
//                 Size = size
//             });
//         }

//         // GET: api/admin/orders/{id}
//         [HttpGet("{id}")]
//         public async Task<ActionResult<AdminOrderDetailResponse>> GetOrderDetails(Guid id)
//         {
//             var order = await _context.Orders
//                 .Include(o => o.Shipper)
//                 .Include(o => o.Driver)
//                 .Include(o => o.OrderItems)
//                 .FirstOrDefaultAsync(o => o.Id == id);

//             if (order == null) 
//                 return NotFound(new { message = "Order not found" });

//             return Ok(new AdminOrderDetailResponse
//             {
//                 Id = order.Id,
//                 Shipper = new AdminUserSummary 
//                 { 
//                     Id = order.Shipper.Id, 
//                     Name = order.Shipper.Name, 
//                     Phone = order.Shipper.Phone,
//                     Email = order.Shipper.Email
//                 },
//                 Driver = order.Driver != null ? new AdminUserSummary 
//                 { 
//                     Id = order.Driver.Id, 
//                     Name = order.Driver.Name, 
//                     Phone = order.Driver.Phone 
//                 } : null,
//                 PickupLocation = order.PickupLocation,
//                 Destination = order.Destination,
//                 ProduceType = order.ProduceType,
//                 Weight = order.Weight,
//                 BoxSize = order.BoxSize,
//                 ReceiverName = order.ReceiverName,
//                 ReceiverPhone = order.ReceiverPhone,
//                 SenderName = order.SenderName ?? order.Shipper.Name,
//                 SpecialInstructions = order.SpecialInstructions,
                
//                 // BACKEND CALCULATED VALUES
//                 EstimatedCost = order.EstimatedCost,
//                 TotalPayable = CalculateTotalPayable(order),
//                 MarketplaceSummary = GetMarketplaceSummary(order),

//                 RecommendedVehicle = order.RecommendedVehicle ?? string.Empty,
//                 SpecialAdvice = order.SpecialAdvice ?? string.Empty,
//                 EstimatedTime = order.EstimatedTime ?? string.Empty,
//                 Status = order.Status.ToString(),
//                 IsInternational = order.IsInternational,
//                 CreatedAt = order.CreatedAt,
//                 AcceptedAt = order.AcceptedAt,
//                 CargoImageUrl = order.CargoImageUrl,
//                 Details = SafeParseJson(order.OrderDetailsJson),
//                 Items = order.OrderItems.Select(i => new AdminMarketplaceOrderItemResponse
//                 {
//                     ProductName = i.ProductName,
//                     GaugeLabel = i.GaugeLabel,
//                     Quantity = i.Quantity,
//                     PriceAtPurchase = i.PriceAtPurchase,
//                     Weight = i.Weight,
//                     ImageUrl = i.ImageUrl,
//                     Description = i.Description
//                 }).ToList()
//             });
//         }

//         // PUT: api/admin/orders/{id}/status
//         [HttpPut("{id}/status")]
//         public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
//         {
//             if (!Enum.TryParse<OrderStatus>(request.Status, true, out var newStatus))
//                 return BadRequest(new { message = "Invalid status" });

//             var order = await _context.Orders
//                 .Include(o => o.Shipper)
//                 .FirstOrDefaultAsync(o => o.Id == id);

//             if (order == null) 
//                 return NotFound(new { message = "Order not found" });

//             var oldStatus = order.Status;
//             order.Status = newStatus;

//             // Update relevant timestamps only if not already set
//             switch (newStatus)
//             {
//                 case OrderStatus.Accepted when order.AcceptedAt == null:
//                     order.AcceptedAt = DateTime.UtcNow;
//                     break;
//                 case OrderStatus.InTransit when order.InTransitAt == null:
//                     order.InTransitAt = DateTime.UtcNow;
//                     break;
//                 case OrderStatus.Cleared when order.ClearedAt == null:
//                     order.ClearedAt = DateTime.UtcNow;
//                     break;
//                 case OrderStatus.Delivered when order.DeliveredAt == null:
//                     order.DeliveredAt = DateTime.UtcNow;
//                     break;
//                 case OrderStatus.Cancelled when order.CancelledAt == null:
//                     order.CancelledAt = DateTime.UtcNow;
//                     break;
//             }

//             await _context.SaveChangesAsync();

//             // Save notification to database (personal to shipper)
//             if (order.ShipperId != null && oldStatus != newStatus)
//             {
//                 var notification = new Notification
//                 {
//                     UserId = order.ShipperId,
//                     Title = "Order Status Update",
//                     Message = $"Your order for {order.ProduceType ?? "cargo"} is now {newStatus.ToString().Replace("_", " ")}.",
//                     Type = "ORDER_UPDATE",
//                     RelatedOrderId = order.Id,
//                     IsRead = false,
//                     CreatedAt = DateTime.UtcNow
//                 };

//                 _context.Notifications.Add(notification);
//                 await _context.SaveChangesAsync();

//                 Console.WriteLine($"DB Notification saved for Shipper {order.ShipperId} - Order {order.Id}");
//             }

//             return Ok(new 
//             { 
//                 message = "Order status updated and notification saved to database",
//                 status = newStatus.ToString()
//             });
//         }
//     }

//     // DTOs for Admin Views
//     public class AdminOrderResponse
//     {
//         public Guid Id { get; set; }
//         public string Status { get; set; } = string.Empty;
//         public bool IsInternational { get; set; }
//         public string PickupLocation { get; set; } = string.Empty;
//         public string Destination { get; set; } = string.Empty;
//         public string ProduceType { get; set; } = string.Empty;
//         public string Weight { get; set; } = string.Empty;
//         public string BoxSize { get; set; } = string.Empty;
//         public string SpecialInstructions { get; set; } = string.Empty;
//         public string ReceiverName { get; set; } = string.Empty;
//         public string ReceiverPhone { get; set; } = string.Empty;
//         public string SenderName { get; set; } = string.Empty;
//         public decimal EstimatedCost { get; set; }
//         public decimal TotalPayable { get; set; }
//         public string MarketplaceSummary { get; set; } = string.Empty;
//         public string RecommendedVehicle { get; set; } = string.Empty;
//         public string SpecialAdvice { get; set; } = string.Empty;
//         public string EstimatedTime { get; set; } = string.Empty;
//         public string? CargoImageUrl { get; set; }
//         public string? DriverName { get; set; }
//         public DateTime CreatedAt { get; set; }
//         public JsonElement Details { get; set; }
//     }

//     public class AdminOrderDetailResponse : AdminOrderResponse
//     {
//         public AdminUserSummary Shipper { get; set; } = new();
//         public AdminUserSummary? Driver { get; set; }
//         public List<AdminMarketplaceOrderItemResponse> Items { get; set; } = new();
//     }

//     public class AdminUserSummary
//     {
//         public Guid Id { get; set; }
//         public string Name { get; set; } = string.Empty;
//         public string Phone { get; set; } = string.Empty;
//         public string? Email { get; set; }
//     }

//     public class AdminMarketplaceOrderResponse
//     {
//         public Guid Id { get; set; }
//         public Guid UserId { get; set; }
//         public string UserName { get; set; } = string.Empty;
//         public string UserPhone { get; set; } = string.Empty;
//         public string UserEmail { get; set; } = string.Empty;
//         public decimal TotalAmount { get; set; }
//         public string Status { get; set; } = "Pending";
//         public string DeliveryAddress { get; set; } = string.Empty;
//         public string ReceiverName { get; set; } = string.Empty;
//         public string ReceiverPhone { get; set; } = string.Empty;
//         public DateTime CreatedAt { get; set; }
//         public List<AdminMarketplaceOrderItemResponse> Items { get; set; } = new();
//     }

//     public class AdminMarketplaceOrderItemResponse
//     {
//         public string ProductName { get; set; } = string.Empty;
//         public string GaugeLabel { get; set; } = string.Empty;
//         public int Quantity { get; set; }
//         public decimal PriceAtPurchase { get; set; }
//         public string Weight { get; set; } = string.Empty;
//         public string? ImageUrl { get; set; }
//         public string? Description { get; set; }
//     }

//     public class UpdateOrderStatusRequest
//     {
//         public string Status { get; set; } = string.Empty;
//     }

//     public class PaginatedResponse<T>
//     {
//         public List<T> Data { get; set; } = new();
//         public int Total { get; set; }
//         public int Page { get; set; }
//         public int Size { get; set; }
//     }
// }









using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using AgroMove.API.Data;
using AgroMove.API.Models;
using AgroMove.API.DTOs.Admin;

namespace AgroMove.API.Controllers
{
    [Route("api/admin/orders")]
    [ApiController]
    [Authorize(Roles = "ADMIN,SUPERADMIN")]
    public class AdminOrderController : ControllerBase
    {
        private readonly AgroMoveDbContext _context;

        public AdminOrderController(AgroMoveDbContext context)
        {
            _context = context;
        }

        // --- BACKEND CALCULATION HELPERS ---
        private decimal CalculateTotalPayable(Order order)
        {
            if (order.OrderItems != null && order.OrderItems.Any())
            {
                return order.OrderItems.Sum(i => i.PriceAtPurchase * i.Quantity);
            }
            return order.EstimatedCost;
        }

        private string GetMarketplaceSummary(Order order)
        {
            if (order.OrderItems != null && order.OrderItems.Any())
            {
                return string.Join(", ", order.OrderItems.Select(i => $"{i.Quantity}x {i.ProductName}"));
            }
            return "Logistics Service";
        }

        private JsonElement SafeParseJson(string? jsonString)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(jsonString))
                    return JsonDocument.Parse("{}").RootElement;
                return JsonSerializer.Deserialize<JsonElement>(jsonString);
            }
            catch
            {
                return JsonDocument.Parse("{}").RootElement;
            }
        }

        // GET: api/admin/orders/admin/marketplace/orders
        [HttpGet("admin/marketplace/orders")]
        public async Task<ActionResult<PaginatedResponse<AdminMarketplaceOrderResponse>>> GetAllMarketplaceOrders(
            [FromQuery] string status = "",
            [FromQuery] int page = 1,
            [FromQuery] int size = 20)
        {
            var query = _context.MarketplaceOrders
                .Include(o => o.Items)
                .Include(o => o.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.Status == status);
            }

            var total = await query.CountAsync();

            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            var response = orders.Select(o => new AdminMarketplaceOrderResponse
            {
                Id = o.Id,
                UserId = o.UserId,
                UserName = o.User?.Name ?? "Unknown",
                UserPhone = o.User?.Phone ?? "N/A",
                UserEmail = o.User?.Email ?? "N/A",
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                DeliveryAddress = o.DeliveryAddress,
                ReceiverName = o.ReceiverName,
                ReceiverPhone = o.ReceiverPhone,
                CreatedAt = o.CreatedAt,
                Items = o.Items.Select(i => new AdminMarketplaceOrderItemResponse
                {
                    ProductName = i.ProductName,
                    GaugeLabel = i.GaugeLabel,
                    Quantity = i.Quantity,
                    PriceAtPurchase = i.PriceAtPurchase,
                    Weight = i.Weight ?? "N/A",
                    ImageUrl = i.ImageUrl,
                    Description = i.Description
                }).ToList()
            }).ToList();

            return Ok(new PaginatedResponse<AdminMarketplaceOrderResponse>
            {
                Data = response,
                Total = total,
                Page = page,
                Size = size
            });
        }

        // NEW: GET api/admin/orders/admin/marketplace/orders/{id}
        // Fetch single marketplace order details (admin only)
        [HttpGet("admin/marketplace/orders/{id}")]
        public async Task<ActionResult<AdminMarketplaceOrderDetailResponse>> GetMarketplaceOrderDetail(Guid id)
        {
            var order = await _context.MarketplaceOrders
                .Include(o => o.Items)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound(new { message = "Marketplace order not found" });

            var response = new AdminMarketplaceOrderDetailResponse
            {
                Id = order.Id,
                UserId = order.UserId,
                UserName = order.User?.Name ?? "Unknown",
                UserPhone = order.User?.Phone ?? "N/A",
                UserEmail = order.User?.Email ?? "N/A",
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                DeliveryAddress = order.DeliveryAddress,
                ReceiverName = order.ReceiverName,
                ReceiverPhone = order.ReceiverPhone,
                CreatedAt = order.CreatedAt,
                Items = order.Items.Select(i => new AdminMarketplaceOrderItemResponse
                {
                    ProductName = i.ProductName,
                    GaugeLabel = i.GaugeLabel,
                    Quantity = i.Quantity,
                    PriceAtPurchase = i.PriceAtPurchase,
                    Weight = i.Weight ?? "N/A",
                    ImageUrl = i.ImageUrl,
                    Description = i.Description
                }).ToList()
            };

            return Ok(response);
        }

        // GET: api/admin/orders/active
        [HttpGet("active")]
        public async Task<ActionResult<PaginatedResponse<AdminOrderResponse>>> GetActiveOrders(
            [FromQuery] int page = 1,
            [FromQuery] int size = 50)
        {
            var activeStatuses = new[] { 
                OrderStatus.Pending, 
                OrderStatus.Accepted, 
                OrderStatus.InTransit, 
                OrderStatus.Cleared 
            };

            var query = _context.Orders
                .Where(o => activeStatuses.Contains(o.Status))
                .Include(o => o.Shipper)
                .Include(o => o.Driver)
                .Include(o => o.OrderItems); 

            var total = await query.CountAsync();

            var ordersRaw = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            var responseData = ordersRaw.Select(o => new AdminOrderResponse
            {
                Id = o.Id,
                Status = o.Status.ToString(),
                IsInternational = o.IsInternational,
                PickupLocation = o.PickupLocation,
                Destination = o.Destination,
                ProduceType = o.ProduceType,
                Weight = o.Weight,
                BoxSize = o.BoxSize,
                SpecialInstructions = o.SpecialInstructions,
                ReceiverName = o.ReceiverName,
                ReceiverPhone = o.ReceiverPhone,
                SenderName = o.SenderName ?? o.Shipper?.Name,
                
                EstimatedCost = o.EstimatedCost,
                TotalPayable = CalculateTotalPayable(o),
                MarketplaceSummary = GetMarketplaceSummary(o),

                RecommendedVehicle = o.RecommendedVehicle,
                SpecialAdvice = o.SpecialAdvice,
                EstimatedTime = o.EstimatedTime,
                CargoImageUrl = o.CargoImageUrl,
                DriverName = o.Driver?.Name,
                CreatedAt = o.CreatedAt,
                Details = SafeParseJson(o.OrderDetailsJson)
            }).ToList();

            return Ok(new PaginatedResponse<AdminOrderResponse>
            {
                Data = responseData,
                Total = total,
                Page = page,
                Size = size
            });
        }

        // GET: api/admin/orders/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<AdminOrderDetailResponse>> GetOrderDetails(Guid id)
        {
            var order = await _context.Orders
                .Include(o => o.Shipper)
                .Include(o => o.Driver)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) 
                return NotFound(new { message = "Order not found" });

            return Ok(new AdminOrderDetailResponse
            {
                Id = order.Id,
                Shipper = new AdminUserSummary 
                { 
                    Id = order.Shipper.Id, 
                    Name = order.Shipper.Name, 
                    Phone = order.Shipper.Phone,
                    Email = order.Shipper.Email ?? "N/A"
                },
                Driver = order.Driver != null ? new AdminUserSummary 
                { 
                    Id = order.Driver.Id, 
                    Name = order.Driver.Name, 
                    Phone = order.Driver.Phone 
                } : null,
                PickupLocation = order.PickupLocation,
                Destination = order.Destination,
                ProduceType = order.ProduceType,
                Weight = order.Weight,
                BoxSize = order.BoxSize,
                ReceiverName = order.ReceiverName,
                ReceiverPhone = order.ReceiverPhone,
                SenderName = order.SenderName ?? order.Shipper.Name,
                SpecialInstructions = order.SpecialInstructions,
                
                EstimatedCost = order.EstimatedCost,
                TotalPayable = CalculateTotalPayable(order),
                MarketplaceSummary = GetMarketplaceSummary(order),

                RecommendedVehicle = order.RecommendedVehicle ?? string.Empty,
                SpecialAdvice = order.SpecialAdvice ?? string.Empty,
                EstimatedTime = order.EstimatedTime ?? string.Empty,
                Status = order.Status.ToString(),
                IsInternational = order.IsInternational,
                CreatedAt = order.CreatedAt,
                AcceptedAt = order.AcceptedAt,
                InTransitAt = order.InTransitAt,
                ClearedAt = order.ClearedAt,
                DeliveredAt = order.DeliveredAt,
                CancelledAt = order.CancelledAt,
                CargoImageUrl = order.CargoImageUrl,
                Details = SafeParseJson(order.OrderDetailsJson),
                Items = order.OrderItems.Select(i => new AdminMarketplaceOrderItemResponse
                {
                    ProductName = i.ProductName,
                    GaugeLabel = i.GaugeLabel ?? "N/A",
                    Quantity = i.Quantity,
                    PriceAtPurchase = i.PriceAtPurchase
                }).ToList()
            });
        }

        // PUT: api/admin/orders/{id}/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
        {
            if (!Enum.TryParse<OrderStatus>(request.Status, true, out var newStatus))
                return BadRequest(new { message = "Invalid status" });

            var order = await _context.Orders
                .Include(o => o.Shipper)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) 
                return NotFound(new { message = "Order not found" });

            var oldStatus = order.Status;
            order.Status = newStatus;

            switch (newStatus)
            {
                case OrderStatus.Accepted when order.AcceptedAt == null:
                    order.AcceptedAt = DateTime.UtcNow;
                    break;
                case OrderStatus.InTransit when order.InTransitAt == null:
                    order.InTransitAt = DateTime.UtcNow;
                    break;
                case OrderStatus.Cleared when order.ClearedAt == null:
                    order.ClearedAt = DateTime.UtcNow;
                    break;
                case OrderStatus.Delivered when order.DeliveredAt == null:
                    order.DeliveredAt = DateTime.UtcNow;
                    break;
                case OrderStatus.Cancelled when order.CancelledAt == null:
                    order.CancelledAt = DateTime.UtcNow;
                    break;
            }

            await _context.SaveChangesAsync();

            if (order.ShipperId != null && oldStatus != newStatus)
            {
                var notification = new Notification
                {
                    UserId = order.ShipperId,
                    Title = "Order Status Update",
                    Message = $"Your order for {order.ProduceType ?? "cargo"} is now {newStatus.ToString().Replace("_", " ")}.",
                    Type = "ORDER_UPDATE",
                    RelatedOrderId = order.Id,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
            }

            return Ok(new 
            { 
                message = "Order status updated and notification saved to database",
                status = newStatus.ToString()
            });
        }

        // PUT: api/admin/orders/admin/marketplace/orders/{id}/status
        [HttpPut("admin/marketplace/orders/{id}/status")]
        public async Task<IActionResult> UpdateMarketplaceOrderStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
        {
            var validStatuses = new[] { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };
            if (!validStatuses.Contains(request.Status))
                return BadRequest(new { message = "Invalid status" });

            var order = await _context.MarketplaceOrders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound(new { message = "Marketplace order not found" });

            var oldStatus = order.Status;
            order.Status = request.Status;

            await _context.SaveChangesAsync();

            if (order.UserId != null && oldStatus != request.Status)
            {
                var notification = new Notification
                {
                    UserId = order.UserId,
                    Title = "Marketplace Order Update",
                    Message = $"Your marketplace order is now {request.Status}.",
                    Type = "MARKETPLACE_UPDATE",
                    RelatedOrderId = order.Id,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
            }

            return Ok(new 
            { 
                message = "Marketplace order status updated successfully",
                newStatus = request.Status 
            });
        }
    }

    // DTOs
    public class AdminOrderResponse
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsInternational { get; set; }
        public string PickupLocation { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public string ProduceType { get; set; } = string.Empty;
        public string Weight { get; set; } = string.Empty;
        public string BoxSize { get; set; } = string.Empty;
        public string SpecialInstructions { get; set; } = string.Empty;
        public string ReceiverName { get; set; } = string.Empty;
        public string ReceiverPhone { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public decimal EstimatedCost { get; set; }
        public decimal TotalPayable { get; set; }
        public string MarketplaceSummary { get; set; } = string.Empty;
        public string RecommendedVehicle { get; set; } = string.Empty;
        public string SpecialAdvice { get; set; } = string.Empty;
        public string EstimatedTime { get; set; } = string.Empty;
        public string? CargoImageUrl { get; set; }
        public string? DriverName { get; set; }
        public DateTime CreatedAt { get; set; }
        public JsonElement Details { get; set; }
    }

    public class AdminOrderDetailResponse : AdminOrderResponse
    {
        public AdminUserSummary Shipper { get; set; } = new();
        public AdminUserSummary? Driver { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime? InTransitAt { get; set; }
        public DateTime? ClearedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public List<AdminMarketplaceOrderItemResponse> Items { get; set; } = new();
    }

    public class AdminUserSummary
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
    }

    public class AdminMarketplaceOrderResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserPhone { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending";
        public string DeliveryAddress { get; set; } = string.Empty;
        public string ReceiverName { get; set; } = string.Empty;
        public string ReceiverPhone { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<AdminMarketplaceOrderItemResponse> Items { get; set; } = new();
    }

    public class AdminMarketplaceOrderDetailResponse : AdminMarketplaceOrderResponse
    {
        // Inherits all properties from list response
        // Can add extra if needed
    }

    public class AdminMarketplaceOrderItemResponse
    {
        public string ProductName { get; set; } = string.Empty;
        public string GaugeLabel { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal PriceAtPurchase { get; set; }
        public string Weight { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateOrderStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }

    public class PaginatedResponse<T>
    {
        public List<T> Data { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int Size { get; set; }
    }
}