using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using AtRng.MobileTTA;


// this might have some conflicts with GameManager...

public class MapInit : MonoBehaviour {
    [Serializable]
    struct IntVector2 {
        public int x;
        public int y;
        public int Player;
    }

    [SerializeField]
    Grid m_gridToInit; //

    [SerializeField]
    Unit m_testUnit;

    [SerializeField]
    List<IntVector2> m_tilesToInitUnits;

    // Use this for initialization
    void Start () {
        m_gridToInit.InitializeGrid();
        for (int i = 0; i < m_tilesToInitUnits.Count; i++) {

            Unit unit_to_place_on_tile = GameObject.Instantiate<Unit>(m_testUnit);
            IUnit iUnit = unit_to_place_on_tile;
            IGamePlayer p = GameManager.GetInstance<GameManager>().GetPlayer(m_tilesToInitUnits[i].Player);
            iUnit.AssignPlayerOwner(p);

            Tile tileAtXY = m_gridToInit.GetTileAt( (m_tilesToInitUnits[i].y), (m_tilesToInitUnits[i].x));

            tileAtXY.SetPlaceable(unit_to_place_on_tile);
        }
    }
	
}
