using Newtonsoft.Json;
using NUnit.Framework;
using PimDeWitte.UnityMainThreadDispatcher;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class FirebaseClient
{
    private readonly FirebaseConfig _config;
    private string userId = "";
    private readonly HttpClient _httpClient;
    private readonly string authDataPath = Path.Combine(Application.persistentDataPath, "firebase_auth.json");

    public void setUserId(string id)
    {
        userId = id;
    }

    public FirebaseClient(FirebaseConfig Config)
    {
        _config = Config;
        _httpClient = new HttpClient();
    }

    [Serializable]
    public class FirebaseAuthCache
    {
        public string localId;
    }

    public string LoadCachedLocalId()
    {
        if (File.Exists(authDataPath))
        {
            string json = File.ReadAllText(authDataPath);
            var cache = JsonUtility.FromJson<FirebaseAuthCache>(json);
            return cache.localId;
        }

        return null;
    }

    public void SaveCachedLocalId(string localId)
    {
        var cache = new FirebaseAuthCache
        {
            localId = localId,
        };
        string cacheJson = JsonUtility.ToJson(cache);
        File.WriteAllText(authDataPath, cacheJson);
    }

    public class FirebaseAuthResponse
    {
        public string localId { get; set; }
    }

    [Serializable]
    public class FirebaseLoginPayload
    {
        public string email;
        public string password;
        public bool returnSecureToken = true;
    }

    public async Task<FirebaseAuthResponse> SignInWithEmailAndPasswordAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Email and password cannot be null or empty.");

        var url = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={_config.apiKey}";

        var payload = new FirebaseLoginPayload
        {
            email = email,
            password = password,
            returnSecureToken = true
        };

        string jsonPayload = JsonUtility.ToJson(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync(url, content);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException("Firebase login failed.");
            }
            var authResponse = JsonConvert.DeserializeObject<FirebaseAuthResponse>(json);
            var cache = new FirebaseAuthCache
            {
                localId = authResponse.localId,
            };
            string cacheJson = JsonUtility.ToJson(cache);
            File.WriteAllText(authDataPath, cacheJson);

            return authResponse;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public async Task<FirebaseAuthResponse> SignUpWithEmailAndPasswordAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Email and password cannot be null or empty.");

        var url = $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={_config.apiKey}";

        var payload = new FirebaseLoginPayload
        {
            email = email,
            password = password,
            returnSecureToken = true
        };

        string jsonPayload = JsonUtility.ToJson(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync(url, content);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException("Firebase registration failed. Response: " + json);
            }

            var authResponse = JsonConvert.DeserializeObject<FirebaseAuthResponse>(json);
            var cache = new FirebaseAuthCache
            {
                localId = authResponse.localId,
            };
            string cacheJson = JsonUtility.ToJson(cache);
            File.WriteAllText(authDataPath, cacheJson);

            return authResponse;
        }
        catch (Exception ex)
        {
            throw new Exception("Error during Firebase registration: " + ex.Message, ex);
        }
    }

    public async Task<FirebaseResponse> AddUserAsync(string username, string userId)
    {
        var url = $"{_config.basePath}/users/{userId}.json";

        try
        {
            using (HttpClient client = new HttpClient())
            {
                var userData = new Dictionary<string, string>
            {
                { "username", username }
            };

                string jsonData = JsonConvert.SerializeObject(userData);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PutAsync(url, content);
                string resRawJson = await response.Content.ReadAsStringAsync();

                return new FirebaseResponse { RawJson = resRawJson };
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Error in AddUserAsync: {ex.Message}");
            throw;
        }
    }


    /// <summary>
    /// Asynchronously retrieves data from the specified path in Firebase Realtime Database.
    /// </summary>
    /// <param name="firebasePath">The path in the database to retrieve data from.</param>
    /// <returns>A FirebaseResponse containing the raw JSON response.</returns>
    /// <exception cref="ArgumentException">Thrown when firebasePath is null or empty.</exception>
    public async Task<FirebaseResponse> GetAsync(string firebasePath)
    {
        if (string.IsNullOrWhiteSpace(firebasePath))
            throw new ArgumentException("firebasePath cannot be null or empty.", nameof(firebasePath));

        var url = $"{_config.basePath}/{userId}/{firebasePath}.json";

        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string rawJson = await response.Content.ReadAsStringAsync();
            return new FirebaseResponse { RawJson = rawJson };
        }
        catch (Exception ex)
        {
           UnityEngine.Debug.LogError($"Error in GetAsync: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Synchronously retrieves data from the specified path in Firebase Realtime Database.
    /// </summary>
    /// <param name="firebasePath">The path in the database to retrieve data from.</param>
    /// <returns>A FirebaseResponse containing the raw JSON response.</returns>
    /// <exception cref="ArgumentException">Thrown when firebasePath is null or empty.</exception>
    public FirebaseResponse GetSync(string firebasePath)
    {
        if (string.IsNullOrWhiteSpace(firebasePath))
            throw new ArgumentException("firebasePath cannot be null or empty.", nameof(firebasePath));

        var url = $"{_config.basePath}/{userId}/{firebasePath}.json";

        try
        {
            HttpResponseMessage response = _httpClient.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();

            string rawJson = response.Content.ReadAsStringAsync().Result;
            return new FirebaseResponse { RawJson = rawJson };
        }
        catch (AggregateException ex)
        {
           UnityEngine.Debug.LogError($"Error in GetSync: {ex.Message}");
            throw ex.Flatten().InnerException;
        }
    }

    /// <summary>
    /// Asynchronously writes or replaces data at the specified path in Firebase Realtime Database.
    /// </summary>
    /// <param name="firebasePath">The path in the database where data should be written.</param>
    /// <param name="rawJson">The JSON data to write.</param>
    /// <returns>A FirebaseResponse containing the raw JSON response from Firebase.</returns>
    /// <exception cref="ArgumentException">Thrown when firebasePath or rawJson is null or empty.</exception>
    public async Task<FirebaseResponse> PutAsync(string firebasePath, string rawJson)
    {
        if (string.IsNullOrWhiteSpace(firebasePath))
            throw new ArgumentException("firebasePath cannot be null or empty.", nameof(firebasePath));

        var url = $"{_config.basePath}/{userId}/{firebasePath}.json";

        try
        {
            using (HttpClient client = new HttpClient())
            {
                StringContent content = new StringContent(rawJson, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PutAsync(url, content);
                string resRawJson = response.Content.ReadAsStringAsync().Result;
                return new FirebaseResponse { RawJson = resRawJson };
            }
        }
        catch (Exception ex)
        {
           UnityEngine.Debug.LogError($"Error in PutAsync: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Synchronously writes or replaces data at the specified path in Firebase Realtime Database.
    /// </summary>
    /// <param name="firebasePath">The path in the database where data should be written.</param>
    /// <param name="rawJson">The JSON data to write.</param>
    /// <returns>A FirebaseResponse containing the raw JSON response from Firebase.</returns>
    /// <exception cref="ArgumentException">Thrown when firebasePath or rawJson is null or empty.</exception>
    public FirebaseResponse PutSync(string firebasePath, string rawJson)
    {
        if (string.IsNullOrWhiteSpace(firebasePath))
            throw new ArgumentException("firebasePath cannot be null or empty.", nameof(firebasePath));

        var url = $"{_config.basePath}/{userId}/{firebasePath}.json";

        try
        {
            using (HttpClient client = new HttpClient())
            {
                StringContent content = new StringContent(rawJson, Encoding.UTF8, "application/json");
                HttpResponseMessage response = client.PutAsync(url, content).GetAwaiter().GetResult();
                string resRawJson = response.Content.ReadAsStringAsync().Result;
                return new FirebaseResponse { RawJson = resRawJson };
            }
        }
        catch (AggregateException ex)
        {
           UnityEngine.Debug.LogError($"Error in PutSync: {ex.Message}");
            throw ex.Flatten().InnerException;
        }
    }

    /// <summary>
    /// Asynchronously adds data to a list at the specified path in Firebase Realtime Database.
    /// </summary>
    /// <param name="firebasePath">The path in the database where data should be added.</param>
    /// <param name="rawJson">The JSON data to add.</param>
    /// <returns>A FirebaseResponse containing the raw JSON response from Firebase.</returns>
    /// <exception cref="ArgumentException">Thrown when firebasePath or rawJson is null or empty.</exception>
    public async Task<FirebaseResponse> PostAsync(string firebasePath, string rawJson)
    {
        if (string.IsNullOrWhiteSpace(firebasePath))
            throw new ArgumentException("firebasePath cannot be null or empty.", nameof(firebasePath));

        var url = $"{_config.basePath}/{userId}/{firebasePath}.json";

        try
        {
            using (HttpClient client = new HttpClient())
            {
                StringContent content = new StringContent(rawJson, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(url, content);

                string resRawJson = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    UnityEngine.Debug.LogError($"PostAsync Error: {response.StatusCode}, {resRawJson}");
                    throw new HttpRequestException($"Firebase PostAsync failed with status: {response.StatusCode}");
                }

                UnityEngine.Debug.Log($"PostAsync Success: {resRawJson}");
                return new FirebaseResponse { RawJson = resRawJson };
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Error in PostAsync: {ex.Message}");
            throw;
        }
    }



    /// <summary>
    /// Synchronously adds data to a list at the specified path in Firebase Realtime Database.
    /// </summary>
    /// <param name="firebasePath">The path in the database where data should be added.</param>
    /// <param name="rawJson">The JSON data to add.</param>
    /// <returns>A FirebaseResponse containing the raw JSON response from Firebase.</returns>
    /// <exception cref="ArgumentException">Thrown when firebasePath or rawJson is null or empty.</exception>
    public FirebaseResponse PostSync(string firebasePath, string rawJson)
    {
        if (string.IsNullOrWhiteSpace(firebasePath))
            throw new ArgumentException("firebasePath cannot be null or empty.", nameof(firebasePath));

        var url = $"{_config.basePath}/{userId}/{firebasePath}.json";

        try
        {
            using (HttpClient client = new HttpClient())
            {
                StringContent content = new StringContent(rawJson, Encoding.UTF8, "application/json");
                HttpResponseMessage response = client.PostAsync(url, content).GetAwaiter().GetResult();
                string resRawJson = response.Content.ReadAsStringAsync().Result;
                return new FirebaseResponse { RawJson = resRawJson };
            }
        }
        catch (AggregateException ex)
        {
           UnityEngine.Debug.LogError($"Error in PostSync: {ex.Message}");
            throw ex.Flatten().InnerException;
        }
    }

    /// <summary>
    /// Asynchronously updates data at the specified path in Firebase Realtime Database.
    /// </summary>
    /// <param name="firebasePath">The path in the database where data should be updated.</param>
    /// <param name="rawJson">The JSON data to update.</param>
    /// <returns>A FirebaseResponse containing the raw JSON response from Firebase.</returns>
    /// <exception cref="ArgumentException">Thrown when firebasePath or rawJson is null or empty.</exception>
    public async Task<FirebaseResponse> PatchAsync(string firebasePath, string rawJson)
    {
        if (string.IsNullOrWhiteSpace(firebasePath))
            throw new ArgumentException("firebasePath cannot be null or empty.", nameof(firebasePath));

        var url = $"{_config.basePath}/{userId}/{firebasePath}.json";

        try
        {
            using (HttpClient client = new HttpClient())
            {
                StringContent content = new StringContent(rawJson, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PutAsync(url, content);
                string resRawJson = response.Content.ReadAsStringAsync().Result;
                return new FirebaseResponse { RawJson = resRawJson };
            }
        }
        catch (Exception ex)
        {
           UnityEngine.Debug.LogError($"Error in PatchAsync: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Synchronously updates data at the specified path in Firebase Realtime Database.
    /// </summary>
    /// <param name="firebasePath">The path in the database where data should be updated.</param>
    /// <param name="rawJson">The JSON data to update.</param>
    /// <returns>A FirebaseResponse containing the raw JSON response from Firebase.</returns>
    /// <exception cref="ArgumentException">Thrown when firebasePath or rawJson is null or empty.</exception>
    public FirebaseResponse PatchSync(string firebasePath, string rawJson)
    {
        if (string.IsNullOrWhiteSpace(firebasePath))
            throw new ArgumentException("firebasePath cannot be null or empty.", nameof(firebasePath));

        var url = $"{_config.basePath}/{userId}/{firebasePath}.json";

        try
        {
            using (HttpClient client = new HttpClient())
            {
                StringContent content = new StringContent(rawJson, Encoding.UTF8, "application/json");
                HttpResponseMessage response = client.PutAsync(url, content).GetAwaiter().GetResult();
                string resRawJson = response.Content.ReadAsStringAsync().Result;
                return new FirebaseResponse { RawJson = resRawJson };
            }
        }
        catch (AggregateException ex)
        {
           UnityEngine.Debug.LogError($"Error in PatchSync: {ex.Message}");
            throw ex.Flatten().InnerException;
        }
    }

    /// <summary>
    /// Asynchronously deletes data at the specified path in Firebase Realtime Database.
    /// </summary>
    /// <param name="firebasePath">The path in the database where data should be deleted.</param>
    /// <returns>A boolean indicating whether the operation was successful.</returns>
    /// <exception cref="ArgumentException">Thrown when firebasePath is null or empty.</exception>
    public async Task<bool> DeleteAsync(string firebasePath)
    {
        if (string.IsNullOrWhiteSpace(firebasePath))
            throw new ArgumentException("firebasePath cannot be null or empty.", nameof(firebasePath));

        var url = $"{_config.basePath}/{userId}/{firebasePath}.json";

        try
        {
            HttpResponseMessage response = await _httpClient.DeleteAsync(url);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
           UnityEngine.Debug.LogError($"Error in DeleteAsync: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Synchronously deletes data at the specified path in Firebase Realtime Database.
    /// </summary>
    /// <param name="firebasePath">The path in the database where data should be deleted.</param>
    /// <returns>A boolean indicating whether the operation was successful.</returns>
    /// <exception cref="ArgumentException">Thrown when firebasePath is null or empty.</exception>
    public bool DeleteSync(string firebasePath)
    {
        if (string.IsNullOrWhiteSpace(firebasePath))
            throw new ArgumentException("firebasePath cannot be null or empty.", nameof(firebasePath));

        var url = $"{_config.basePath}/{userId}/{firebasePath}.json";

        try
        {
            HttpResponseMessage response = _httpClient.DeleteAsync(url).Result;
            return response.IsSuccessStatusCode;
        }
        catch (AggregateException ex)
        {
           UnityEngine.Debug.LogError($"Error in DeleteSync: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Asynchronously streams changes from a Firebase Realtime Database path and triggers a callback on data updates.
    /// </summary>
    /// <param name="firebasePath">The path in the database to stream changes from.</param>
    /// <param name="onDataReceived">The callback function to handle data updates. Parameters: event type and JSON data.</param>
    /// <exception cref="ArgumentException">Thrown when firebasePath is null or empty.</exception>
    public async Task StreamAsync(string firebasePath, Action<string, string> onDataReceived)
    {
        if (string.IsNullOrWhiteSpace(firebasePath))
            throw new ArgumentException("firebasePath cannot be null or empty.", nameof(firebasePath));

        var url = $"{_config.basePath}/{userId}/{firebasePath}.json";

        try
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Accept", "text/event-stream");

            using (HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();

                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = await reader.ReadLineAsync();
                        if (string.IsNullOrWhiteSpace(line))
                            continue;

                        if (line.StartsWith("event:"))
                        {
                            string eventName = line.Substring("event:".Length).Trim();
                            string dataLine = await reader.ReadLineAsync();
                            if (dataLine.StartsWith("data:"))
                            {
                                string data = dataLine.Substring("data:".Length).Trim();
                                 onDataReceived(eventName, data);
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            
           UnityEngine.Debug.LogError($"Error in StreamAsync: {ex.Message}");
            throw;
        }
    }


    /// <summary>
    /// Starts listening for changes in a Firebase Realtime Database path and triggers a callback on data updates.
    /// </summary>
    /// <param name="firebasePath">The path in the database to listen to.</param>
    /// <param name="onDataChanged">The callback function to handle data updates. Parameters: event type and JSON data.</param>
    /// <example>
    public void StartListening(string firebasePath, Action<string, string> onDataChanged)
    {
        _ = Task.Run(async () =>
        {
            while (true)
            {
                try
                {
                    await StreamAsync(firebasePath, (eventName, data) =>
                    {
                        if (eventName == "put" || eventName == "patch")
                        {
                            UnityMainThreadDispatcher.Instance().Enqueue(() =>
                            {
                                onDataChanged(eventName, data);
                            });
                        }
                    });
                }
                catch (Exception ex)
                {
#if UNITY_EDITOR
                    if (UnityEditor.EditorApplication.isPlaying)
                        UnityEngine.Debug.LogError($"Error in StartListening: {ex.Message}");
                    await Task.Delay(5000); // Retry after delay
#endif
                }
            }
        });
    }
}