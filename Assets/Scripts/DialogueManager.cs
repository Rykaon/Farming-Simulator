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

public class DialogueManager : MonoBehaviour
{
    [Header("Globals Ink File")]
    [SerializeField] private InkFile globalsInkFile;

    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Choices UI")]
    [SerializeField] private GameObject[] choices;
    private TextMeshProUGUI[] choicesText;

    private Story currentStory;

    [SerializeField] private PlayerController_Farm playerControllerFarm;
    [SerializeField] private PlayerManager PC_Manager;
    [SerializeField] private PlayerInventory playerInventory;

    public bool isActive = false;

    public static DialogueManager instance;

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
            if (playerControls.UI.A.WasPressedThisFrame())
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

        currentStory.variablesState["PlayerArgent"] = playerInventory.nbArgent;
        currentStory.variablesState["NbPlanteRouge"] = playerInventory.nbAttack;
        currentStory.variablesState["NbPlanteBleu"] = playerInventory.nbMove;
        currentStory.variablesState["NbPlanteJaune"] = playerInventory.nbBoost;

        currentStory.BindExternalFunction("PlantBuy", (string PlantToBuy) =>
        {
            playerInventory.BuyPlant(PlantToBuy);
        });
        currentStory.BindExternalFunction("PlantSell", (string PlantToSell) =>
        {
            playerInventory.SellPlant(PlantToSell);
        });

        ContinueStory();
    }

    private IEnumerator ExitDialogueMode()
    {
        yield return new WaitForSeconds(0.2f);
        dialogueVariables.StopListening(currentStory);

        currentStory.UnbindExternalFunction("PlantBuy");
        currentStory.UnbindExternalFunction("PlantSell");

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
        }
        else
        {
            StartCoroutine(ExitDialogueMode());
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
    }

    public void EnableDisable(bool enabled)
    {
        if (enabled)
        {
            playerControls.Gamepad.Disable();
            playerControls.UI.Enable();
            isActive = true;
            PlayerController_Fight.instance.isActive = false;
            PlayerController_Farm.instance.isActive = false;
        }
        else
        {
            playerControls.Gamepad.Enable();
            playerControls.UI.Disable();
            isActive = false;
            PlayerController_Farm.instance.isActive = true;
        }
    }
}
