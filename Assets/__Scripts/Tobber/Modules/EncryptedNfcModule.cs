using UnityEngine;
using UnityEngine.Events;

class EncryptedNfcModule : PuzzleModule
{
    [Header("Inspector Events")]
    public VoidEvent OnDecryptionSuccess;

    [SerializeField] private int xorKey = 0b_10101010;
    [SerializeField] private int count = 3;

    private int receivedCount = 0;

    public override void setInput(int[] input)
    {
        if (input == null || input.Length == 0)
            return;

        int decrypted = Decode(input[0]);

        if (decrypted != outputPorts[receivedCount])
        {
            receivedCount = 0;
            return;
        }

        receivedCount++;

        if (receivedCount >= count)
        {
            OnDecryptionSuccess?.Invoke();
            receivedCount = 0;
        }
    }

    private int Decode(int encrypted)
    {
        // XOR + rotate right by 1 bit
        int xored = encrypted ^ xorKey;
        int rotated = (xored >> 1) | ((xored & 1) << 7);
        return rotated;
    }
}
