using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueNPC", menuName = "Dialogue/Data/New Dialogue NPC")]
public class DialogueNPCSO : ScriptableObject
{
    public string npcName;
    public Sprite npcPortrait;
    public Color npcColor;
    public List<string> dialogueLines;

}
