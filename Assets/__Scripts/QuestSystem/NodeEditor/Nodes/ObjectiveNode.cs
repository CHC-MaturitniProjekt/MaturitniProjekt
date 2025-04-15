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

            var objectiveTypeField = new PopupField<string>("Objective Type", new List<string> { "Collect", "Interact", "PickUp", "GoTo" }, 0) { value = ObjectiveType };
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
            
            CreateCriteriaFields();
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
                    var existingCollectCriteria = CompletionCriteria.OfType<MoneyCollectionCriteria>().FirstOrDefault();
                    if (existingCollectCriteria != null)
                    {
                        amountField.value = existingCollectCriteria.RequiredAmount;
                    }
                    amountField.RegisterValueChangedCallback(evt =>
                    {
                        if (existingCollectCriteria == null)
                        {
                            existingCollectCriteria = new MoneyCollectionCriteria();
                            CompletionCriteria.Add(existingCollectCriteria);
                        }
                        existingCollectCriteria.RequiredAmount = evt.newValue;
                    });
                    criteriaContainer.Add(amountField);
                    break;

                case "Interact":
                    var npcField = new TextField("NPC Name") { value = "" };
                    var existingNpcCriteria = CompletionCriteria.OfType<NpcInteractionCriteria>().FirstOrDefault();
                    if (existingNpcCriteria != null)
                    {
                        npcField.value = existingNpcCriteria.NpcName;
                    }
                    npcField.RegisterValueChangedCallback(evt =>
                    {
                        if (existingNpcCriteria == null)
                        {
                            existingNpcCriteria = new NpcInteractionCriteria();
                            CompletionCriteria.Add(existingNpcCriteria);
                        }
                        existingNpcCriteria.NpcName = evt.newValue;
                    });
                    criteriaContainer.Add(npcField);
                    break;

                case "PickUp":
                    var pickUpField = new IntegerField("Item ID") { value = 0 };
                    var existingPickUpCriteria = CompletionCriteria.OfType<ItemCollectionCriteria>().FirstOrDefault();
                    if (existingPickUpCriteria != null)
                    {
                        pickUpField.value = existingPickUpCriteria.RequiredItemCount;
                    }
                    pickUpField.RegisterValueChangedCallback(evt =>
                    {
                        if (existingPickUpCriteria == null)
                        {
                            existingPickUpCriteria = new ItemCollectionCriteria();
                            CompletionCriteria.Add(existingPickUpCriteria);
                        }
                        existingPickUpCriteria.RequiredItemCount = evt.newValue;
                    });
                    criteriaContainer.Add(pickUpField);
                    break;
            }
        }

    }
}
#endif