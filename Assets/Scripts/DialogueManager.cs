using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Ink.Runtime;
using UnityEngine.EventSystems;
using Ink.Parsed;
using Story = Ink.Runtime.Story;
using Choice = Ink.Runtime.Choice;
using Ink.UnityIntegration;
using System.Linq.Expressions;

public class DialogueManager : MonoBehaviour
{
    [Header("Globals Ink File")]
    [SerializeField] private InkFile globalsInkFile;

    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI displayNameText;
    [SerializeField] private Animator portraitAnimator;
    private Animator layoutAnimator;

    [Header("Choices UI")]
    [SerializeField] private GameObject[] choices;
    private TextMeshProUGUI[] choicesText;

    private Story currentStory;

    [SerializeField] private PlayerController_Farm playerControllerFarm;
    [SerializeField] private PlayerManager PC_Manager;
    [SerializeField] private PlayerInventory inventory;

    public bool isActive = false;
    private bool isInitialized = false;

    public static DialogueManager instance;

    private const string SPEAKER_TAG = "speaker";
    private const string PORTRAIT_TAG = "portrait";
    private const string LAYOUT_TAG = "layout";

    private DialogueVariables dialogueVariables;

    private PlayerControls playerControls;

    private void Awake()
    {
        if(instance != null)
        {
            Debug.LogWarning("Found more than one Dialogue Manager in the scene !");
        }

        instance = this;

        dialogueVariables= new DialogueVariables(globalsInkFile.filePath);
    }

    public static DialogueManager GetInstance()
    {
        return instance;
    }

    private void Start()
    {
        isActive = false;
        dialoguePanel.SetActive(false);

        layoutAnimator = dialoguePanel.GetComponent<Animator>();

        playerControls = PC_Manager.playerControls;

        choicesText = new TextMeshProUGUI[choices.Length];
        int index = 0;
        foreach (GameObject choice in choices)
        {
            choicesText[index] = choice.GetComponentInChildren<TextMeshProUGUI>();
            index++;
        }

    }

    private void Update()
    {
        if (isActive)
        {
            if (playerControls.UI.A.WasPressedThisFrame() && currentStory.currentChoices.Count == 0)
            {
                ContinueStory();
            }

            if (playerControls.UI.B.WasPressedThisFrame())
            {
                StartCoroutine(ExitDialogueMode());
            }
        }
    }

    public void EnterDialogueMode(TextAsset inkJSON)
    {
        EnableDisable(true);

        currentStory = new Story(inkJSON.text);
        isActive = true;
        dialoguePanel.SetActive(true);

        dialogueVariables.StartListening(currentStory);

        UpdateVariables();

        currentStory.BindExternalFunction("PlantSellBuy", (string PlantToBuy, int PlantPrice, bool SellOrBuy) =>
        {
            inventory.SellBuyPlant(PlantToBuy, PlantPrice, SellOrBuy);
        });

        // Reset portrait, layout and speaker
        displayNameText.text = "???";
        portraitAnimator.Play("default");
        layoutAnimator.Play("right");

        ContinueStory();
    }

    public void UpdateVariables()
    {
        currentStory.variablesState["PlayerArgent"] = inventory.nbArgent;

        for (int i = 0; i < inventory.itemList.Count; i++)
        {
            currentStory.variablesState["NbPlant" + (i + 1).ToString()] = inventory.itemNbrList[i];

            if (!isInitialized)
            {
                currentStory.variablesState["NamePlant" + (i + 1).ToString()] = inventory.itemList[i].plantName;
                currentStory.variablesState["PricePlant" + (i + 1).ToString()] = inventory.itemList[i].sellPrice;
                isInitialized = true;
            }
        }
    }

    private IEnumerator ExitDialogueMode()
    {
        yield return new WaitForSeconds(0.2f);
        dialogueVariables.StopListening(currentStory);

        currentStory.UnbindExternalFunction("PlantSellBuy");

        dialoguePanel.SetActive(false);
        dialogueText.text = "";

        EnableDisable(false);
    }

    private void ContinueStory()
    {
        if (currentStory.canContinue)
        {
            dialogueText.text = currentStory.Continue();

            DisplayChoices();

            HandleTags(currentStory.currentTags);
        }
        else
        {
            StartCoroutine(ExitDialogueMode());
        }
    }

    private void HandleTags(List<string> currentTags)
    {
        // loop through each tag and handle it accoringly
        foreach (string tag in currentTags)
        {
            // parse the tag
            string[] splitTag = tag.Split(':');
            if(splitTag.Length != 2 ) {
                Debug.LogError("Tag could not be appropriately parsed: " + tag);
            }
            string tagKey = splitTag[0].Trim();
            string tagValue = splitTag[1].Trim();

            switch (tagKey)
            {
                case SPEAKER_TAG:
                    displayNameText.text = tagValue;
                    break;
                case PORTRAIT_TAG:
                    portraitAnimator.Play(tagValue);
                    break;
                case LAYOUT_TAG:
                    layoutAnimator.Play(tagValue);
                    break;
                default:
                    Debug.LogWarning("Tag came but is not valid :" + tag);
                    break;
            }
        }
    }

    private void DisplayChoices()
    {
        List<Choice> currentChoices = currentStory.currentChoices;

        // defensive check to make sure our UI can support the number of choices coming in
        if(currentChoices.Count > choices.Length)
        {
            Debug.LogError("More choices were given than the UI can support. Number of choices given :" + currentChoices.Count);
        }

        int index = 0;
        // enable and initialize the choices up to the amount of choices for this line of dialogue
        foreach (Choice choice in currentChoices)
        {
            choices[index].gameObject.SetActive(true);
            choicesText[index].text = choice.text;
            index++;
        }

        // go through the remaining choices the UI supports and make sure they're hidden
        for (int i = index; i < choices.Length; i++)
        {
            choices[i].gameObject.SetActive(false);
        }

        StartCoroutine(SelectFirstChoice());
    }

    private IEnumerator SelectFirstChoice()
    {
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(choices[0].gameObject);
    }

    public void MakeChoice(int choiceIndex)
    {
        currentStory.ChooseChoiceIndex(choiceIndex);
        //ContinueStory();
    }

    public void EnableDisable(bool enabled)
    {
        if (enabled)
        {
            isActive = true;
            PC_Manager.ChangeState(PlayerManager.ControlState.FarmUI);
        }
        else
        {
            isActive = false;
            PC_Manager.ChangeState(PlayerManager.ControlState.Farm);
        }
    }
}
