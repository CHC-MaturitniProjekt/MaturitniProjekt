#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.__Scripts.QuestSystem.NodeEditor
{
    public class RewardNode : QuestNode
    {
        public string RewardType;
        public int RewardValue;
        private List<string> RewardTypes = new List<string>() { "Money" };

        public RewardNode() 
        {
            QuestType = NodeTypes.RewardNode;
            TitleColor = new Color(0f / 255f, 0f / 255f, 50f / 255f);
        }

        public override void DrawNode()
        {
            var inputPort = new CustomPort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, ConnectionType.Reward);
            inputPort.portName = "Rewards";
            inputContainer.Add(inputPort);

            var titleElement = this.titleContainer;
            titleElement.style.backgroundColor = new StyleColor(TitleColor);

            var rewardTypeContainer = new VisualElement();
            rewardTypeContainer.name = "RewardType";
            Add(rewardTypeContainer);

            IntegerField rewardValueField = new IntegerField("Reward value") { value = RewardValue };
            rewardValueField.RegisterValueChangedCallback(evt =>
            {
                RewardValue = evt.newValue;
            });
            rewardTypeContainer.Add(rewardValueField);

            var rewardType = new PopupField<string>("Reward type", RewardTypes, RewardTypes.IndexOf(RewardType));
            rewardType.RegisterValueChangedCallback(evt => {
                RewardType = evt.newValue;
                rewardValueField.label = "Money amount";
            });
            rewardType.label = "Reward Type";
            Add(rewardType);
            rewardType.value = RewardType;

            RefreshExpandedState();
            RefreshPorts();
        }

    }
}
#endif