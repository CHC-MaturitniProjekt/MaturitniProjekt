using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

[CreateAssetMenu(fileName = "SyntaxHighlighter", menuName = "ScriptableObjects/SyntaxHighlighter")]
[Serializable]
public class SyntaxHighlighterSO : ScriptableObject
{
	public List<SyntaxHighlighterWord> words;
}

[Serializable] 
public class SyntaxHighlighterWord
{
    public string word = "default"; 
    public Color color = Color.blue; 
}