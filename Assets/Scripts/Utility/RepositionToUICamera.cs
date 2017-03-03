﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepositionToUICamera : MonoBehaviour {

    public GameObject m_Source;
    public GameObject m_Target;
    private void Awake() {
        m_Target.transform.SetParent(CameraManager.Instance.GetUICameraTransform());
        OnAwake();
    }
    // Update is called once per frame
    void Update() {
        if (m_Source) {
            Vector3 ToPosition = CameraManager.Instance.FromGameToUIVector(m_Source.transform.position);
            m_Target.transform.position = ToPosition;
            OnUpdate();
        }
        else {
            Destroy(gameObject);
        }
    }
    protected virtual void OnAwake() {
    }
    protected virtual void OnUpdate() {
    }
}