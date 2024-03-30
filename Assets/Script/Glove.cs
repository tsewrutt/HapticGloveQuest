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

    private GameObject interactingObject;

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
        AllFingersGrab, // If object is tagged "texture" we dont grab, we call TextureFeel()
        
        //Wood Haptics//
        ThumbWoodHaptic,
        IndexWoodHaptic,
        MiddleWoodHaptic,
        RingWoodHaptic,
        PinkyWoodHaptic,

        //Concrete Haptics//
        ThumbConcreteHaptic,
        IndexConcreteHaptic,
        MiddleContreteHaptic,
        RingContreteHaptic,
        PinkConcreteHaptic,

        //Plastic Haptics//
        ThumbPlasticHaptic,
        IndexPlasticHaptic,
        MiddlePlasticHaptic,
        RingPlasticHaptic,
        PinkPlasticHaptic,

        //Sand Haptics//
        ThumbSandHaptic,
        IndexSandHaptic,
        MiddleSandHaptic,
        RingSandHaptic,
        PinkSandHaptic,

    }

    private TypeOfGloveInteraction gloveState;

    private bool isGrabbing = false;
    private SteamVR_Behaviour_Pose trackedObject;
    public SteamVR_Action_Boolean grabPinchAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabPinch");
    public SteamVR_Action_Vibration hapticPalmAction = SteamVR_Input.GetAction<SteamVR_Action_Vibration>("Haptic");
    //public SteamVR_Action_Vibration hapticThumbAction = SteamVR_Input.GetAction<SteamVR_Action_Vibration>("Haptic_Thumb");
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
        
        UpdateGloveState();
    }

    //This one will check the fingers are colliding with an object
    //and switch the states
    void UpdateGloveState()
    {
        // Check if finger tips are touching the obj
        Collider[] thumbColliders = Physics.OverlapSphere(tipColliders[0].transform.position, 0.05f);
        Collider[] indexColliders = Physics.OverlapSphere(tipColliders[1].transform.position, 0.05f);
        Collider[] middleColliders = Physics.OverlapSphere(tipColliders[2].transform.position, 0.05f);
        Collider[] ringColliders = Physics.OverlapSphere(tipColliders[3].transform.position, 0.05f);
        Collider[] pinkyColliders = Physics.OverlapSphere(tipColliders[4].transform.position, 0.05f);

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

        }

        //if finger touches obj then -> haptic activate, and that's for all
        // but if one finger or 2 fingers that is not a team then only haptic active but not grab
        //grab is called for when combination of fingers availble 
        // we can check if any of those fingers are colliding then proceed on grabbing


        //if (!isThumbTouchingObject && !isIndexTouchingObject && !isRingTouchingObject && !isMiddleTouchingObject && !isRingTouchingObject && !isPinkyTouchingObject)
        //{
        //    gloveState = TypeOfGloveInteraction.Idle;
        //}
        //else if (isThumbTouchingObject && isIndexTouchingObject && isRingTouchingObject && isMiddleTouchingObject && isRingTouchingObject && isPinkyTouchingObject)
        //{

        //}
        // Set glove state based on the detected interaction for each finger
        if (isThumbTouchingObject)
        {
            //gloveState = TypeOfGloveInteraction.OneFingerPress; //This would be checking all other finger to be off
         
        }
        else if (isThumbTouchingWood)
        {
            gloveState = TypeOfGloveInteraction.ThumbWoodHaptic;
        }
        else if (isThumbTouchingConcrete)
        {
            gloveState = TypeOfGloveInteraction.ThumbConcreteHaptic;
        }
        else if (isThumbTouchingPlastic)
        {
            gloveState = TypeOfGloveInteraction.ThumbPlasticHaptic;
        }
        else if (isThumbTouchingSand)
        {
            gloveState = TypeOfGloveInteraction.ThumbSandHaptic;
        }
        else if (isIndexTouchingObject)
        {
            gloveState = TypeOfGloveInteraction.OneFingerPress;
        }
        // Repeat the same for other fingers...
        else
        {
            gloveState = TypeOfGloveInteraction.Idle;
        }


    }

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