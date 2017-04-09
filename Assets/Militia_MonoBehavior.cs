using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AtRng.MobileTTA;

public class Militia_MonoBehavior : MonoBehaviour {
    [SerializeField]
    private Unit unit_prefab;

    const int MILITIA_ID = 10;

    Unit pendingMilitia;

    private bool IsAdjacentToAlliedUnit(Tile t) {
        List<Tile> lt = GameManager.GetInstance<GameManager>().GetGrid().GetCircumference(t, 1);
        for (int i = 0; i < lt.Count; i++) {
            IPlaceable ip = lt[i].GetPlaceable();
            if (ip != null) {
                Unit u = ip as Unit;
                if (u != null && GameManager.GetInstance<GameManager>().CurrentPlayer().Equals(u.GetPlayerOwner()) ) {
                    return true;
                }
            }
        }
        return false;
    }
    private void OnMouseUp() {
        if (pendingMilitia != null) {
            RaycastHit2D rh2d = Physics2D.Raycast(new Vector2(CameraManager.Instance.GameCamera().ScreenToWorldPoint(Input.mousePosition).x,
                                          CameraManager.Instance.GameCamera().ScreenToWorldPoint(Input.mousePosition).y),
                                          Vector2.zero, 0f, 1 << LayerMask.NameToLayer("Grid"));

            Tile CurrentlyOverTile = null;
            if (rh2d) {
                CurrentlyOverTile = rh2d.transform.GetComponent<Tile>();
            }
            bool unit_placed = false;
            if (CurrentlyOverTile != null) {
                // Tile is not null
                if (IsAdjacentToAlliedUnit(CurrentlyOverTile) && !CurrentlyOverTile.IsOccupied()) {
                    BasePlayer igp = GameManager.GetInstance<GameManager>().CurrentPlayer();
                    pendingMilitia.AssignedToTile = CurrentlyOverTile;
                    pendingMilitia.AssignPlayerOwner(igp.ID);

                    (igp as IGamePlayer).ExpendUnitActionPoint();
                    (igp as IGamePlayer).ExpendUnitActionPoint();

                    if (pendingMilitia.GetPlayerOwner() != null) {
                        pendingMilitia.GetPlayerOwner().PlaceUnitOnField(pendingMilitia);
                    }
                    unit_placed = true;
                }
            }

            if (unit_placed) {
                Debug.Log("[Militia_MB/OnMouseUp] Successfully Placed");
                pendingMilitia.AttemptRelease(unit_placed);
            }
            else {
                Debug.Log("[Militia_MB/OnMouseUp] Failed To Place Unit, Destroy");
                Destroy(pendingMilitia.gameObject);
            }

            SingletonMB.GetInstance<GameManager>().GetGrid().ClearPathableTiles();
        }
    }

    private void OnMouseDown() {
        Debug.Log("[Militia_MB/OnMouseDown] GenerateMilitia");

        if (GameManager.GetInstance<GameManager>().CurrentPlayer().GetEnoughActionPoints(3)){
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
    }
}
