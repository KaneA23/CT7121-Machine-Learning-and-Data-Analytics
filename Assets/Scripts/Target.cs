using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    [HideInInspector] public Spawner spawner;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log(collision.gameObject.tag + collision.gameObject.name);
        if (collision.gameObject.name == "Agent")
        {
            //Debug.Log("Touched Agent");
            //Destroy(gameObject);
            spawner.ResetSpawn();
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        //Debug.Log(collision.gameObject.tag + collision.gameObject.name);
    }

    //private void OnDestroy() {
    //	spawner.ResetSpawn();
    //}
}
