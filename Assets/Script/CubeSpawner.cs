using UnityEngine;

public class CubeSpawner : MonoBehaviour
{
    // Reference to the cube objects
    public GameObject[] cubes;

    // Array to store original transforms of cubes
    private Transform[] originalTransforms;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize the array to store original transforms
        originalTransforms = new Transform[cubes.Length];

        // Save the original transform of each cube
        for (int i = 0; i < cubes.Length; i++)
        {
            originalTransforms[i] = cubes[i].transform;
        }
    }

    // OnCollisionEnter is called when collision occurs
    private void OnCollisionEnter(Collision collision)
    {
        // Check if collision involves the plane
        //if (collision.gameObject.CompareTag("InteractingObject"))
        //{
            // Reset the position of each cube to its original transform
            for (int i = 0; i < cubes.Length; i++)
            {
                cubes[i].transform.position = originalTransforms[i].position;
                cubes[i].transform.rotation = originalTransforms[i].rotation;
            }
        //}
    }
}