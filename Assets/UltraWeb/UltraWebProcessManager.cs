using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using UnityEngine;

public class UltraWebProcessManager : MonoBehaviour
{
    private NamedPipeClientStream pipe;
    private StreamWriter writer;
    private StreamReader reader;

    private CancellationTokenSource cancelSource = new();
    private readonly ConcurrentQueue<string> fireAndForgetQueue = new();
    private readonly ConcurrentQueue<(string command, TaskCompletionSource<string> tcs)> responseQueue = new();
    private readonly SemaphoreSlim queueSignal = new(0);

    public static UltraWebProcessManager Instance { get; private set; }
    public enum MouseEventType { Move, Down, Up }
    public enum MouseButton { Left, Right, Middle, None }

    private async void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        await ConnectToPipeAsync();
    }

    private async Task ConnectToPipeAsync()
    {
        pipe = new NamedPipeClientStream(".", "UltraPipe", PipeDirection.InOut, PipeOptions.Asynchronous);

        try
        {
            Debug.Log("Connecting to UltraHost pipe...");
            await pipe.ConnectAsync(5000);
            Debug.Log("Connected to UltraHost.");

            writer = new StreamWriter(pipe) { AutoFlush = true };
            reader = new StreamReader(pipe);

            _ = Task.Run(() => ProcessQueueAsync(cancelSource.Token));
        }
        catch (IOException ex)
        {
            Debug.LogError($"Failed to connect to UltraHost: {ex.Message}");
        }
    }

    private async Task ProcessQueueAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await queueSignal.WaitAsync(token);

            if (token.IsCancellationRequested)
                break;


            if (responseQueue.TryDequeue(out var item))
            {
                try
                {
                    await writer.WriteLineAsync(item.command);
                    string response = await ReadMessageAsync(writer.BaseStream);
                    item.tcs.TrySetResult(response);
                }
                catch (Exception ex)
                {
                    item.tcs.TrySetException(ex);
                }
                continue;
            }


            if (fireAndForgetQueue.TryDequeue(out string simpleCommand))
            {
                try
                {
                    await writer.WriteLineAsync(simpleCommand);
                    await reader.ReadLineAsync();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Pipe write error (fire-and-forget): {ex.Message}");
                }
            }
        }
    }
    private async Task<string> ReadMessageAsync(Stream stream)
    {
        var lengthBuffer = new byte[4];
        int bytesRead = await stream.ReadAsync(lengthBuffer, 0, 4);
        if (bytesRead != 4)
            throw new IOException("Failed to read message length from pipe");

        int messageLength = BitConverter.ToInt32(lengthBuffer, 0);
        var messageBuffer = new byte[messageLength];
        int offset = 0;

        while (offset < messageLength)
        {
            int chunk = await stream.ReadAsync(messageBuffer, offset, messageLength - offset);
            if (chunk == 0)
                throw new IOException("Unexpected end of stream");
            offset += chunk;
        }

        return Encoding.UTF8.GetString(messageBuffer);
    }

    public void SendCommand(string command)
    {
        fireAndForgetQueue.Enqueue(command);
        queueSignal.Release();
    }

    public Task<string> SendCommandAsync(string command)
    {
        var tcs = new TaskCompletionSource<string>();
        responseQueue.Enqueue((command, tcs));
        queueSignal.Release();
        return tcs.Task;
    }

    public Task<string> CreateWindow(int width, int height)
        => SendCommandAsync($"CREATE_VIEW:{width}x{height}");

    public Task<string> LoadURL(string url)
        => SendCommandAsync($"LOAD_URL:{url}");

    public Task<string> RequestBitmapAsync()
        => SendCommandAsync("GET_BITMAP");

    public Task<string> SendMouseEvent(int x, int y, MouseEventType type, MouseButton button = MouseButton.None)
    {
        return SendCommandAsync($"MOUSE:{x},{y},{type.ToString().ToLower()},{button.ToString().ToLower()}");
    }

    public Task<string> SendMouseScroll(int deltaY)
    {
        return SendCommandAsync($"SCROLL:0,{deltaY}");
    }

    public Task<string> SendKeyDown(KeyCode key)
    {
        return SendCommandAsync($"KEY:{(int)key},down");
    }

    public Task<string> SendKeyUp(KeyCode key)
    {
        return SendCommandAsync($"KEY:{(int)key},up");
    }

    public Task<string> SendCharPress(char c)
    {
        int keyCode = c;
        return SendCommandAsync($"KEY:{keyCode},char,{c}");
    }

    private void OnDestroy()
    {
        cancelSource.Cancel();
        writer?.Dispose();
        reader?.Dispose();
        pipe?.Dispose();
    }
}
