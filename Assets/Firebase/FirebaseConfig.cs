public class FirebaseConfig
{
    public string basePath { get; set; }
    public string apiKey { get; set; }

    public FirebaseConfig(string BasePath, string apiKey)
    {
        basePath = BasePath;
        this.apiKey = apiKey;
    }
}
