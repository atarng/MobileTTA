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
    public Grid GetGrid() {
        return m_gridInstance;
    }

    Queue<Player> m_turnQueue = new Queue<Player>();
    Dictionary<int, Player> m_idPlayerMap = new Dictionary<int, Player>();

    // TEMPPPP
    public Unit m_unitPrefab;
    public int[] m_testDeckList;

    private List<UnitManager.UnitDefinition> to_insert = new List<UnitManager.UnitDefinition>();
    private void Start() {
        for (int i = 0; i < m_testDeckList.Length; i++) {
            UnitManager um = SingletonMB.GetInstance<UnitManager>();
            UnitManager.UnitDefinition ud = um.GetDefinition(m_testDeckList[i]);
            if (ud != null) {
                to_insert.Add(ud);
            }
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

        m_turnQueue.Peek().Reset();
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

        m_turnQueue.Peek().Reset();
    }

    public void HandleCombat(IPlaceable combatant1, IPlaceable combatant2) {
        IUnit iu1 = combatant1 as IUnit;
        IUnit iu2 = combatant2 as IUnit;
        int iu1p_damageToDo = 0;
        int iu1s_damageToDo = 0;
        int iu2p_damageToDo = 0;
        int iu2s_damageToDo = 0;
        if (iu1 != null) {
            iu1p_damageToDo = iu1.IsPhysicalAttack()  ? iu1.GetAttackValue() : 0;
            iu1s_damageToDo = iu1.IsSpiritualAttack() ? iu1.GetAttackValue() : 0;
        }
        if (iu2 != null) {
            iu2p_damageToDo = iu2.IsPhysicalAttack()  ? iu2.GetAttackValue() : 0;
            iu2s_damageToDo = iu2.IsSpiritualAttack() ? iu2.GetAttackValue() : 0;
        }
        combatant1.TakeDamage(iu2p_damageToDo, 0);
        combatant1.TakeDamage(iu2s_damageToDo, 1);

        combatant2.TakeDamage(iu1p_damageToDo, 0);
        combatant2.TakeDamage(iu1s_damageToDo, 1);

    }
    ///*
    //*/
}