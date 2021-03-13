using UnityEngine;

[System.Serializable]
public class TextBoxDialogue
{
    //All the criteria of a scroll. An array of these is given to every NPC to store their converstation information
    [Tooltip("true if this is the NPC's text box, false it it is said by the character")]
    public bool thisNPCSays;
    [Tooltip("true if this is the character's box and there are multiple options to answer")]
    public bool optionsBox;
    [Tooltip("true if this textbox is in large font with screenshake")]
    public bool isYelling;
    [Tooltip("if not an options textbox, should be only 1 integer (0 -> # of boxes). If options, separate integers with '-' in order of text options")]
    public string connectsToTextBoxes;
    [Tooltip("what the box will say. If multiple options, separate with a '-'")]
    [TextArea(3,10)] public string textBox;
    [Tooltip("does this textbox give the character something or change his skills?")]
    public string givesCharacter;
}
