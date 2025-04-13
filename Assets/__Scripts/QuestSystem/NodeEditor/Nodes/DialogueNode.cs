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
    public class DialogueNode : QuestNode
    {
        public string DialogueName;
        public int NPCID;
        public int order;
        public bool isCompleted;
        public bool isSMS;

        public DialogueNode()
        {
            QuestType = NodeTypes.DialogueNode;
        }

        public override void DrawNode()
        {
            var inputPort = new CustomPort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, ConnectionType.Dialogue);
            inputPort.portName = "Dialogue";
            inputContainer.Add(inputPort);

            var titleElement = this.titleContainer;
            titleElement.style.backgroundColor = new StyleColor(new Color(50f / 255f, 0f / 255f, 50f / 255f));

            var dialogueNameField = new TextField("Dialogue Name") { value = DialogueName };
            dialogueNameField.RegisterValueChangedCallback(evt => DialogueName = evt.newValue);
            Add(dialogueNameField);

            var npcIDField = new IntegerField("NPC ID") { value = NPCID };
            npcIDField.RegisterValueChangedCallback(evt => NPCID = evt.newValue);
            Add(npcIDField);
            
            var orderField = new IntegerField("Order") { value = order };
            orderField.RegisterValueChangedCallback(evt => order = evt.newValue);
            Add(orderField);

            var smsField = new Toggle("Is SMS") { value = isSMS };
            smsField.RegisterValueChangedCallback(evt => isSMS = evt.newValue);
            Add(smsField);
            
            RefreshExpandedState();
            RefreshPorts();
        }

    }
}
#endif