﻿using System.Security.Cryptography;
using System.Text;
using ConsoleSalesApp.Data;
using ConsoleSalesApp.Model;

namespace ConsoleSalesApp;

public static class Auth
{
    // login
    // kullanıcı adı veya şifre hatalı
    // böyle bir kullanıcı yok
    // şifremi unuttum

    // register
    // yeni kullanıcı kaydı
    // bu kullanıcı zaten kayıtlı, şifreni mi unuttun?
    // bu isimde kullanıcı kayıtlı
    private static readonly AppDbContext _context = new AppDbContext();

    public enum LoginStatus
    {
        LoggedIn,
        UserNotFound,
        WrongCredentials,
    }
    // enumların karşılığında veritabanında bir değer tutmuyorsak
    // o zaman olduğu gibi bırakabiliriz

    public enum RegisterStatus
    {
        Successful,
        UsernameExists
    }

    public static LoginStatus Login(string email, string password, out User? loggedInUser)
    {
        loggedInUser = null;
        var user = _context.Users.FirstOrDefault(u => u.Email == email);
        if (user == null)
        {
            return LoginStatus.UserNotFound;
        }

        if (user.Password != Hash(password))
        {
            return LoginStatus.WrongCredentials;
        }

        loggedInUser = user;

        return LoginStatus.LoggedIn;
    }

    public static RegisterStatus Register(string firstName,string lastName, string email, string password, out User? loggedInUser)
    {
        loggedInUser = null;

        var doesUserExist = _context.Users.Any(u => u.Email == email);
        if (doesUserExist)
        {
            return RegisterStatus.UsernameExists;
        }

        var user = new User
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Password = Hash(password),
            CustomerSpecificInfo = "",
            SalesPersonSpecificInfo = ""
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        loggedInUser = user;

        return RegisterStatus.Successful;
    }

    private static string Hash(string rawData)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            // Girdiyi byte dizisine çevir
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

            // Byte dizisini hex string'e çevir
            StringBuilder builder = new StringBuilder();
            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2")); // "x2" => 2 karakterlik hex
            }

            return builder.ToString();
        }
    }
}