using ConsoleSalesApp.Model;

public enum OrderStatus
{
    Pending=1,     // Beklemede
    Preparing=2,  // Hazırlanıyor
    Delivered=3,   // Teslim edildi
    Cancelled=4    // İptal edildi
}
public class Order
{
    public int Id { get; set; }
    public string OrderNumber { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public string OrderContext { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.Now;
    public int UserId { get; set; }
    public User User { get; set; }
    public int? SalesPersonId { get; set; }
    public User SalesPerson { get; set; }
    public List<OrderItem> OrderItems { get; set; }

}