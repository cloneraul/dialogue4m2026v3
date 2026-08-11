using System;
using UnityEngine;

public class PortaController : MonoBehaviour
{
    private Animator anim;
    private bool isOpen;
    private bool _isInteractable;
    private bool isInteractable
    {
        get => _isInteractable;
        set
        {
            _isInteractable = value;
            InteractOM.Interactable(_isInteractable);
        }
    }

    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && !isInteractable)
        {
            InteractOM.OnInteract += AbrirPorta;
            isInteractable = true;
           // InteractOM.Interactable(isInteractable);
            InteractOM.PositionInteract(transform.position);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player" && isInteractable)
        {
            InteractOM.OnInteract -= AbrirPorta;
            isInteractable = false;
           // InteractOM.Interactable(isInteractable);
        }
    }

    private void AbrirPorta()
    {
        if (!isOpen)
        {
            SaveSystem.Instance.SetPlayerLevel(2);
            SaveSystem.Instance.SaveDataInFile(0);
            anim.StopPlayback();
            anim.Play("PortaAbrindo");
            isOpen = true;
        }
        else
        {
            anim.StopPlayback();
            anim.Play("PortaFechando");
            isOpen = false;
        }
    }
}
