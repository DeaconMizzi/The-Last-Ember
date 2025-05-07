using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueNode", menuName = "Dialogue/Node")]
public class DialogueNode : ScriptableObject
{
   public string speakerName;
    [TextArea] public string dialogueText;
    public Sprite portrait;

    public List<DialogueChoice> choices;
    public DialogueNode nextAutoNode;
    
}
