
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

using UnityEngine;
namespace AtRng.MobileTTA {

    public class SaveGameManager { //: MonoBehaviour {
        static SaveGame s_saveGame = null;
        private void Start() {
/*
            Load();
            if (s_saveGame != null ) {
                if (s_saveGame.Exists("HelloWorld")) {
                    string testString = (string)s_saveGame.LoadFrom("HelloWorld");
                    Debug.Log("[SaveGameManger/Start] testString: " + testString);
                }
                s_saveGame.SaveTo("HelloWorld", "Huh, did it work?");
            }
            Save();
*/
        }
        public static SaveGame GetSaveGameData() {
            if (s_saveGame == null) {
                Load();
            }
            return s_saveGame;
        }
        public static void Save() {
            Stream stream = null;
            string path = Path.Combine(Application.persistentDataPath, "testSave.data");
            stream = File.Open(path, FileMode.Create);

            BinaryFormatter bformatter = new BinaryFormatter();
            bformatter.Binder = new VersionDeserializationBinder();
            bformatter.TypeFormat = System.Runtime.Serialization.Formatters.FormatterTypeStyle.TypesWhenNeeded;

            try {
                bformatter.Serialize(stream, s_saveGame);
            }
            catch {
            }
            finally {
                if(stream != null) {
                    stream.Close();
                }
            }
        }
        public static void Load() {
            string path = Path.Combine(Application.persistentDataPath, "testSave.data");
            if (File.Exists(path)) {
                try {
                    Stream stream = File.Open(path, FileMode.Open);
                    BinaryFormatter bformatter = new BinaryFormatter();
                    bformatter.Binder = new VersionDeserializationBinder();
                    bformatter.TypeFormat = System.Runtime.Serialization.Formatters.FormatterTypeStyle.TypesWhenNeeded;

                    try {
                        s_saveGame = (SaveGame)bformatter.Deserialize(stream);
                    }
                    catch (SerializationException) {
                        //SaveGameManager.ClearAllSaves();
                        Debug.LogError("Yeah... that didn't work.");
                    }
                    finally {
                        stream.Close();
                    }
                }
                catch (Exception) {
                    //SaveGameManager.ClearAllSaves();
                }
            }
            else {
                if (s_saveGame != null) {
                    s_saveGame.Clear();
                }
                else {
                    Debug.Log("Create New SaveGame.");
                    s_saveGame = new SaveGame();
                }
            }
        }


        [Serializable()]
        public class SaveGame : ISerializable {
            // a mapping of potential save data that can be looked up by name.
            public Hashtable m_saveData = null;

            // test variables.
            public SaveGame() { m_saveData = new Hashtable(); }

            // Deserialize expects this function.
            public SaveGame(SerializationInfo info, StreamingContext ctxt) {
                /*
                // Don't Worry about this version business
                int loadedVersion = (int)(info.GetValue("saveVersion", typeof(int)) ?? 0);
                if (loadedVersion == VERSION) {
                    //Get the values from info and assign them to the appropriate properties
                    m_saveData = (Hashtable)info.GetValue("saveDictionary", typeof(Hashtable));
                }
                else {
                    SaveGameManager.ClearAllSaves();
                    m_saveData = new Hashtable();
                }
                */
                m_saveData = (Hashtable)info.GetValue("saveDictionary", typeof(Hashtable));
            }
            // DO NOT ACTUALLY USE THIS.
            // Serialization function.
            public void GetObjectData(SerializationInfo info, StreamingContext ctxt) {
                //info.AddValue("saveVersion", VERSION);
                info.AddValue("saveDictionary", m_saveData, typeof(Hashtable));
            }
            // This function is primarily used to check for entry
            // while assigning to primitives.
            public bool Exists(string id) { return (m_saveData != null) ? (m_saveData.ContainsKey(id)) : false; }
            /***
             * Saving into dictionary entries 
             */
            public void SaveTo(string id, object obj) {
                if (Exists(id)) {
                    m_saveData[id] = obj;
                }
                else {
                    m_saveData.Add(id, obj);
                }
            }

            /***
             * Loading from dictionary entries 
             */
            public object LoadFrom(string id) {
                return (Exists(id)) ? m_saveData[id] : null;
            }
            void Delete(string id) {
                if (Exists(id)) {
                    m_saveData.Remove(id);
                }
            }
            public void Clear() {
                m_saveData.Clear();
            }
        }
    }



    // This is required to guarantee a fixed serialization assembly name,
    // which Unity likes to randomize on each compile.
    // Do not change this
    public sealed class VersionDeserializationBinder : SerializationBinder {
        public override Type BindToType(string assemblyName, string typeName) {
            if (!string.IsNullOrEmpty(assemblyName) && !string.IsNullOrEmpty(typeName)) {
                Type typeToDeserialize = null;
                assemblyName = Assembly.GetExecutingAssembly().FullName;
                // The following line of code returns the type. 
                typeToDeserialize = Type.GetType(String.Format("{0}, {1}", typeName, assemblyName));
                return typeToDeserialize;
            }
            return null;
        }
    }
}