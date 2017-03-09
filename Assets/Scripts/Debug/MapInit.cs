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
        public int unitType;
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

            UnitManager.UnitDefinition ud = UnitManager.GetInstance<UnitManager>().GetDefinition(m_tilesToInitUnits[i].unitType);
            unit_to_place_on_tile.ReadDefinition(ud);

            IUnit iUnit = unit_to_place_on_tile;
            IGamePlayer p = GameManager.GetInstance<GameManager>().GetPlayer(m_tilesToInitUnits[i].Player);
            iUnit.AssignPlayerOwner(p);
            p.GetCurrentSummonedUnits().Add(iUnit);

            Tile tileAtXY = m_gridToInit.GetTileAt( (m_tilesToInitUnits[i].x), (m_tilesToInitUnits[i].y));

            tileAtXY.SetPlaceable(unit_to_place_on_tile);
        }
    }
	
}
