using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AtRng.MobileTTA;
using WF.AT;

public class UnitManager : SingletonMB {

    [Serializable]
    public class UnitDefinition {
        public int PhysicalHealth { get; private set; }
        public int SpiritualHealth { get; private set; }
        public int AttackValue { get; private set; }
        public bool AttackType { get; private set; }
        public int AttackRange { get; private set; }
        public int Movement { get; private set; }
        public string ArtKey { get; private set; }

        public void ParseData(List<string> data) {
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
    [Serializable]
    struct KeyPrefabPair{
        public string ID;
        public GameObject Prefab;
    }
    [SerializeField]
    KeyPrefabPair[] m_keyToPrefabArray;
    Dictionary<string, GameObject> m_keyToPrefabMap = new Dictionary<string, GameObject>();

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
                    to_insert.ParseData(kvs.val);
                    m_definitions.Add(defID, to_insert);
                }
            }
        }
    }
    public UnitDefinition GetDefinition(long id) {
        return m_definitions.ContainsKey(id) ? m_definitions[id] : null;
    }

    public GameObject GetArtFromKey( string key ) {
        return m_keyToPrefabMap[key];
    }
}