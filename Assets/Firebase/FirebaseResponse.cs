using Newtonsoft.Json;

public class FirebaseResponse
{
    public string RawJson { get; set; }
    public T ResultAs<T>()
    {
        return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(RawJson);
    }
}