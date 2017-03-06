using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AtRng.MobileTTA;

public class GameManager : SingletonMB {

    [SerializeField]
    private int m_number_of_players;

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
        for (int i = 0; i < m_number_of_players; i++) {
            Player p = GameObject.Instantiate<Player>(m_playerPrefab);

            p.PopulateDeck(to_insert);
            p.Draw();

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
}