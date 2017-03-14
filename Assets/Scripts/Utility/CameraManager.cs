using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour {
    [SerializeField]
    private Camera m_uiCamera = null;
    [SerializeField]
    private Camera m_gameCamera = null;

    [SerializeField]
    private Camera m_compositeCamera = null;

    public Camera GameCamera() {
        return m_gameCamera;
    }

    public Camera UICamera{
        get {
            return m_uiCamera.gameObject.activeInHierarchy ? m_uiCamera : m_compositeCamera;
        }
    }

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
        Vector3 toReturn = UICamera.ScreenToWorldPoint(screenPoint);
        return toReturn;
    }

    public Vector3 FromUIToGameVector(Vector3 source) {
        Vector3 screenPoint = UICamera.WorldToScreenPoint(source);
        Vector3 toReturn    = m_gameCamera.ScreenToWorldPoint(screenPoint);
        return toReturn;
    }

    public Vector3 FromVPToGameCamera(Vector3 source) {
        Vector3 toReturn = m_gameCamera.ViewportToWorldPoint(source);
        return toReturn;
    }

    public Transform GetUICameraTransform() {
        return UICamera.transform;
    }
}