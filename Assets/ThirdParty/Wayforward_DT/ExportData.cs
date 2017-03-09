using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace WF.AT {

    public class ExportData : ScriptableObject {
        public int currentColumns = 1;
        public string tableID = "test";
        public List<KeyValueString> dictionary = new List<KeyValueString>();

        public int Rows { get { return dictionary.Count; } }
        public int Columns { get { return currentColumns; } }

        [System.Serializable]
        public class KeyValueString : System.Object, IComparable<KeyValueString> {
            public string key;
            public List<string> val;

            public KeyValueString(string k, string v) {
                key = k;
                if (val == null) {
                    val = new List<string>();
                }
                val.Add(v);
            }

            public override string ToString() {
                string buildString = string.Format("{0}", this.key);
                foreach (string s in val) {
                    buildString += ("|" + s);
                }
                return buildString;
            }

            public void MatchColumns(int columnCount) {
                while (val.Count < columnCount) {
                    val.Add("");
                }
            }

            // Default comparer for Part type. 
            public int CompareTo(KeyValueString compare) {
                // A null value means that this object is greater. 
                if (compare == null) return 1;
                else {
                    int left = 0, right = 0;
                    int.TryParse(key, out left);
                    int.TryParse(compare.key, out right);
                    return left.CompareTo(right);
                }
            }

        }


        public void AddRow(KeyValueString kvs) {
            dictionary.Add(kvs);
        }
        public void RemoveRow(int index) {
            dictionary.RemoveAt(index);
        }

        public void MoveUpRow(int index) {
            if (index > 0) {
                KeyValueString tmp = dictionary[index];
                dictionary[index] = dictionary[index - 1];
                dictionary[index - 1] = tmp;
            }
        }

        public void MoveRowTo(int oldIndex, int newIndex) {
            if (oldIndex > 0) {
                KeyValueString tmp = dictionary[oldIndex];
                dictionary.RemoveAt(oldIndex);
                if (newIndex >= dictionary.Count) {
                    dictionary.Add(tmp);
                }
                else if (newIndex <= 0) {
                    dictionary.Insert(1, tmp);
                }
                else {
                    dictionary.Insert(newIndex, tmp);
                }

            }
        }

        public void AddColumn() {
            currentColumns++;
            foreach (KeyValueString kvs in dictionary) {
                kvs.val.Add("");
            }
        }
        public void RemoveColumn(int index) {
            currentColumns--;
            foreach (KeyValueString kvs in dictionary) {
                kvs.val.RemoveAt(index);
            }
        }
        public void MoveUpColumn(int index) {
            if (index > 0) {
                foreach (KeyValueString kvs in dictionary) {
                    string temp = kvs.val[index];
                    kvs.val[index] = kvs.val[index - 1];
                    kvs.val[index - 1] = temp;
                }
            }
        }

        public List<string> this[int param] {
            get {
                return dictionary[param].val;
            }
        }

        public List<string> this[string param] {
            get {
                for (int i = 0; i < dictionary.Count; i++) {
                    if (dictionary[i].key.Equals(param)) {
                        return dictionary[i].val;
                    }
                }
                return null;//param;
            }
        }

        //
        public void SetValue(string key, string val, int valIndex) {
            for (int i = 0; i < dictionary.Count; i++) {
                if (dictionary[i].key.Equals(key)) {
                    dictionary[i].val[valIndex] = val;
                }
            }
        }

        public bool Exists(string key) {
            for (int i = 0; i < dictionary.Count; i++) {
                if (dictionary[i].key == key)
                    return true;
            }
            return false;
        }

        public int GetKeyIndex(string key) {
            for (int i = 0; i < dictionary.Count; i++) {
                if (dictionary[i].key == key)
                    return i;
            }
            return -1;
        }

        public KeyValueString GetKeyValueStringAtIndex(int index) {
            if (dictionary.Count > index) {
                return dictionary[index];
            }
            else {
                return null;
            }
        }

        public void Sort() {
            if (dictionary.Count > 2) {
                dictionary.Sort(1, dictionary.Count - 1, null);
            }
        }

#if UNITY_EDITOR
        public void Export() {
            string exportPath = EditorUtility.SaveFilePanel("Export To...", Application.dataPath, tableID, "csv");
            List<string> keyValPairs = new List<string>();
            for (int i = 0; i < dictionary.Count; i++) {
                string stringBuild = dictionary[i].key;
                for (int j = 0; j < dictionary[i].val.Count; ++j) {
                    stringBuild += ("|" + dictionary[i].val[j]);
                }
                keyValPairs.Add(stringBuild);
            }
            System.IO.File.WriteAllLines(exportPath, keyValPairs.ToArray());
        }
#endif

    }
}