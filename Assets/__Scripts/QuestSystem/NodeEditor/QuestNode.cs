using System.Collections;
using System.Collections.Generic;
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
    public string QuestType;

    public bool EntryPoint = false;

    public virtual void DrawNode()
    {
        RefreshExpandedState();
        RefreshPorts();
    }
}

public class MainQuestNode : QuestNode
{
    public string QuestName;
    public string QuestDescription;
    public string QuestType;
    public Color TitleColor = new Color(50f / 255f, 0f / 255f, 0f / 255f);

    public bool EntryPoint = false;

    public override void DrawNode()
    {
        var titleElement = this.titleContainer;
        titleElement.style.backgroundColor = new StyleColor(TitleColor);

        var inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
        inputPort.portName = "Input";
        inputContainer.Add(inputPort);

        var nextQuestPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float));
        nextQuestPort.portName = "Next quest(s)";
        inputContainer.Add(nextQuestPort);

        var objPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float));
        objPort.portName = "Objectives";
        inputContainer.Add(objPort);

        var rewardPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float));
        rewardPort.portName = "Rewards";
        inputContainer.Add(rewardPort);

        var questNameField = new TextField("Quest Name") { value = QuestName };
        questNameField.RegisterValueChangedCallback(evt => QuestName = evt.newValue);
        mainContainer.Add(questNameField);

        var descriptionField = new TextField("Description") { value = QuestDescription };
        descriptionField.RegisterValueChangedCallback(evt => QuestDescription = evt.newValue);
        mainContainer.Add(descriptionField);

        base.DrawNode();
    }

}

public class ObjectiveNode : QuestNode
{
    public string ObjectiveDescription;
    public string ObjectiveType; 
    public string CompletionCriteria;
    public bool isOptional;

    public Color TitleColor = new Color(0f / 255f, 50f / 255f, 0f / 255f);

    public override void DrawNode()
    {
        var inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(float));
        inputPort.portName = "Objective";
        outputContainer.Add(inputPort);

        var titleElement = this.titleContainer;
        titleElement.style.backgroundColor = new StyleColor(TitleColor);

        var objectiveDescriptionField = new TextField("Objective Description") { value = ObjectiveDescription };
        objectiveDescriptionField.RegisterValueChangedCallback(evt => ObjectiveDescription = evt.newValue);
        Add(objectiveDescriptionField);

        var objectiveTypeField = new TextField("Objective Type") { value = ObjectiveType };
        objectiveTypeField.RegisterValueChangedCallback(evt => ObjectiveType = evt.newValue);
        Add(objectiveTypeField);

        var completionCriteriaField = new TextField("Completion Criteria") { value = CompletionCriteria };
        completionCriteriaField.RegisterValueChangedCallback(evt => CompletionCriteria = evt.newValue);
        Add(completionCriteriaField);

        var optional = new Toggle("Is optional") { value = isOptional };
        optional.RegisterValueChangedCallback(evt => isOptional = evt.newValue);
        Add(optional);

        base.DrawNode();

    }

}

public class RewardNode : QuestNode
{
    public string RewardType;
    public int RewardValue;
    public Color TitleColor = new Color(0f /255f, 0f /255f, 50f /255f);

    public override void DrawNode()
    {
        var inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(float));
        inputPort.portName = "Rewards";
        outputContainer.Add(inputPort);

        var titleElement = this.titleContainer;
        titleElement.style.backgroundColor = new StyleColor(TitleColor);

        var dynamicFieldCont = new VisualElement();
        Add(dynamicFieldCont);

        var rewardType = new PopupField<string>("Reward type", RewardTypes, 0);
        rewardType.RegisterValueChangedCallback(evt => {
            RewardType = evt.newValue;
            dynamicFieldCont.Clear();

            switch (evt.newValue)
            {
                case "PerkPoints":
                    dynamicFieldCont.Add(new IntegerField("Perk point ammount") { value = 0 });                             //fix save nebude fakat
                    break;

                case "Money":
                    dynamicFieldCont.Add(new IntegerField("Money ammount") { value = 0 });
                    break;
            }

        });
        rewardType.label = "Reward Type";
        Add(rewardType);

        base.DrawNode();
    }

}