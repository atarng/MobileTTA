using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AtRng.MobileTTA;

public class GameManager : SingletonMB {

    [SerializeField]
    //private int m_number_of_players;
    private List<Transform> playerLocations;

    [SerializeField]
    private Player m_playerPrefab;

    [SerializeField]
    private Grid m_gridInstance;

    Queue<Player> m_turnQueue = new Queue<Player>();
    Dictionary<int, Player> m_idPlayerMap = new Dictionary<int, Player>();

    public Unit[] m_testList;
    private List<IUnit> to_insert = new List<IUnit>();
    private void Start() {
        for (int i = 0; i < m_testList.Length; i++) {
            to_insert.Add(m_testList[i]);
        }
        InitializePlayers();
    }

    public void InitializePlayers() {
        for (int i = 0; i < playerLocations.Count; i++) { //m_number_of_players;
            Player p = GameObject.Instantiate<Player>(m_playerPrefab);

            p.transform.position = CameraManager.Instance.FromUIToGameVector( playerLocations[i].position );//new Vector3(i, 0.5f, 10) );
            Vector3 pPos = p.transform.position;
            pPos.z = 0;
            p.transform.position = pPos;
            p.PopulateDeck(to_insert);
            for (int j = 0; j < 3; j++) {
                p.Draw();
            }

            m_idPlayerMap.Add(i, p);
            m_turnQueue.Enqueue(p);
        }
    }
    public IGamePlayer GetPlayer(int id) {
        return m_idPlayerMap[id];
    }

    public Player CurrentPlayer() {
        return m_turnQueue.Peek();
    }

    public void UpdateTurn() {
        Player p = m_turnQueue.Dequeue();
        m_turnQueue.Enqueue(p);
    }
    ///*
    private void Update() {
        if (Input.GetMouseButtonDown(0)) {
            //Debug.Log("[GameManager] MouseDown");

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) {
                Debug.Log("Name = " + hit.collider.name);
                Debug.Log("Tag = " + hit.collider.tag);
                Debug.Log("Hit Point = " + hit.point);
                Debug.Log("Object position = " + hit.collider.gameObject.transform.position);
                Debug.Log("--------------");
            }
        }
    }
    //*/
}