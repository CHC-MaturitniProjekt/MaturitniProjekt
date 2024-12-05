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
        public List<string> RewardTypes = new List<string>() { "PerkPoints", "Money" };

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
}
