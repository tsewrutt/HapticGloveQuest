using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Timers;
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

    //GameObjectGrabbed
    private GameObject interactingObject;

    [Header("Tip Collision")]
    public GameObject tipColliderPrefab;
    

    private Vector3 thumbColliderPos;
    private Vector3 indexColliderPos;
    private Vector3 middleColliderPos;
    private Vector3 ringColliderPos;
    private Vector3 pinkyColliderPos;

    //Thumb Booleans//
    private bool isThumbTouchingObject = false;
    private bool isThumbTouchingWood = false;
    private bool isThumbTouchingConcrete = false;
    private bool isThumbTouchingPlastic = false;
    private bool isThumbTouchingSand = false;

    //Index Booleans//
    private bool isIndexTouchingObject = false;
    private bool isIndexTouchingWood = false;
    private bool isIndexTouchingConcrete = false;
    private bool isIndexTouchingPlastic = false;
    private bool isIndexTouchingSand = false;

    //Middle Booleans//
    private bool isMiddleTouchingObject = false;
    private bool isMiddleTouchingWood = false;
    private bool isMiddleTouchingConcrete = false;
    private bool isMiddleTouchingPlastic = false;
    private bool isMiddleTouchingSand = false;

    //Ring Booleans//
    private bool isRingTouchingObject = false;
    private bool isRingTouchingWood = false;
    private bool isRingTouchingConcrete = false;
    private bool isRingTouchingPlastic = false;
    private bool isRingTouchingSand = false;

    //Pinky Booleans//
    private bool isPinkyTouchingObject = false;
    private bool isPinkyTouchingWood = false;
    private bool isPinkyTouchingConcrete = false;
    private bool isPinkyTouchingPlastic = false;
    private bool isPinkyTouchingSand = false;

    public float grabThresholdDistance = 0.1f;

    bool[] thumb_tip_haptics = { false, false, false, false, false };
    bool[] index_tip_haptics = { false, false, false, false, false };
    bool[] middle_tip_haptics = { false, false, false, false, false };
    bool[] ring_tip_haptics = { false, false, false, false, false };
    bool[] pinky_tip_haptics = { false, false, false, false, false };



    private Transform[] thumbBones;
    private Transform[] indexBones;
    private Transform[] middleBones;
    private Transform[] ringBones;
    private Transform[] pinkyBones;

    private Collider[] thumbColliders;
    private Collider[] indexColliders;
    private Collider[] middleColliders;
    private Collider[] ringColliders;
    private Collider[] pinkyColliders; 

    public enum TypeOfGloveInteraction
    {
        Idle, //This includes releasing an object
        IndexMiddleRingPress,
        IndexMiddlePress,
        IndexPress,
        ThumbAndIndexGrab,
        ThumbAndMiddleGrab,
        ThumbAndRingGrab,
        ThumbAndPinkyGrab,
        AllFingersGrab, // If object is tagged "texture" we dont grab, we call TextureFeel()
        
        //Wood Haptics//
        WoodHaptic,

        //Concrete Haptics//
        ConcreteHaptic,

        //Plastic Haptics//
        PlasticHaptic,

        //Sand Haptics//
        SandHaptic,

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

    private GameObject handColliderRightObject;

    void Start()
    {
        handColliderRightObject = GameObject.Find("HandColliderRight(Clone)");

        // Check if the object is found
        if (handColliderRightObject != null)
        {
            // Do something with the found object
            Debug.Log("Found HandColliderRight(Clone) object!");

            //GetHandCollider Component
        }
        else
        {
            // Object not found
            Debug.LogWarning("HandRight(Clone) object not found!");
        }

        gloveState = TypeOfGloveInteraction.Idle;

    }
   
    // Update is called once per frame
    void Update()
    {
        //Get Tip Position And Update the colliders to match where tip is going
        if (skeleton != null)
        {
            
            // Check if the object is found
            if (handColliderRightObject != null)
            {
                //GetHandCollider Component
                HandCollider handCollider = handColliderRightObject.GetComponent<HandCollider>();
                if (handCollider != null)
                {
                    //Do all transform
                    // Access the collider transforms from the HandCollider script
                    thumbBones = handCollider.fingerColliders.thumbColliders;
                    indexBones = handCollider.fingerColliders.indexColliders;
                    middleBones = handCollider.fingerColliders.middleColliders;
                    ringBones = handCollider.fingerColliders.ringColliders;
                    pinkyBones = handCollider.fingerColliders.pinkyColliders;

                    thumbColliderPos = thumbBones[0].position;
                    indexColliderPos = indexBones[0].position;
                    middleColliderPos = middleBones[0].position;
                    ringColliderPos = ringBones[0].position;
                    pinkyColliderPos = pinkyBones[0].position;

                }
                else
                {
                    Debug.Log("HandCollider Component not found");
                }
            }
            else
            {
                // Object not found
                Debug.LogWarning("HandRight(Clone) object not found!");
            }


        }

        UpdateGloveState();
        DoGloveAction();
    }

    void DoGloveAction()
    {
        switch(gloveState)
        {
            case TypeOfGloveInteraction.IndexPress:
                Debug.Log("Do Index Press Action");
                index_tip_haptics[0] = true;
                index_tip_haptics[1] = true;
                index_tip_haptics[2] = true;
                index_tip_haptics[3] = true;
                index_tip_haptics[4] = true;
                activate_haptics(hapticIndexAction, SteamVR_Input_Sources.RightHand, index_tip_haptics,1f);

                break;

            case TypeOfGloveInteraction.IndexMiddlePress:
                Debug.Log("Do Index & Middle Press Action");
                index_tip_haptics[0] = true;
                index_tip_haptics[1] = true;
                index_tip_haptics[2] = true;
                index_tip_haptics[3] = true;
                index_tip_haptics[4] = true;

                middle_tip_haptics[0] = true;
                middle_tip_haptics[1] = true;
                middle_tip_haptics[2] = true;
                middle_tip_haptics[3] = true;
                middle_tip_haptics[4] = true;
                activate_haptics(hapticIndexAction, SteamVR_Input_Sources.RightHand, index_tip_haptics, 0.0f);
                activate_haptics(hapticMiddleAction, SteamVR_Input_Sources.RightHand, middle_tip_haptics, 0.0f);

                break;

            case TypeOfGloveInteraction.IndexMiddleRingPress:
                Debug.Log("Do Index & Middle & Ring Press Action");
                index_tip_haptics[0] = true;
                index_tip_haptics[1] = true;
                index_tip_haptics[2] = true;
                index_tip_haptics[3] = true;
                index_tip_haptics[4] = true;

                middle_tip_haptics[0] = true;
                middle_tip_haptics[1] = true;
                middle_tip_haptics[2] = true;
                middle_tip_haptics[3] = true;
                middle_tip_haptics[4] = true;

                ring_tip_haptics[0] = true;
                ring_tip_haptics[1] = true;
                ring_tip_haptics[2] = true;
                ring_tip_haptics[3] = true;
                ring_tip_haptics[4] = true;

                activate_haptics(hapticIndexAction, SteamVR_Input_Sources.RightHand, index_tip_haptics, 0.0f);
                activate_haptics(hapticMiddleAction, SteamVR_Input_Sources.RightHand, middle_tip_haptics, 0.0f);
                activate_haptics(hapticRingAction, SteamVR_Input_Sources.RightHand, ring_tip_haptics, 0.0f);
                break;

            case TypeOfGloveInteraction.ThumbAndIndexGrab:
                Debug.Log("Thumb and Index Grab");
                activate_haptics(hapticThumbAction, SteamVR_Input_Sources.RightHand, thumb_tip_haptics, 0.25f);
                activate_haptics(hapticIndexAction, SteamVR_Input_Sources.RightHand, index_tip_haptics, 0.25f);
                //Grabbing 

                Vector3 midpt = (thumbColliders[0].transform.position + indexColliders[0].transform.position) /2f;
                Vector3 direction = indexColliders[0].transform.position - thumbColliders[0].transform.position;

                //Quaternion rotation = Quaternion.LookRotation(direction);
                //interactingObject.transform.position = midpt;
                //interactingObject.transform.rotation = rotation;

                break;

            case TypeOfGloveInteraction.ThumbAndMiddleGrab:
                Debug.Log("Thumb and Middle Grab");
                activate_haptics(hapticThumbAction, SteamVR_Input_Sources.RightHand, thumb_tip_haptics, 0.25f);
                activate_haptics(hapticMiddleAction, SteamVR_Input_Sources.RightHand, middle_tip_haptics, 0.25f);

                break;
            
            case TypeOfGloveInteraction.ThumbAndRingGrab:
                Debug.Log("Thumb and Pinky Grab");
                activate_haptics(hapticThumbAction, SteamVR_Input_Sources.RightHand, thumb_tip_haptics, 0.25f);
                activate_haptics(hapticRingAction, SteamVR_Input_Sources.RightHand, ring_tip_haptics, 0.25f);

                break;

            case TypeOfGloveInteraction.ThumbAndPinkyGrab:
                Debug.Log("Thumb and Pinky Grab");
                activate_haptics(hapticThumbAction, SteamVR_Input_Sources.RightHand, thumb_tip_haptics, 0.25f);
                activate_haptics(hapticPinkyAction, SteamVR_Input_Sources.RightHand, pinky_tip_haptics, 0.25f);

                break;

            case TypeOfGloveInteraction.WoodHaptic:
                Debug.Log("Wood Feel Haptic");
                
                if (isThumbTouchingWood)
                {
                    thumb_tip_haptics[0] = false;
                    thumb_tip_haptics[1] = true;
                    thumb_tip_haptics[2] = false;
                    thumb_tip_haptics[3] = true;
                    thumb_tip_haptics[4] = false;
                    activate_haptics(hapticThumbAction, SteamVR_Input_Sources.RightHand, thumb_tip_haptics, 0.05f);
                   
                }
                if (isIndexTouchingWood)
                {
                    index_tip_haptics[0] = false;
                    index_tip_haptics[1] = true;
                    index_tip_haptics[2] = false;
                    index_tip_haptics[3] = true;
                    index_tip_haptics[4] = false;
                    activate_haptics(hapticIndexAction, SteamVR_Input_Sources.RightHand, index_tip_haptics, 0.05f);

                }
                if (isMiddleTouchingWood)
                {
                    middle_tip_haptics[0] = false;
                    middle_tip_haptics[1] = true;
                    middle_tip_haptics[2] = false;
                    middle_tip_haptics[3] = true;
                    middle_tip_haptics[4] = false;
                    activate_haptics(hapticMiddleAction, SteamVR_Input_Sources.RightHand, middle_tip_haptics, 0.05f);
                }
                if (isRingTouchingWood)
                {
                    ring_tip_haptics[0] = false;
                    ring_tip_haptics[1] = true;
                    ring_tip_haptics[2] = false;
                    ring_tip_haptics[3] = true;
                    ring_tip_haptics[4] = false;
                    activate_haptics(hapticRingAction, SteamVR_Input_Sources.RightHand, ring_tip_haptics, 0.05f);
                }
                if (isPinkyTouchingWood)
                {
                    pinky_tip_haptics[0] = false;
                    pinky_tip_haptics[1] = true;
                    pinky_tip_haptics[2] = false;
                    pinky_tip_haptics[3] = true;
                    pinky_tip_haptics[4] = false;
                    activate_haptics(hapticRingAction, SteamVR_Input_Sources.RightHand, pinky_tip_haptics, 0.05f);
                }
                break;

            case TypeOfGloveInteraction.ConcreteHaptic:
                Debug.Log("Concrete Feel Haptic");
                if (isThumbTouchingConcrete)
                {
                    thumb_tip_haptics[0] = true;
                    thumb_tip_haptics[1] = true;
                    thumb_tip_haptics[2] = true;
                    thumb_tip_haptics[3] = true;
                    thumb_tip_haptics[4] = true;
                    activate_haptics(hapticThumbAction, SteamVR_Input_Sources.RightHand, thumb_tip_haptics, 0.08f);

                }
                if (isIndexTouchingConcrete)
                {
                    index_tip_haptics[0] = true;
                    index_tip_haptics[1] = true;
                    index_tip_haptics[2] = true;
                    index_tip_haptics[3] = true;
                    index_tip_haptics[4] = true;
                    activate_haptics(hapticIndexAction, SteamVR_Input_Sources.RightHand, index_tip_haptics, 0.08f);

                }
                if (isMiddleTouchingConcrete)
                {
                    middle_tip_haptics[0] = true;
                    middle_tip_haptics[1] = true;
                    middle_tip_haptics[2] = true;
                    middle_tip_haptics[3] = true;
                    middle_tip_haptics[4] = true;
                    activate_haptics(hapticMiddleAction, SteamVR_Input_Sources.RightHand, middle_tip_haptics, 0.08f);
                }
                if (isRingTouchingConcrete)
                {
                    ring_tip_haptics[0] = true;
                    ring_tip_haptics[1] = true;
                    ring_tip_haptics[2] = true;
                    ring_tip_haptics[3] = true;
                    ring_tip_haptics[4] = true;
                    activate_haptics(hapticRingAction, SteamVR_Input_Sources.RightHand, ring_tip_haptics, 0.08f);
                }
                if (isPinkyTouchingConcrete)
                {
                    pinky_tip_haptics[0] = true;
                    pinky_tip_haptics[1] = true;
                    pinky_tip_haptics[2] = true;
                    pinky_tip_haptics[3] = true;
                    pinky_tip_haptics[4] = true;
                    activate_haptics(hapticRingAction, SteamVR_Input_Sources.RightHand, pinky_tip_haptics, 0.08f);
                }
                break;

            case TypeOfGloveInteraction.PlasticHaptic:
                Debug.Log("Plastic Feel Haptic");
                if (isThumbTouchingPlastic)
                {
                    thumb_tip_haptics[0] = true;
                    thumb_tip_haptics[1] = true;
                    thumb_tip_haptics[2] = false;
                    thumb_tip_haptics[3] = false;
                    thumb_tip_haptics[4] = true;
                    activate_haptics(hapticThumbAction, SteamVR_Input_Sources.RightHand, thumb_tip_haptics, 0.03f);

                }
                if (isIndexTouchingPlastic)
                {
                    index_tip_haptics[0] = true;
                    index_tip_haptics[1] = true;
                    index_tip_haptics[2] = false;
                    index_tip_haptics[3] = false;
                    index_tip_haptics[4] = true;
                    activate_haptics(hapticIndexAction, SteamVR_Input_Sources.RightHand, index_tip_haptics, 0.03f);

                }
                if (isMiddleTouchingPlastic)
                {
                    middle_tip_haptics[0] = true;
                    middle_tip_haptics[1] = true;
                    middle_tip_haptics[2] = false;
                    middle_tip_haptics[3] = false;
                    middle_tip_haptics[4] = true;
                    activate_haptics(hapticMiddleAction, SteamVR_Input_Sources.RightHand, middle_tip_haptics, 0.03f);
                }
                if (isRingTouchingPlastic)
                {
                    ring_tip_haptics[0] = true;
                    ring_tip_haptics[1] = true;
                    ring_tip_haptics[2] = false;
                    ring_tip_haptics[3] = false;
                    ring_tip_haptics[4] = true;
                    activate_haptics(hapticRingAction, SteamVR_Input_Sources.RightHand, ring_tip_haptics, 0.03f);
                }
                if (isPinkyTouchingPlastic)
                {
                    pinky_tip_haptics[0] = true;
                    pinky_tip_haptics[1] = true;
                    pinky_tip_haptics[2] = false;
                    pinky_tip_haptics[3] = false;
                    pinky_tip_haptics[4] = true;
                    activate_haptics(hapticRingAction, SteamVR_Input_Sources.RightHand, pinky_tip_haptics, 0.03f);
                }
                break;

            case TypeOfGloveInteraction.SandHaptic:
                Debug.Log("Sand Feel Haptic");
                if (isThumbTouchingSand)
                {
                    thumb_tip_haptics[0] = false;
                    thumb_tip_haptics[1] = true;
                    thumb_tip_haptics[2] = true;
                    thumb_tip_haptics[3] = true;
                    thumb_tip_haptics[4] = false;
                    activate_haptics(hapticThumbAction, SteamVR_Input_Sources.RightHand, thumb_tip_haptics, 0.1f);

                }
                if (isIndexTouchingSand)
                {
                    index_tip_haptics[0] = false;
                    index_tip_haptics[1] = true;
                    index_tip_haptics[2] = true;
                    index_tip_haptics[3] = true;
                    index_tip_haptics[4] = false;
                    activate_haptics(hapticIndexAction, SteamVR_Input_Sources.RightHand, index_tip_haptics, 0.1f);

                }
                if (isMiddleTouchingSand)
                {
                    middle_tip_haptics[0] = false;
                    middle_tip_haptics[1] = true;
                    middle_tip_haptics[2] = true;
                    middle_tip_haptics[3] = true;
                    middle_tip_haptics[4] = false;
                    activate_haptics(hapticMiddleAction, SteamVR_Input_Sources.RightHand, middle_tip_haptics, 0.1f);
                }
                if (isRingTouchingSand)
                {
                    ring_tip_haptics[0] = false;
                    ring_tip_haptics[1] = true;
                    ring_tip_haptics[2] = true;
                    ring_tip_haptics[3] = true;
                    ring_tip_haptics[4] = false;
                    activate_haptics(hapticRingAction, SteamVR_Input_Sources.RightHand, ring_tip_haptics, 0.1f);
                }
                if (isPinkyTouchingSand)
                {
                    pinky_tip_haptics[0] = false;
                    pinky_tip_haptics[1] = true;
                    pinky_tip_haptics[2] = true;
                    pinky_tip_haptics[3] = true;
                    pinky_tip_haptics[4] = false;
                    activate_haptics(hapticRingAction, SteamVR_Input_Sources.RightHand, pinky_tip_haptics, 0.1f);
                }
                break;


            case TypeOfGloveInteraction.Idle:
                Debug.Log("Glove not interacting anymore");
                ResetGloveState();
                interactingObject = null;
                break;
        }
    }

    //This one will check the fingers are colliding with an object
    //and switch the states

        //ThumbAndIndexGrab,
        //ThumbAndMiddleGrab,
        //ThumbAndRingGrab,
        //ThumbAndPinkyGrab,
        //AllFingersGrab, // If object is tagged "texture" we dont grab, we call TextureFeel()
        
        ////Wood Haptics//
        //WoodHaptic,

        ////Concrete Haptics//
        //ConcreteHaptic,

        ////Plastic Haptics//
        //PlasticHaptic,

        ////Sand Haptics//
        //SandHaptic,
    //We may have huge latency
    //Possible solution is to do check hand, if you no object associated with hand, then run inside updateglovestate

    void ResetGloveState() {

        ResetThumbBooleans();
        ResetIndexBooleans();
        ResetMiddleBooleans();
        ResetRingBooleans();
        ResetPinkyBooleans();
    
    }

    void UpdateGloveState()
    {
        // Check if finger tips are touching the obj
        thumbColliders = Physics.OverlapSphere(thumbColliderPos, 0.03f);
        indexColliders = Physics.OverlapSphere(indexColliderPos, 0.03f);
        middleColliders = Physics.OverlapSphere(middleColliderPos, 0.03f);
        ringColliders = Physics.OverlapSphere(ringColliderPos, 0.03f);
        pinkyColliders = Physics.OverlapSphere(pinkyColliderPos, 0.03f);

        //Check Collider on Thumb
        foreach (Collider collider in thumbColliders)
        {
            if (collider.CompareTag("InteractingObject"))
            {
                isThumbTouchingObject = true;
                isThumbTouchingWood = false;
                isThumbTouchingConcrete = false;
                isThumbTouchingPlastic = false;
                isThumbTouchingSand = false;

                break;
            }
            else if (collider.CompareTag("Wood"))
            {
                //vibrate for wood but not grab
                isThumbTouchingWood = true;
                isThumbTouchingObject = false;
                isThumbTouchingConcrete = false;
                isThumbTouchingPlastic = false;
                isThumbTouchingSand = false;
                break;
            }
            else if (collider.CompareTag("Concrete"))
            {
                //vibrate for concrete but not grab
                isThumbTouchingConcrete = true;
                isThumbTouchingObject = false;
                isThumbTouchingWood = false;
                isThumbTouchingPlastic = false;
                isThumbTouchingSand = false;
                break;
            }
            else if (collider.CompareTag("Plastic"))
            {
                //vibrate for plastic but not grab
                isThumbTouchingPlastic = true;
                isThumbTouchingObject = false;
                isThumbTouchingWood = false;
                isThumbTouchingConcrete = false;
                isThumbTouchingSand = false;

                break;
            }
            else if (collider.CompareTag("Sand"))
            {
                //vibrate for sand but not grab
                isThumbTouchingSand = true;
                
                isThumbTouchingObject = false;
                isThumbTouchingWood = false;
                isThumbTouchingConcrete = false;
                isThumbTouchingPlastic = false;
                break;
            }
            else
            {
                ResetThumbBooleans();
                
            }

        }

        //Check Collider on Index
        foreach (Collider collider in indexColliders)
        {
            if (collider.CompareTag("InteractingObject"))
            {
                isIndexTouchingObject = true;

                isIndexTouchingWood = false;
                isIndexTouchingConcrete = false;
                isIndexTouchingPlastic = false;
                isIndexTouchingSand = false;
                break;
            }
            else if (collider.CompareTag("Wood"))
            {
                //vibrate for wood but not grab
                isIndexTouchingWood = true;

                isIndexTouchingObject = false;
                isIndexTouchingConcrete = false;
                isIndexTouchingPlastic = false;
                isIndexTouchingSand = false;
                break;
            }
            else if (collider.CompareTag("Concrete"))
            {
                //vibrate for concrete but not grab
                isIndexTouchingConcrete = true;

                isIndexTouchingObject = false;
                isIndexTouchingWood = false;
                isIndexTouchingPlastic = false;
                isIndexTouchingSand = false;
                break;
            }
            else if (collider.CompareTag("Plastic"))
            {
                //vibrate for plastic but not grab
                isIndexTouchingPlastic = true;

                isIndexTouchingObject = false;
                isIndexTouchingWood = false;
                isIndexTouchingConcrete = false;
                isIndexTouchingSand = false;
                break;
            }
            else if (collider.CompareTag("Sand"))
            {
                //vibrate for sand but not grab
                isIndexTouchingSand = true;
                
                isIndexTouchingObject = false;
                isIndexTouchingWood = false;
                isIndexTouchingConcrete = false;
                isIndexTouchingPlastic = false;
                break;
            }
            else
            {
                ResetIndexBooleans();
            }

        }

        //Check Collider on Middle
        foreach (Collider collider in middleColliders)
        {
            if (collider.CompareTag("InteractingObject"))
            {
                isMiddleTouchingObject = true;
                
                isMiddleTouchingWood = false;
                isMiddleTouchingConcrete = false;
                isMiddleTouchingPlastic = false;
                isMiddleTouchingSand = false;

                interactingObject = collider.gameObject;

                break;
            }
            else if (collider.CompareTag("Wood"))
            {
                //vibrate for wood but not grab
                isMiddleTouchingWood = true;

                isMiddleTouchingObject = false;
                isMiddleTouchingConcrete = false;
                isMiddleTouchingPlastic = false;
                isMiddleTouchingSand = false;

                break;
            }
            else if (collider.CompareTag("Concrete"))
            {
                //vibrate for concrete but not grab
                isMiddleTouchingConcrete = true;

                isMiddleTouchingObject = false;
                isMiddleTouchingWood = false;
                isMiddleTouchingPlastic = false;
                isMiddleTouchingSand = false;
                break;
            }
            else if (collider.CompareTag("Plastic"))
            {
                //vibrate for plastic but not grab
                isMiddleTouchingPlastic = true;

                isMiddleTouchingObject = false;
                isMiddleTouchingWood = false;
                isMiddleTouchingConcrete = false;
                isMiddleTouchingSand = false;
                break;
            }
            else if (collider.CompareTag("Sand"))
            {
                //vibrate for sand but not grab
                isMiddleTouchingSand = true;

                isMiddleTouchingObject = false;
                isMiddleTouchingWood = false;
                isMiddleTouchingConcrete = false;
                isMiddleTouchingPlastic = false;
                break;
            }
            else
            {
                ResetMiddleBooleans();
                break;
            }

        }

        //Check Collider on Ring
        foreach (Collider collider in ringColliders)
        {
            if (collider.CompareTag("InteractingObject"))
            {
                isRingTouchingObject = true;

                isRingTouchingWood = false;
                isRingTouchingConcrete = false;
                isRingTouchingPlastic = false;
                isRingTouchingSand = false;

                break;
            }
            else if (collider.CompareTag("Wood"))
            {
                //vibrate for wood but not grab
                isRingTouchingWood = true;

                isRingTouchingObject = false;
                isRingTouchingConcrete = false;
                isRingTouchingPlastic = false;
                isRingTouchingSand = false;
                break;
            }
            else if (collider.CompareTag("Concrete"))
            {
                //vibrate for concrete but not grab
                isRingTouchingConcrete = true;

                isRingTouchingObject = false;
                isRingTouchingWood = false;
                isRingTouchingPlastic = false;
                isRingTouchingSand = false;
                break;
            }
            else if (collider.CompareTag("Plastic"))
            {
                //vibrate for plastic but not grab
                isRingTouchingPlastic = true;

                isRingTouchingObject = false;
                isRingTouchingWood = false;
                isRingTouchingConcrete = false;
                isRingTouchingSand = false;
                break;
            }
            else if (collider.CompareTag("Sand"))
            {
                //vibrate for sand but not grab
                isRingTouchingSand = true;

                isRingTouchingObject = false;
                isRingTouchingWood = false;
                isRingTouchingConcrete = false;
                isRingTouchingPlastic = false;
                break;
            }
            else
            {
                ResetRingBooleans();
            }

        }

        //Check Collider on Pinky
        foreach (Collider collider in pinkyColliders)
        {
            if (collider.CompareTag("InteractingObject"))
            {
                isPinkyTouchingObject = true;

                isPinkyTouchingWood = false;
                isPinkyTouchingConcrete = false;
                isPinkyTouchingPlastic = false;
                isPinkyTouchingSand = false;
                break;
            }
            else if (collider.CompareTag("Wood"))
            {
                //vibrate for wood but not grab
                isPinkyTouchingWood = true;

                isPinkyTouchingObject = false;
                isPinkyTouchingConcrete = false;
                isPinkyTouchingPlastic = false;
                isPinkyTouchingSand = false;
                break;
            }
            else if (collider.CompareTag("Concrete"))
            {
                //vibrate for concrete but not grab
                isPinkyTouchingConcrete = true;

                isPinkyTouchingObject = false;
                isPinkyTouchingWood = false;
                isPinkyTouchingPlastic = false;
                isPinkyTouchingSand = false;
                break;
            }
            else if (collider.CompareTag("Plastic"))
            {
                //vibrate for plastic but not grab
                isPinkyTouchingPlastic = true;

                isPinkyTouchingObject = false;
                isPinkyTouchingWood = false;
                isPinkyTouchingConcrete = false;
                isPinkyTouchingSand = false;
                break;
            }
            else if (collider.CompareTag("Sand"))
            {
                //vibrate for sand but not grab
                isPinkyTouchingSand = true;

                isPinkyTouchingObject = false;
                isPinkyTouchingWood = false;
                isPinkyTouchingConcrete = false;
                isPinkyTouchingPlastic = false;
                break;
            }
            else
            {
                ResetPinkyBooleans();
            }

        }

        //if finger touches obj then -> haptic activate, and that's for all
        // but if one finger or 2 fingers that is not a team then only haptic active but not grab
        //grab is called for when combination of fingers availble 
        // we can check if any of those fingers are colliding then proceed on grabbing


        // Set glove state based on the detected interaction for each finger


        //===================== Press =================//
        // Index Press
        if ((isIndexTouchingObject && !isThumbTouchingObject))
        {
            //Index + Middle Press
            gloveState = TypeOfGloveInteraction.IndexPress;

        }
        else if ((isIndexTouchingObject && isMiddleTouchingObject))
        {
            //Index + Middle Press
            gloveState = TypeOfGloveInteraction.IndexMiddlePress;

        }
        else if ((isIndexTouchingObject && isMiddleTouchingObject && isRingTouchingObject))
        {
            //Index + Ring + Middle Press
            gloveState = TypeOfGloveInteraction.IndexMiddleRingPress;

        }
        //===================== Grab =================//
        else if ((isIndexTouchingObject && isThumbTouchingObject))
        {
            //Index + Thumb = Grab
            gloveState = TypeOfGloveInteraction.ThumbAndIndexGrab;

        }
        else if ((isThumbTouchingObject && isIndexTouchingObject && isMiddleTouchingObject && isRingTouchingObject && isPinkyTouchingObject))
        {
            //All Fingers Grab
            gloveState = TypeOfGloveInteraction.AllFingersGrab;

        }
        else if ((isThumbTouchingObject && isMiddleTouchingObject))
        {
            //Thumb + Middle Grab
            gloveState = TypeOfGloveInteraction.ThumbAndMiddleGrab;

        }
        else if ((isThumbTouchingObject && isRingTouchingObject))
        {
            //Thumb + Ring Grab
            gloveState = TypeOfGloveInteraction.ThumbAndRingGrab;
        }
        else if ((isThumbTouchingObject && isRingTouchingObject))
        {
            //Thumb + Pinky Grab
            gloveState = TypeOfGloveInteraction.ThumbAndPinkyGrab;

        }
        //================ Textures ==================//
        else if (isThumbTouchingWood || isIndexTouchingWood || isMiddleTouchingWood || isRingTouchingWood || isPinkyTouchingWood)
        {
            //Wood Interaction
            gloveState = TypeOfGloveInteraction.WoodHaptic;
        
        }
        else if (isThumbTouchingConcrete || isIndexTouchingConcrete || isMiddleTouchingConcrete || isRingTouchingConcrete || isPinkyTouchingConcrete)
        {
            //Concrete Interaction
            gloveState = TypeOfGloveInteraction.ConcreteHaptic;
        
        }
        else if (isThumbTouchingPlastic || isIndexTouchingPlastic || isMiddleTouchingPlastic || isRingTouchingPlastic || isPinkyTouchingPlastic)
        {
            //Plastic Interaction
            gloveState = TypeOfGloveInteraction.PlasticHaptic;
        
        }
        else if (isThumbTouchingSand || isIndexTouchingSand || isMiddleTouchingSand || isRingTouchingSand || isPinkyTouchingSand)
        {
            //Sand Interaction
            gloveState = TypeOfGloveInteraction.SandHaptic;

        }
        else
        {
            gloveState = TypeOfGloveInteraction.Idle;
        }


    }


//    void ResetColliders()
//    {
//        thumbColliders ;
//        pindexColliders;
//        middleColliders;
//        ringColliders;
//        pinkyColliders;


//}
//Thumb Resets//
void ResetThumbBooleans()
    {
        isThumbTouchingObject = false;
        isThumbTouchingWood = false;
        isThumbTouchingConcrete = false;
        isThumbTouchingPlastic = false;
        isThumbTouchingSand = false;
    }
    void ResetThumbTouchingObject()
    {
        isThumbTouchingObject = false;
    }

    void ResetThumbTouchingWood()
    {
        isThumbTouchingWood = false;
    }
    void ResetThumbTouchingConcrete()
    {
        isThumbTouchingConcrete = false;
    }
    void ResetThumbTouchingPlastic()
    {
        isThumbTouchingPlastic = false;
    }
    void ResetThumbTouchingSand()
    {
        isThumbTouchingSand = false;
    }

    //Index Resets//
    void ResetIndexBooleans()
    {
        isIndexTouchingObject = false;
        isIndexTouchingWood = false;
        isIndexTouchingConcrete = false;
        isIndexTouchingPlastic = false;
        isIndexTouchingSand = false;
    }
    void ResetIndexTouchinObject()
    {
        isIndexTouchingObject = false;
    }
    void ResetIndexTouchingWood()
    {
        isIndexTouchingWood = false;
    }
    void ResetIndexTouchingConcrete()
    {
        isIndexTouchingConcrete = false;
    }
    void ResetIndexTouchingPlastic()
    {
        isIndexTouchingPlastic = false;
    }
    void ResetIndexTouchingSand()
    {
        isIndexTouchingSand = false;
    }

    void ResetMiddleBooleans()
    {
        isMiddleTouchingObject = false;
        isMiddleTouchingWood = false;
        isMiddleTouchingConcrete = false;
        isMiddleTouchingPlastic = false;
        isMiddleTouchingSand = false;

    }
    void ResetMiddleTouchinObject()
    {
        isMiddleTouchingObject = false;
    }
    void ResetMiddleTouchingWood()
    {
        isMiddleTouchingWood = false;
    }
    void ResetMiddleTouchingConcrete()
    {
        isMiddleTouchingConcrete = false;
    }
    void ResetMiddleTouchingPlastic()
    {
        isMiddleTouchingPlastic = false;
    }
    void ResetMiddleTouchingSand()
    {
        isMiddleTouchingSand = false;
    }

    //Ring Resets//
    void ResetRingBooleans()
    {
         //Ring Booleans//
        isRingTouchingObject = false;
        isRingTouchingWood = false;
        isRingTouchingConcrete = false;
        isRingTouchingPlastic = false;
        isRingTouchingSand = false;

    }
    void ResetRingTouchinObject()
    {
        isRingTouchingObject = false;
    }
    void ResetRingTouchingWood()
    {
        isRingTouchingWood = false;
    }
    void ResetRingTouchingConcrete()
    {
        isRingTouchingConcrete = false;
    }
    void ResetRingTouchingPlastic()
    {
        isRingTouchingPlastic = false;
    }
    void ResetRingTouchingSand()
    {
        isRingTouchingSand = false;
    }


    void ResetPinkyBooleans()
    {
       
        isPinkyTouchingObject = false;
        isPinkyTouchingWood = false;
        isPinkyTouchingConcrete = false;
        isPinkyTouchingPlastic = false;
        isPinkyTouchingSand = false;
    }
    void ResetPinkyTouchinObject()
    {
        isPinkyTouchingObject = false;
    }
    void ResetPinkyTouchingWood()
    {
        isPinkyTouchingWood = false;
    }
    void ResetPinkyTouchingConcrete()
    {
        isPinkyTouchingConcrete = false;
    }
    void ResetPinkyTouchingPlastic()
    {
        isPinkyTouchingPlastic = false;
    }
    void ResetPinkyTouchingSand()
    {
        isPinkyTouchingSand = false;
    }

    public bool activate_haptics(SteamVR_Action_Vibration finger, SteamVR_Input_Sources hand, bool[] finger_tips, float tension_steps)
    {
        if (tension_steps < 0f || tension_steps > 1f || finger_tips.Length != 5)
            return false;

        int temp = 0;

        for (int i = 0; i < 5; i++)
            if (finger_tips[i])
                temp |= 1 << i;

        if (tension_steps > 1)
            tension_steps /= 240f;

        finger.Execute(0, 0, temp, tension_steps, hand);


        //Changes all hapts to false for different texture

        for(int i = 0; i < 5; i++)
        {
            finger_tips[i] = false; 
        }

        return true;
    }


    //void CallGrabRoutine()
    //{
    //    gloveState = TypeOfGloveInteraction.OneFingerPress;
    //    switch (gloveState)
    //    {
    //        case TypeOfGloveInteraction.IndexAndThumbGrab:


    //            break;

    //    }

    //}

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