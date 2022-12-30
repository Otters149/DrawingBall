using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class JFSerialize : MonoBehaviour
{
    private static int _iterations = 2;
    private static int _keySize = 256;

    private static string _hash = "SHA1";
    private static string _salt = "aselrias38490a32";
    private static string _vector = "8947az34awl34kjq";
    private static string _private = "mSpUsXuZw4";

    static public string Serialize(string data)
    {
        return Encrypt<AesManaged>(data);
    }

    static public string Deserialize(string data, out bool hasError)
    {
        return Decrypt<AesManaged>(data, out hasError);
    }

    static private string Encrypt<T>(string data) where T : SymmetricAlgorithm, new()
    {
        byte[] vectorBytes = Encoding.ASCII.GetBytes(_vector);
        byte[] saltBytes = Encoding.ASCII.GetBytes(_salt);
        byte[] valueBytes = Encoding.UTF8.GetBytes(data);

        byte[] encrypted;
        using (T cipher = new T())
        {
            PasswordDeriveBytes _passwordBytes =
                new PasswordDeriveBytes(_private, saltBytes, _hash, _iterations);
            byte[] keyBytes = _passwordBytes.GetBytes(_keySize / 8);

            cipher.Mode = CipherMode.CBC;

            using (ICryptoTransform encryptor = cipher.CreateEncryptor(keyBytes, vectorBytes))
            {
                using (MemoryStream to = new MemoryStream())
                {
                    using (CryptoStream writer = new CryptoStream(to, encryptor, CryptoStreamMode.Write))
                    {
                        writer.Write(valueBytes, 0, valueBytes.Length);
                        writer.FlushFinalBlock();
                        encrypted = to.ToArray();
                    }
                }
            }
            cipher.Clear();
        }
        return Convert.ToBase64String(encrypted);
    }

    static private string Decrypt<T>(string data, out bool hasError) where T : SymmetricAlgorithm, new()
    {
        byte[] vectorBytes = Encoding.ASCII.GetBytes(_vector);
        byte[] saltBytes = Encoding.ASCII.GetBytes(_salt);
        byte[] valueBytes = Convert.FromBase64String(data);

        byte[] decrypted;
        int decryptedByteCount = 0;

        using (T cipher = new T())
        {
            PasswordDeriveBytes _passwordBytes = new PasswordDeriveBytes(_private, saltBytes, _hash, _iterations);
            byte[] keyBytes = _passwordBytes.GetBytes(_keySize / 8);

            cipher.Mode = CipherMode.CBC;

            try
            {
                using (ICryptoTransform decryptor = cipher.CreateDecryptor(keyBytes, vectorBytes))
                {
                    using (MemoryStream from = new MemoryStream(valueBytes))
                    {
                        using (CryptoStream reader = new CryptoStream(from, decryptor, CryptoStreamMode.Read))
                        {
                            decrypted = new byte[valueBytes.Length];
                            decryptedByteCount = reader.Read(decrypted, 0, decrypted.Length);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                hasError = true;
                return ex.Message;
            }

            cipher.Clear();
        }

        hasError = false;
        return Encoding.UTF8.GetString(decrypted, 0, decryptedByteCount);
    }
}
