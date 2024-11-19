using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class QuestGraph : EditorWindow
{
    private QuestGraphView questGraphView;

    [MenuItem("Quest/Quest System")]
    public static void OpenQuestWindow()
    {
        var window = GetWindow<QuestGraph>();
        window.titleContent = new GUIContent("Quest Graph");
    }

    private void OnEnable()
    {
        ConstructGraphView();
        GenerateToolBar();

        var graphSaveUtility = GraphSaveUtility.GetInstance(questGraphView);
        graphSaveUtility.LoadGraph();

    }

    private void ConstructGraphView()
    {
        questGraphView = new QuestGraphView
        {
            name = "Quest Graph"
        };

        questGraphView.StretchToParentSize();
        rootVisualElement.Add(questGraphView);

    }

    private void GenerateToolBar()
    {
        var toolbar = new Toolbar();

        var createNodeButton = new Button(() =>
        {
            ShowNodeCreationDropdown();
        })
        {
            text = "Create Node"
        };
        toolbar.Add(createNodeButton);
        toolbar.Add(new Button(() => SaveData()) { text = "Save data" });

        rootVisualElement.Add(toolbar);
    }

    private void ShowNodeCreationDropdown()
    {
        var dropdownMenu = new GenericMenu();

        dropdownMenu.AddItem(new GUIContent("Quest Node"), false, () =>
        {
            questGraphView.CreateNode(QuestNode.NodeTypes.MainQuestNode);
        });

        dropdownMenu.AddItem(new GUIContent("Objective Node"), false, () =>
        {
            questGraphView.CreateNode(QuestNode.NodeTypes.ObjectiveNode);
        });

        dropdownMenu.AddItem(new GUIContent("Reward Node"), false, () =>
        {
            questGraphView.CreateNode(QuestNode.NodeTypes.RewardNode);
        });

        dropdownMenu.ShowAsContext();
    }

    private void SaveData()
    {
        Debug.Log("save");
        var saveUtility = GraphSaveUtility.GetInstance(questGraphView);
        saveUtility.SaveGraph();
    }

    private void OnDisable()
    {
        rootVisualElement.Remove(questGraphView);
    }

    private void LoadGraph()
    {
        Debug.Log("loading");
        var saveUtility = GraphSaveUtility.GetInstance(questGraphView);
        saveUtility.LoadGraph();


    }
}
