using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixelCrushers.DialogueSystem.ChatMapper;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Experimental.GraphView.Port;

namespace Assets.__Scripts.QuestSystem.NodeEditor
{
    public class MainQuestNode : QuestNode
    {
        public string QuestName;
        public string QuestDescription;
        public int QuestID;
        
        public MainQuestNode()
        {
            QuestType = NodeTypes.MainQuestNode;
            TitleColor = new Color(50f / 255f, 0f / 255f, 0f / 255f);
        }

        public override void DrawNode()
        {
            var titleElement = this.titleContainer;
            titleElement.style.backgroundColor = new StyleColor(TitleColor);

            var inputPort = new CustomPort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, ConnectionType.Quest);
            inputPort.portName = "Input";
            inputContainer.Add(inputPort);

            var nextQuestPort = new CustomPort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, ConnectionType.Quest);
            nextQuestPort.portName = "Next quest(s)";
            outputContainer.Add(nextQuestPort);

            var objPort = new CustomPort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, ConnectionType.Objective);
            objPort.portName = "Objectives";
            outputContainer.Add(objPort);

            var rewardPort = new CustomPort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, ConnectionType.Reward);
            rewardPort.portName = "Rewards";
            outputContainer.Add(rewardPort);

            var questIDField = new IntegerField("Quest ID") { value = QuestID };
            questIDField.RegisterValueChangedCallback(evt => QuestID = evt.newValue);
            mainContainer.Add(questIDField);
            
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
}
