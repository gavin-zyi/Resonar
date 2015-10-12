using UnityEngine;
using System.Collections;

// ReSharper disable CheckNamespace
public class Bomb : GameBase
{
    protected override void Start()
    {
        
    }

    protected override void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Explode();
        }
    }

    private void Explode()
    {

        for (var i = 0; i < 200; i++)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.localScale = Vector3.one*0.1f;
            cube.transform.position = transform.position;
            cube.AddComponent<Rigidbody>();
            var dir = Random.insideUnitSphere*200f;
            dir.y = Random.Range(1, 200);
            cube.GetComponent<Rigidbody>().AddForce(dir);
            cube.GetComponent<Rigidbody>().AddTorque(Random.insideUnitSphere * 200f);
            cube.layer = LayerMask.NameToLayer("Bodies");
            Destroy(cube, 5f);
        }

        Destroy(gameObject);


    }

    protected void OnCollisionEnter(Collision collision)
    {
        var p = GameObject.Find("Player");
        //p.rigidbody.AddForce(-p.rigidbody.velocity.normalized * 10, ForceMode.Force);
        //Destroy(gameObject);
    }
}