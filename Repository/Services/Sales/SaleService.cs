using ERP.Repository.Configuration.Enum;
using ERP.Repository.Configuration.Exception_Extender;
using ERP.Repository.Configuration.Helper;
using ERP.Repository.Configuration.Validation;
using ERP.Repository.Interface.AuditLogs;
using ERP.Repository.Interface.CurrentUser;
using ERP.Repository.Interface.Data.SalesData;
using ERP.Repository.Interface.Data.UserAccountData;
using ERP.Repository.Interface.Sales;
using ERP.Repository.Model.Inventories;
using ERP.Repository.Model.Orders;
using ERP.Repository.ViewModel.Sales;
using FluentValidation;
using QuestPDF.Fluent;

namespace ERP.Repository.Services.Sales
{
    public class SaleService(
        ISalesData _sales,
        IUserAccountData _users,
        ICurrentUserService _currentUser,
        IAuditLogService _auditLog,
        IValidator<SaleReturnPostViewModel> _returnValidator
    ) : ISaleService
    {
        public async Task<SalesPageViewModel> GetSales(
            int page,
            int pageSize,
            string? name,
            int filter,
            int orderTypeId,
            DateOnly? from,
            DateOnly? to,
            CancellationToken cancellationToken = default
        )
        {
            PageQueryValidator.Ensure(page, pageSize);

            if (from > to)
            {
                throw new BadRequest("The start date must be on or before the end date");
            }

            var result = await _sales.GetSalesWithoutTracking(
                page,
                pageSize,
                name,
                filter,
                orderTypeId,
                from,
                to,
                cancellationToken
            );
            var totalCount = await _sales.SalesCount(name, orderTypeId, from, to, cancellationToken);
            var totalAmount = await _sales.SalesTotalAmount(name, orderTypeId, from, to, cancellationToken);

            return new SalesPageViewModel
            {
                Sales = result,
                PageCount = (int)Math.Ceiling((double)totalCount / pageSize),
                Rows = totalCount,
                TotalAmount = totalAmount,
            };
        }

        public async Task<SaleReturnPageViewModel> GetReturns(
            int page,
            int pageSize,
            string? name,
            DateOnly? from,
            DateOnly? to,
            CancellationToken cancellationToken = default
        )
        {
            PageQueryValidator.Ensure(page, pageSize);

            if (from > to)
            {
                throw new BadRequest("The start date must be on or before the end date");
            }

            var result = await _sales.GetReturnsWithoutTracking(page, pageSize, name, from, to, cancellationToken);
            var totalCount = await _sales.ReturnsCount(name, from, to, cancellationToken);

            return new SaleReturnPageViewModel
            {
                Returns = result,
                PageCount = (int)Math.Ceiling((double)totalCount / pageSize),
                Rows = totalCount,
            };
        }

        // Takes back units of a completed sale. Good units go back to stock; damaged ones are logged as a return
        // then as damage, so stock ends where it was. Either way the Return movement counts against demand.
        public async Task ReturnItems(SaleReturnPostViewModel saleReturn)
        {
            await _returnValidator.EnsureValidAsync(saleReturn);

            var sale = await _sales.GetSaleForReturnWithTracking(saleReturn.SaleId);

            if (sale?.Order == null)
            {
                throw new NotFound("Sale not found");
            }

            var now = DateTime.UtcNow;
            var userId = _auditLog.CurrentUserId;
            var reason = saleReturn.Reason!.Trim().ToLower();
            var subtotal = sale.Order.OrderLines.Sum(l => l.Amount);

            foreach (var item in saleReturn.Items)
            {
                var lines = sale.Order.OrderLines.Where(l => l.ProductId == item.ProductId).ToList();

                if (lines.Count == 0)
                {
                    throw new BadRequest("That product is not part of this sale");
                }

                var name = lines[0].Product?.Name ?? "unknown product";
                var sold = lines.Sum(l => l.Quantity);
                var remaining = sold - sale.Returns.Where(r => r.ProductId == item.ProductId).Sum(r => r.Quantity);

                if (item.Quantity > remaining)
                {
                    throw new BadRequest($"Only {remaining} '{name}' can still be returned from this sale");
                }

                // The units' share of what the customer paid: sold price, scaled by the order discount.
                var unitPrice = lines.Sum(l => l.Amount) / sold;
                var refund =
                    subtotal == 0
                        ? 0
                        : Math.Round(
                            unitPrice * item.Quantity * sale.Total / subtotal,
                            2,
                            MidpointRounding.AwayFromZero
                        );

                var inventory = lines[0].Inventory!;
                var damaged = item.ConditionId == (int)ReturnConditionEnum.Damaged;

                var returnRow = new SaleReturn
                {
                    ProductId = item.ProductId,
                    InventoryId = inventory.Id,
                    Quantity = item.Quantity,
                    ConditionId = item.ConditionId,
                    Reason = reason,
                    Refund = refund,
                    Created_By = userId,
                    Created_At = now,
                };
                sale.Returns.Add(returnRow);

                inventory.Quantity += item.Quantity;
                inventory.InventoryTransactions.Add(
                    Movement(InventoryLabelEnum.Return, item.Quantity, inventory.Quantity, userId, now)
                );

                // The return still counts against demand; damaged units then leave stock as a damaged adjustment.
                if (damaged)
                {
                    inventory.Quantity -= item.Quantity;
                    inventory.InventoryTransactions.Add(
                        Movement(InventoryLabelEnum.Adjustment, -item.Quantity, inventory.Quantity, userId, now)
                    );
                    inventory.StockAdjustments.Add(
                        new StockAdjustment
                        {
                            ReasonId = (int)AdjustmentReasonEnum.Damaged,
                            Quantity = -item.Quantity,
                            Note = reason,
                            SaleReturn = returnRow,
                            Created_By = userId,
                            Created_At = now,
                        }
                    );
                }

                inventory.StatusId = ReorderRatio.Ratio(inventory.Quantity, inventory.ReorderPoint);
                inventory.Updated_By = userId;
                inventory.Updated_At = now;

                _auditLog.Log(
                    AuditModuleEnum.Order,
                    damaged ? AuditActionEnum.ReturnItemDamage : AuditActionEnum.ReturnStock,
                    $"Returned {item.Quantity} '{name}' from SAL-{sale.Id} ({(damaged ? "damaged" : "good")}): {reason}, refunded ₱{refund:N2}",
                    sale.OrderId,
                    now
                );
            }

            await _sales.SaveChanges();
        }

        private static InventoryTransaction Movement(
            InventoryLabelEnum label,
            int quantity,
            int stockLevel,
            int? userId,
            DateTime now
        ) =>
            new()
            {
                QuantityChanged = quantity,
                StockLevel = stockLevel,
                InventoryLabelId = (int)label,
                Created_By = userId,
                Created_At = now,
            };

        public async Task<SaleDateRangeViewModel> GetSaleDateRange(CancellationToken cancellationToken = default)
        {
            var (first, last) = await _sales.SaleDateRange(cancellationToken);

            return new SaleDateRangeViewModel { First = PhDay(first), Last = PhDay(last) };
        }

        public async Task<SaleReportFile> GetSalesReportPdf(
            string? period,
            DateOnly? date,
            CancellationToken cancellationToken = default
        )
        {
            var report = await GetSalesReport(
                ReportPeriods.Parse(period),
                date ?? DemandWeek.PhToday(),
                cancellationToken
            );
            var content = new SalesReportDocument(report).GeneratePdf();
            var fileName = $"sales-{report.Period.ToString().ToLowerInvariant()}-{report.From:yyyy-MM-dd}.pdf";

            return new SaleReportFile(content, fileName);
        }

        private async Task<SaleReportViewModel> GetSalesReport(
            SaleReportPeriod period,
            DateOnly date,
            CancellationToken cancellationToken
        )
        {
            var (from, to) = ReportPeriods.Range(period, date);

            var sales = await _sales.GetSalesForReport(from, to, cancellationToken);

            var gross = sales.Sum(s => s.Subtotal);
            var net = sales.Sum(s => s.Total);
            var refunded = sales.Sum(s => s.Refunded);

            return new SaleReportViewModel
            {
                Period = period,
                From = from,
                To = to,
                GeneratedAt = DateTime.UtcNow + DemandWeek.PhOffset,
                GeneratedBy = await CurrentUserName(cancellationToken),
                SalesCount = sales.Count,
                Gross = gross,
                // The stored total is what was charged, so the discount is whatever makes up the difference.
                Discount = gross - net,
                Net = net,
                Refunded = refunded,
                AfterRefunds = net - refunded,
                Average = sales.Count == 0 ? 0 : net / sales.Count,
                Sales = period == SaleReportPeriod.Daily ? sales : [],
                Rows = period switch
                {
                    SaleReportPeriod.Weekly => DayRows(sales, from, to),
                    SaleReportPeriod.Monthly => WeekRows(sales, from, to),
                    _ => [],
                },
                TopProducts = TopProducts(sales),
            };
        }

        // One row per day, days without sales included.
        private static List<SaleReportRow> DayRows(List<SaleReportSale> sales, DateOnly from, DateOnly to)
        {
            var rows = new List<SaleReportRow>();

            for (var day = from; day <= to; day = day.AddDays(1))
            {
                rows.Add(Row($"{day:ddd, MMM d}", sales, day, day));
            }

            return rows;
        }

        // One row per Monday-to-Sunday week, cut at the month's first and last day.
        private static List<SaleReportRow> WeekRows(List<SaleReportSale> sales, DateOnly from, DateOnly to)
        {
            var rows = new List<SaleReportRow>();
            var number = 1;

            for (var start = from; start <= to; number++)
            {
                var end = DemandWeek.StartOf(start).AddDays(6);
                if (end > to)
                {
                    end = to;
                }

                var days = start == end ? $"{start:MMM d}" : $"{start:MMM d}–{end:MMM d}";
                rows.Add(Row($"Week {number}: {days}", sales, start, end));
                start = end.AddDays(1);
            }

            return rows;
        }

        private static SaleReportRow Row(string label, List<SaleReportSale> sales, DateOnly from, DateOnly to)
        {
            var inRange = sales.Where(s => PhDay(s.Created_At) is { } day && day >= from && day <= to).ToList();
            var gross = inRange.Sum(s => s.Subtotal);
            var net = inRange.Sum(s => s.Total);
            var refunded = inRange.Sum(s => s.Refunded);

            return new SaleReportRow
            {
                Label = label,
                Sales = inRange.Count,
                Gross = gross,
                Discount = gross - net,
                Net = net,
                Refunded = refunded,
                AfterRefunds = net - refunded,
            };
        }

        private static List<SaleReportProduct> TopProducts(List<SaleReportSale> sales) =>
            sales
                .SelectMany(s => s.Lines)
                .GroupBy(l => l.ProductId)
                .Select(g => new SaleReportProduct
                {
                    Name = g.First().ProductName,
                    Unit = g.First().Unit,
                    Quantity = g.Sum(l => l.Quantity),
                    Revenue = g.Sum(l => l.Amount),
                })
                .OrderByDescending(p => p.Revenue)
                .ThenByDescending(p => p.Quantity)
                .ThenBy(p => p.Name)
                .Take(10)
                .ToList();

        private async Task<string> CurrentUserName(CancellationToken cancellationToken)
        {
            if (_currentUser.UserId is not { } userId)
            {
                return "";
            }

            var user = await _users.GetUserByIdWithoutTracking(userId, cancellationToken);

            return $"{user?.UserInformation?.FirstName} {user?.UserInformation?.LastName}".Trim();
        }

        // The Philippine day a UTC time falls on, the same day rule the list's from/to filter uses.
        private static DateOnly? PhDay(DateTime? utc) =>
            utc is { } value ? DateOnly.FromDateTime(value + DemandWeek.PhOffset) : null;
    }
}
