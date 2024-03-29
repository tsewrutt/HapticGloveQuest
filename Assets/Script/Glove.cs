using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Reflection;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;
using static Valve.VR.InteractionSystem.Sample.CustomSkeletonHelper;

public class Glove : MonoBehaviour
{
    // Start is called before the first frame update
    public Handheld otherHand;
    public SteamVR_Input_Sources handType;
    public SteamVR_Behaviour_Skeleton skeleton;

    [Header("Tip Collision")]
    public GameObject tipColliderPrefab;
    private GameObject[] tipColliders;

    //public HandPhysics handPhy;
    //Thumb

    //0: Thumb Proximal
    //1: Thumb Intermediate
    //2: Thumb Distal
    //3: Thumb Tip
    //Index Finger:

    //4: Index Proximal
    //5: Index Intermediate
    //6: Index Distal
    //7: Index Tip
    //Middle Finger:

    //8: Middle Proximal
    //9: Middle Intermediate
    //10: Middle Distal
    //11: Middle Tip
    //Ring Finger:

    //12: Ring Proximal
    //13: Ring Intermediate
    //14: Ring Distal
    //15: Ring Tip
    //Pinky Finger:

    //16: Pinky Proximal
    //17: Pinky Intermediate
    //18: Pinky Distal
    //19: Pinky Tip
    //Wrist:

    //20: Wrist

    public float grabThresholdDistance = 0.1f;

    public enum TypeOfGloveInteraction
    {
        Idle, //This includes releasing an object
        OneFingerPress,
        TwoFingerPress,
        ThreeFingerPress,
        IndexAndThumbGrab,
        MiddleAndThumbGrab,
        RingAndThumbGrab,
        PinkyAndThumbGrab,
        AllFingersGrab, // If object is texture we dont grab, we call TextureFeel()
        
    }

    private TypeOfGloveInteraction gloveState;

    private bool isGrabbing = false;
    private SteamVR_Behaviour_Pose trackedObject;
    public SteamVR_Action_Boolean grabPinchAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabPinch");
    public SteamVR_Action_Vibration hapticPalmAction = SteamVR_Input.GetAction<SteamVR_Action_Vibration>("Haptic");
    public SteamVR_Action_Vibration hapticThumbAction = SteamVR_Input.GetAction<SteamVR_Action_Vibration>("Haptic_Thumb");
    public SteamVR_Action_Vibration hapticIndexAction = SteamVR_Input.GetAction<SteamVR_Action_Vibration>("Haptic_Index");
    public SteamVR_Action_Vibration hapticMiddleAction = SteamVR_Input.GetAction<SteamVR_Action_Vibration>("Haptic_Middle");
    public SteamVR_Action_Vibration hapticRingAction = SteamVR_Input.GetAction<SteamVR_Action_Vibration>("Haptic_Ring");
    public SteamVR_Action_Vibration hapticPinkyAction = SteamVR_Input.GetAction<SteamVR_Action_Vibration>("Haptic_Pinky");

    void Start()
    {
        tipColliders = new GameObject[5];

        for (int i = 0; i < 5; i++)
        {
            tipColliders[i] = Instantiate(tipColliderPrefab, transform);
        }

        gloveState = TypeOfGloveInteraction.Idle;

    }
   
    // Update is called once per frame
    void Update()
    {
        //Get Tip Position And Update the colliders to match where tip is going
        if (skeleton != null)
        {
            Vector3 thumbTipPosition = skeleton.GetBonePosition(3);
            tipColliders[0].transform.position = thumbTipPosition;
            
            Vector3 indexTipPosition = skeleton.GetBonePosition(7);
            tipColliders[1].transform.position = indexTipPosition;

            Vector3 middleTipPosition = skeleton.GetBonePosition(11);
            tipColliders[2].transform.position = middleTipPosition;

            Vector3 ringTipPosition = skeleton.GetBonePosition(15);
            tipColliders[3].transform.position = ringTipPosition;

            Vector3 pinkyTipPosition = skeleton.GetBonePosition(19);
            tipColliders[4].transform.position = pinkyTipPosition;

        }
        
        
       // CheckFingerHover();
        CheckGrab();
    }

    //This one will check the fingers are colliding with an object
    //and switch the states
    void CheckGrab()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 0.05f);

    }

    void CallGrabRoutine()
    {
        gloveState = TypeOfGloveInteraction.OneFingerPress;
        switch (gloveState)
        {
            case TypeOfGloveInteraction.IndexAndThumbGrab:
                

                break;

        }

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
