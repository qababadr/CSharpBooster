using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace CBoosterSharp.Network.Crypto;

public class DefaultCryptoService : ICryptoService
{
    public void DeleteProtectedFile(string filename)
    {
        string appDataFolder = Environment
            .GetFolderPath(Environment.SpecialFolder.ApplicationData);

        string filePath = Path.Combine(appDataFolder, AppName, filename);

        File.Delete(filePath);
    }

    public void ProtectAndSave(string data, string fileName)
    {
        string appDataFolder = Environment
            .GetFolderPath(Environment.SpecialFolder.ApplicationData);

        string filePath = Path.Combine(appDataFolder, AppName, fileName);

        byte[] encryptedData = ProtectedData.Protect(
            Encoding.UTF8.GetBytes(data),
            null,
            DataProtectionScope.CurrentUser
        );

        string dir = Path.GetDirectoryName(filePath)
            ?? throw new Exception("Enable to read the directory information");

        Directory.CreateDirectory(dir);

        File.WriteAllBytes(filePath, encryptedData);
    }

    public bool ProtectedFileExist(string filename)
    {
        string appDataFolder = Environment
            .GetFolderPath(Environment.SpecialFolder.ApplicationData);

        string filePath = Path.Combine(appDataFolder, AppName, filename);

        return File.Exists(filePath);
    }

    public string RetrieveAndUnprotect(string fileName)
    {
        string appDataFolder = Environment
            .GetFolderPath(Environment.SpecialFolder.ApplicationData);

        string filePath = Path.Combine(appDataFolder, AppName, fileName);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                "The encrypted file does not exist.",
                filePath
            );
        }

        byte[] encryptedData = File.ReadAllBytes(filePath);

        byte[] decryptedData = ProtectedData.Unprotect(
            encryptedData,
            null,
            DataProtectionScope.CurrentUser
        );

        return Encoding.UTF8.GetString(decryptedData);
    }

    private static string AppName =>
        Assembly
        .GetExecutingAssembly()
        .GetName()
        .Name ?? "DefaultAppName";
}
