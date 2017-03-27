using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using AtRng.MobileTTA;
using WF.AT;

public class UnitManager : SingletonMB {

    public interface UnitDesciption {
        int DefinitionID { get; }
    }

    [Serializable]
    public class UnitPersistence : UnitDesciption {
        [SerializeField]
        int m_definitionID;
        public int DefinitionID {
            get {
                return m_definitionID;
            }
            private set {
                m_definitionID = value;
            }
        }

        public int PHealthModifier { get; private set; }
        public int SHealthModifier { get; private set; }
        public int AttackModifier { get; private set; }

        public int Experience { get; private set; }
        public int TargetExperience { get; private set; }

        [SerializeField]
        int m_level;
        public int Level {
            get { return m_level; }
            private set {
                m_level = value;
            }
        }

        public Guid UnitID { get; private set; }

        public UnitPersistence(int defID, bool randomNature = true) {
            DefinitionID = defID;
            UnitID = System.Guid.NewGuid();
        }

        public void AddExperience(int expToAdd) {
            Experience += expToAdd;
            if (TargetExperience > 0 && Experience > TargetExperience) {
                LevelUp();
            }
        }
        private void LevelUp() {
            Experience -= TargetExperience;
        }
    }

    [Serializable]
    public class UnitDefinition : UnitDesciption {
        public int DefinitionID { get; private set; }
        public int PhysicalHealth { get; private set; }
        public int SpiritualHealth { get; private set; }
        public int AttackValue { get; private set; }
        public bool AttackType { get; private set; }
        public int AttackRange { get; private set; }
        public int Movement { get; private set; }
        public string ArtKey { get; private set; }

        public void ParseData(int id, List<string> data) {
            DefinitionID    = id;
            PhysicalHealth  = int.Parse(data[0]);
            SpiritualHealth = int.Parse(data[1]);
            AttackValue     = int.Parse(data[2]);
            AttackType      = data[3].Equals("Spiritual") || data[3].Equals("S");
            Movement        = int.Parse(data[4]);
            AttackRange     = int.Parse(data[5]);
            ArtKey          = data[6];
        }
    }

    Dictionary<long, UnitDefinition> m_definitions = new Dictionary<long, UnitDefinition>();
    List<UnitDefinition> m_definitionsAsList = new List<UnitDefinition>();

    [Serializable]
    struct KeyPrefabPair{
        public string ID;
        public ArtPrefab Prefab;
    }

    // this is for the sprite art.
    [SerializeField]
    KeyPrefabPair[] m_keyToPrefabArray;
    Dictionary<string, ArtPrefab> m_keyToPrefabMap = new Dictionary<string, ArtPrefab>();

    ///*
    protected override void OnAwake() {
        for (int i = 0; i < m_keyToPrefabArray.Length; i++) {
            m_keyToPrefabMap.Add(m_keyToPrefabArray[i].ID, m_keyToPrefabArray[i].Prefab);
        }
        LoadDefinitions();
    }
//*/
    void LoadDefinitions() {
        int defID = -1;
        m_definitions.Clear();

        DataTableLookup.GetCurrentDataTable(DataTableLookup.DataTables.UnitDefinitions);
        for (int i = 0; i < DataTableLookup.currentDataObject.Rows; i++) {
            ExportData.KeyValueString kvs = DataTableLookup.GetKeyValueString(i);
            if (kvs != null) {
                if (int.TryParse(kvs.key, out defID)) {
                    UnitDefinition to_insert = new UnitDefinition();
                    to_insert.ParseData(defID, kvs.val);
                    m_definitions.Add(defID, to_insert);
                    //m_definitionsAsList.Add(to_insert);
                }
            }
        }
    }

    public UnitDefinition GetDefinition(int id) {
        return m_definitions.ContainsKey(id) ? m_definitions[id] : null;
        //return m_definitionsAsList[id];
    }
    long[] notCollectibles = new long[3] { 0, 7, 8 };
    public List<UnitDefinition> GetAsCollection() {
        List<UnitDefinition> toRet = new List<UnitDefinition>();

        List<long> asList = notCollectibles.ToList();
        for (Dictionary<long, UnitDefinition>.Enumerator iter = m_definitions.GetEnumerator(); iter.MoveNext();) {
            if (!asList.Contains(iter.Current.Key)) {
                toRet.Add(iter.Current.Value);
            }
        }

        return toRet;
    }

    public ArtPrefab GetArtFromKey( string key ) {
        return m_keyToPrefabMap[key];
    }
}