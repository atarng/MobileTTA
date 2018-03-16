/**************************************************
 * <CLASS_NAME>
 * <DESCRIPTION OF CLASS>
 * 
 * author: Alfred Tarng <copy_right_information>
 *************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AtRng.MobileTTA
{
public class OrientationMonitor : MonoBehaviour {
    public static event Action<Vector2> OnResolutionChange;
    public static event Action<DeviceOrientation> OnOrientationChange;

    [Tooltip("How long to wait until we check again.")]
    [SerializeField]
    private float m_check_delay = 0.5f;

    // Current Resolution
    private static Vector2 s_resolution;
    // Current Device Orientation
    private static DeviceOrientation s_orientation;
    private static bool s_is_alive = true;                    // Keep this script running?

    #region MONOBEHAVIORS
    private void Start() {
        StartCoroutine(CheckForChange());
    }
    private void OnDestroy(){ s_is_alive = false; }
    #endregion


    IEnumerator CheckForChange()
    {
        s_resolution = new Vector2(Screen.width, Screen.height);
        yield return new WaitForSeconds(1.0f);

        if (OnResolutionChange != null) OnResolutionChange(s_resolution);

        s_orientation = Input.deviceOrientation;

        while (s_is_alive)
        {
            // Check for a Resolution Change
            if (s_resolution.x != Screen.width || s_resolution.y != Screen.height)
            {
                s_resolution = new Vector2(Screen.width, Screen.height);
                if (OnResolutionChange != null) OnResolutionChange(s_resolution);
            }

            // Check for an Orientation Change
            switch (Input.deviceOrientation)
            {
                // Ignore
                case DeviceOrientation.Unknown:
                case DeviceOrientation.FaceUp:
                case DeviceOrientation.FaceDown:
                    break;
                //Portrait, PortraitUpsideDown, LandscapeLeft, LandscapeRight
                default:
                    if (s_orientation != Input.deviceOrientation)
                    {
                        s_orientation = Input.deviceOrientation;
                        if (OnOrientationChange != null) OnOrientationChange(s_orientation);
                    }
                    break;
            }
            yield return new WaitForSeconds(m_check_delay);
        }
    }
}}