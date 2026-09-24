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
        if (other.CompareTag("Player") && !isInteractable)
        {
            InteractOM.OnInteract += AbrirPorta;
            isInteractable = true;
            InteractOM.PositionInteract(transform.position);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && isInteractable)
        {
            InteractOM.OnInteract -= AbrirPorta;
            isInteractable = false;
        }
    }

    private void AbrirPorta()
    {
        if (!isOpen)
        {
            // Salva a abertura de porta no Autosave (Slot 0)
            if (SaveSystem.Instance != null)
            {
                SaveData data = SaveSystem.Instance.GetSaveData(0) ?? new SaveData();
                SaveSystem.Instance.SetSaveData(data, 0);
                SaveSystem.Instance.SaveDataInFile(0);
            }

            if (anim != null)
            {
                anim.StopPlayback();
                anim.Play("PortaAbrindo");
            }
            isOpen = true;
        }
        else
        {
            if (anim != null)
            {
                anim.StopPlayback();
                anim.Play("PortaFechando");
            }
            isOpen = false;
        }
    }
}