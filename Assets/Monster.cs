using System;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

// ReSharper disable CheckNamespace
// ReSharper disable FunctionNeverReturns
public class Monster : GameBase
{
    private enum State
    {
        Roam,
        Chase,
        Search,
        Attack
    }

    private State state;

    private const int FieldOfVision = 180;
    private const int FieldStep = 20;
    private const int FieldRays = FieldOfVision / FieldStep;
    
    public float Speed = 8f;
    public float Radius = 2f;

    private Transform target;
    private int wallMask;
    private int monsterMask;
    private int playerMask;
    private int navMask;
    private int lookMask;
    private int playerLayer;

    private Vector3? lastKnown;

    private Layout layout;

    private const int Height = 2;

    private const float DamageThreshold = 1;
    private float damageTimer;

    private const float MaxHealth = 10;
    private float health;
    
    private Vector2 GetRandomNear()
    {
        var cur = Layout.WorldToGrid(transform.position);
        var right = new Vector2(1, 0);
        var down = new Vector2(0, 1);

        while (true)
        {
            switch (Random.Range(0, 4))
            {
                case 0:
                    if (layout.Get(cur - right) != null &&
                        (layout.Get(cur - right).Type & Layout.MapTileType.Floor) == Layout.MapTileType.Floor)
                    {
                        return cur - right;
                    }
                    break;
                case 1:
                    if (layout.Get(cur + right) != null &&
                        (layout.Get(cur + right).Type & Layout.MapTileType.Floor) == Layout.MapTileType.Floor)
                    {
                        return cur + right;
                    }
                    break;
                case 2:
                    if (layout.Get(cur - down) != null &&
                        (layout.Get(cur - down).Type & Layout.MapTileType.Floor) == Layout.MapTileType.Floor)
                    {
                        return cur - down;
                    }
                    break;
                case 3:
                    if (layout.Get(cur + down) != null &&
                        (layout.Get(cur + down).Type & Layout.MapTileType.Floor) == Layout.MapTileType.Floor)
                    {
                        return cur + down;
                    }
                    break;
            }

        }
    }

    protected override void Start()
    {
        //rigidbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
        layout = GameObject.Find("Player").GetComponent<Layout>();

        target = GameObject.Find("Player").transform;
        wallMask = 1 << LayerMask.NameToLayer("Wall");
        monsterMask = 1 << LayerMask.NameToLayer("Monster");
        playerMask = 1 << LayerMask.NameToLayer("Player");
        navMask = wallMask | monsterMask;
        lookMask = playerMask | wallMask;

        playerLayer = LayerMask.NameToLayer("Player");

        health = MaxHealth;
    }

    protected override void Update()
    {

    }

    private bool IsPlayerInVision()
    {
        var dir = (target.position - transform.position).normalized;
        RaycastHit see;
        return Vector3.Angle(transform.forward, dir) <= 90f &&
               Physics.Raycast(transform.position, dir, out see, Mathf.Infinity, lookMask) && 
               see.transform.gameObject.layer == playerLayer;
    }

    protected void FixedUpdate()
    {
        switch (state)
        {
            case State.Roam:
                {
                    if (IsPlayerInVision())
                    {
                        state = State.Chase;
                        return;
                    }

                    if (!lastKnown.HasValue || Vector3.Distance(transform.position, lastKnown.Value) < 0.1f)
                    {
                        var set = Layout.GridToWorld(GetRandomNear());
                        set.y = Height;
                        lastKnown = set;
                    }

//                    iTween.LookUpdate(gameObject, lastKnown.Value, 1f);
//                    transform.position += transform.forward*Time.deltaTime*Speed*0.5f;
                    if ((Navigate(lastKnown.Value, Speed * 0.5f) & monsterMask) == monsterMask)
                    {
                        var set = Layout.GridToWorld(GetRandomNear());
                        set.y = Height;
                        lastKnown = set;
                    }

                }
                break;
            case State.Chase:
                {
                    if (!IsPlayerInVision())
                    {
                        state = State.Search;
                        return;
                    }

                    Debug.DrawLine(transform.position, target.position, Color.red);

                    if (Vector3.Distance(transform.position, target.position) < Radius*2f)
                    {
                        state = State.Attack;
                        return;
                    }

                    Navigate(target.position, Speed);
                    lastKnown = target.position;
                }
                break;
            case State.Search:
                {
                    if (IsPlayerInVision())
                    {
                        state = State.Chase;
                        return;
                    }

                    if (!lastKnown.HasValue || Vector3.Distance(transform.position, lastKnown.Value) < Radius)
                    {
                        lastKnown = null;
                        state = State.Roam;
                        return;
                    }

                    Debug.DrawLine(transform.position, lastKnown.Value, Color.white);

                    Navigate(lastKnown.Value, Speed);
                }
                break;
            case State.Attack:
                {
                    if (Vector3.Distance(transform.position, target.position) > Radius*2f)
                    {
                        state = State.Chase;
                        lastKnown = target.position;
                        damageTimer = 0;
                    }

                    iTween.LookUpdate(gameObject, target.position, 1f);
                    
                    if ((damageTimer += Time.deltaTime) >= DamageThreshold)
                    {
                        target.GetComponent<Hero>().Damage(3f);
                        damageTimer = 0;
                    }
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private int Navigate(Vector3 point, float speed)
    {
        var mask = 0;
        var dir = (point - transform.position).normalized;

        var threat = 0;
        for (var i = 0; i <= FieldOfVision; i += FieldStep)
        {
            var test = Quaternion.AngleAxis(-i, transform.up)*transform.right;

            RaycastHit hit;
            if (!Physics.Raycast(transform.position, test, out hit, Radius, navMask)) continue;

            Debug.DrawLine(transform.position, hit.point, Color.green);
            dir += hit.normal*Mathf.Max(1, speed/1);
            threat++;
            mask |= (1 << hit.transform.gameObject.layer);
        }

        if (threat == FieldRays)
        {
            iTween.LookUpdate(gameObject, point, 1f);
            return mask;
        }

        var rot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime*speed);

        transform.position += transform.forward*Time.deltaTime*speed;

        return mask;
    }
    
    protected void OnCollisionEnter(Collision collision)
    {
    }


    protected void OnCollisionExit(Collision collision)
    {
    }

    private void Explode()
    {
        for (var i = 0; i < 200; i++)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.localScale = Vector3.one * 0.1f;
            cube.transform.position = transform.position;
            cube.AddComponent<Rigidbody>();
            var dir = Random.insideUnitSphere * 600f;
            dir.y = Random.Range(1, 200);
            cube.GetComponent<Rigidbody>().AddForce(dir);
            cube.GetComponent<Rigidbody>().AddTorque(Random.insideUnitSphere * 200f);
            cube.layer = LayerMask.NameToLayer("Bodies");
            Destroy(cube, 5f);
        }

        Destroy(gameObject);
    }

    public void Damage(float amount)
    {
        if ((health -= amount) <= 0)
        {
            //Explode();
            Destroy(gameObject);
        }

        StartCoroutine(DamageFlash());
    }

    private IEnumerator DamageFlash()
    {
        GetComponent<Renderer>().material.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        GetComponent<Renderer>().material.color = Color.white;
    }
}