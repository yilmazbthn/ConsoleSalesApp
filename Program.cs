using ConsoleSalesApp;
using ConsoleSalesApp.Data;
using ConsoleSalesApp.Model;
using Microsoft.EntityFrameworkCore;

class Program
{
    private static User? _loggedInUser;
   private static ConsoleMenu _mainMenu = new ("Console Satış Uygulaması");
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
        Console.ForegroundColor = ConsoleColor.Blue;
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
            .AddMenu("Güncel Sipariş İşlemleri", ViewCurrentOrder);
        _customerMenu
            .AddMenu("Ürünleri Listele", ListAllOrderItem)
            .AddMenu("Sepetim", ShowCart)
            .AddMenu("Güncel Siparişlerim",OrderHistroy)
            .AddMenu("Sipariş Geçmişim", OrderNow);
        
        

        _mainMenu.Show();
        
    }

    private static void OrderNow()
    {
        Console.Clear();
        Console.WriteLine("Aktif Siparişiniz:\n");

        var activeOrders = _context.Orders
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.OrderItem)
            .Where(o => o.UserId == _loggedInUser.Id &&
                        (o.Status == OrderStatus.Pending || o.Status == OrderStatus.Preparing))
            .OrderByDescending(o => o.OrderDate)
            .ToList();

        if (!activeOrders.Any())
        {
            Console.WriteLine("Şu anda aktif bir siparişiniz bulunmamaktadır.");
        }
        else
        {
            foreach (var activeOrder in activeOrders)
            {
                Console.WriteLine($"Sipariş No : {activeOrder.OrderNumber}");
                Console.WriteLine($"Tarih      : {activeOrder.OrderDate}");
                Console.WriteLine($"Durum      : {GetOrderStatusText(activeOrder.Status)}");

                foreach (var item in activeOrder.OrderDetails)
                {
                    Console.WriteLine($" - {item.OrderItem.ProductName} | {item.Quantity} x {item.UnitPrice} = {item.TotalPrice} TL");
                }

                var total = activeOrder.OrderDetails.Sum(i => i.TotalPrice);
                Console.WriteLine($"Toplam Tutar: {total} TL");
                Console.WriteLine(new string('-', 40));
            }
        }


        Console.WriteLine("\nAna menüye dönmek için bir tuşa basın...");
        Console.ReadKey();
    }


    private static string GetOrderStatusText(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Pending => " Beklemede",
            OrderStatus.Preparing => " Hazırlanıyor",
            OrderStatus.Delivered => " Teslim Edildi",
            OrderStatus.Cancelled => " İptal Edildi",
            _ => "Bilinmiyor"
        };
    }
    private static void OrderHistroy()
    {
        Console.Clear();
        Console.WriteLine("Sipariş Geçmişiniz:\n");

        var userOrders = _context.Orders
            .Where(o => o.UserId == _loggedInUser.Id)
            .OrderByDescending(o => o.OrderDate).Include(order => order.OrderDetails)
            .ThenInclude(orderDetail => orderDetail.OrderItem)
            .ToList();

        if (userOrders.Count == 0)
        {
            Console.WriteLine("Hiç sipariş geçmişiniz bulunmamaktadır.");
        }
        else
        {
            foreach (var order in userOrders)
            {
                Console.WriteLine($"Sipariş No: {order.OrderNumber}");
                Console.WriteLine($"Tarih     : {order.OrderDate}");
                Console.WriteLine($"Durum      : {GetOrderStatusText(order.Status)}");


                foreach (var detail in order.OrderDetails)
                {
                    Console.WriteLine($"  - {detail.OrderItem.ProductName} | {detail.Quantity} x {detail.UnitPrice} TL = {detail.TotalPrice} TL");
                }

                var total = order.OrderDetails.Sum(d => d.TotalPrice);
                Console.WriteLine($"Toplam Tutar: {total} TL");
                Console.WriteLine(new string('-', 40));
            }
        }

        Console.WriteLine("\nAna menüye dönmek için bir tuşa basın...");
        Console.ReadKey();
        
    }
    
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
                Console.WriteLine($"{selectedProduct.ProductName} sepete eklendi.");
            }
            else
            {
                Console.WriteLine("Geçersiz seçim!");
            }

            Console.WriteLine("Devam etmek için bir tuşa basın...");
            Console.ReadKey();
        }
    }


    private static void ShowCart()
    {
        Console.Clear();
        Console.WriteLine("Sepetiniz:\n");

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
            var total = Cart.Sum(p => p.UnitPrice);
            Console.WriteLine($"Sepet Tutarı : {total} TL");
            
            
            bool validInput = false;
            while (!validInput)
            {
                

                var input = Helper.Ask("Sepeti Onaylıyor Musunuz ? (e/h)\n Ana Menü=0",true);
                switch (input.ToLower())
                {
                    case "e":
                        var newOrder = new Order
                        {
                            OrderNumber = $"ORD-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                            OrderDate = DateTime.Now,
                            Status = OrderStatus.Pending,
                            UserId = _loggedInUser.Id,
                            OrderDetails = new List<OrderDetail>()
                        };

                        bool stokYetersiz = false;

                        foreach (var item in Cart)
                        {
                            var productInDb = _context.OrderItems.FirstOrDefault(p => p.Id == item.Id);
                            if (productInDb != null)
                            {
                                if (productInDb.Stock <= 0)
                                {
                                    Helper.ShowErrorMsg($"{item.ProductName} ürünü stokta yok. Sipariş iptal edildi.");
                                    stokYetersiz = true;
                                    break;
                                }
                                
                                productInDb.Stock -= 1;
                                
                                newOrder.OrderDetails.Add(new OrderDetail
                                {
                                    OrderItemId = item.Id,
                                    Quantity = 1,
                                    UnitPrice = item.UnitPrice
                                });
                            }
                        }

                        if (stokYetersiz)
                        {
                            Cart.Clear();
                            break;
                        }

                        _context.Orders.Add(newOrder);
                        _context.SaveChanges();

                        Helper.ShowSuccessMsg("Sipariş başarıyla verildi ve stoktan düşüldü.");
                        Cart.Clear();
                        validInput = true; // döngüden çıkmak için
                        break;

                    case "h":
                        Cart.Clear();
                        break;
                    case "0":
                        _customerMenu.Show();
                        break;
                    default:
                        Console.WriteLine("Geçersiz giriş! Lütfen 'e', 'h' veya '0' girin.");
                        break;
                }
            }

            


        }
    

        Console.WriteLine("\nAna menüye dönmek için bir tuşa basın...");
        Console.ReadKey();
    }



    private static void ViewCurrentOrder()
    {
        Console.Clear();
        Console.WriteLine("Aktif Siparişler(Beklemede/Hazırlanıyor)");
        var activeOrders = _context.Orders
            .Where(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.Preparing)
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.OrderItem)
            .Include(o => o.User).Include(order => order.UserId)
            .OrderByDescending(o => o.OrderDate)
            .ToList();
        if (!activeOrders.Any())
        {
            Console.WriteLine("Aktif Sipariş Yok");
        }
        else
        {
            foreach (var order in activeOrders)
            {
                Console.WriteLine($"Sipariş No : {order.OrderNumber}");
                Console.WriteLine($"Kullanıcı  : {_context.Users.Find(order.UserId)?.FirstName+ _context.Users.Find(order.UserId)?.LastName ?? "Bilinmiyor"}");
                Console.WriteLine($"Tarih      : {order.OrderDate}");
                Console.WriteLine($"Durum      : {GetOrderStatusText(order.Status)}");

                foreach (var detail in order.OrderDetails)
                {
                    Console.WriteLine($"   - {detail.OrderItem.ProductName} | {detail.Quantity} x {detail.UnitPrice} TL");
                }

                Console.WriteLine(new string('-', 40));
            }
        }
        Console.WriteLine("\nAna menüye dönmek için bir tuşa basın...");
        Console.ReadKey();
    }

    private static void ViewAllOrder()
    {
        Console.Clear();
        Console.WriteLine("📄 TÜM SİPARİŞLER:\n");

        var allOrders = _context.Orders
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.OrderItem)
            .Include(o => o.User).Include(order => order.UserId)
            .OrderByDescending(o => o.OrderDate)
            .ToList();

        foreach (var order in allOrders)
        {
            Console.WriteLine($"Sipariş No : {order.OrderNumber}");
            Console.WriteLine($"Kullanıcı  : {_context.Users.Find(order.UserId)?.FirstName+ _context.Users.Find(order.UserId)?.LastName ?? "Bilinmiyor"}");
            Console.WriteLine($"Tarih      : {order.OrderDate}");
            Console.WriteLine($"Durum      : {GetOrderStatusText(order.Status)}");

            foreach (var detail in order.OrderDetails)
            {
                Console.WriteLine($"   - {detail.OrderItem.ProductName} | {detail.Quantity} x {detail.UnitPrice} TL");
            }

            Console.WriteLine($"💰 Toplam: {order.OrderDetails.Sum(d => d.TotalPrice)} TL");
            Console.WriteLine(new string('-', 40));
        }

        Console.WriteLine("\nAna menüye dönmek için bir tuşa basın...");
        Console.ReadKey();
    }


    private static void ViewAllUser()
    {
        Console.Clear();
        Console.WriteLine("TÜM KULLANICILAR:\n");

        var users = _context.Users.ToList();

        if (!users.Any())
        {
            Console.WriteLine("Hiç kullanıcı bulunamadı.");
        }
        else
        {
            foreach (var user in users)
            {
                Console.WriteLine($"ID: {user.Id} | Ad Soyad: {user.FirstName} {user.LastName} | E-posta: {user.Email}");
            }
        }

        Console.WriteLine("\nAna menüye dönmek için bir tuşa basın...");
        Console.ReadKey();
    }


    private static void ViewRapor()
    {
        Console.Clear();
        Console.WriteLine("SATIŞ RAPORU:\n");

        var totalOrders = _context.Orders.Count();
        var totalRevenue = _context.Orders
            .SelectMany(o => o.OrderDetails)
            .Sum(d => d.TotalPrice);

        var totalUsers = _context.Users.Count();
        var totalProducts = _context.OrderItems.Count();

        Console.WriteLine($"Toplam Sipariş   : {totalOrders}");
        Console.WriteLine($"Toplam Kullanıcı : {totalUsers}");
        Console.WriteLine($"Toplam Ürün      : {totalProducts}");
        Console.WriteLine($"Toplam Gelir     : {totalRevenue} TL");

        Console.WriteLine("\nAna menüye dönmek için bir tuşa basın...");
        Console.ReadKey();
    }


    private static void UpdateProduct()
    {
        Console.Clear();
        Console.WriteLine("Ürün Güncelleme\n");

        var products = _context.OrderItems.ToList();

        if (products.Count == 0)
        {
            Console.WriteLine("Hiç ürün yok.");
            return;
        }

        for (int i = 0; i < products.Count; i++)
        {
            Console.WriteLine($"{i + 1}-) {products[i].ProductName} | Fiyat: {products[i].UnitPrice} TL | Stok: {products[i].Stock}");
        }

        var selectedIndex = Helper.AskNumber("Güncellemek istediğiniz ürünün numarasını girin: ") - 1;

        if (selectedIndex < 0 || selectedIndex >= products.Count)
        {
            Helper.ShowErrorMsg("Geçersiz ürün numarası.");
            return;
        }

        var selectedProduct = products[selectedIndex];

        var newName = Helper.Ask($"Yeni Ürün Adı ({selectedProduct.ProductName}): ", true);
        var newPrice = Helper.AskNumber($"Yeni Fiyat ({selectedProduct.UnitPrice}): ");
        var newStock = Helper.AskNumber($"Yeni Stok Adedi ({selectedProduct.Stock}): ");

        selectedProduct.ProductName = string.IsNullOrWhiteSpace(newName) ? selectedProduct.ProductName : newName;
        selectedProduct.UnitPrice = newPrice;
        selectedProduct.Stock = newStock;

        _context.OrderItems.Update(selectedProduct);
        _context.SaveChanges();

        Helper.ShowSuccessMsg("Ürün başarıyla güncellendi.");
    }


    private static void AddProduct()
    {
        var inputName = Helper.Ask("Ürün Adı:");
        var inputPrice = Helper.AskNumber("Ürün Fiyatı: ");
        var inputStock = Helper.AskNumber("Stok Adedi: ");
        var newProduct = new OrderItem()
        {
            ProductName = inputName,
            UnitPrice = inputPrice,
            Stock = inputStock,

        };
        _context.OrderItems.Add(newProduct);
        _context.SaveChanges();
    }
}