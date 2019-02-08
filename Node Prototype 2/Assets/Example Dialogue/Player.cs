using System.Collections;
using System.Collections.Generic;
using RPG.Example;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float _maxDistance = 5;

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * _maxDistance);
    }

    // Update is called once per frame
    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.E)) return;

        RaycastHit hitInfo;
        if (!Physics.Raycast(transform.position, transform.forward, out hitInfo, _maxDistance)) return;
        var convo = hitInfo.collider.GetComponent<ExampleConversationStarter>();
        if (convo != null) convo.StartConversation();
    }
}
