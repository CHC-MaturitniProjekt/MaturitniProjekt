using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

public static class SaveSystem
{
    private static readonly string savePath = Application.persistentDataPath + "/savedata.json";
    private static readonly string encryptionKey = "testovaci-klic"; // Toban dej sem potom treba GUID z db

    public static bool SavePlayer(Movement player, PickUp pickUp, TimeManager time, CameraController cameraController)
    {
        try
        {
            GameObject[] interactableObjects = GameObject.FindGameObjectsWithTag("Item");
            List<ItemInteract> items = interactableObjects.Select(obj => obj.GetComponent<ItemInteract>()).Where(item => item != null).ToList();
            PlayerData data = new PlayerData(player, pickUp, time, items, cameraController);
            string jsonData = JsonUtility.ToJson(data, true);
            string encryptedData = Encrypt(jsonData, encryptionKey);
            File.WriteAllText(savePath, encryptedData);
            return true; 
        }
        catch (Exception ex)
        {
            Debug.LogError("Save failed: " + ex.Message);
            return false;
        }
    }

    public static PlayerData LoadPlayer()
    {
        if (!File.Exists(savePath))
        {
            Debug.Log("No save file at: " + savePath);
            return null;
        }

        try
        {
            string encryptedData = File.ReadAllText(savePath);
            string jsonData = Decrypt(encryptedData, encryptionKey);
            PlayerData data = JsonUtility.FromJson<PlayerData>(jsonData);
            return data;
        }
        catch (Exception ex)
        {
            Debug.LogError("Load failed: " + ex.Message);
            return null;
        }
    }

    public static bool ResetPlayer()
    {
        try
        {
            PlayerData data = new PlayerData();
            string jsonData = JsonUtility.ToJson(data, true);
            string encryptedData = Encrypt(jsonData, encryptionKey);
            File.WriteAllText(savePath, encryptedData);
            Debug.Log("Game reset and data encrypted.");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError("Reset failed: " + ex.Message);
            return false;
        }
    }

    public static bool SaveExists()
    {
        return File.Exists(savePath);
    }

    private static string Encrypt(string plainText, string key)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        Array.Resize(ref keyBytes, 32);
        using (Aes aes = Aes.Create())
        {
            aes.Key = keyBytes;
            aes.GenerateIV();
            byte[] iv = aes.IV;
            using (var encryptor = aes.CreateEncryptor(aes.Key, iv))
            using (var ms = new MemoryStream())
            {
                ms.Write(iv, 0, iv.Length);
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (var sw = new StreamWriter(cs))
                {
                    sw.Write(plainText);
                }
                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }

    private static string Decrypt(string cipherText, string key)
    {
        byte[] fullCipher = Convert.FromBase64String(cipherText);
        byte[] iv = new byte[16];
        byte[] cipher = new byte[fullCipher.Length - iv.Length];

        Array.Copy(fullCipher, iv, iv.Length);
        Array.Copy(fullCipher, iv.Length, cipher, 0, cipher.Length);

        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        Array.Resize(ref keyBytes, 32);
        using (Aes aes = Aes.Create())
        {
            aes.Key = keyBytes;
            using (var decryptor = aes.CreateDecryptor(aes.Key, iv))
            using (var ms = new MemoryStream(cipher))
            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
            using (var sr = new StreamReader(cs))
            {
                return sr.ReadToEnd();
            }
        }
    }
}