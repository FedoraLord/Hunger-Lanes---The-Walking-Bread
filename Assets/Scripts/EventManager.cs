using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EventManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject titleScreen;
    public GameObject gameScreen;
    public Image background;
    public Image leftCharacter;
    public Image rightCharacter;
    public Text dialogueTextBox;
    public DecisionUI[] buttons;

    [Header("Data")]
    public string TESTING_StartingEventName;
    public int maxCharsPerDialogue;
    public float textWipeSpeed;
    public Event[] events;

    private Event currentEvent;
    private int dialogueIndex;
    private bool continueFlag;
    private bool isDeciding;
    private float ignoreContinueInputUntil;

    private Dialogue CurrentDialogue
    {
        get { return currentEvent.dialogues[dialogueIndex]; }
    }

    [System.Serializable]
    public struct DecisionUI
    {
        public Button button;
        public Text textComponent;
    }

    private void Start()
    {
        titleScreen.SetActive(true);
        gameScreen.SetActive(false);
    }

    private void Update()
    {
        if (!isDeciding && ignoreContinueInputUntil < Time.time)
        {
            if (Input.GetKeyUp(KeyCode.Mouse0) || Input.GetKeyUp(KeyCode.KeypadEnter) || Input.GetKeyUp(KeyCode.Space))
            {
                continueFlag = true;
            }
        }
    }

    public void PlayGame()
    {
        titleScreen.SetActive(false);
        gameScreen.SetActive(true);

        IgnoreContinueFlag();

        if (!string.IsNullOrEmpty(TESTING_StartingEventName))
            PlayEvent(TESTING_StartingEventName);
        else if (events.Length > 0)
            PlayEvent(events[0].name);
    }

    private void IgnoreContinueFlag()
    {
        continueFlag = false;
        ignoreContinueInputUntil = Time.time + 0.1f;
    }

    private void PlayEvent(string name)
    {
        foreach (var item in buttons)
        {
            item.button.gameObject.SetActive(false);
        }

        if (string.IsNullOrEmpty(name))
        {
            titleScreen.SetActive(true);
            gameScreen.SetActive(false);
            return;
        }
        else if(name.ToLower() == "exitgame")
        {
            QuitGame();
            return;
        }

        List<Event> eventQuery = events.Where(x => x.name == name).ToList();
        if (eventQuery.Count > 1)
        {
            Debug.LogWarning(string.Format("HEY DUMMY! There are more than one events named {0} so I'm playing the first one in the list.", name));
        }
        else if (eventQuery.Count == 0)
        {
            Debug.LogWarning(string.Format("HEY DUMMY! There are no events named {0} so I'm stopping.", name));
            return;
        }

        currentEvent = eventQuery.First();
        dialogueIndex = 0;
        if (currentEvent.background)
            background.sprite = currentEvent.background;
        PlayDialogue();
    }

    private void NextDialogue()
    {
        dialogueIndex++;
        PlayDialogue();
    }

    private void PlayDialogue()
    {
        leftCharacter.gameObject.SetActive(CurrentDialogue.leftCharacter);
        leftCharacter.sprite = CurrentDialogue.leftCharacter;
        rightCharacter.gameObject.SetActive(CurrentDialogue.rightCharacter);
        rightCharacter.sprite = CurrentDialogue.rightCharacter;
        StartCoroutine(TextWipe());
    }

    private IEnumerator TextWipe()
    {
        List<string> splitTexts = AutoSplitText();
        bool isLastDialogue = (dialogueIndex == currentEvent.dialogues.Length - 1);

        for (int i = 0; i < splitTexts.Count; i++)
        {
            dialogueTextBox.text = string.Empty;
            for (int j = 0; j < splitTexts[i].Length; j++)
            {
                if (splitTexts[i][j] != ' ')
                {
                    yield return new WaitForSeconds(textWipeSpeed);
                }

                if (ConsumeContinueFlag())
                {
                    dialogueTextBox.text = splitTexts[i];
                    break;
                }
                else
                {
                    dialogueTextBox.text += splitTexts[i][j];
                }
            }

            bool isLastSplitText = (i == splitTexts.Count - 1);

            if (!isLastSplitText || !isLastDialogue)
            {
                yield return new WaitUntil(ConsumeContinueFlag);
            }
        }

        if (!isLastDialogue)
            NextDialogue();
        else
            DisplayDecisions();
    }

    private List<string> AutoSplitText()
    {
        if (CurrentDialogue.text.Length > maxCharsPerDialogue)
        {
            List<string> texts = new List<string>();
            string remainingText = CurrentDialogue.text;
            int lastSentenceEnd = 0;

            for (int i = 0; i < remainingText.Length; i++)
            {
                if (remainingText[i] == '.')
                {
                    if (i < maxCharsPerDialogue)
                    {
                        lastSentenceEnd = i + 1;
                    }
                    else
                    {
                        texts.Add(remainingText.Substring(0, lastSentenceEnd));
                        remainingText = remainingText.Substring(lastSentenceEnd).Trim();
                        i = lastSentenceEnd = 0;
                    }
                }
            }

            if (!string.IsNullOrEmpty(remainingText))
            {
                if (remainingText.Length > maxCharsPerDialogue)
                {
                    if (lastSentenceEnd > 0)
                    {
                        texts.Add(remainingText.Substring(0, lastSentenceEnd));
                        remainingText = remainingText.Substring(lastSentenceEnd).Trim();
                    }
                    else
                    {
                        Debug.LogWarning(string.Format("Sentence too long in event '{0}', dialogueIndex '{1}'. Need to add a period every {2} characters or increase maxCharsPerDialogue", 
                            currentEvent.name, dialogueIndex, maxCharsPerDialogue.ToString()));
                    }
                }
                texts.Add(remainingText);
            }

            return texts;
        }
        else
        {
            return new List<string>() { CurrentDialogue.text };
        }
    }

    private bool ConsumeContinueFlag()
    {
        bool tmp = continueFlag;
        continueFlag = false;
        return tmp;
    }

    private void DisplayDecisions()
    {
        if (currentEvent.decisions.Length == 0)
        {
            if (string.IsNullOrEmpty(currentEvent.defaultNextEvent))
            {
                Debug.LogWarning("There are no decisions at the end of this event, and defaultNextEvent is empty.");
            }
            else
            {
                StartCoroutine(PlayEventOnContinueFlag(currentEvent.defaultNextEvent));
                return;
            }
        }
        else if (currentEvent.decisions.Length > buttons.Length)
        {
            Debug.LogWarning("There are more decisions in this event than there are buttons assigned in the inspector.");
        }

        for (int i = 0; i < currentEvent.decisions.Length; i++)
        {
            buttons[i].button.gameObject.SetActive(true);
            buttons[i].textComponent.text = currentEvent.decisions[i].buttonText;
        }

        isDeciding = true;
    }

    public void DecisionButtonClicked(int buttonIndex)
    {
        isDeciding = false;
        IgnoreContinueFlag();

        string nextEvent = currentEvent.decisions[buttonIndex].nextEvent;
        PlayEvent(nextEvent);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private IEnumerator PlayEventOnContinueFlag(string name)
    {
        yield return new WaitUntil(ConsumeContinueFlag);
        PlayEvent(name);
    }
}
