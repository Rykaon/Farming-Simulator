using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController_Farm : MonoBehaviour
{
    public static PlayerController_Farm instance;

    [Header("Component References")]
    [SerializeField] private PlayerManager PC_Manager;

    private PlayerControls playerControls;
    private Pathfinding pathfinding;
    public RadialMenu rm;

    [Header("Properties")]
    [SerializeField] float moveSpeed;
    [SerializeField] float collisionDetectionDistance;

    private Vector3 movement;
    public GameObject previousTarget = null;
    //public TTileManager currentTileManager;

    public bool isPlacing = false;
    public BuildObject buildObject;

    private const string isWalking = "isWalking";
    private const string isRunning = "isRunning";

    public bool isActive = true;
    public bool canUseTool = true;

    public bool isPlaying = false;

    public bool LBRBisPressed = false;

    public bool hasMovementBeenReset;

    ////////////////////////////////////////////////////////////////////////////////////////////////
    // Ce script là est un peu plus touffu que le PlayerController_Fight. Basiquement, y'a deux   //
    // grandes fonctionnalité qui sont gérées ici : le déplacement du personnage pendant la phase //
    // de farm et l'execution de l'outil qu'il a sélectionné grâce au menu d'action Farm.         //
    //                                                                                            //
    // Les détails de la logique de ces fonctionnalités sont dans les commentaires au-dessus des  //
    // fonctions correspondantes.                                                                 //
    ////////////////////////////////////////////////////////////////////////////////////////////////

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        playerControls = PC_Manager.playerControls;
        pathfinding = PC_Manager.pathfinding;
        playerControls.Gamepad.Enable();
        playerControls.UI.Disable();
    }

    public PathNode GetCurrentNode()
    {
        return PlayerManager.instance.pathfinding.GetNodeWithCoords(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////
    // La fonction de mouvement est appelée indépendemment de l'input du joystick. Même si le     //
    // joueur ne se déplace pas, la fonction est appelée, ce qui lui permet de s'arrêter et de    //
    // et de quand même checker sur quelle case il se trouve pour l'execution de ses différentes  //
    // action (la logique avec previousTarget et tout). Elle fonctionne très bien, pas besoin     //
    // d'y toucher, elle s'adapte déjà à tous les comportements du joueur.                        //
    ////////////////////////////////////////////////////////////////////////////////////////////////

    private void Move(Vector2 value)
    {
        Vector3 direction = new Vector3(value.x, 0f, value.y);

        if (isActive)
        {
            if (playerControls.Gamepad.LeftStick.ReadValue<Vector2>() != Vector2.zero && !RaycastCollision())
            {
                movement += direction.x * Utilities.GetTransformRight(Camera.main.transform) * moveSpeed * Time.deltaTime;
                movement += direction.z * Utilities.GetTransformForward(Camera.main.transform) * moveSpeed * Time.deltaTime;
            }

            //PC_Manager.rigidBody.AddForce(movement);

            if (value == Vector2.zero)
            {
                PC_Manager.rigidBody.velocity = Vector3.zero;
            }
            else
            {
                PC_Manager.rigidBody.velocity = movement;
            }

            if (value.magnitude > 0.1f)
            {
                PC_Manager.animator.SetBool("isMoving", true);
            }
            else
            {
                PC_Manager.animator.SetBool("isMoving", false);
            }
            
            if (movement != Vector3.zero)
            {
                //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(movement.normalized), 0.15f);
                LookAt(value);
            }
        }

        PathNode node = GetCurrentNode();
        GameObject target = null;

        if (node != null)
        {
            if (!node.isVirtual)
            {
                if (isPlacing)
                {
                    if (!node.isSeeded)
                    {
                        target = node.tile;
                    }
                }
                else
                {
                    target = node.tile;
                }
            }
        }

        if (previousTarget != null)
        {
            Outline(false, previousTarget.transform);
        }
        if (target != null)
        {
            Outline(true, target.transform);
        }
        previousTarget = target;

        movement = Vector3.zero;
    }

    private void Outline(bool enable, Transform transform)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            for (int j = 0; j < transform.GetChild(i).GetComponent<MeshRenderer>().materials.Length; j++)
            {
                if (enable)
                {
                    Utilities.SetEmission(transform.GetChild(i).GetComponent<MeshRenderer>().materials[j], 0.35f);
                }
                else
                {
                    Utilities.SetEmission(transform.GetChild(i).GetComponent<MeshRenderer>().materials[j], 0f);
                }
            }
        }
    }

    public void LookAt(Vector2 value)
    {
        Vector3 direction = PC_Manager.rigidBody.velocity;
        direction.y = 0f;

        if (value.sqrMagnitude > 0.1f && direction.sqrMagnitude > 0.1f)
        {
            PC_Manager.rigidBody.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }
        else
        {
            if (!PC_Manager.rigidBody.isKinematic)
            {
                PC_Manager.rigidBody.angularVelocity = Vector3.zero;
            }
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////
    // Deux petites fonctions utilitaires pour les mouvements du perso comme reset sa velocity    //
    // quand on passe au mode Fight ou checker les collisions pour s'assurer de ne pas passer     //
    // au travers des objets dans la scène en cas d'une trop grande vitesse.                      //
    ////////////////////////////////////////////////////////////////////////////////////////////////

    private void ResetMovement()
    {
        movement = Vector3.zero;
        PC_Manager.animator.SetBool(isWalking, false);
        PC_Manager.animator.SetBool(isRunning, false);
        PC_Manager.rigidBody.velocity = Vector3.zero;
        hasMovementBeenReset = true;
    }

    private bool RaycastCollision()
    {
        bool isCollisionDetected = false;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, collisionDetectionDistance))
        {
            Debug.DrawRay(transform.position, transform.forward * collisionDetectionDistance, Color.red);
            if (hit.collider.tag == "Level")
            {
                isCollisionDetected = true;
            }
        }

        return isCollisionDetected;
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////
    // Lorsque le joueur appuie sur le bouton X (l'input est check dans l'update), on lance cette    //
    // coroutine. Peut importe l'outil du joueur, on lance la coroutine, c'est ici que les           //
    // spécificités du comportement de chaque outil est gérer.                                       //
    //                                                                                               //
    // La sélection de l'outil ne se fait pas ici mais dans le script MenuAction (cf. les            //
    // commentaires que j'ai fait dans le PlayerController_Fight).                                   //
    //                                                                                               //
    // Outil Plante : En sélectionnant l'option "Plante" du menu Farm, on ouvre un sous-menu.        //
    // Chaque bouton de ce sous-menu est bind à un préfab d'une des plantes (qui est stockée         //
    // dans PC_Manager.plant pour savoir quelle est la plante qu'il est en train de planter).        //
    // PC_Manager.plant permet aussi d'avoir une référence pour dialoguer avec l'inventaire en       //
    // utilisant les fonction de la classe statique Utilities (cf. mes commentaires dans le          //
    // script Utilities).                                                                            //
    // Lorsque le joueur appuie sur le bouton X avec cet outil, il instantie une plante qui le       //
    // suit et la variable isPlacing est set à true. La suite de la logique pour placer les          //
    // plantes sur une case est gérée dans l'update (cf. commentaires correspondants plus bas)       //
    //                                                                                               //
    // Outil Objet : Pour l'instant on a pas design les objets, mais c'est excatement la même        //
    // logique que pour les plantes.                                                                 //
    //                                                                                               //
    // Outil Collect/Water/Sun : Là c'est une logique un peu différente. On va communiquer           //
    // directement avec la grille pour savoir les propriétés de la case sur laquelle se trouve       //
    // le joueur. En gros dans tous les cas, PathNode node correspond à la case actuelle du          //
    // joueur. Si PathNode == null, c'est que le joueur est en dehors de la grille.                  //
    // Si node == virtual, c'est que le joueur est sur la grille mais que la case est virtuelle,     //
    // il n'est pas sur une case plantable du vaisseau. Dans ces deux cas on ne fait rien.           //
    // Si les deux conditions sont fausses, alors le joueur est sur une case valide et on            //
    // vérifie ses propriétés.                                                                       //
    //                                                                                               //
    // Chaque case réelle du pathfinding contient plusieurs références : node.tile est une référence //
    // directe à la tile (le préfab instantié pour la case). node.tileManager est un script attaché  //
    // au gameObject de la case qui va gérer et stocker toutes les informations sur ce que contient  //
    // la case (les plantes, leur stade d'évolution, l'état de la case (mouillée, ensolleillée)).    //
    // node.seed et node.plant gardent en mémoire les graines plantées.                              //
    //                                                                                               //
    // Si une graine est plantée mais que la plante n'est pas à maturité (arrosée), alors            //
    // node.seed != null et node.plant == null. Si elle est à maturité, alors node.plant != null.    //
    // Toutes les précisions sur le comportement de node.tileManager pour comprendre les             //
    // actions Collect/Water/Sun sont directement dans les commentaires du script Tile Manager.      // 
    //                                                                                               //
    // C'est ici qu'il faudra gérer l'update de l'inventaire du joueur s'il collecte les plantes     //
    // pour récupérer des graines.                                                                   //
    ///////////////////////////////////////////////////////////////////////////////////////////////////

    IEnumerator ExecuteAction()
    {
        PathNode node = PlayerManager.instance.pathfinding.GetNodeWithCoords(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));

        switch (PC_Manager.actionState)
        {
            case PlayerManager.ActionState.Plant:
                if (Utilities.GetNumberOfItemByPrefab(PC_Manager.inventory.inventory, PC_Manager.objectPrefab) > 0)
                {
                    InitializeWithObject(PC_Manager.objectPrefab);
                }
                break;

            case PlayerManager.ActionState.Object:
                /*if (Utilities.GetNumberOfItemByPrefab(PC_Manager.inventory.inventory, PC_Manager.object) > 0)
                {
                    InitializeWithObject(PC_Manager.object);
                }*/
                break;

            case PlayerManager.ActionState.Collect:
                if (node != null)
                {
                    if (!node.isVirtual)
                    {
                        if (node.isPlant)
                        {
                            if (node.tileManager.growState == TileManager.GrowState.High)
                            {
                                node.tileManager.ChangeSeedType(TileManager.SeedType.None);
                            }
                        }
                    }
                }
                break;

            case PlayerManager.ActionState.Water:
                if (node != null)
                {
                    if (!node.isVirtual)
                    {
                        if (node.tileManager.seedType == TileManager.SeedType.Seeded)
                        {
                            if (node.tileManager.wetToDirt != null)
                            {
                                StopCoroutine(node.tileManager.wetToDirt);
                            }

                            if (node.tileManager.sunToDirt != null)
                            {
                                StopCoroutine(node.tileManager.sunToDirt);
                            }

                            node.tileManager.ChangeTileState(TileManager.TileState.WetDirt);
                        }
                    }
                }
                break;

            case PlayerManager.ActionState.Sun:
                if (node != null)
                {
                    if (!node.isVirtual)
                    {
                        if (node.tileManager.seedType == TileManager.SeedType.Seeded)
                        {
                            if (node.tileManager.wetToDirt != null)
                            {
                                StopCoroutine(node.tileManager.wetToDirt);
                            }

                            if (node.tileManager.sunToDirt != null)
                            {
                                StopCoroutine(node.tileManager.sunToDirt);
                            }

                            node.tileManager.ChangeTileState(TileManager.TileState.SunDirt);
                        }
                    }
                }
                break;
        }
        float elapsedTime = 0f;

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        isPlaying = false;
        yield return null;
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////
    // Les trois fonctions qui suivent sont les fonctions utilitaires qui permettent d'instantier    //
    // les plantes et les objets et des les placer sur une case. Ces fonctions sont utilisées        //
    // dans l'update plus bas, plus de précision dans le commentaire correspondant pour la logique.  //
    //                                                                                               //
    // Les prefabs des objets plaçables, donc les prefabs des scriptableObject PlantItem et          //
    // ObjectItem doivent avoir un script BuildObject qui va contenir toutes les fonctions           //
    // utilitaires en interne pour pouvoir leur modifier leur rotation et tout, plus de précision    //
    // sur cette logique dans les commentaires du script BuildObject.                                //
    //                                                                                               //
    // Lorsque le prefab est instantié, je lui ajoute un script BuildObjectDrag qui gère le snap de  //
    // l'objet par rapport au joueur s'il n'est pas sur une case valide ou sur la case du joueur si  //
    // elle est valide. Une fois que l'objet est placé, le script est Destroy.                       //
    ///////////////////////////////////////////////////////////////////////////////////////////////////

    public Vector3 SnapCoordinateToGrid(Vector3 position)
    {
        if (buildObject != null)
        {
            /*if (GetCurrentNode() != null)
            {
                position = new Vector3(position.x, 0, position.z);

                buildObject.transform.localScale = Vector3.one;
            }
            else
            {
                position = new Vector3(transform.position.x, 0, transform.position.z);
                buildObject.transform.localScale = Vector3.zero;
            }*/

            position = new Vector3(position.x, 0, position.z);

            buildObject.transform.localScale = Vector3.one;
        }

        return position;
    }

    public void InitializeWithObject(GameObject tile)
    {
        isPlacing = true;

        GameObject obj;

        obj = Instantiate(tile, new Vector3(Mathf.RoundToInt(PC_Manager.transform.position.x), 0, Mathf.RoundToInt(PC_Manager.transform.position.z)), Quaternion.identity);
        obj.transform.localScale = new Vector3(0, 0, 0);

        buildObject = obj.GetComponent<BuildObject>();
        buildObject.Initialize();
        obj.AddComponent<BuildObjectDrag>();
    }

    public bool CanBePlaced()
    {
        PathNode node = PlayerManager.instance.pathfinding.GetNodeWithCoords(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));

        if (node != null)
        {
            if (!node.isVirtual)
            {
                if (!node.isSeeded)
                {
                    if (Utilities.GetNumberOfItemByPrefab(PC_Manager.inventory.inventory, PC_Manager.objectPrefab) > 0)
                    {
                        Utilities.RemoveItemByPrefab(PC_Manager.inventory.inventory, PC_Manager.objectPrefab);
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////
    // Move() est la seule fonction appelée en permanence. Si le joueur a selectionné l'outil        //
    // Plante ou Objet et qu'il a instantié le prefab, le booléen isPlacing est set à true.          //
    // Pendant ce temps, PathNode node correspond à la case actuelle du joueur (cf. commentaire de   //
    // la coroutine ExecuteAction).                                                                  //
    //                                                                                               //
    // LB et RB servent à rotate l'item respectivement dans le sens anti-horaire et horaire.         //
    // Le bouton B sert à annuler l'action et Destroy l'item. Le bouton A sert à valider le          //
    // placement de l'objet dans sa position et rotation actuelle. On vérifie d'abord si la case     //
    // est valide, si oui on place l'objet et on update le TileManager de la case et l'inventaire    //
    // du joueur, sinon on Destroy l'item comme s'il avait appuyé sur B.                             //
    //                                                                                               //
    // Lorsque l'item est définitivement placé, isPlacing est set à false et on recommence à         //
    // écouter le bouton X pour utiliser l'action correspondante à l'outil actuel du joueur,         //
    // ou le bouton Y pour ouvrir le menu Farm et choisir un nouvel outil.                           //
    ///////////////////////////////////////////////////////////////////////////////////////////////////

    private void Update()
    {
        Move(playerControls.Gamepad.LeftStick.ReadValue<Vector2>());

        if (isActive)
        {
            hasMovementBeenReset = false;

            if (isPlacing)
            {
                PathNode node = PlayerManager.instance.pathfinding.GetNodeWithCoords(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));

                if ((!playerControls.Gamepad.LB.IsPressed() && !playerControls.Gamepad.RB.IsPressed()) && LBRBisPressed)
                {
                    LBRBisPressed = false;
                }

                if (playerControls.Gamepad.A.IsPressed())
                {
                    if (CanBePlaced())
                    {
                        
                        buildObject.Place();
                        node.tileManager.ChangeSeedType(TileManager.SeedType.Seeded);
                        buildObject = null;
                        LBRBisPressed = false;
                        isPlacing = false;
                    }
                    else
                    {
                        Destroy(buildObject.gameObject);
                        buildObject = null;
                        LBRBisPressed = false;
                        isPlacing = false;
                    }
                }
                else if (playerControls.Gamepad.B.IsPressed())
                {
                    Destroy(buildObject.gameObject);
                    buildObject = null;
                    LBRBisPressed = false;
                    isPlacing = false;
                }
                else if (playerControls.Gamepad.LB.IsPressed() && !LBRBisPressed)
                {
                    buildObject.Rotate(-1);

                    LBRBisPressed = true;
                }
                else if (playerControls.Gamepad.RB.IsPressed() && !LBRBisPressed)
                {
                    buildObject.Rotate(1);

                    LBRBisPressed = true;
                }
            }
            else
            {
                if (playerControls.Gamepad.Y.IsPressed())
                {
                    rm.gameObject.SetActive(true);
                }

                if (playerControls.Gamepad.X.IsPressed() && !isPlaying)
                {
                    isPlaying = true;
                    StartCoroutine(ExecuteAction());
                }
            }
        }
        else
        {
            if (!hasMovementBeenReset)
            {
                ResetMovement();
            }
        }
    }
}
