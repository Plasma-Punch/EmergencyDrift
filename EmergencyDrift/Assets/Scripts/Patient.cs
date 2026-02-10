using System;
using System.Collections;
using UnityEngine;

public class Patient : MonoBehaviour
{
    [SerializeField]
    private GameEvent _pickedUp;
    [SerializeField]
    private GameEvent _deSpawned;

    [HideInInspector]
    public float BleedTime;

    private void Start()
    {
        StartCoroutine(BleedOut());
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag != "Player") return;
        _pickedUp.Raise(this, EventArgs.Empty);
        Destroy(gameObject);
    }

    private IEnumerator BleedOut()
    {
        yield return new WaitForSeconds(BleedTime);
        _deSpawned.Raise(this, EventArgs.Empty);
        Destroy(gameObject);
    }
}
