using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace SolarPuttyDecrypt
{
    class Program
    {
        static void Main(string[] args){

            if (args.Length < 1){
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Usage: SolarPuttyBrute.exe <session.dat file path> [wordlist path|password]");
                Console.WriteLine("Leave the second argument empty if you don't want to provide a password/wordlist.");
                Console.ResetColor();
                return;
            }

            if (!File.Exists(args[0]))
            {
                Console.WriteLine($"{args[0]} does not exists!");
                return;
            }

            string CurrDir = Environment.CurrentDirectory;
            DoImport(args[0], (args.Length < 2 ? "" : args[1]), CurrDir);
        }
        static void DoImport(string dialogFileName, string password, string CurrDir)
        {
            using (FileStream fileStream = new FileStream(dialogFileName, FileMode.Open))
            {
                using (StreamReader streamReader = new StreamReader(fileStream))
                {
                    string text = streamReader.ReadToEnd();
                    if (File.Exists(password))
                    {
                        Console.WriteLine("Bruteforcing password with the provided wordlist...");
                        string[] passwords = File.ReadAllLines(password);
                        for(int i = 0; i < passwords.Length - 1; i++)
                        {
                            try
                            {
                                string json_text = Crypto.Decrypt(passwords[i], text);
                                var obj = JsonConvert.DeserializeObject(json_text);
                                string idented = JsonConvert.SerializeObject(obj, Formatting.Indented);
                                Console.WriteLine($"Password found: {passwords[i]}");
                                Console.WriteLine("These are the sessions: ");
                                Console.WriteLine(idented);
                                return;

                            } catch (CryptographicException ex)
                            {
                                continue;
                            } catch (JsonReaderException ex)
                            {
                                continue;
                            }
                        }

                        Console.WriteLine("The password is not in the wordlist!");
                    }
                    else
                    {
                        try
                        {
                            var text2 = (password == "") ? Crypto.Deob(text) : Crypto.Decrypt(password, text);
                            if (text2 == null)
                            {
                                Console.WriteLine("Something strange ocurred while recovering the data!");
                                return;
                            }
                            var obj = JsonConvert.DeserializeObject(text2);
                            var f = JsonConvert.SerializeObject(obj, Formatting.Indented);
                            Console.WriteLine("\n" + f + "\n");
                            using (StreamWriter outputFile = new StreamWriter(Path.Combine(CurrDir, "SolarPutty_sessions_decrypted.txt")))
                                outputFile.WriteLine(f);
                        }
                        catch (CryptographicException ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Seems that the password is wrong...");
                            Console.ResetColor();

                            fileStream.Close();
                            Environment.Exit(1);
                        }
                        catch (FormatException message)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Check the session file syntax!");
                            Console.ResetColor();

                            fileStream.Close();
                            Environment.Exit(1);
                        }
                    }
                }
            }            
        }
    }
}

internal class Crypto
{
    public static string Decrypt(string passPhrase, string cipherText)
    {
        byte[] array = Convert.FromBase64String(cipherText);
        byte[] salt = array.Take(24).ToArray();
        byte[] rgbIV = array.Skip(24).Take(24).ToArray();
        byte[] array2 = array.Skip(48).Take(array.Length - 48).ToArray();
        using (Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(passPhrase, salt, 1000))
        {
            byte[] bytes = rfc2898DeriveBytes.GetBytes(24);
            using (TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider = new TripleDESCryptoServiceProvider())
            {
                tripleDESCryptoServiceProvider.Mode = CipherMode.CBC;
                tripleDESCryptoServiceProvider.Padding = PaddingMode.PKCS7;
                using (ICryptoTransform transform = tripleDESCryptoServiceProvider.CreateDecryptor(bytes, rgbIV))
                {
                    using (MemoryStream memoryStream = new MemoryStream(array2))
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Read))
                        {
                            byte[] array3 = new byte[array2.Length];
                            int count = cryptoStream.Read(array3, 0, array3.Length);
                            memoryStream.Close();
                            cryptoStream.Close();
                            return Encoding.UTF8.GetString(array3, 0, count);
                        }
                    }
                }
            }
        }
    }

    public static string Deob(string cipher)
    {
        byte[] encryptedData = Convert.FromBase64String(cipher);
        try
        {
            byte[] bytes = ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.CurrentUser);
            return Encoding.Unicode.GetString(bytes);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("This data seems encrypted! use a password.");
            Console.ResetColor();
            Environment.Exit(1);
        }
        return string.Empty;
    }
}