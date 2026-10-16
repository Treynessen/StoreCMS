using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Treynessen.Security;
using Treynessen.Database.Entities;

// [Комментарий ИИ-агента]: Подключён только вспомогательный код паролей. Контекст EF, хост, конфигурация и БД не открываются.
namespace Treynessen.Database.Entities
{
    public class User { public string Password { get; set; } }
}

internal static class PasswordChecks
{
    private static int checks;
    private static void Check(bool value, string name)
    {
        if (!value) throw new InvalidOperationException(name);
        checks++;
        Console.WriteLine("PASS: " + name);
    }

    public static int Main()
    {
        var watch = Stopwatch.StartNew();
        const string password = "Regression-password-2026!";
        string first = SecurityFunctions.GetPasswordHash(password);
        string second = SecurityFunctions.GetPasswordHash(password);
        var a = Convert.FromBase64String(first);
        var b = Convert.FromBase64String(second);
        Check(first != second, "same password has different hashes");
        Check(a.Length == 61 && a[0] == 1, "Identity V3 format with 256-bit subkey");
        Check(a[1] == 0 && a[2] == 0 && a[3] == 0 && a[4] == 1, "PBKDF2-HMAC-SHA256");
        Check(((uint)a[5] << 24 | (uint)a[6] << 16 | (uint)a[7] << 8 | a[8]) == 600000, "explicit 600000 iterations");
        Check(a[12] == 16 && !a.Skip(13).Take(16).SequenceEqual(b.Skip(13).Take(16)), "individual random 128-bit salts");
        var user = new User { Password = first };
        Check(SecurityFunctions.VerifyPassword(user, password), "correct password accepted");
        Check(!SecurityFunctions.VerifyPassword(user, "incorrect"), "incorrect password rejected");
        Check(!SecurityFunctions.VerifyPassword(user, password.ToUpperInvariant()), "case-sensitive password");
        Check(!SecurityFunctions.VerifyPassword(user, null), "null password rejected");
        Check(!SecurityFunctions.VerifyPassword(user, ""), "empty password rejected");
        Check(!SecurityFunctions.VerifyPassword(null, password), "null user rejected");
        foreach (string malformed in new[] { null, "", "not base64!", "AQ==", "", new string('a', 32), "Aa" + new string('a', 30), Convert.ToBase64String(new byte[49]) })
            Check(!SecurityFunctions.VerifyPassword(new User { Password = malformed }, password), "malformed/legacy hash rejected");
        string unicode = "Пароль с пробелами и Unicode!";
        Check(SecurityFunctions.VerifyPassword(new User { Password = SecurityFunctions.GetPasswordHash(unicode) }, unicode), "Unicode hash round trip");
        var results = new string[4];
        Parallel.For(0, results.Length, i => results[i] = SecurityFunctions.GetPasswordHash(password));
        Check(results.Distinct().Count() == results.Length, "concurrent hashing uses independent salts");
        Check(results.All(h => SecurityFunctions.VerifyPassword(new User { Password = h }, password)), "concurrent hashes verify");
        string initial = SecurityFunctions.GetPasswordHash("admin");
        user.Password = initial;
        Check(SecurityFunctions.VerifyPassword(user, "admin"), "initial password is supported by modern hasher");
        user.Password = first;
        Check(!SecurityFunctions.VerifyPassword(user, "admin"), "initial password fails after hash replacement");
        Console.WriteLine("Completed: " + checks + "; elapsed ms: " + watch.ElapsedMilliseconds);
        return 0;
    }
}
