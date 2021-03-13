using System;
using UnityEngine;
using UnityEngine.UI;

public class ScrollButton : MonoBehaviour
{
    private DialogueManager dialogueManager;
    private CharacterChanger changer;
    private Button button;
    private int[] nextBoxIndexs;

    void Start()
    {
        dialogueManager = GameObject.Find("Scripts").GetComponent<DialogueManager>();
        changer = GameObject.Find("Scripts").GetComponent<CharacterChanger>();
        button = GetComponent<Button>();
        nextBoxIndexs = Array.ConvertAll(dialogueManager.GiveNextTextbox().Split('-'), s => int.Parse(s));  //find all possible indexes of next textboxes
    }

    public void nextBox(int buttonNumber)
    {
        changer.SkillGain(dialogueManager.GivesCharacter());
        dialogueManager.DisplayNextBox(nextBoxIndexs[buttonNumber]);                                        //open up box w/ right index
        dialogueManager.SetCurrentIndex(nextBoxIndexs[buttonNumber]);                                       //set that index
        if (button != null) button.interactable = false;                                                    //runs if not an option scroll
        else
        {
            Button[] buttons = GetComponentsInChildren<Button>();                                           //runs if option scroll
            foreach (Button b in buttons) b.interactable = false;
            dialogueManager.MakeSelection();
        }
    }
}
