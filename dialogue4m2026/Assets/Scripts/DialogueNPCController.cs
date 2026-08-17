using System;
using UnityEngine;

public class DialogueNPCController : MonoBehaviour
{
    [SerializeField] private DialogueNPCSO dialogueNpc;
    
    public string NpcName => dialogueNpc.npcName;
    public Sprite NpcPortrait => dialogueNpc.npcPortrait;
    public Color NpcColor => dialogueNpc.npcColor;
    public string[] DialogueLines => dialogueNpc.dialogueLines.ToArray();
    
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
        GetComponent<MeshRenderer>().material.color = NpcColor;
    }

    private void OnValidate()
    {
        GetComponent<MeshRenderer>().material.color = NpcColor;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && !isInteractable)
        {
            InteractOM.OnInteract  += ShowDialogue;
            isInteractable = true;
            InteractOM.PositionInteract(transform.position);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player" && isInteractable)
        {
            InteractOM.OnInteract -= ShowDialogue;
            isInteractable = false;
        }
    }

    private void ShowDialogue()
    {
        SaveSystem.Instance.LoadDataInFile();
        Debug.Log(NpcName+": "+DialogueLines[0]);
        DialogueOM.NameSet(NpcName);
        DialogueOM.PortraitSet(NpcPortrait);
        DialogueOM.DialogueSet(DialogueLines[0]);
        DialogueOM.ShowDialogue();
    }
}
