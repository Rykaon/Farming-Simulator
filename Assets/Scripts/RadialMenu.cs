﻿using UnityEngine;
using UnityEngine.EventSystems;

using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class RadialMenu : MonoBehaviour {
    public PlayerControls playerControls { get; private set; }
    [SerializeField] public PlayerManager PC_Manager;

    public bool isActive = false;

    [SerializeField]
    public bool isSubRM = false;

    [SerializeField]
    public RadialMenu upRM;

    public Sprite[] listeInterfaceInfo;
    public GameObject interfaceInfo;

    [HideInInspector]
    public RectTransform rt;
    //public RectTransform baseCircleRT;
    //public Image selectionFollowerImage;

    [Tooltip("Adjusts the radial menu for use with a gamepad or joystick. You might need to edit this script if you're not using the default horizontal and vertical input axes.")]
    public bool useGamepad = false;

    [Tooltip("With lazy selection, you only have to point your mouse (or joystick) in the direction of an element to select it, rather than be moused over the element entirely.")]
    public bool useLazySelection = true;


    [Tooltip("If set to true, a pointer with a graphic of your choosing will aim in the direction of your mouse. You will need to specify the container for the selection follower.")]
    public bool useSelectionFollower = true;

    [Tooltip("If using the selection follower, this must point to the rect transform of the selection follower's container.")]
    public RectTransform selectionFollowerContainer;

    [Tooltip("This is the text object that will display the labels of the radial elements when they are being hovered over. If you don't want a label, leave this blank.")]
    public Text textLabel;

    [Tooltip("This is the list of radial menu elements. This is order-dependent. The first element in the list will be the first element created, and so on.")]
    public List<RadialMenuElement> elements = new List<RadialMenuElement>();


    [Tooltip("Controls the total angle offset for all elements. For example, if set to 45, all elements will be shifted +45 degrees. Good values are generally 45, 90, or 180")]
    public float globalOffset = 0f;


    public float currentAngle = 0f; //Our current angle from the center of the radial menu.


    public int index = 0; //The current index of the element we're pointing at.

    private int elementCount;

    private float angleOffset; //The base offset. For example, if there are 4 elements, then our offset is 360/4 = 90

    private int previousActiveIndex = 0; //Used to determine which buttons to unhighlight in lazy selection.

    private PointerEventData pointer;

    private int saveIndexForSound=-1;

    ////////////////////////////////////////////////////////////////////////////////////////////////
    // Ce script fait un peu peur si tu l'as jamais ouvert, mais en vérité y'a pas énromément     //
    // de choses importante pour le reste de l'architecture. C'est essentiellement des fonctions  //
    // utilitaires internes au fonctionnement du menu.                                            //
    //                                                                                            //
    // Basiquement, lorsque le gameObject du menu (l'objet sur lequel le script est attaché),     //
    // les fonction OnEnabled et OnDisabled sont appelée et c'est là que le changement            //
    // du Control.State du PlayerManager se fait pour passer soit de Farm à Farm.UI soit de       //
    // Fight à Fight.UI (ou inversement, cf. les commentaires de la fonction LongInputPress dans  //
    // le script PlayerManager).                                                                  //
    // C'est aussi là où on effectue le changement d'actionMap pour le newInputSystem, et c'est   //
    // ça qui permet de ne pas avoir besoin de booléen pour checker quand le joueur relâche le    //
    // bouton Y, car instantanément l'actionMap change et techniquement il appuie toujours sur le //
    // bouton mais l'input est plus écouté. Enfin bref c'est du détail.                           //
    //                                                                                            //
    // Y'a juste dans l'Update() que y'a un ou deux trucs à voir sinon rien d'autre (voir le      //
    // commentaire correspondant plus bas).                                                       //
    ////////////////////////////////////////////////////////////////////////////////////////////////

    void Awake()
    {
        playerControls = PlayerManager.instance.playerControls;
        pointer = new PointerEventData(EventSystem.current);

        rt = GetComponent<RectTransform>();

        if (rt == null)
        {
            Debug.LogError("Radial Menu: Rect Transform for radial menu " + gameObject.name + " could not be found. Please ensure this is an object parented to a canvas.");
        }

        if (useSelectionFollower && selectionFollowerContainer == null)
        {
            Debug.LogError("Radial Menu: Selection follower container is unassigned on " + gameObject.name + ", which has the selection follower enabled.");
        }

        elementCount = elements.Count;

        angleOffset = (360f / (float)elementCount);

        //Loop through and set up the elements.
        for (int i = 0; i < elementCount; i++)
        {
            if (elements[i] == null)
            {
                Debug.LogError("Radial Menu: element " + i.ToString() + " in the radial menu " + gameObject.name + " is null!");
                continue;
            }
            elements[i].parentRM = this;

            elements[i].setAllAngles((angleOffset * i) + globalOffset, angleOffset);

            elements[i].assignedIndex = i;
        }
    }

    private void OnEnable()
    {
        EnableDisable(true);
    }

    private void OnDisable()
    {
        EnableDisable(false);
    }

    public void EnableDisable(bool enabled)
    {
        if (enabled)
        {
            isActive = true;
            AudioManager.instance.PlayVariation("MenuOpen", 0.2f, 0.1f);
            if (PC_Manager.controlState == PlayerManager.ControlState.Farm)
            {
                PC_Manager.ChangeState(PlayerManager.ControlState.FarmUI);
            }
            else if (PC_Manager.controlState == PlayerManager.ControlState.Fight)
            {
                PC_Manager.ChangeState(PlayerManager.ControlState.FightUI);
            }
        }
        else
        {
            isActive = false;
            AudioManager.instance.PlayVariation("MenuClose", 0.2f, 0.1f);

            if (elements[0].actionType == RadialMenuElement.ActionType.Push || elements[0].actionType == RadialMenuElement.ActionType.Pull || elements[0].actionType == RadialMenuElement.ActionType.Move)
            {
                PC_Manager.ChangeState(PlayerManager.ControlState.Fight);
            }
            else
            {
                PC_Manager.ChangeState(PlayerManager.ControlState.Farm);
            }
        }
    }


    void Start()
    {
        playerControls = PlayerManager.instance.playerControls;

        if (useGamepad)
        {
            EventSystem.current.SetSelectedGameObject(gameObject, null); //We'll make this the active object when we start it. Comment this line to set it manually from another script.
            /*if (useSelectionFollower && selectionFollowerContainer != null)
            {
                for (int i = 0; i < elements.Count; i++)
                {
                    if (elements[i].active)
                    {
                        selectionFollowerContainer.rotation = Quaternion.Euler(0, 0, -globalOffset * i); //Point the selection follower at the first active element.
                        break;
                    }
                }
            }*/
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////
    // Dans l'update, si le bouton du menu selectionné par le joueur est actif (donc pas les      //
    // boutons invisibles sur le menu radial, sans actions), on écoute si le joueur appuie sur le //
    // bouton A. Si le joueur est en mode Farm ou en mode Fight, on appelle pas la même fonction  //
    // mais dans les deux cas, c'est la même logique, c'est une fonction qui se trouve dans le    //
    // script MenuAction. Chaque éléments actif du menu (dans le script RadialMenuElements)       //
    // contient une instance de cette classe abstraite MenuAction. Plus de précisions dans les    //
    // commentaires du script MenuAction.                                                         //
    ////////////////////////////////////////////////////////////////////////////////////////////////

    void Update()
    {
        if (isActive)
        {
            interfaceInfo.SetActive(true);

            bool joystickMoved = playerControls.UI.Navigate.ReadValue<Vector2>() != Vector2.zero;

            float rawAngle;

            if (!useGamepad)
            {
                rawAngle = Mathf.Atan2(Input.mousePosition.y - rt.position.y, Input.mousePosition.x - rt.position.x) * Mathf.Rad2Deg;
            }
            else
            {
                rawAngle = Mathf.Atan2(playerControls.UI.Navigate.ReadValue<Vector2>().y, playerControls.UI.Navigate.ReadValue<Vector2>().x) * Mathf.Rad2Deg;
            }

            if (!useGamepad)
            {
                currentAngle = normalizeAngle(-rawAngle + 90 - globalOffset + (angleOffset / 2f));
            }
            else if (joystickMoved)
            {
                currentAngle = normalizeAngle(-rawAngle + 90 - globalOffset + (angleOffset / 2f));
            }

            //Handles lazy selection. Checks the current angle, matches it to the index of an element, and then highlights that element.
            if (angleOffset != 0 && useLazySelection)
            {
                //Current element index we're pointing at.
                index = (int)(currentAngle / angleOffset);

                if (elements[index] != null)
                {
                    if (index < listeInterfaceInfo.Length)
                    {
                        interfaceInfo.GetComponent<Image>().sprite = listeInterfaceInfo[index];
                    }
                    

                    if (elements[index].active)
                    {
                        //Select it.
                        selectButton(index);
                        

                        if (saveIndexForSound != index)
                        {
                            AudioManager.instance.PlayVariation("MenuChangeSelection", 0.2f, 0.1f);
                            saveIndexForSound = index;
                        }

                        //If we click or press a "submit" button (Button on joystick, enter, or spacebar), then we'll execut the OnClick() function for the button.
                        switch (PlayerManager.instance.controlState)
                        {
                            case PlayerManager.ControlState.Farm:
                                if (playerControls.UI.A.IsPressed())
                                {
                                    ExecuteEvents.Execute(elements[index].button.gameObject, pointer, ExecuteEvents.submitHandler);
                                    elements[index].action.ExecuteAction(null);
                                    if (elements[index].actionType != RadialMenuElement.ActionType.Submenu)
                                    {
                                        interfaceInfo.SetActive(false);
                                    }
                                }
                                break;

                            case PlayerManager.ControlState.Fight:
                                if (playerControls.UI.A.IsPressed())
                                {
                                    ExecuteEvents.Execute(elements[index].button.gameObject, pointer, ExecuteEvents.submitHandler);
                                    elements[index].action.SelectActionTarget();
                                    if (elements[index].actionType != RadialMenuElement.ActionType.Submenu)
                                    {
                                        interfaceInfo.SetActive(false);
                                    }
                                }
                                break;
                        }
                    }
                    else
                    {
                        if (previousActiveIndex != index)
                        {
                            if (elements[previousActiveIndex].active)
                            {
                                elements[previousActiveIndex].unHighlightThisElement(pointer);
                            }
                        }
                    }

                    if (playerControls.UI.B.IsPressed())
                    {
                        if (isSubRM)
                        {
                            transform.gameObject.SetActive(false);
                            elements[index].parentRM.upRM.EnableDisable(true);
                        }
                        else
                        {
                            transform.gameObject.SetActive(false);
                            interfaceInfo.SetActive(false);
                        }
                    }
                }
            }

            //Updates the selection follower if we're using one.
            if (useSelectionFollower && selectionFollowerContainer != null)
            {
                if (!useGamepad || joystickMoved)
                {
                    
                }

                if (rawAngle != 0)
                {
                    selectionFollowerContainer.rotation = Quaternion.Euler(0, 0, rawAngle + 270);
                }
                else
                {
                    selectionFollowerContainer.rotation = Quaternion.Euler(0, 0, 90 + 270);
                    elements[previousActiveIndex].unHighlightThisElement(pointer);
                    elements[index].unHighlightThisElement(pointer);
                    previousActiveIndex = 0;
                    index = 0;
                }
                
            }
        } 
    }


    //Selects the button with the specified index.
    private void selectButton(int i)
    {
        if (elements[i].selected == false)
        {
            
        }

        elements[i].highlightThisElement(pointer); //Select this one

        if (previousActiveIndex != i)
        {
            elements[previousActiveIndex].unHighlightThisElement(pointer); //Deselect the last one.
        }

        previousActiveIndex = i;
    }

    //Keeps angles between 0 and 360.
    private float normalizeAngle(float angle)
    {
        angle = angle % 360f;

        if (angle < 0)
            angle += 360;

        return angle;
    }
}
