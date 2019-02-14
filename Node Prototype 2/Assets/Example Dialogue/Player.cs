using System.Collections;
using System.Collections.Generic;
using RPG.Example;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

public class Player : MonoBehaviour
{
    private bool _disabledInput = false;
    [SerializeField] private float _maxDistance = 5;

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * _maxDistance);
    }

    // Update is called once per frame
    private void Update()
    {
        if (_disabledInput || !Input.GetKeyDown(KeyCode.E)) return;

        RaycastHit hitInfo;
        if (!Physics.Raycast(transform.position, transform.forward, out hitInfo, _maxDistance)) return;
        var convo = hitInfo.collider.GetComponent<ExampleConversationStarter>();
        if (convo != null) convo.StartConversation();
    }

    private void OnEnable()
    {
        ExampleDialogueHandler.OnDialogueStart += DisableInput;
        ExampleDialogueHandler.OnDialogueEnd += EnableInput;
    }

    private void OnDisable()
    {
        ExampleDialogueHandler.OnDialogueStart -= DisableInput;
        ExampleDialogueHandler.OnDialogueEnd -= EnableInput;
    }

    private void EnableInput()
    {
        _disabledInput = false;
        var tpuc = GetComponentInParent<ThirdPersonUserControl>();
        if (tpuc != null) tpuc.DisabledInput = false;
    }

    private void DisableInput()
    {
        _disabledInput = true;
        var tpuc = GetComponentInParent<ThirdPersonUserControl>();
        if (tpuc != null) tpuc.DisabledInput = true;
    }
}
