using System;
using UnityEngine;

public static class DialogueOM
{
    public static event Action<string> OnNameSet;

    public static void NameSet(string name)
    {
        OnNameSet?.Invoke(name);
    }

    public static event Action<Sprite> OnPortraitSet;

    public static void PortraitSet(Sprite portrait)
    {
        OnPortraitSet?.Invoke(portrait);
    }
    
    public static event Action<string> OnDialogueSet;

    public static void DialogueSet(string dialogue)
    {
        OnDialogueSet?.Invoke(dialogue);
    }
    
    public static event Action OnShowDialogue;
    
    public static void ShowDialogue()
    {
        OnShowDialogue?.Invoke();
    }
}
