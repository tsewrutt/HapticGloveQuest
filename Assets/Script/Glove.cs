using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class Glove : MonoBehaviour
{
    // Start is called before the first frame update
    public Handheld otherHand;
    public SteamVR_Input_Sources handType;
    private Transform[] fingerHoverPoints; // Will assign points in inspector for each finger tips

    //public Transform palmHapticPt;
    //public Transform thumbHapticPt;
    //public Transform indexHapticPt;
    //public Transform middleHapticPt;
    //public Transform ringHapticPt;
    //public Transform pinkyHapticPt;


    public float grabThresholdDistance = 0.1f;


    private bool isGrabbing = false;
    private SteamVR_Behaviour_Pose trackedObject;

    public SteamVR_Action_Boolean grabPinchAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabPinch");

    public SteamVR_Action_Vibration hapticAction = SteamVR_Input.GetAction<SteamVR_Action_Vibration>("Haptic");

    void Start()
    {

        //fingerHoverPoints[0] = palmHapticPt;
        //fingerHoverPoints[1] = thumbHapticPt;
        //fingerHoverPoints[2] = indexHapticPt;
        //fingerHoverPoints[3] = middleHapticPt;
        //fingerHoverPoints[4] = ringHapticPt;
        //fingerHoverPoints[5] = pinkyHapticPt;
    }

    // Update is called once per frame
    void Update()
    {
        CheckFingerHover();
        CheckGrab();
    }

    void CheckFingerHover()
    {

    }

    void CheckGrab()
    {

    }

    public bool isActive
    {
        get
        {
            if (trackedObject != null)
                return trackedObject.isActive;

            return this.gameObject.activeInHierarchy;
        }
    }

    public bool isPoseValid
    {
        get
        {
            return trackedObject.isValid;
        }
    }

    public void DetachObject(GameObject objectToDetach, bool restoreOriginalParent = true)
    {
        //int index = attachedObjects.FindIndex(l => l.attachedObject == objectToDetach);
    }
}
