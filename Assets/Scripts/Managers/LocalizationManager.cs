using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

using AtRng.MobileTTA;
using WF.AT;

public class LocalizationManager : SingletonMB {
    struct LocalizationUnit {
        string m_localized_text;
        LocalizationUnit(string text) {
            m_localized_text = text;
        }
    };

    Dictionary<string, string> m_definitions = new Dictionary<string, string>();
    // Dictionary<string, LocalizationUnit> m_definitions = new Dictionary<long, LocalizationUnit>();

    protected override void OnAwake() {
        LoadDefinitions();
    }

    void LoadDefinitions() {
        string localization_key = "";
        m_definitions.Clear();

        DataTableLookup.GetCurrentDataTable(DataTableLookup.DataTables.LocalizationTable);
        for (int i = 0; i < DataTableLookup.currentDataObject.Rows; i++) {
            ExportData.KeyValueString kvs = DataTableLookup.GetKeyValueString(i);
            if (kvs != null) {
                m_definitions.Add(kvs.key, kvs.val[0]);
                // if (string.TryParse(, out localization_key)) {
                //     LocalizationUnit to_insert = new LocalizationUnit(kvs.value[0]);
                // }
            }
        }
    }

    string GetTextFromKey(string key) {
        return m_definitions[key];
    }

}
