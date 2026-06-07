namespace CBoosterSharp.Network.Crypto;

public interface ICryptoService
{
    public void ProtectAndSave(string data, string fileName);
    public string RetrieveAndUnprotect(string fileName);
    public void DeleteProtectedFile(string filename);
    public bool ProtectedFileExist(string filename);
}
