using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BoardManager))]
public class BoardManagerEditor : Editor
{
	public override void OnInspectorGUI()
	{
		BoardManager boardManager = target as BoardManager;
		base.OnInspectorGUI();
		
		if(GUILayout.Button("Log cell"))
			boardManager.LogCell();

		GUILayout.BeginHorizontal();

		if (boardManager.IsPause)
		{
			if(GUILayout.Button("Continue"))
				boardManager.Continue();

			if(GUILayout.Button("Step"))
				boardManager.DoStep();
		}
		else
		{
			if (GUILayout.Button("Pause"))
				boardManager.Pause();
		}



		GUILayout.EndHorizontal();
	}
}
