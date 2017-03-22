using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEditor;

using AtRng.MobileTTA;

[CustomEditor(typeof(LevelEditorGrid))]
public class LevelEditorGridInspector : Editor {
    const string LEVEL_DATA_PATH = "LevelData/";
    const string FILE_EXT = ".asset";
    string fileName;
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        if (GUILayout.Button("Save Map")) {
            SaveMap();
        }

        EditorGUILayout.BeginHorizontal();

        fileName = EditorGUILayout.TextField(fileName);
        if (GUILayout.Button("LoadMap")) {
            LoadMap(fileName);
        }

        EditorGUILayout.EndHorizontal();

    }

    private void SaveMap() {

        Debug.Log("[LevelEditorGridInspector] Save Map.");

        LevelEditorGrid leg = target as LevelEditorGrid;
        LevelScriptableObject lso = leg.AsScriptableObject();

        //LevelScriptableObject lso = ScriptableObject.CreateInstance<LevelScriptableObject>();
        if(fileName.Length > 0) {
            try {
                AssetDatabase.CreateAsset(lso, "Assets/Resources/" + LEVEL_DATA_PATH + fileName + FILE_EXT);
                AssetDatabase.SaveAssets();
            }
            catch (Exception e) {
                Debug.LogError(e.Message);
            }
        }
    }

    private void LoadMap(string filePath) {
        Debug.Log("[LevelEditorGridInspector] Load Map: " + filePath);

        string fullPath = EditorUtility.OpenFilePanel("Open Map", "", "asset");
        fileName = Path.GetFileName(fullPath);
        if (fileName.Length > 0) {
            fileName = fileName.Remove(fileName.Length - FILE_EXT.Length); //".map"
            try{
                LevelScriptableObject lso = (LevelScriptableObject)Resources.Load(LEVEL_DATA_PATH + fileName);
                LevelEditorGrid leg = target as LevelEditorGrid;
                if(leg != null && lso != null) {
                    leg.ReadScriptableObject(lso);
                }
            }
            catch (Exception e) {
                Debug.LogError(e.Message);
            }
        }
    }
}
