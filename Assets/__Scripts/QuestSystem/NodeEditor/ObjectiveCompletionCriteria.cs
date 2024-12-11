using System;

[Serializable]
public class ItemCollectionCriteria : ICompletionCriteria
{
    public int RequiredItemCount;
    private int _currentItemCount;

    public void Initialize(params object[] parameters)
    {
        if (parameters.Length > 0 && parameters[0] is int itemCount)
        {
            RequiredItemCount = itemCount;
        }
    }

    public bool IsCompleted()
    {
        return _currentItemCount >= RequiredItemCount;
    }

    public void CollectItem()
    {
        _currentItemCount++;
    }
}

[Serializable]
public class MoneyCollectionCriteria : ICompletionCriteria
{
    public int RequiredAmount;
    private int _currentAmount;

    public void Initialize(params object[] parameters)
    {
        if (parameters.Length > 0 && parameters[0] is int amount)
        {
            RequiredAmount = amount;
        }
    }

    public bool IsCompleted()
    {
        return _currentAmount >= RequiredAmount;
    }

    public void CollectMoney(int amount)
    {
        _currentAmount += amount;
    }
}

[Serializable]
public class NpcInteractionCriteria : ICompletionCriteria
{
    public string NpcName;
    private bool _isInteracted;

    public void Initialize(params object[] parameters)
    {
        if (parameters.Length > 0 && parameters[0] is string npcName)
        {
            NpcName = npcName;
        }
    }

    public bool IsCompleted()
    {
        return _isInteracted;
    }

    public void InteractWithNpc()
    {
        _isInteracted = true;
    }
}