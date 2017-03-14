#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Collections;

namespace WF.AT {

    public static class EditorHelper {
        private static int s_PreviousIndent = 0;

        public static Rect CreateHorizontal(string title, Action drawAction, int indentLevel) {
            Rect fieldRect;

            EditorGUI.indentLevel = indentLevel;
            fieldRect = EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(title);
            EditorGUI.indentLevel = 0;
            drawAction();
            EditorGUILayout.EndHorizontal();
            return fieldRect;
        }

        public static void ZeroIndent() {
            s_PreviousIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
        }

        public static void RestoreIndent() {
            EditorGUI.indentLevel = s_PreviousIndent;
        }

        public static void DrawLine() {
            GUILayout.Box(MakeColorTexture(Color.yellow), GUILayout.ExpandWidth(true), GUILayout.Height(1));
        }

        public static Texture2D MakeColorTexture(Color col) {
            Texture2D newColor = new Texture2D(1, 1);
            newColor.SetPixel(1, 1, col);
            newColor.wrapMode = TextureWrapMode.Repeat;
            newColor.Apply();
            return newColor;
        }

        public static void CreateAddRemoveEntry(Action addAction, Action removeAction, bool horizontal = false) {

            if (!horizontal)
                EditorGUILayout.BeginVertical();
            else
                EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("-", GUILayout.Width(30), GUILayout.ExpandWidth(false))) {
                if (removeAction != null)
                    removeAction();
            }
            if (GUILayout.Button("+", GUILayout.Width(30), GUILayout.ExpandWidth(false))) {
                if (addAction != null)
                    addAction();
            }

            if (!horizontal)
                EditorGUILayout.EndVertical();
            else
                EditorGUILayout.EndHorizontal();
        }
    }
}
#endif