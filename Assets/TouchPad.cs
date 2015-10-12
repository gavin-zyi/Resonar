using System;
using UnityEngine;
using System.Collections;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

public class TouchPad : MonoBehaviour
{
    public Transform Player;
    public Texture CoverTexture;
    public Texture KnobTexture;

    private int currentFinger;
    private GUITexture cover;
    private Rect neutral;
    private Rect center;
    private int totalSize;
    private float knobRadius;

    private Vector2 prev;

    private Hero hero;
    private Vector2 drag;

    // Use this for initialization
    protected void Start()
    {
        hero = Player.GetComponent<Hero>();
        totalSize = (int) (Screen.height*0.3f);
        CreateCover();
        CreateKnob();
        ResetState();
    }

    private void CreateCover()
    {
        var obj = new GameObject("Cover", typeof (GUITexture));
        obj.transform.parent = transform;

        cover = obj.GetComponent<GUITexture>();
        cover.texture = CoverTexture;
        var inset = new Rect(-totalSize*0.5f, -totalSize*0.5f, totalSize, totalSize);

        inset.x += transform.position.x*Screen.width;
        inset.y += transform.position.y*Screen.height;

        cover.pixelInset = inset;
    }

    private void CreateKnob()
    {
        gameObject.AddComponent<GUITexture>();
        var gui = gameObject.GetComponent<GUITexture>();
        gui.texture = KnobTexture;

        knobRadius = totalSize*0.25f;
        gui.pixelInset = new Rect(-knobRadius, -knobRadius, totalSize*0.5f, totalSize*0.5f);

        neutral = GetComponent<GUITexture>().pixelInset;
        neutral.x += transform.position.x*Screen.width;
        neutral.y += transform.position.y*Screen.height;

        center = new Rect
            {
                x = neutral.x + transform.position.x*Screen.width + neutral.width*0.5f,
                y = neutral.y + transform.position.y*Screen.height + neutral.height*0.5f,
            };

    }

    private void ResetState()
    {
        currentFinger = -1;
        GetComponent<GUITexture>().pixelInset = neutral;
        prev = Vector2.zero;
        drag = Vector2.zero;
    }

    // Update is called once per frame
    protected void Update()
    {
        UpdateTouch();

        Player.GetComponent<Rigidbody>().velocity = Player.forward*Hero.Move*drag.y;
        Player.RotateAround(Player.up, Hero.Angle*drag.x);
        hero.TiltCamera(drag.y);
    }

    private void UpdateTouch()
    {
        for (var i = 0; i < Input.touchCount; i++)
        {
            var touch = Input.GetTouch(i);

            if (currentFinger == -1 && cover.HitTest(touch.position))
            {
                currentFinger = touch.fingerId;
                OnTouchStart(touch);
                break;
            }

            if (currentFinger != touch.fingerId)
            {
                continue;
            }

            switch (touch.phase)
            {
                case TouchPhase.Moved:
                    OnTouchMove(touch);
                    break;
                case TouchPhase.Ended:
                    OnTouchEnd(touch);
                    currentFinger = -1;
                    break;
            }
            break;
        }
    }

    private void OnTouchStart(Touch touch)
    {
        if (!cover.GetComponent<GUITexture>().HitTest(touch.position))
        {
            return;
        }

        PushKnob(touch);
    }

    private void OnTouchMove(Touch touch)
    {
        PushKnob(touch);
    }

    private void PushKnob(Touch touch)
    {
        var diff = touch.position - (prev == Vector2.zero ? touch.position : prev);

        if (Mathf.Abs(diff.x - 0) > Mathf.Epsilon || Mathf.Abs(diff.y - 0) > Mathf.Epsilon)
        {
            var dx = touch.position.x - center.x;
            var dy = touch.position.y - center.y;

            var distance = Mathf.Sqrt(dx*dx + dy*dy);
            var angle = Mathf.Atan2(dy, dx);

            if (distance > knobRadius)
            {
                dx = Mathf.Cos(angle)*knobRadius;
                dy = Mathf.Sin(angle)*knobRadius;
            }

            var inset = GetComponent<GUITexture>().pixelInset;
            inset.x = dx + neutral.x;
            inset.y = dy + neutral.y;
            GetComponent<GUITexture>().pixelInset = inset;

            drag.Set(dx/knobRadius, dy/knobRadius);
        }

        prev = touch.position;
    }

    private void OnTouchEnd(Touch touch)
    {
        ResetState();
    }
}