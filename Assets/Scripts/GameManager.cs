using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AtRng.MobileTTA;

public class GameManager : MonoBehaviour {
    [SerializeField]
    private int m_number_of_players;

    [SerializeField]
    private Player m_playerPrefab;

    [SerializeField]
    private Grid m_gridInstance;

    Queue<Player> m_turnQueue;


    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
}