/***
 * author: Alfred Tarng 
 */

#if UNITY_EDITOR
using UnityEditor;

using UnityEngine;


using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions; // needed for Regex

namespace WF.AT {
    public class DataTable : EditorWindow {
	    public const string DATA_TABLE_PATH = "DataTables/";
	    public const string TEMP_TABLE = "DataTables/Temp";
	
	    private DataTableLookup.DataTables currentType = DataTableLookup.DataTables.UnitDefinitions;
	    public ExportData                 currentDataTable;
	    public Action<string>             selectedCallback = null;

	    List<KeyValuePair<int, ExportData.KeyValueString>> filteredResults = new List<KeyValuePair<int, ExportData.KeyValueString>>();

	    int  colIndex_M = 0;
	    int  colIndex = 0;

	    string  searchFilter = "";
        string  prevSearchFilter = "";
        string  sColumnFilter = "";
        int     columnFilter = -1, prevColumnFilter = -1;
	    Vector2 scrollPos;
        Vector2 scrollPos_2;

	    [MenuItem("Custom/Tables/Data Tables #&d")]
	    //Initializes
	    public static void Init()
	    {
		    DataTable idt = (DataTable)EditorWindow.GetWindow<DataTable>();
		    idt.selectedCallback = null;	
		    idt.Load();
	    }
	
	    //Loads the dictionary
	    public void Load() {
		    selectedCallback = null;
		    if(currentDataTable) {
			    EditorUtility.SetDirty(currentDataTable);
		    }
		    AssetDatabase.SaveAssets();

		    /////

		    currentDataTable = (ExportData)Resources.Load(DATA_TABLE_PATH + currentType);
		    if(currentDataTable == null) {
			    string tableName = ((DataTableLookup.DataTables)currentType).ToString("F");
			    currentDataTable = ScriptableObject.CreateInstance<ExportData>();
			    AssetDatabase.CreateAsset(currentDataTable, "Assets/Resources/" + DATA_TABLE_PATH + tableName + ".asset");
			    AssetDatabase.SaveAssets();
		    }
	    }
	
	    //Saves the dictionary
	    void OnDestroy()
	    {
		    selectedCallback = null;
		    if(currentDataTable)
		    {
			    EditorUtility.SetDirty(currentDataTable);
		    }
		    AssetDatabase.SaveAssets();
	    }

	    void FilterResults()
	    {
            scrollPos = Vector2.zero;
		    filteredResults.Clear();

		    ExportData.KeyValueString pair;
        
		    for(int i = 0; i < currentDataTable.dictionary.Count; ++i)
		    {
			    pair = currentDataTable.dictionary[i];
                if(columnFilter < 0 && (i == 0 || pair.key.ToLower().Contains(searchFilter)))
                {// there is no column filter, look through keys
				    filteredResults.Add(new KeyValuePair<int, ExportData.KeyValueString>(i, pair));
			    }
                else
                {
                    if(columnFilter >= 0 && columnFilter < pair.val.Count)
                    {// there is a column filter, filter only for specific column.
                        if(i == 0 || pair.val[columnFilter].ToLower().Contains(searchFilter) ||
                           (!string.IsNullOrEmpty(pair.val[columnFilter].ToLower()) && searchFilter.Equals("*")))
                        {
                            filteredResults.Add(new KeyValuePair<int, ExportData.KeyValueString>(i, pair));
                        }
                    }
                    else
                    {// there is no column filter, look through all columns
                        for(int j = 0; j < pair.val.Count; ++j)
                        {
                            if(pair.val[j].ToLower().Contains(searchFilter))
                            {
                                filteredResults.Add(new KeyValuePair<int, ExportData.KeyValueString>(i, pair));
                                break;
                            }
                        }
                    }
                }
		    }
	    }

	    void DrawFilteredResults()
	    {
		    scrollPos = GUILayout.BeginScrollView(scrollPos);
		    for(int i = 0; i < filteredResults.Count; i++)
		    {
			    GUILayout.BeginHorizontal();
			    {
				    if(selectedCallback != null)
				    {
					    if(GUILayout.Button ("<<"))
					    {
						    selectedCallback(filteredResults[i].Value.key);
						    selectedCallback = null;
						    this.Close ();
					    }
				    }

				    filteredResults[i].Value.key = EditorGUILayout.TextField(filteredResults[i].Value.key, GUILayout.Width(100));
				    GUILayout.Space (8);

				    List<String> filteredListVals = filteredResults[i].Value.val;
				    for(int j = 0; j < filteredListVals.Count; ++j)
				    {
					    filteredListVals[j] = EditorGUILayout.TextField( filteredListVals[j], GUILayout.MaxWidth(300));
					    GUILayout.Space(2);
				    }
                    /*
				    // Stretch to fill.
				    filteredListVals[filteredListVals.Count-1] = EditorGUILayout.TextField( filteredListVals[filteredListVals.Count-1], 
	       			    GUILayout.MinWidth(GUI.skin.textField.CalcSize(new GUIContent(filteredListVals[filteredListVals.Count-1])).x) );
                    */         
			    }
			    GUILayout.EndHorizontal();
		    }
		    GUILayout.EndScrollView();
	    }
	
	    //Table GUI
	    void OnGUI()
	    {
		    GUILayout.BeginVertical();
		    DataTableLookup.DataTables itemType = currentType;
            GUILayout.Label(string.Format("DataTable:"), GUILayout.MaxWidth(100f)); // {0}", lang));
		    GUILayout.BeginHorizontal();
            {
                currentType = (DataTableLookup.DataTables)EditorGUILayout.EnumPopup(currentType);
                if (itemType != currentType)
                {
                    Load();
                    searchFilter = "";
                }

                if (GUILayout.Button("Save", GUILayout.Width(240f)))
                {
                    if (currentDataTable != null)
                    {
                        EditorUtility.SetDirty(currentDataTable);
                        AssetDatabase.SaveAssets();
                    }
                }
            }GUILayout.EndHorizontal ();

		    GUILayout.Space(8);
		    GUILayout.BeginHorizontal();
		    {
			    //
			    GUILayout.Label(string.Format("Search Filter:"), GUILayout.Width(90));
			    searchFilter = EditorGUILayout.TextField(searchFilter, GUILayout.Width(300)).ToLower();

                GUILayout.Label(string.Format("Column: "), GUILayout.Width(90));
                sColumnFilter = EditorGUILayout.TextField(sColumnFilter, GUILayout.Width(300)).ToLower();
                sColumnFilter = Regex.Replace(sColumnFilter, @"[^0-9]", "");
                columnFilter = (string.IsNullOrEmpty(sColumnFilter)) ? -1 : int.Parse(sColumnFilter);
		    }
		    GUILayout.EndHorizontal();

            if(GUI.changed && (searchFilter != prevSearchFilter || columnFilter != prevColumnFilter))
		    {
			    prevSearchFilter = searchFilter;
                prevColumnFilter = columnFilter;
			    FilterResults();
		    }

		    if(searchFilter != "")
		    {
			    DrawFilteredResults();
			    GUILayout.EndVertical();
			    return;
		    }

		    GUI.contentColor = Color.yellow;
            scrollPos_2 = GUILayout.BeginScrollView(scrollPos_2);
            {
                float width =  200;
                if( currentDataTable.dictionary.Count > 0 )
                {
        		    Rect headerRect = EditorGUILayout.BeginHorizontal ();
        		    {
        			    int headerRow = 0;
        			    if(selectedCallback != null)
        			    {
        				    GUI.backgroundColor = Color.cyan;
        				    if(GUILayout.Button ("<<", GUILayout.Width(48)))
        				    {
        					    selectedCallback(currentDataTable.dictionary[headerRow].key);
        					    selectedCallback = null;
        					    this.Close ();
        				    }
        				    GUI.backgroundColor = Color.white;
        			    }
        			
        			    currentDataTable.dictionary[headerRow].key = EditorGUILayout.TextField(currentDataTable.dictionary[headerRow].key, GUILayout.Width(100));
        			    GUILayout.Space(8);
        			
        			    List<string> valueList = currentDataTable.dictionary[headerRow].val;
        			    for(int column = 0; column < valueList.Count; ++column)
        			    {
                            valueList[column] = EditorGUILayout.TextField( valueList[column], GUILayout.MinWidth(80), GUILayout.MaxWidth(120));
        				    GUILayout.Space(2);
        			    }
                        GUILayout.Space(45);
        		    }
        		    EditorGUILayout.EndHorizontal ();
        		    GUI.contentColor = Color.white;
                    width = headerRect.width;
                }
                
    		    EditorHelper.DrawLine ();

    		    scrollPos = GUILayout.BeginScrollView(scrollPos, false, true);
    		    {
    			    if(currentDataTable != null)
    			    {
    				    for(int row = 1; row < currentDataTable.dictionary.Count; row++)
    				    {
    					    GUILayout.BeginHorizontal(GUILayout.MinWidth(width));

    					    if(selectedCallback != null)
    					    {
    						    GUI.backgroundColor = Color.cyan;
    						    if(GUILayout.Button ("<<", GUILayout.Width(48)))
    						    {
    							    selectedCallback(currentDataTable.dictionary[row].key);
    							    selectedCallback = null;
    							    this.Close ();
    						    }
    						    GUI.backgroundColor = Color.white;
    					    }

    					    currentDataTable.dictionary[row].key = EditorGUILayout.TextField(currentDataTable.dictionary[row].key, GUILayout.Width(100));
    					    GUILayout.Space(8);

    					    List<string> valueList = currentDataTable.dictionary[row].val;
    					    for(int column = 0; column < valueList.Count; ++column)
    					    {
                                valueList[column] = EditorGUILayout.TextField( valueList[column], GUILayout.MinWidth(80), GUILayout.MaxWidth(120));
    						    GUILayout.Space(2);
    					    }

    	                    if(row > 0)
    	                    {
    	                        if( GUILayout.Button ("x", GUILayout.MaxWidth(30)) )
    	    				    {
    	    					    RemoveRow(row);
    	    				    }
    	                    }
    					    GUILayout.EndHorizontal();
    				    }
    			    }
    		    } GUILayout.EndScrollView();
            } GUILayout.EndScrollView();
            
		    GUILayout.BeginHorizontal();
		    {
			    if(GUILayout.Button ("Add Row"))
			    {
				    if(currentDataTable != null)
				    {
					    ExportData.KeyValueString newRow= new ExportData.KeyValueString("$key", "val");
					    newRow.MatchColumns(currentDataTable.currentColumns);
					    currentDataTable.dictionary.Add(newRow);
				    }
			    }
			    if(GUILayout.Button ("Add Column"))
			    {
				    if(currentDataTable != null)
					    currentDataTable.AddColumn(); // (new ExportData.KeyValueString("$key", "val"));
			    }
		    }
		    GUILayout.EndHorizontal();
                
            EditorGUILayout.BeginHorizontal ();
		    {
    //			string text = EditorGUILayout.TextField(""+rowIndex, GUILayout.Width(50));
    //			text = Regex.Replace(text, @"0-9]", "");
    //			int.TryParse(text, out rowIndex);
    //			if(GUILayout.Button ("Remove Row"))
    //			{
    //				if(currentDataTable != null)
    //				{
    //					RemoveRow(rowIndex);
    //				}
    //			}
                string text = EditorGUILayout.TextField(""+colIndex_M, GUILayout.Width(50));
                text = Regex.Replace(text, @"0-9]", "");
                int.TryParse(text, out colIndex_M);
                if(GUILayout.Button("Move Column Left", GUILayout.Width(250)))
                {
                    if(currentDataTable != null)
                    {
                        currentDataTable.MoveUpColumn(colIndex_M);
                    }
                }

			    text = EditorGUILayout.TextField(""+colIndex, GUILayout.Width(50));
			    text = Regex.Replace(text, @"0-9]", "");
			    int.TryParse(text, out colIndex);
			    if(GUILayout.Button ("Remove Column"))
			    {
				    if(currentDataTable != null)
				    {
					    currentDataTable.RemoveColumn(colIndex);
				    }
			    }
		    }
            EditorGUILayout.EndHorizontal();
            /*
		    GUILayout.BeginHorizontal();
		    {
                GUILayout.Label("Move Row Up By(^): ", GUILayout.Width(125));
			    string text = EditorGUILayout.TextField(""+rowIndex_1, GUILayout.Width(50));
			    text = Regex.Replace(text, @"0-9]", "");
			    int.TryParse(text, out rowIndex_1);

                if(windowWidth.width > 0)
                {
                    fillerSpace = Mathf.Max((int)(windowWidth.width/2 - 190), 0);
                }
                GUILayout.Space(fillerSpace);

                text = EditorGUILayout.TextField(""+colIndex_1, GUILayout.Width(50));
                text = Regex.Replace(text, @"0-9]", "");
                int.TryParse(text, out colIndex_1);
			    if(GUILayout.Button("Move Column Left", GUILayout.Width(250)))
			    {
				    if(currentDataTable != null)
				    {
					    currentDataTable.MoveUpColumn(colIndex_1);
				    }
			    }
		    }
		    GUILayout.EndHorizontal();
            */
		    GUILayout.BeginHorizontal();
            {
    		    if(GUILayout.Button ("Export"))
    		    {
    			    if(currentDataTable != null) currentDataTable.Export ();
    		    }
                if(GUILayout.Button ("SORT"))
                {
                    if(currentDataTable != null) currentDataTable.Sort();
                }
            } GUILayout.EndHorizontal();

		    GUILayout.EndVertical();
	    }

	    void RemoveRow(int index) {
		    if( index < currentDataTable.dictionary.Count)
		    {
			    currentDataTable.dictionary.RemoveAt(index);
		    }
	    }
    }
}
#endif