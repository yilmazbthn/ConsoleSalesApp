namespace ConsoleSalesApp.Model;

public enum SalesRole
{
    Customer=1,
    Administrator=2,
    SalesPerson=3
}

public class User
{
    public int Id { get; set; }

    public string FirstName { get; set; }
    public string LastName { get; set; }
    
    public string Email { get; set; }

    public string Password { get; set; }  

    public SalesRole Role { get; set; }= SalesRole.Customer;




    public string? CustomerSpecificInfo { get; set; }


    public string? SalesPersonSpecificInfo { get; set; }


    public List<Order> Orders { get; set; }
}
