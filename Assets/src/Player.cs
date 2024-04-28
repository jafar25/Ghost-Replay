using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float moveX = Input.GetAxis("Horizontal") * Time.deltaTime * 6;
        float moveZ = Input.GetAxis("Vertical") * Time.deltaTime * 6;
        if (moveX != 0 || moveZ != 0)
        {
            transform.Translate(new Vector3(moveX, 0, moveZ));
        }
    }
}
