using UnityEngine;
using System;

public class DialogueManager : MonoBehaviour
{
    private string[] textBoxes;
    private bool[] whoSays;
    private bool[] isOptionScroll;
    private bool[] isYelling;
    private string[] whatComesNext;
    private string[] givesCharacter;
    private int currentIndex;

    public ScrollInstantiate2 scrolls;

    void Start()
    {
        textBoxes = new string[30];
        whoSays = new bool[30];
        isOptionScroll = new bool[30];
        isYelling = new bool[30];
        whatComesNext = new string[30];
        givesCharacter = new string[30];
        currentIndex = 0;
    }

    //get all convo infomation from NPC character and input into scrolls
    public void StartDialogue(TextBoxDialogue[] dialogue)
    {
        Array.Clear(textBoxes, 0, textBoxes.Length);
        Array.Clear(whoSays, 0, whoSays.Length);
        Array.Clear(isOptionScroll, 0, whoSays.Length);
        Array.Clear(isYelling, 0, whoSays.Length);
        Array.Clear(whatComesNext, 0, whoSays.Length);
        Array.Clear(givesCharacter, 0, whoSays.Length);

        for (int i = 0; i < dialogue.Length; i++)
        {
            whoSays[i] = dialogue[i].thisNPCSays;
            isOptionScroll[i] = dialogue[i].optionsBox;
            isYelling[i] = dialogue[i].isYelling;
            textBoxes[i] = dialogue[i].textBox;
            whatComesNext[i] = dialogue[i].connectsToTextBoxes;
            givesCharacter[i] = dialogue[i].givesCharacter;
        }
        currentIndex = 0;
        DisplayNextBox(currentIndex);
    }

    //called from scroll button
    public void DisplayNextBox(int index)
    {
        if (index == 100) //ends convo
        {
            EndDialogue();
            return;
        }

        scrolls.MakeScroll(isOptionScroll[index], whoSays[index], isYelling[index], textBoxes[index]);
    }

    //called from scroll button if whatComesNext = 100
    public void EndDialogue()
    {
        StartCoroutine(scrolls.ResetConversation());
    }

    //called from scroll button
    public string GiveNextTextbox()
    {
        return whatComesNext[currentIndex];
    }

    //gives next scroll's index
    public void SetCurrentIndex(int newIndex)
    {
        currentIndex = newIndex;
    }
    public string GivesCharacter()
    {
        return givesCharacter[currentIndex];
    }
    //called from ScrollInstantiate2 when user makes a selection
    public void MakeSelection()
    {
        StartCoroutine(scrolls.SelectOption());
    }
}