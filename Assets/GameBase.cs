using UnityEngine;
using System.Collections;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
public abstract class GameBase : MonoBehaviour
{
    protected abstract void Start();

    protected abstract void Update();
}