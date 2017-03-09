using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepositionToUICamera : MonoBehaviour {

    public Transform m_Source;
    public Transform m_Target;

    public Vector3 m_offset;

    private void Awake() {
        if (gameObject && m_Target && CameraManager.Instance) {
            m_Target.SetParent(CameraManager.Instance.GetUICameraTransform());
            OnAwake();
        }
    }

    private void OnDestroy() {
        if (m_Target) Destroy(m_Target.gameObject);
        if (m_Source) Destroy(m_Source.gameObject);
    }

    // Update is called once per frame
    void Update() {
        if (m_Source) {
            Vector3 ToPosition = CameraManager.Instance.FromGameToUIVector(m_Source.position);
            m_Target.position = ToPosition + m_offset;

            m_Target.localRotation = m_Source.localRotation;

            OnUpdate();
        }
        else {
            Destroy(gameObject);
        }
    }
    protected virtual void OnAwake() { }
    protected virtual void OnUpdate() { }
}
