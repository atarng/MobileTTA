using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AtRng.MobileTTA;

public class Militia_MonoBehavior : MonoBehaviour {
    [SerializeField]
    private GameUnit unit_prefab;

    const int MILITIA_ID = 10;

    GameUnit pendingMilitia;

    private bool IsAdjacentToAlliedUnit(Tile t) {
        List<Tile> lt = GameManager.GetInstance<GameManager>().GetGrid().GetCircumference(t, 1);
        for (int i = 0; i < lt.Count; i++) {
            IPlaceable ip = lt[i].GetPlaceable();
            if (ip != null) {
                GameUnit u = ip as GameUnit;
                if (u != null && GameManager.GetInstance<GameManager>().CurrentPlayer().Equals(u.GetPlayerOwner()) ) {
                    return true;
                }
            }
        }
        return false;
    }
    private void OnMouseUp() {
        if (pendingMilitia != null) {
            bool unit_placed = false;
            Vector3 v3 = CameraManager.Instance.GameCamera().ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D rh2d = Physics2D.Raycast(new Vector2(v3.x, v3.y), Vector2.zero, 0f, 1 << LayerMask.NameToLayer("Grid"));

            Tile CurrentlyOverTile = null;
            if (rh2d) {
                CurrentlyOverTile = rh2d.transform.GetComponent<Tile>();
            }
            if (CurrentlyOverTile != null) {
                // Tile is not null
                if (IsAdjacentToAlliedUnit(CurrentlyOverTile) && !CurrentlyOverTile.IsOccupied()) {
                    BasePlayer igp = GameManager.GetInstance<GameManager>().CurrentPlayer();
                    pendingMilitia.AssignedToTile = CurrentlyOverTile;
                    pendingMilitia.AssignPlayerOwner(igp.ID);

                    (igp as IGamePlayer).UpdatePlayerHealth(igp.Health - 10);

                    if (pendingMilitia.GetPlayerOwner() != null) {
                        pendingMilitia.GetPlayerOwner().PlaceUnitOnField(pendingMilitia);
                    }
                    unit_placed = true;
                }
            }

            if (unit_placed) {
                //Debug.Log("[Militia_MB/OnMouseUp] Successfully Placed");
                pendingMilitia.AttemptRelease(unit_placed);
            }
            else {
                //Debug.Log("[Militia_MB/OnMouseUp] Failed To Place Unit, Destroy");
                Destroy(pendingMilitia.gameObject);
            }

            pendingMilitia = null;
            SingletonMB.GetInstance<GameManager>().GetGrid().ClearPathableTiles();
        }
    }

    private void OnMouseDown() {
        //Debug.Log("[Militia_MB/OnMouseDown] GenerateMilitia");
        BasePlayer bp = GameManager.GetInstance<GameManager>().CurrentPlayer();
        if (bp.Health <= 10) {
            SceneControl.GetCurrentSceneControl().DisplayWarning("Not Enough Wealth to Conscript Militia.");
        }
        else if (bp.GetEnoughActionPoints(3)) {
            GenerateMilitia();
            BasePlayer cp = GameManager.GetInstance<GameManager>().CurrentPlayer();
            SingletonMB.GetInstance<GameManager>().GetGrid().DisplaySummonableTiles(cp);
        }
    }

    public void GenerateMilitia() {
        UnitManager.UnitDefinition ud = UnitManager.GetInstance<UnitManager>().GetDefinition(MILITIA_ID);
        pendingMilitia = GameObject.Instantiate(SingletonMB.GetInstance<GameManager>().m_unitPrefab);
        pendingMilitia.ReadDefinition(ud);
        pendingMilitia.AttemptSelection();
        
        pendingMilitia.transform.SetParent(transform);
        pendingMilitia.transform.localScale = Vector3.one;
        pendingMilitia.transform.localRotation = Quaternion.identity;
    }
}
