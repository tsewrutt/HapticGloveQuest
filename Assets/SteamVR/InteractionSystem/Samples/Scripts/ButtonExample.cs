using UnityEngine;
using System.Collections;
using UnityEngine.UI;
namespace Valve.VR.InteractionSystem.Sample
{
    public class ButtonExample : MonoBehaviour
    {
        public HoverButton hoverButton;

        public GameObject prefab;

        private GameObject[] ballArray;
        private int maxObj = 5;
        private int oldIndex = 0;

        public Text pressText;
        private int pressInt = 0;

        private MeshRenderer meshRenderer;
        private void Start()
        {
            hoverButton.onButtonDown.AddListener(OnButtonDown);
            ballArray = new GameObject[maxObj];
            pressText.text = "Button Pressed:\n" + pressInt + " times";
        }

        private void OnButtonDown(Hand hand)
        {
            StartCoroutine(DoPlant());
        }

        private IEnumerator DoPlant()
        {
            GameObject planting = GameObject.Instantiate<GameObject>(prefab);
            planting.transform.position = this.transform.position;
            //planting.transform.rotation = Quaternion.Euler(0, Random.value * 360f, 0);
            
            UpdateText();


            meshRenderer = planting.GetComponent<MeshRenderer>();
            Material mat = meshRenderer.materials[Random.Range(0, 3)];
            meshRenderer.material = mat;
            Debug.Log(mat + " " + meshRenderer.materials.Length);


            ballArray[oldIndex] = planting;

            //increment the oldest index, wrap around once reached max
            oldIndex = (oldIndex + 1) % maxObj;

            Rigidbody rigidbody = planting.GetComponent<Rigidbody>();
            if (rigidbody != null)
                rigidbody.isKinematic = true;

            float startTime = Time.time;
            float overTime = 0.5f;
            float endTime = startTime + overTime;

            while (Time.time < endTime)
            {
                //planting.transform.localScale = Vector3.Slerp(initialScale, targetScale, (Time.time - startTime) / overTime);
                yield return null;
            }


            if (rigidbody != null)
                rigidbody.isKinematic = false;
        }

        private void UpdateText()
        {
            pressInt++;
            pressText.text = "Button Pressed:\n" + pressInt + " times";
        }
    }
}