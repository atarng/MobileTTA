/**************************************************
 * LayoutHander.cs
 * <DESCRIPTION OF CLASS>
 * 
 * author: Alfred Tarng <copy_right_information>
 *************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AtRng.MobileTTA;

public class LayoutHandler : MonoBehaviour {

    #region PRIVATE_MEMBERS
    [SerializeField]
    private Transform m_horizontal_transform;

    [SerializeField]
    private Transform m_vertical_transform;

    [SerializeField]
    private Transform[] m_list_of_children;
    #endregion

    #region PUBLIC_MEMBERS
    #endregion

    #region MONOBEHAVIORS
    private void Start()
    {
        OrientationMonitor.OnOrientationChange += SwitchLayout;
        OrientationMonitor.OnResolutionChange += SwitchLayout;
    }
    #endregion

    #region PRIVATE_METHODS
    public void SwitchLayout(DeviceOrientation device_orientation)
    {
        Debug.Log(string.Format("Switch Layout Based on Orientation: {0}", device_orientation));
        Transform parent = transform;
        switch (device_orientation) {
            case DeviceOrientation.Portrait:
            case DeviceOrientation.PortraitUpsideDown:
                parent = m_horizontal_transform;
                break;
            case DeviceOrientation.LandscapeLeft:
            case DeviceOrientation.LandscapeRight:
                parent = m_vertical_transform;
                break;
            default:
                break;
        }

        foreach (Transform t in m_list_of_children)
        {
            t.SetParent(parent);
            t.localScale = Vector3.one;
            RectTransform tr = t as RectTransform;
            if (tr) { tr.anchoredPosition = Vector3.zero; }
            else { tr.localPosition = Vector3.zero; }
        }

    }
    public void SwitchLayout(Vector2 device_resolution)
    {
        Debug.Log(string.Format("Switch Layout Based On Resolution: {0}", device_resolution));
        foreach (Transform t in m_list_of_children)
        {
            t.SetParent((device_resolution.x > device_resolution.y) ? m_horizontal_transform : m_vertical_transform);
            t.localScale = Vector3.one;
            RectTransform tr = t as RectTransform;
            if (tr) { tr.anchoredPosition = Vector3.zero; }
            else { tr.localPosition = Vector3.zero; }
        }
    }
    #endregion

    #region PUBLIC_METHODS

    #endregion
}