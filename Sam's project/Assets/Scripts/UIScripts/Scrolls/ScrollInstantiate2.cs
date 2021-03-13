using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScrollInstantiate2 : MonoBehaviour
{
    public GameObject scroll;
    public GameObject optionsScroll;

    public float textTypeSpeed;
    public float fadeInSpeed;
    [Range (0.0f, 1.0f)] public float originalOffset;
    [Range (0.0f, 1.0f)] public float inBetweenOffset;
    [Range (0.0f, 1.0f)] public float bottomBoundary;
    public float unfurlSpeed;
    public float moveInSpeed;
    public float moveOutSpeed;
    public float moveOutUpAmount;
    public float moveHolderSpeed;

    [Range(2, 4)] public int yellingSizeMult;
    [Range(3, 6)] public int goalLineCount;
    [Range(10, 30)] public int minCharFor2Lines;
    [Range(40, 80)] public int maxCharactersAcross;

    private RectTransform[] textAreas = new RectTransform[7];
    private GameObject[] scrolls = new GameObject[7];
    private RectTransform thisRecTranform;
    private Vector3 screenMidpoint;

    private int pixelsPerCharLR;
    private int pixelsPerCharUD;

    private string lastTextBox;
    private int currentScrollIndex;
    private float currentScrollOffset;
    private bool isMovingWholeHolder;
    private bool isUnfurling;

    //-------------------------------------------------------------------------------------------------------

    private void Start()
    {
        screenMidpoint = GetComponentInParent<RectTransform>().position;
        thisRecTranform = GetComponent<RectTransform>();
        thisRecTranform.sizeDelta = screenMidpoint * 2;
        thisRecTranform.position = screenMidpoint;
        thisRecTranform.localScale = Vector3.one;

        currentScrollOffset = originalOffset * screenMidpoint.y * 2;
        currentScrollIndex = -1;
        isMovingWholeHolder = false;

        if (screenMidpoint.y < 300) { pixelsPerCharUD = 24; pixelsPerCharLR = 10; }
        else if (screenMidpoint.y < 600) { pixelsPerCharUD = 48; pixelsPerCharLR = 19; }
        else { pixelsPerCharUD = 72; pixelsPerCharLR = 29; }
    }

    //TODO: add yelling functionality
    public void MakeScroll(bool inputScroll, bool thisNPCSays, bool yelling, string scrollText)
    {
        int charactersAcross;
        int linesDown;
        string printLine;
        Vector3 posInHolder;

        charactersAcross = 1;
        linesDown = 1;
        printLine = "";
        posInHolder = screenMidpoint;

        if (!inputScroll)
        {
            //determine how wide the scroll needs to be
            if (scrollText.Length < minCharFor2Lines) charactersAcross = scrollText.Length;
            else if (scrollText.Length < minCharFor2Lines * goalLineCount) charactersAcross = minCharFor2Lines;
            else charactersAcross = Mathf.Min((scrollText.Length / goalLineCount), maxCharactersAcross);

            //determine how tall the scroll needs to be
            int lineLengthRemaining = charactersAcross;
            string[] words = scrollText.Split(' ');
            foreach (string word in words)
            {
                if ((lineLengthRemaining - word.Length) > 1)         //if the word and a space won't exceed the space in the line
                {
                    lineLengthRemaining -= word.Length + 1;         //detract the length from the tracker
                    printLine += word + " ";                        //and add the word and a space to the string
                }
                else if ((lineLengthRemaining - word.Length) > 0)   //if just the word will fit
                {
                    lineLengthRemaining -= word.Length;             //detract the length from the tracker
                    printLine += word;                              //and add just the word to the string
                }
                else                                                //otherwise the word would exceed the bounds of the line
                {
                    linesDown++;                                        //in which case add another line
                    for (int i = 0; i < lineLengthRemaining; i++)
                    {
                        printLine += " ";                           //add enough spaces to fill out the line
                    }
                    printLine += word + " ";                        //add the word onto that new line
                    lineLengthRemaining = charactersAcross - word.Length - 1; //and reset the lineLengthRemaining
                }
            }
        }
        else   //options scroll
        {
            printLine = scrollText;

            string[] options = scrollText.Split('-');

            foreach (string option in options) if (option.Length > charactersAcross) charactersAcross = option.Length;  //how wide
            charactersAcross += 3;                                                                                      //add numbers
            linesDown = options.Length;                                                                                 //how tall
        }

        //if the previous scroll hasn't finished being typed out and the user taps, finish it before adding a new scroll
        if (currentScrollIndex > -1 && scrolls[currentScrollIndex].name != "OptionsScroll(Clone)") StartCoroutine(TypeSentence(lastTextBox, true));
        StopAllCoroutines();

        //make room in memory for adding a new scroll if necessary
        if (currentScrollIndex >= scrolls.Length - 1) DestroyFirstBox();

        //now begin to create the new scroll
        currentScrollIndex++;

        //give the new scroll the right position
        posInHolder += Vector3.right * (thisNPCSays ? -1 : 1) * (screenMidpoint.x - (charactersAcross * pixelsPerCharLR) / 2);
        posInHolder += Vector3.down * currentScrollOffset;

        //actually instantiate the new scroll
        if (!inputScroll) scrolls[currentScrollIndex] = Instantiate(scroll, transform, false);
        else scrolls[currentScrollIndex] = Instantiate(optionsScroll, transform, false);
        scrolls[currentScrollIndex].transform.position = posInHolder + thisRecTranform.position - Vector3.right * screenMidpoint.x;  //place the scroll
        textAreas[currentScrollIndex] = scrolls[currentScrollIndex].GetComponent<RectTransform>();
        textAreas[currentScrollIndex].sizeDelta = new Vector2(charactersAcross * pixelsPerCharLR, 0.0f);                    //start furled
        AreButtonsEnabled(false);                                                                                           //keep inactive until ready

        //move it onto the screen
        StartCoroutine(MoveIn(charactersAcross * pixelsPerCharLR, thisNPCSays));
        StartCoroutine(Unfurl(linesDown * pixelsPerCharUD, printLine));

        //move the whole holder up if this is going to be too big
        float makeRoom = currentScrollOffset + linesDown * pixelsPerCharUD + screenMidpoint.y * 2 * inBetweenOffset - thisRecTranform.position.y + screenMidpoint.y;
        if ((makeRoom) > bottomBoundary * screenMidpoint.y * 2 && !isMovingWholeHolder) StartCoroutine(MoveUpScreen(makeRoom));
    }

    //-------------------------------------------------------------------------------------------------------

    //coroutine for moving the conversation feed up
    IEnumerator MoveUpScreen(float startingPoint)
    {
        isMovingWholeHolder = true;

        while (startingPoint > bottomBoundary * screenMidpoint.y * 2)
        {
            thisRecTranform.position += Vector3.up * moveHolderSpeed;
            startingPoint -= moveHolderSpeed;
            yield return null;
        }

        isMovingWholeHolder = false;
        if (!isUnfurling) AreButtonsEnabled(true);
    }

    //coroutine for opening a scroll before showing the text
    IEnumerator Unfurl(float goalHeight, string printLine)
    {
        float unfurlStep = Mathf.Sqrt(goalHeight) * unfurlSpeed;                //go faster for large scrolls
        if (scrolls[currentScrollIndex].name != "OptionsScroll(Clone)") currentScrollOffset += inBetweenOffset * screenMidpoint.y * 2;
        isUnfurling = true;

        for (float h = 0; h < goalHeight; h += unfurlStep)
        {
            textAreas[currentScrollIndex].sizeDelta += Vector2.up * unfurlStep;
            if (scrolls[currentScrollIndex].name != "OptionsScroll(Clone)") currentScrollOffset += unfurlStep;

            yield return null;
        }

        isUnfurling = false;
        if (!isMovingWholeHolder) AreButtonsEnabled(true);
        if (scrolls[currentScrollIndex].name == "Scroll(Clone)") StartCoroutine(TypeSentence(printLine, false));
        else StartCoroutine(FadeIn(printLine));
    }

    //coroutine for moving the scroll onto the screen
    IEnumerator MoveIn(float size, bool left)
    {
        float moveInStep = Mathf.Sqrt(size) * moveInSpeed;
        float initialOffset = size + 20;

        scrolls[currentScrollIndex].transform.position += Vector3.right * (left ? -1 : 1) * initialOffset;

        for (float x = initialOffset; x > moveInStep; x -= moveInStep)
        {
            scrolls[currentScrollIndex].transform.position += Vector3.right * moveInStep * (left ? 1 : -1);
            yield return null;
        }
        scrolls[currentScrollIndex].transform.position = new Vector3(left ? textAreas[currentScrollIndex].sizeDelta.x/2 :
            2*screenMidpoint.x - textAreas[currentScrollIndex].sizeDelta.x/2, scrolls[currentScrollIndex].transform.position.y,
            scrolls[currentScrollIndex].transform.position.z);
    }

    //coroutines for displaying text, FadeIn for options scrolls, type sentence for text scrolls
    IEnumerator FadeIn(string text)
    {
        AreButtonsEnabled(false);
        Color naturalColor = new Color(50 / 255f, 50 / 255f, 50 / 255f);

        string[] options = text.Split('-');                                                         //turn into component options
        for (int i = 0; i < options.Length; i++) options[i] = i + 1 + ") " + options[i];               //add a number

        Button[] buttons = scrolls[currentScrollIndex].GetComponentsInChildren<Button>();
        for (int i = 0; i < buttons.Length; i++)
        {
            if (i < options.Length)
            {
                buttons[i].GetComponentInChildren<Text>().color = new Color(naturalColor.r, naturalColor.g, naturalColor.b, 0);
                buttons[i].GetComponentInChildren<Text>().text = options[i];
                buttons[i].GetComponentInChildren<Text>().fontSize = pixelsPerCharUD;
            }
            else buttons[i].gameObject.SetActive(false);                                            //deactivate unused buttons
        }

        float a = 0.0f;                                                                             //fade the options in
        while (a < 1.0f - fadeInSpeed)
        {
            a += fadeInSpeed;
            foreach (Button b in buttons) b.GetComponentInChildren<Text>().color = new Color(naturalColor.r, naturalColor.g, naturalColor.b, a);
            yield return null;
        }
        foreach (Button b in buttons) b.GetComponentInChildren<Text>().color = new Color(naturalColor.r, naturalColor.g, naturalColor.b, 1);

        AreButtonsEnabled(true);
    }
    IEnumerator TypeSentence(string sentence, bool superFast)
    {
        Text textBox;

        textBox = scrolls[currentScrollIndex].GetComponentInChildren<Text>();
        textBox.fontSize = pixelsPerCharUD;
        lastTextBox = sentence;

        if (superFast) textBox.text = sentence;
        else
        {
            textBox.text = "";
            foreach (char letter in sentence)
            {
                textBox.text += letter;
                if (letter.Equals(' ')) yield return null;
                else yield return new WaitForSeconds(textTypeSpeed);
            }
        }
    }

    //coroutine for ending a conversation, called in the dialogue manager
    public IEnumerator ResetConversation()
    {
        currentScrollOffset = originalOffset * screenMidpoint.y * 2;
        currentScrollIndex = -1;
        StopAllCoroutines();

        for (float i = 1; i > 0; i -= moveOutSpeed)
        {
            thisRecTranform.localScale = new Vector3(i, i, 1f);
            thisRecTranform.position += Vector3.up * moveOutUpAmount;
            yield return null;
        }

        for (int i = 0; i < scrolls.Length; i++)
        {
            Destroy(scrolls[i]);
            Destroy(textAreas[i]);
        }

        thisRecTranform.position = new Vector3(screenMidpoint.x, screenMidpoint.y, 0);
        thisRecTranform.localScale = new Vector3(1f, 1f, 1f);
    }
    public IEnumerator SelectOption()
    {
        for (float x = 0; x < maxCharactersAcross * pixelsPerCharLR + 40; x += 60 * moveInSpeed)
        {
            scrolls[currentScrollIndex - 1].transform.position += Vector3.right * 60 * moveInSpeed;
            yield return null;
        }
        scrolls[currentScrollIndex - 1].SetActive(false);
    }

    private void DestroyFirstBox()
    {
        Destroy(scrolls[0]);
        Destroy(textAreas[0]);

        for (int i = 0; i < scrolls.Length - 1; i++)
        {
            scrolls[i] = scrolls[i + 1];
            textAreas[i] = textAreas[i + 1];
        }

        currentScrollIndex--;
    }

    private void AreButtonsEnabled(bool yes)
    {
        if (textAreas[currentScrollIndex].GetComponent<Button>() != null)
            textAreas[currentScrollIndex].GetComponent<Button>().interactable = yes;
        else
        {
            Button[] buttons = textAreas[currentScrollIndex].GetComponentsInChildren<Button>();
            foreach (Button b in buttons) b.interactable = yes;
        }
    }
}