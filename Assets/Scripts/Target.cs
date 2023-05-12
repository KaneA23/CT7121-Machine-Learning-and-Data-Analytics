using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    [HideInInspector] public Spawner spawner;

    public Vector3 TargetUpVector
    {
        get
        {
            return GetComponent<BoxCollider>().transform.up;
        }
    }

    public Vector3 TargetCenterPosition
    {
        get
        {
            return GetComponent<BoxCollider>().transform.position;
        }
    }

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
        if (collision.gameObject.CompareTag("Obstacles"))
        {
            transform.localPosition = new Vector3(Random.Range(-10f, 10f), 1.5f, Random.Range(-10f, 10f));
        }
    }

    //private void OnDestroy() {
    //	spawner.ResetSpawn();
    //}
}
