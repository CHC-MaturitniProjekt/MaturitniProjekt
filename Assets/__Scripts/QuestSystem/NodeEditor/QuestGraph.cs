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

        var nodeCreateButton = new Button(() =>
        {
            questGraphView.CreateNode("Quest Node");
        });
        nodeCreateButton.text = "Create Node";
        toolbar.Add(nodeCreateButton);

        toolbar.Add(new Button(() => SaveData()) { text = "Save data"});

        rootVisualElement.Add(toolbar);
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
        /*if (string.IsNullOrEmpty(name))
        {
            EditorUtility.DisplayDialog("Error", "Error: File missing", "ok");
        }*/
        Debug.Log("loading");
        var saveUtility = GraphSaveUtility.GetInstance(questGraphView);
        saveUtility.LoadGraph();


    }
}
