using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUIController : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image portraitImage;
    [SerializeField] private TMP_Text dialogueText;

    private bool isVisible;
    [SerializeField] private float typingSpeed;

    private string fullDialogue;
    private string typedDialogue;
    private CanvasGroup canvasGroup;

    private void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }


    private void OnEnable()
    {
        DialogueOM.OnShowDialogue += ShowDialogue;
        DialogueOM.OnNameSet += SetNPCName;
        DialogueOM.OnPortraitSet += SetPortrait;
        DialogueOM.OnDialogueSet += SetDialogue;
    }

    private void OnDisable()
    {
        DialogueOM.OnShowDialogue -= ShowDialogue;
        DialogueOM.OnNameSet -= SetNPCName;
        DialogueOM.OnPortraitSet -= SetPortrait;
        DialogueOM.OnDialogueSet -= SetDialogue;
    }

    private void SetDialogue(string obj)
    {
        fullDialogue = obj;
        typedDialogue = "";
    }

    private void SetPortrait(Sprite obj)
    {
        portraitImage.sprite = obj;
    }

    private void SetNPCName(string obj)
    {
        nameText.text = obj;
    }

    private void ShowDialogue()
    {
        if (!isVisible)
        {
            isVisible = true;
            canvasGroup.alpha = 1;
            StartCoroutine(TypeDialogue());
        }
    }

    private IEnumerator TypeDialogue()
    {
        while(String.CompareOrdinal(typedDialogue, fullDialogue) != 0)
        {
            typedDialogue = fullDialogue.Substring(0, typedDialogue.Length + 1);
            dialogueText.text = typedDialogue;
            yield return new WaitForSeconds(typingSpeed);
        }
        yield return new WaitForSeconds(2f);
        canvasGroup.alpha = 0;
        isVisible = false;
    }
}
