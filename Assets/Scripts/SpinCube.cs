using UnityEngine;

public class SpinCube : MonoBehaviour
{

    private void Update()
    {
        gameObject.transform.Rotate(Vector3.one * 100 * Time.deltaTime);
    }

}
