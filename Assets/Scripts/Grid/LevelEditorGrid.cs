﻿
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AtRng.MobileTTA {
    public enum PlaceableType {
        Unit,
        Impassable,
        Tile
    }
    // Runtime "LevelEditorPlaceable"?
    // WHAT IS THIS??!!
    public struct LevelEditorPlaceableRT {
        public int X, Y;
        public TileTraversalEnum tileTraversal;
        public int unitDefAtLocation;
        public int playerOwner;
    }

    [Serializable]
    public struct LevelEditorPlaceable {
        public PlaceableType placeableType;
        public int X;
        public int Y;
        public int ID;
        public int PlayerID;

        public string m_on_death_trigger;
        public string m_on_attacked_trigger;
        public int m_on_attacked_counter;

        public string m_on_move_trigger;
        public int m_on_move_counter;
        public Vector2 m_on_target_move;
    }

    // Monobehavior
    public class LevelEditorGrid : Grid {
#if UNITY_EDITOR
        const string FILE_EXT = "asset";
        const string PREFAB_EXT = "prefab";

        ///*
        [SerializeField]
        List<LevelEditorPlaceable> placeablesList;
        public LevelEditorPlaceable[] GetPlaceablesList() {
            return placeablesList.ToArray();
        }
        //*/

        [SerializeField]
        UnitManager.UnitPersistence[] m_opponentDeckList;
        public UnitManager.UnitPersistence[] GetOpponentDeckList() {
            return m_opponentDeckList;
        }

        [SerializeField]
        bool m_usesPlayerDeckList = true;
        public bool UsesPlayerDeckList() { return m_usesPlayerDeckList; }

        [SerializeField]
        bool m_usesAIOpponent = false;
        public bool UsesAIOpponent() { return m_usesAIOpponent; }

        [SerializeField]
        EditorUnit m_unitPrefab;
        [SerializeField]
        Impassable m_impassable;

        //
        [SerializeField]
        InputField m_widthTextField;
        [SerializeField]
        InputField m_heightTextField;

        public override int Width {
            get {
                return base.Width;
            }
            protected set {
                base.Width = value;
                m_widthTextField.text = base.Width.ToString();
            }
        }
        public override int Height {
            get {
                return base.Height;
            }
            protected set {
                base.Height = value;
                m_heightTextField.text = base.Height.ToString();
            }
        }

        public LevelScriptableObject AsScriptableObject() {
            //new LevelScriptableObject(placeablesArray);
            LevelScriptableObject ret = ScriptableObject.CreateInstance<LevelScriptableObject>();
            ret.ReadLevelEditorGrid(this);
            return ret;
        }

        public void AddToPlaceablesList( int id, int x, int y, PlaceableType pt ) {
            LevelEditorPlaceable lep = new LevelEditorPlaceable();
            lep.ID = id;
            lep.X = x;
            lep.Y = y;
            lep.placeableType = pt;
            lep.PlayerID = 1;

            placeablesList.Add( lep );
        }

        public void RemoveTileInfo(int x, int y) {
            LevelEditorPlaceable found = placeablesList.Find((candidate) => {
                bool match = candidate.X == x;
                match &= (candidate.Y == y);
                match &= (candidate.placeableType == PlaceableType.Tile);
                return match;

            });
            placeablesList.Remove(found);
        }

        public void RemoveFromPlaceables(IUnit toFind) {
            LevelEditorPlaceable found = placeablesList.Find((candidate)=> {

                bool match = candidate.X == toFind.AssignedToTile.xPos;
                match &= (candidate.Y == toFind.AssignedToTile.yPos);
                match &= (candidate.ID == toFind.GetID());

                return match;

            });
            if (!placeablesList.Remove(found)) {
                Debug.LogError("Could Not Remove something not in List.");
            }
        }

        private void LoadPlaceables() {
            for (int i = placeablesList.Count - 1; i >= 0; i--) {
                Tile tileAtXY = GetTileAt((placeablesList[i].Y), (placeablesList[i].X));
                if (tileAtXY != null) {
                    switch (placeablesList[i].placeableType) {
                        case PlaceableType.Unit:
                            // Create Unit
                            EditorUnit unit_to_place_on_tile = GameObject.Instantiate<EditorUnit>(m_unitPrefab);
                            UnitManager.UnitDefinition ud = UnitManager.GetInstance<UnitManager>().GetDefinition(placeablesList[i].ID);
                            unit_to_place_on_tile.ReadDefinition(ud);
                            unit_to_place_on_tile.transform.localScale = Vector3.one * .01f;

                            // Assign to Tile.
                            tileAtXY.SetPlaceable(unit_to_place_on_tile);

                            IUnit iUnit = unit_to_place_on_tile;
                            iUnit.AssignPlayerOwner(placeablesList[i].PlayerID);
                            iUnit.AssignedToTile = tileAtXY;

                            break;
                        case PlaceableType.Impassable:
                            Impassable impassable = GameObject.Instantiate<Impassable>(m_impassable);

                            //GameObject go = new GameObject();
                            //Impassable impassable = go.AddComponent<Impassable>();

                            tileAtXY.SetPlaceable(impassable);

                            break;
                        case PlaceableType.Tile:
                            tileAtXY.SetTileTraversal((TileTraversalEnum)placeablesList[i].ID);
                            break;
                    }
                }
                else {
                    Debug.LogWarning(string.Format("No Tile At: ({0}, {1})", (placeablesList[i].X), (placeablesList[i].Y)));
                    placeablesList.RemoveAt(i);
                }
            }
        }

        //
        public void ReadScriptableObject(LevelScriptableObject lso) {
            placeablesList = new List<LevelEditorPlaceable>(); //[lso.PlaceablesArray.Length];
            //placeablesArray.Count
            for (int i = 0; i < lso.PlaceablesArray.Length; i++) {
                //placeablesArray[i] = lso.PlaceablesArray[i];
                placeablesList.Add(lso.PlaceablesArray[i]);
            }

            m_opponentDeckList = new UnitManager.UnitPersistence[lso.OpposingDeckList.Length];
            for (int i = 0; i < m_opponentDeckList.Length; i++) {
                m_opponentDeckList[i] = lso.OpposingDeckList[i];
            }

            Width  = lso.Width;
            Height = lso.Height;

            m_usesPlayerDeckList = lso.UsesPlayerDeckList;
            m_usesAIOpponent = lso.UsesAIOpponent;
        }

        public override void InitializeGrid(int width = -1, int height = -1) {
            base.InitializeGrid(width, height);
            LoadPlaceables();
        }
        public void LoadLevelData() {
            string path_to_load_level = EditorUtility.OpenFilePanel("Load Level", "Assets/Resources/LevelData", FILE_EXT);
            string fileName = Path.GetFileName(path_to_load_level);
            bool isArena = path_to_load_level.Contains("Arena");
            if (fileName.Length > 0) {
                fileName = fileName.Remove(fileName.Length - (FILE_EXT.Length + 1)); //".map"
                try {
                    LevelScriptableObject lso =
                        (LevelScriptableObject)Resources.Load("LevelData/" + (isArena ? "Arena/" : "Campaign/") + fileName);
                    if ( lso != null ) {
                        ReadScriptableObject(lso);
                        ClearGrid();
                        InitializeGrid();
                    }
                }
                catch (Exception e) {
                    Debug.LogError(e.Message);
                }
            }

            // Load Placeables.

        }
        public void SaveLevelData() {
            const string LEVEL_DATA_PATH = "Assets/Resources/LevelData/";
            //var path_to_save_level = EditorUtility.SaveFilePanel("Save Level as \'.prefab\'", LEVEL_DATA_PATH, "LevelEditorGrid." + PREFAB_EXT, PREFAB_EXT);
            var path_to_save_level = EditorUtility.SaveFilePanel("Save Level as \'.asset\'", LEVEL_DATA_PATH, "test." + FILE_EXT, FILE_EXT);
            bool isArena = path_to_save_level.Contains("Arena");

            string fileName = Path.GetFileName(path_to_save_level);
            if(fileName.Length > 0) {
                //Debug.Log(string.Format("Fullpath: {0} filename: {1}",path_to_save_level, fileName));
                try {
                    LevelScriptableObject lso = AsScriptableObject();
                    AssetDatabase.CreateAsset(lso,  LEVEL_DATA_PATH + (isArena ? "Arena/" : "Campaign/") + fileName);
                    AssetDatabase.SaveAssets();
                }
                catch (Exception e) {
                    Debug.LogError(e.Message);
                }
            }
        }

        public void UpdateDimensions() {
            Debug.Log("[LevelEditorGrid/UpdateDimensions]");
            Width  = int.Parse( m_widthTextField.text);
            Height = int.Parse(m_heightTextField.text);

            ClearGrid();
            InitializeGrid();
        }
#endif
    }
}