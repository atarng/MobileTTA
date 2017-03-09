/***
 * Author: Alfred Tarng
 */
using UnityEngine;
using System.Collections;

namespace WF.AT {
    public static class DataTableLookup {
        public enum DataTables {
            DebugOptions = 0,
            UnitDefinitions = 1,
        }

        public const string DICTIONARY_PATH = "DataTables/{0}";
        public const string DEFAULT_DICT = "DataTables/DebugOptions";
        public static ExportData currentDataObject;
        static DataTables m_currentTable;

        public static bool KeyExists(string key) {
            if (currentDataObject == null) {
                GetCurrentDataTable();
            }
            return currentDataObject.Exists(key);
        }
        public static string GetDataTableValue(string key, int index = 0)//GetLocalizedString
        {
            if (currentDataObject == null) {
                GetCurrentDataTable();
            }
            return currentDataObject[key][index];
        }
        public static int IndexOf(string key) {
            if (currentDataObject == null) {
                GetCurrentDataTable();
            }
            return currentDataObject.GetKeyIndex(key);
        }
        public static string GetStringByIndices(int index, int index2) {
            if (currentDataObject == null) {
                GetCurrentDataTable();
            }
            if (index < 0 || index >= currentDataObject.dictionary.Count) {
                return null;
            }
            return currentDataObject[index][index2];
        }

        public static ExportData.KeyValueString GetKeyValueString(int index) {
            if (currentDataObject == null)
                GetCurrentDataTable();

            if (index < 0 || index >= currentDataObject.dictionary.Count)
                return null;

            return currentDataObject.dictionary[index];
        }
        public static int GetNumEntries() {
            if (currentDataObject == null) {
                GetCurrentDataTable();
            }
            return currentDataObject.dictionary.Count;
        }

        public static ExportData GetDefaultDataTable() {
            return (ExportData)Resources.Load(DEFAULT_DICT, typeof(ExportData));
        }
        public static void GetCurrentDataTable(DataTables tableType = DataTables.DebugOptions) {
            if (m_currentTable != tableType || currentDataObject == null) {
                m_currentTable = tableType;
                string path = string.Format(DICTIONARY_PATH, tableType.ToString("F"));
                currentDataObject = (ExportData)Resources.Load(path, typeof(ExportData));
                if (currentDataObject == null) {
                    currentDataObject = GetDefaultDataTable();
                }
            }
        }

        public static void UnloadCurrentTable() {
            currentDataObject = null;
            Resources.UnloadUnusedAssets();
        }
    }
}