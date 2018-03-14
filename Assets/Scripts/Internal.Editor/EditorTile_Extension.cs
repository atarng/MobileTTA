using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using AtRng.MobileTTA;

#if UNITY_EDITOR
public class EditorTile_Extension : MonoBehaviour {
    public InputField inputField;
    private TileTraversalEnum m_tte = TileTraversalEnum.All;
    private Tile m_selectedTile = null;
    private bool m_painting = false;

    [SerializeField]
    private Image m_spriteRenderer;
    [SerializeField]
    private Sprite[] m_spriteArray;

    private void Start() {
        inputField.text = ((int)m_tte).ToString();
        inputField.onEndEdit.AddListener(delegate {
            int test = 0;
            int.TryParse(inputField.text, out test);
            m_tte = (TileTraversalEnum)test;
            //m_spriteRenderer.color = Color.white;

            switch (m_tte) {
                case TileTraversalEnum.None:
                    // IMPASSABLE
                    m_spriteRenderer.sprite = m_spriteArray[3];
                    break;
                case TileTraversalEnum.CanFly:
                    // Lake
                    m_spriteRenderer.sprite = m_spriteArray[1];
                    break;
                case TileTraversalEnum.FlyAndClimb:
                    // Mountain
                    m_spriteRenderer.sprite = m_spriteArray[2];
                    break;
                case TileTraversalEnum.All:
                default:
                    m_spriteRenderer.sprite = m_spriteArray[0];
                    break;
            }
        });
    }

    /*** For Painting Mode ***/
    private void Update() {
        if (m_painting) {
            if (Input.GetMouseButtonDown(0)) {
                Vector3 v3 = CameraManager.Instance.GameCamera().ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D rh2d = Physics2D.Raycast(new Vector2(v3.x, v3.y), Vector2.zero, 0f, 1 << LayerMask.NameToLayer("Grid"));
                if (rh2d) {
                    m_selectedTile = rh2d.transform.GetComponent<Tile>();
                }
            }
            else if (Input.GetMouseButtonUp(0) && m_selectedTile != null) {
                Vector3 v3 = CameraManager.Instance.GameCamera().ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D rh2d = Physics2D.Raycast(new Vector2(v3.x, v3.y), Vector2.zero, 0f, 1 << LayerMask.NameToLayer("Grid"));
                if (rh2d && rh2d.transform.GetComponent<Tile>() == m_selectedTile) {
                    //m_selectedTile.SetTileTraversal(m_tte);
                    SetTileAndUpdateEditor(m_selectedTile, m_tte);
                }
                else {
                    m_selectedTile = null;
                    m_painting = false;
                }
            }
        }
    }

    private void OnMouseDown() {}
    private void OnMouseUp() {
        Vector3 v3 = CameraManager.Instance.GameCamera().ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D rh2d = Physics2D.Raycast(new Vector2(v3.x, v3.y), Vector2.zero, 0f, 1 << LayerMask.NameToLayer("Grid"));
        if (rh2d ) {//&&  == m_selectedTile) {
            Tile t = rh2d.transform.GetComponent<Tile>();
            if(t != null) {
                SetTileAndUpdateEditor(t, m_tte);
                m_painting = true;
            }
        }
        else {
            //m_selectedTile = null;
            m_painting = false;
        }
    }

    private void SetTileAndUpdateEditor( Tile t, TileTraversalEnum tte ) {
        t.SetTileTraversal(tte);

        EditorManager em = ((EditorManager)SceneControl.GetCurrentSceneControl());
        if (em != null) {
            switch (tte) {
                case TileTraversalEnum.All:
                    em.LevelEditorGrid.RemoveTileInfo(t.xPos, t.yPos);
                    break;
                default:
                    em.LevelEditorGrid.RemoveTileInfo(t.xPos, t.yPos);
                    em.LevelEditorGrid.AddToPlaceablesList((int)tte, t.xPos, t.yPos, PlaceableType.Tile);
                    break;
            }
        }
    }

}
#endif