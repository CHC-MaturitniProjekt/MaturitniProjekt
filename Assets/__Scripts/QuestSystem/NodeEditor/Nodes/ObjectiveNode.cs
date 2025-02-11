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
    public class ObjectiveNode : QuestNode
    {
        public string ObjectiveDescription;
        public string ObjectiveType;
        public bool isOptional;
        public bool isCompleted;
        public List<ICompletionCriteria> CompletionCriteria = new List<ICompletionCriteria>();

        public ObjectiveNode()
        {
            CompletionCriteria = new List<ICompletionCriteria>();
            QuestType = NodeTypes.ObjectiveNode;
        }

        public override void DrawNode()
        {
            var inputPort = new CustomPort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, ConnectionType.Objective);
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
}
