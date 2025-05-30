namespace ConsoleSalesApp.Model;


public class OrderHistory
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public DateTime OrderDate { get; set; }=DateTime.Now;
    public string Details { get; set; } = string.Empty;
}
