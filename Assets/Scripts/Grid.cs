using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour {

    [SerializeField]
    int m_width = 0, m_height = 0;

    [SerializeField]
    Tile m_tilePrefab;
    List<Tile> m_grid;


    // Use this for initialization
    void Start () {

        float x_offset = (m_width  / 2f) - .5f;
        float y_offset = (m_height / 2f) - .5f;

        m_grid = new List<Tile>();
        for (int i = 0; i < m_width; i++) {
            for (int j = 0; j < m_height; j++) {
                Tile go = GameObject.Instantiate<Tile>( m_tilePrefab );
                go.gameObject.name = "Tile_" + i + "_" + j;

                Transform tTransform = go.transform;
                tTransform.SetParent(transform);
                tTransform.Translate(i - x_offset, j - y_offset, 0);
                m_grid.Add(go);
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
