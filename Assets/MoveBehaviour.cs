using UnityEngine;
using System.Collections;

public class MoveBehaviour : MonoBehaviour
{
    public void Move(Vector3 velocity)
    {
        transform.position += velocity;
    }

}
