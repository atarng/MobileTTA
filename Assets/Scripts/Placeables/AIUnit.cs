using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using AtRng.MobileTTA;
using System;

// probably should not make this extend RepositionToUICamera as a class, but just require this as a monobehavior.
/* Interfaces IPlaceable, */
//
public class AIUnit : GameUnit {
    public void DetermineTargetTiles(out Tile toMoveTo, out Tile toInteractWith) {
        toMoveTo = null;
        toInteractWith = null;

        Grid g = SingletonMB.GetInstance<GameManager>().GetGrid();
        Dictionary<Tile, List<Vector2>> m_tileMapToPath = new Dictionary<Tile, List<Vector2>>();
        Queue<Tile> tilesToExplore = new Queue<Tile>();

        // For if it stays in place.
        m_tileMapToPath.Add(AssignedToTile, new List<Vector2>());
        m_tileMapToPath[AssignedToTile].Add(new Vector2(AssignedToTile.xPos, AssignedToTile.yPos));

        // Initialized Queue.
        List<Tile> initTiles = g.GetCircumference(AssignedToTile, 1);
        for (int i = 0; i < initTiles.Count; i++) {
            BaseUnit bu = initTiles[i].GetPlaceable() as BaseUnit;
            if (bu != null && bu.GetPlayerOwner() != GetPlayerOwner()) {
                // we've found an enemy tile to interact with.
                toInteractWith = bu.AssignedToTile;
                break;
            }

            tilesToExplore.Enqueue(initTiles[i]);

            List<Vector2> v2l = new List<Vector2>();
            v2l.Add(new Vector2(initTiles[i].xPos, initTiles[i].yPos));

            m_tileMapToPath.Add(initTiles[i], v2l);
        }

        // Breath First Search for a target.
        while (toInteractWith == null && tilesToExplore.Count > 0) {
            Tile t = tilesToExplore.Dequeue();

            if (t.GetPlaceable() is Impassable) {
                continue;
            }

            List<Tile> candidateTiles = g.GetCircumference(t, 1);

            for (int i = 0; i < candidateTiles.Count; i++) {

                BaseUnit bu = candidateTiles[i].GetPlaceable() as BaseUnit;
                if (bu != null && bu.GetPlayerOwner() != GetPlayerOwner()) {
                    // we've found an enemy tile to interact with.
                    toInteractWith = bu.AssignedToTile;
                    break;
                }
                else {
                    // check to see if we've already visited this tile before.
                    if (!m_tileMapToPath.ContainsKey(candidateTiles[i])) {
                        tilesToExplore.Enqueue(candidateTiles[i]);
                        List<Vector2> v2l = new List<Vector2>();
                        if (m_tileMapToPath.ContainsKey(t)) {

                            // Add To Map with Existing Path + This Location.
                            v2l.AddRange(m_tileMapToPath[t]);
                            v2l.Add(new Vector2(candidateTiles[i].xPos, candidateTiles[i].yPos));

                            m_tileMapToPath.Add(candidateTiles[i], v2l);

                        }
                        else {
                            Debug.LogError("[AIUnit] This should be impossible");
                        }
                    }
                }
                //if we visited it already, don't do anything.
            }
        }

        if (toInteractWith != null) {

            List<Tile> candidatesToMoveTo = g.GetCircumference(toInteractWith, GetAttackRange());
            int shortestPathCount = -1;
            int index_of_shortest_path = -1;
            for (int i = 0; i < candidatesToMoveTo.Count; i++) {

                if (m_tileMapToPath.ContainsKey(candidatesToMoveTo[i])) {
                    if (candidatesToMoveTo[i].GetPlaceable() == null || candidatesToMoveTo[i].GetPlaceable().Equals(this) ) {
                        List<Vector2> path = m_tileMapToPath[candidatesToMoveTo[i]];
                        if (shortestPathCount < 0 || shortestPathCount > path.Count) {

                            shortestPathCount = path.Count;
                            index_of_shortest_path = i;

                        }
                    }
                }

            }

            if (index_of_shortest_path >= 0) {
                if (m_tileMapToPath.ContainsKey(candidatesToMoveTo[index_of_shortest_path])) {
                    List<Vector2> path = m_tileMapToPath[candidatesToMoveTo[index_of_shortest_path]];
                    for (int i = GetMaxMovement() - 1; i >= 0; i--) {
                        if (path.Count > i) {
                            Vector2 v2 = path[i];
                            Tile mt = g.GetTileAt((int)v2.y, (int)v2.x);
                            if (mt != null && mt.GetPlaceable() == null || mt.GetPlaceable().Equals(this) ) {
                                toMoveTo = mt;
                                return;
                            }
                        }
                    }
                }
                else {
                    Debug.LogError("[AIUnit] HOW????");
                }
            }

            if (toMoveTo == null) {
                string s = "";
                if (index_of_shortest_path >= 0) { 
                    List<Vector2> path = m_tileMapToPath[candidatesToMoveTo[index_of_shortest_path]];
                    path.ForEach((item) => { s += string.Format("({0}, {1})", item.x, item.y); });
                }
                Debug.LogWarning(string.Format("Path of unpathable: {0}, index_ofSP: {1}", s, index_of_shortest_path));
            }

        }
        else {
            Debug.LogError("[AIUnit] Probably Should have won by now.");
        }
    }
}