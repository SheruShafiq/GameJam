using UnityEngine;

public class KYS : MonoBehaviour
{
    void Start()
    {
        // Schedule the object to be destroyed after 2 seconds
        Destroy(gameObject, 3f);
    }
}
