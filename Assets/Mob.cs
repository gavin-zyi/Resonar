using UnityEngine;
using System.Collections;

// ReSharper disable CheckNamespace
public class Mob : MonoBehaviour
{
    private const int FieldOfVision = 180;
    private const int FieldStep = 20;
    private const int FieldRays = FieldOfVision/FieldStep;

    public float Speed;
    public float Radius;

    private Transform target;
    private int wallMask;
    private bool sighted = false;

    private Vector3? lastKnown;

    // Use this for initialization
    protected void Start()
    {
        target = GameObject.Find("Target").transform;
        wallMask = 1 << LayerMask.NameToLayer("Wall");
    }

    // Update is called once per frame
    protected void Update()
    {
        target.Translate(Input.GetAxis("Horizontal")*Time.deltaTime*4, 0, Input.GetAxis("Vertical")*Time.deltaTime*4);
    }

    protected void FixedUpdate()
    {
        var current = target.position;
        var dir = (current - transform.position).normalized;

        RaycastHit see;
        if (Vector3.Angle(transform.forward, dir) > 90f || (!Physics.Raycast(transform.position, dir, out see) || see.transform.gameObject.tag != "Player"))
        {
            if (!lastKnown.HasValue)
            {
                return;
            }

            current = lastKnown.Value;
            dir = (current - transform.position).normalized;
        }
        else
        {
            lastKnown = current;
        }

        Debug.DrawLine(transform.position, current, Color.red);

        if (Vector3.Distance(transform.position, current) < Radius)
        {
            iTween.LookUpdate(gameObject, current, 1f);
            return;
        }
        
        var threat = 0;
        for (int i = 0; i <= FieldOfVision; i += FieldStep)
        {
            var point = (Quaternion.AngleAxis(-i, transform.up)*transform.right);

            RaycastHit hit;
            if (Physics.Raycast(transform.position, point, out hit, Radius, wallMask))
            {
                Debug.DrawLine(transform.position, hit.point, Color.green);
                dir += hit.normal*Mathf.Max(1, Speed/1);
                threat++;
            }
        }

        if (threat == FieldRays)
        {
            iTween.LookUpdate(gameObject, current, 1f);
            return;
        }
        
        var rot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime*Mathf.Max(1, Speed/2));
        transform.position += transform.forward*Time.deltaTime*Speed;
    }
}
