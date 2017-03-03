using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour {
    [SerializeField]
    private Camera m_uiCamera = null;
    [SerializeField]
    private Camera m_gameCamera = null;

    public static CameraManager Instance;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        }
        else {
            Destroy(gameObject);
        }
    }

    public Vector3 FromGameToUIVector( Vector3 source ) {
        Vector3 screenPoint = m_gameCamera.WorldToScreenPoint(source);
        Vector3 toReturn = m_uiCamera.ScreenToWorldPoint(screenPoint);
        return toReturn;
    }

    public Vector3 FromUIToGameVector(Vector3 source) {
        Vector3 screenPoint = m_uiCamera.WorldToScreenPoint(source);
        Vector3 toReturn    = m_gameCamera.ScreenToWorldPoint(screenPoint);
        return toReturn;
    }

    public Transform GetUICameraTransform() {
        return m_uiCamera.transform;
    }
}