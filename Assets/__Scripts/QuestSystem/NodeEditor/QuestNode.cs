using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class QuestNode : Node
{
    public enum NodeTypes
    {
        MainQuestNode,
        ObjectiveNode,
        RewardNode
    }

    public List<string> RewardTypes = new List<string>() { "PerkPoints" , "Money" };

    public string GUID;

    public string QuestName;
    public string QuestDescription;
    public QuestNode.NodeTypes QuestType;
    public List<ICompletionCriteria> CompletionCriteria = new List<ICompletionCriteria>();


    public bool EntryPoint = false;

    public virtual void DrawNode()
    {
        RefreshExpandedState();
        RefreshPorts();
    }
}

public class MainQuestNode : QuestNode
{
    public Color TitleColor = new Color(50f / 255f, 0f / 255f, 0f / 255f);

    public bool EntryPoint = false;

    public override void DrawNode()
    {
        QuestType = NodeTypes.MainQuestNode;

        var titleElement = this.titleContainer;
        titleElement.style.backgroundColor = new StyleColor(TitleColor);

        var inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
        inputPort.portName = "Input";
        inputContainer.Add(inputPort);

        var nextQuestPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float));
        nextQuestPort.portName = "Next quest(s)";
        outputContainer.Add(nextQuestPort);

        var objPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float));
        objPort.portName = "Objectives";
        outputContainer.Add(objPort);

        var rewardPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float));
        rewardPort.portName = "Rewards";
        outputContainer.Add(rewardPort);

        var questNameField = new TextField("Quest Name") { value = QuestName };
        questNameField.RegisterValueChangedCallback(evt => QuestName = evt.newValue);
        mainContainer.Add(questNameField);

        var descriptionField = new TextField("Description") { value = QuestDescription };
        descriptionField.RegisterValueChangedCallback(evt => QuestDescription = evt.newValue);
        mainContainer.Add(descriptionField);

        RefreshExpandedState();
        RefreshPorts();
    }

}

public class ObjectiveNode : QuestNode
{
    public string ObjectiveDescription;
    public string ObjectiveType;
    public bool isOptional;

    public ObjectiveNode()
    {
        CompletionCriteria = new List<ICompletionCriteria>();
    }

    public override void DrawNode()
    {
        QuestType = NodeTypes.ObjectiveNode;

        var inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(float));
        inputPort.portName = "Objective";
        inputContainer.Add(inputPort);

        var titleElement = this.titleContainer;
        titleElement.style.backgroundColor = new StyleColor(new Color(0f / 255f, 50f / 255f, 0f / 255f));

        var objectiveDescriptionField = new TextField("Objective Description") { value = ObjectiveDescription };
        objectiveDescriptionField.RegisterValueChangedCallback(evt => ObjectiveDescription = evt.newValue);
        Add(objectiveDescriptionField);

        var objectiveTypeField = new PopupField<string>("Objective Type", new List<string> { "Collect", "Interact" }, 0) { value = ObjectiveType };
        objectiveTypeField.RegisterValueChangedCallback(evt =>
        {
            ObjectiveType = evt.newValue;
            CreateCriteriaFields();
        });
        Add(objectiveTypeField);

        var criteriaContainer = new VisualElement();
        criteriaContainer.name = "CriteriaContainer";
        Add(criteriaContainer);

        var optional = new Toggle("Is optional") { value = isOptional };
        optional.RegisterValueChangedCallback(evt => isOptional = evt.newValue);
        Add(optional);

        RefreshExpandedState();
        RefreshPorts();
    }

    private void CreateCriteriaFields()
    {
        var criteriaContainer = this.Q<VisualElement>("CriteriaContainer");
        criteriaContainer.Clear();

        switch (ObjectiveType)
        {
            case "Collect":
                var amountField = new IntegerField("Amount") { value = 0 };
                amountField.RegisterValueChangedCallback(evt =>
                {
                    var criteria = CompletionCriteria.OfType<MoneyCollectionCriteria>().FirstOrDefault();
                    if (criteria == null)
                    {
                        criteria = new MoneyCollectionCriteria();
                        CompletionCriteria.Add(criteria);
                    }
                    criteria.RequiredAmount = evt.newValue;
                });
                criteriaContainer.Add(amountField);
                break;

            case "Interact":
                var npcField = new TextField("NPC Name") { value = "" };
                npcField.RegisterValueChangedCallback(evt =>
                {
                    var criteria = CompletionCriteria.OfType<NpcInteractionCriteria>().FirstOrDefault();
                    if (criteria == null)
                    {
                        criteria = new NpcInteractionCriteria();
                        CompletionCriteria.Add(criteria);
                    }
                    criteria.NpcName = evt.newValue;
                });
                criteriaContainer.Add(npcField);
                break;
        }
    }
}

public class RewardNode : QuestNode
{
    public string RewardType;
    public int RewardValue;
    public Color TitleColor = new Color(0f /255f, 0f /255f, 50f /255f);

    public override void DrawNode()
    {
        QuestType = NodeTypes.RewardNode;

        var inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(float));
        inputPort.portName = "Rewards";
        inputContainer.Add(inputPort);

        var titleElement = this.titleContainer;
        titleElement.style.backgroundColor = new StyleColor(TitleColor);

        var dynamicFieldCont = new VisualElement();
        Add(dynamicFieldCont);
        
        IntegerField rewardValueField = new IntegerField("Reward value") { value = RewardValue };
        
        dynamicFieldCont.Add(rewardValueField);

        var rewardType = new PopupField<string>("Reward type", RewardTypes, RewardTypes.IndexOf(RewardType));
        rewardType.RegisterValueChangedCallback(evt => {
            RewardType = evt.newValue;
            dynamicFieldCont.Clear();

            switch (evt.newValue)
            {
                case "PerkPoints":
                    rewardValueField.label = "Perk point amount";
                    break;

                case "Money":
                    rewardValueField.label = "Money amount";
                    break;
            }
        });
        rewardType.label = "Reward Type";
        Add(rewardType);
        rewardType.value = RewardType;

        switch (RewardType)
        {
            case "PerkPoints":
                dynamicFieldCont.Add(new IntegerField("Perk point amount") { value = RewardValue });
                break;

            case "Money":
                dynamicFieldCont.Add(new IntegerField("Money amount") { value = RewardValue });
                break;
        }

        RefreshExpandedState();
        RefreshPorts();
    }

}