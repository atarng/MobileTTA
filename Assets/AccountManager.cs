using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AtRng.MobileTTA;
using WF.AT;

public class AccountManager : SingletonMB {
    [Serializable]
    class CurrencyData {
        public int Coins;
        public int Salvage;
        public CurrencyData() {
            Coins = 0;
            Salvage = 0;
        }
    }

    CurrencyData m_ourCurrency;
    protected override void OnAwake() {
        LoadData();
    }
    private void LoadData() {
        m_ourCurrency = SaveGameManager.GetSaveGameData().LoadFrom("CurrencyData") as CurrencyData;
        if (m_ourCurrency == null) {
            m_ourCurrency = new CurrencyData();
        }
    }
    private void SaveData() {
        SaveGameManager.GetSaveGameData().SaveTo("CurrencyData", m_ourCurrency);
        SaveGameManager.Save();
    }

    public int GetCoins() {
        return m_ourCurrency.Coins;
    }
    public int GetSalvage() {
        return m_ourCurrency.Salvage;
    }

    public void ModifyCoins(int coins) {
        m_ourCurrency.Coins += coins;
        SaveData();
    }
    public void ModifySalvage(int salvage) {
        m_ourCurrency.Salvage += salvage;
        SaveData();
    }
}