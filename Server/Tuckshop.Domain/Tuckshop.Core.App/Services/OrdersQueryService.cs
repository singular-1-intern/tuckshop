namespace Tuckshop.App.Services
{
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using Tuckshop.Core.Models;
  using Tuckshop.Models;
  using Tuckshop.Models.Orders.Enums;
  using Tuckshop.Models.Orders.Queries;
  using Microsoft.EntityFrameworkCore;

  /// <summary>
  /// Orders Query Service
  /// </summary>
  public class OrdersQueryService
  {
    private readonly ModelDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrdersQueryService"/> class.
    /// </summary>
    /// <param name="dbContext">The db context</param>
    public OrdersQueryService(ModelDbContext dbContext)
    {
      this.dbContext = dbContext;
    }

    /// <summary>
    /// Gets the orders for the given criteria
    /// </summary>
    /// <param name="criteria">The order lookup criteria</param>
    /// <returns>A query to retrieve the orders</returns>
    public IQueryable<OrderLookup> GetOrderLookup(OrderLookupCriteria criteria)
    {
      return (from o in this.dbContext.Orders
              from od in o.OrderDetails
              join p in this.dbContext.Products on od.ProductId equals p.ProductId
              join comBy in this.dbContext.Users on o.Completed.By equals comBy.UserId into completedByGroup
              from completedBy in completedByGroup.DefaultIfEmpty()
              join canBy in this.dbContext.Users on o.Cancelled.By equals canBy.UserId into cancelledByGroup
              from cancelledBy in cancelledByGroup.DefaultIfEmpty()
              where (criteria.OrderStatus == null
                     || (criteria.OrderStatus == OrderStatus.Pending && o.Completed.On == null && o.Cancelled.On == null)
                     || (criteria.OrderStatus == OrderStatus.Completed && o.Completed.On != null)
                     || (criteria.OrderStatus == OrderStatus.Cancelled && o.Cancelled.On != null))
                && (criteria.StartDate == null || o.OrderedOn >= criteria.StartDate)
                && (criteria.EndDate == null || o.OrderedOn < criteria.EndDate.Value.AddDays(1))
              select new
              {
                o.OrderId,
                o.CustomerName,
                o.OrderedOn,
                CompletedOn = o.Completed.On,
                CancelledOn = o.Cancelled.On,
                CompletedByFirstName = completedBy.FirstName,
                CompletedByLastName = completedBy.LastName,
                CancelledByFirstName = cancelledBy.FirstName,
                CancelledByLastName = cancelledBy.LastName,
                CancelledReason = o.Cancelled.Reason,
                p.ProductName,
                od.Quantity,
                Price = od.Value / od.Quantity,
                od.Value,
                od.VAT,
              })
               .GroupBy(g => new
               {
                 g.OrderId,
                 g.CustomerName,
                 g.OrderedOn,
                 g.CompletedOn,
                 g.CompletedByFirstName,
                 g.CompletedByLastName,
                 g.CancelledOn,
                 g.CancelledByFirstName,
                 g.CancelledByLastName,
                 g.CancelledReason,
               })
               .Select(orderGroup => new OrderLookup()
               {
                 OrderId = orderGroup.Key.OrderId,
                 CustomerName = orderGroup.Key.CustomerName,
                 OrderedOn = orderGroup.Key.OrderedOn,
                 CompletedOn = orderGroup.Key.CompletedOn,
                 CompletedBy = orderGroup.Key.CompletedByFirstName == null ? string.Empty : $"{orderGroup.Key.CompletedByFirstName} {orderGroup.Key.CompletedByLastName}",
                 CancelledOn = orderGroup.Key.CancelledOn,
                 CancelledBy = orderGroup.Key.CancelledByFirstName == null ? string.Empty : $"{orderGroup.Key.CancelledByFirstName} {orderGroup.Key.CancelledByLastName}",
                 CancelledReason = orderGroup.Key.CancelledReason,
                 OrderTotalExcl = orderGroup.Sum(x => x.Value - x.VAT),
                 OrderTotal = orderGroup.Sum(x => x.Value),
                 Items = orderGroup.Select(x => new OrderDetailLookup()
                 {
                   Product = x.ProductName,
                   Price = x.Value / x.Quantity,
                   Value = x.Value,
                   VAT = x.VAT,
                 }).ToList(),
               });
    }

    public async Task<List<OrderLookup>> GetOrderLookupAsync(OrderLookupCriteria criteria)
    {
      var flatOrderList =
          await (from o in this.dbContext.Orders
                 from od in o.OrderDetails
                 join p in this.dbContext.Products on od.ProductId equals p.ProductId
                 join completedBy in this.dbContext.Users on o.Completed.By equals completedBy.UserId into completedByGroup
                 from completedBy in completedByGroup.DefaultIfEmpty()
                 join cancelledBy in this.dbContext.Users on o.Cancelled.By equals cancelledBy.UserId into cancelledByGroup
                 from cancelledBy in cancelledByGroup.DefaultIfEmpty()
                 where (criteria.OrderStatus == null
                        || (criteria.OrderStatus == OrderStatus.Pending && o.Completed.On == null && o.Cancelled.On == null)
                        || (criteria.OrderStatus == OrderStatus.Completed && o.Completed.On != null)
                        || (criteria.OrderStatus == OrderStatus.Cancelled && o.Cancelled.On != null))
                   && (criteria.StartDate == null || o.OrderedOn >= criteria.StartDate)
                   && (criteria.EndDate == null || o.OrderedOn < criteria.EndDate.Value.AddDays(1))
                 select new
                 {
                   Order = new OrderLookup()
                   {
                     OrderId = o.OrderId,
                     CustomerName = o.CustomerName,
                     OrderedOn = o.OrderedOn,
                     CompletedOn = o.Completed.On,
                     CancelledOn = o.Cancelled.On,
                     CancelledReason = o.Cancelled.Reason,
                     CompletedBy = completedBy.FirstName == null ? string.Empty : $"{completedBy.FirstName} {completedBy.LastName}",
                     CancelledBy = cancelledBy.FirstName == null ? string.Empty : $"{cancelledBy.FirstName} {cancelledBy.LastName}",
                   },
                   OrderDetail = new OrderDetailLookup()
                   {
                     Product = p.ProductName,
                     Price = od.Value / od.Quantity,
                     Value = od.Value,
                     VAT = od.VAT,
                   },
                 }).ToListAsync().ConfigureAwait(false);

      return flatOrderList
              .GroupBy(c => c.Order.OrderId)
              .Select(c => c.First().Order.WithDetails(c.Select(c => c.OrderDetail)))
              .ToList();
    }
  }
}