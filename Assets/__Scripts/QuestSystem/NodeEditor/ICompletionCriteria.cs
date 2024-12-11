public interface ICompletionCriteria
{
    bool IsCompleted();
    void Initialize(params object[] parameters);
}