using ConsoleSalesApp;
using ConsoleSalesApp.Data;
using ConsoleSalesApp.Model;
using Microsoft.EntityFrameworkCore;

class Program
{
    private static User? _loggedInUser;
   private static ConsoleMenu _mainMenu = new ("Console Chat Uygulaması");
    private static ConsoleMenu _salesPersonMenu = new("Satış Temsilcisi Menüsü");
    private static ConsoleMenu _customerMenu = new("Müşteri Menüsü");
    private static ConsoleMenu _adminMenu = new("Admin Menüsü");
    private static AppDbContext _context = new AppDbContext();
    
    static void RegisterUser()
    {
        var inputName = Helper.Ask("Ad", true);
        var inputLastName = Helper.Ask("Soyad", true);
        var inputEmail = Helper.Ask("E-Posta", true);
        var inputPassword = Helper.AskPassword("Şifre");
        var registerStatus = Auth.Register(inputName!, inputLastName!,inputEmail!, inputPassword, out var user);

        if (registerStatus == Auth.RegisterStatus.UsernameExists)
        {
            Helper.ShowErrorMsg("Bu kullanıcı zaten var!");
            Thread.Sleep(1000);
            return;
        }
        
        Helper.ShowSuccessMsg("Kaydın yapıldı,Ana Menüye Yönlendiriliyorsun...");
        Thread.Sleep(1000);
        _loggedInUser = user;
        _mainMenu.Show();
    }
    static void LoginUser()
    { 
        var inputEmail = Helper.Ask("E-posta", true);
        var inputPassword = Helper.AskPassword("Şifre");
        var loginStatus = Auth.Login(inputEmail!, inputPassword, out var user);
        switch (loginStatus)
        {
            case Auth.LoginStatus.LoggedIn:
                _loggedInUser = user;
                if (user.Role==SalesRole.Administrator)
                {
                    _adminMenu.Show();
                }

                if (user.Role == SalesRole.Customer)
                {
                    _customerMenu.Show();
                }

                if (user.Role == SalesRole.SalesPerson)
                {
                    _salesPersonMenu.Show();
                }
                
                break;
            case Auth.LoginStatus.UserNotFound:
                Helper.ShowErrorMsg("Kullanıcın bulunamadı!");
                Thread.Sleep(1000);
                break;
            case Auth.LoginStatus.WrongCredentials:
                Helper.ShowErrorMsg("Eksik veya hatalı giriş yaptın!");
                Thread.Sleep(1000);
                break;
        }
    }

    static void Main(string[] args)
    {
       
        _mainMenu
            .AddMenu("Giriş Yap", LoginUser)
            .AddMenu("Kayıt Ol", RegisterUser);
        _adminMenu
            .AddMenu("Ürün Ekle", AddProduct)
            .AddMenu("Ürün Güncelle", UpdateProduct)
            .AddMenu("Rapor Görüntüle", ViewRapor)
            .AddMenu("Tüm Kullanıcıları Görüntüle", ViewAllUser)
            .AddMenu("Tüm Siparişleri Görüntüle", ViewAllOrder);
        _salesPersonMenu
            .AddMenu("Ürün Ekle", AddProduct)
            .AddMenu("Ürün Güncelle", UpdateProduct)
            .AddMenu("Güncel Sipariş İşlemleri",ViewCurrentOrder)
            .AddMenu("Tüm Siparişleri Görüntüle", ViewAllOrder);
        _customerMenu
            .AddMenu("Ürünleri Listele", ListAllOrderItem)
            .AddMenu("Sepetim", ShowCart)
            .AddMenu("Güncel Siparişlerim",Orders)
            .AddMenu("Sipariş Geçmişim", OrderHistroy);
        
        

        _mainMenu.Show();
        
    }

    private static void OrderHistroy()
    {
        throw new NotImplementedException();
    }

    private static void Orders()
    {
        throw new NotImplementedException();
    }
    //Sepet
    private static List<OrderItem> Cart = new List<OrderItem>();
    
    private static void ListAllOrderItem()
    {
        var salesProduct = _context.OrderItems.ToList();

        while (true)
        {
            Console.Clear();
            Console.WriteLine("Ürün Listesi:");
            for (int i = 0; i < salesProduct.Count; i++)
            {
                Console.WriteLine($"{i + 1}-) {salesProduct[i].ProductName}");
            }
            
            Console.WriteLine("[Boş bırak → Üst menüye dön]");
            Console.WriteLine("Bir ürün numarası seçin ve sepete ekleyin: ");
            string input = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("\nÜst menüye dönülüyor...");
                break;
            }

            if (int.TryParse(input, out int selectedIndex) && selectedIndex >= 1 && selectedIndex <= salesProduct.Count)
            {
                var selectedProduct = salesProduct[selectedIndex - 1];
                Cart.Add(selectedProduct);
                Console.WriteLine($"✅ '{selectedProduct.ProductName}' sepete eklendi.");
            }
            else
            {
                Console.WriteLine("⚠️ Geçersiz seçim!");
            }

            Console.WriteLine("Devam etmek için bir tuşa basın...");
            Console.ReadKey();
        }
    }


    private static void ShowCart()
    {
        Console.Clear();
        Console.WriteLine("🛒 Sepetiniz:\n");

        if (Cart.Count == 0)
        {
            Console.WriteLine("Sepet boş.");
        }
        else
        {
            for (int i = 0; i < Cart.Count; i++)
            {
                Console.WriteLine($"{i + 1}-) {Cart[i].ProductName}");
            }
        }

        Console.WriteLine("\nAna menüye dönmek için bir tuşa basın...");
        Console.ReadKey();
    }



    private static void ViewCurrentOrder()
    {
        throw new NotImplementedException();
    }

    private static void ViewAllOrder()
    {
        throw new NotImplementedException();
    }

    private static void ViewAllUser()
    {
        throw new NotImplementedException();
    }

    private static void ViewRapor()
    {
        throw new NotImplementedException();
    }

    private static void UpdateProduct()
    {
        throw new NotImplementedException();
    }

    private static void AddProduct()
    {
        throw new NotImplementedException();
    }
}