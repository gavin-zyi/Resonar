using System;
using System.Linq;
using UnityEngine;
using System.Collections;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
public class Hero : GameBase
{
    public const float Angle = Mathf.PI*0.02f;
    public const float Move = 16f;

    public float MaxSpeed = 20;
    public float JumpSpeed = 5;

    private int state;

    private int monsterMask;

    private bool Grounded
    {
        get { return state > 0; }
    }

    protected Transform View { get; set; }
    protected Transform Weapon { get; set; }

    private LifeBar life;
    
    protected void Awake()
    {
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
        View = transform.Find("Camera");
        Weapon = transform.Find("Weapon");
    }

    // This part detects whether or not the object is grounded and stores it in a variable
    protected void OnCollisionEnter(Collision collision)
    {
        state++;
    }


    protected void OnCollisionExit(Collision collision)
    {
        state = Mathf.Max(0, state - 1);
    }

    protected override void Start()
    {
        //Physics.gravity = new Vector3(0, -200, 0);
        GetComponent<Rigidbody>().drag = 20;

        monsterMask = 1 << LayerMask.NameToLayer("Monster");
        life = GameObject.Find("Life").GetComponent<LifeBar>();

        StartCoroutine(SwingAttack());

    }

    protected override void Update()
    {
    }

    public void Damage(float amount)
    {
        life.Damage(amount);
    }

    protected void OnGUI()
    {
    }

    protected void FixedUpdate()
    {
        if (!Grounded)
        {
            return;
        }

        if (Input.GetKey(KeyCode.W))
        {
            GetComponent<Rigidbody>().velocity = transform.forward*Move;
            TiltCamera(1);
        }

        if (Input.GetKey(KeyCode.A))
        {
            transform.RotateAround(transform.up, -Angle);
        }

        if (Input.GetKey(KeyCode.S))
        {
            GetComponent<Rigidbody>().velocity = transform.forward*-Move;
            View.RotateAround(transform.right, Mathf.Sin(Time.time * 10f) * 0.001f);
        }

        if (Input.GetKey(KeyCode.D))
        {
            transform.RotateAround(transform.up, Angle);
        }
    }

    public void TiltCamera(float mod)
    {
        View.RotateAround(transform.right, Mathf.Sin(Time.time*10f)*0.001f*mod);
    }
    
    private IEnumerator SwingAttack()
    {
        while (true)
        {
            if (Input.GetKey(KeyCode.Return))
            {
                const float length = 0.1f;
                var side = new Vector3(-0.23f, 0, 0);
                var swing = iTween.Hash("amount", side, "islocal", true, "time", length);
                var slash = iTween.Hash("rotation", new Vector3(0, -90f, 0), "islocal", true, "time", length);
                iTween.MoveAdd(Weapon.Find("Translate").gameObject, swing);
                iTween.RotateTo(Weapon.Find("Translate/Rotate").gameObject, slash);
                yield return new WaitForSeconds(length);

                var hits = Physics.SphereCastAll(transform.position, 1f, transform.forward, 4f, monsterMask);

                foreach (var target in from hit in hits let dir = (hit.transform.position - transform.position).normalized where Vector3.Angle(transform.forward, dir) <= 30f select hit.transform.gameObject.GetComponent<Monster>())
                {
                    target.Damage(2f);
                }

                swing["amount"] = -side;
                slash["rotation"] = Vector3.zero;
                iTween.MoveAdd(Weapon.Find("Translate").gameObject, swing);
                iTween.RotateTo(Weapon.Find("Translate/Rotate").gameObject, slash);
                yield return new WaitForSeconds(length);
            }

            yield return 0;
        }
    }
}