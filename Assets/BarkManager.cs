using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BarkManager : MonoBehaviour
{
    public static BarkManager instance { get; private set; }
    [SerializeField] private int poolSize = 10;
    [SerializeField] private GameObject prefabToPool;
    // Sur un vrai projet, on aurait plutôt une liste de composants
    // pour éviter de devoir un GetComponent à chaque object pull pour l'initialiser
    private List<BarkDialogue> pool = new List<BarkDialogue>();
    private List<BarkDialogue> objectsInUse = new List<BarkDialogue>();

    public GameObject testLock;

    public List<string> JoueurPerdUnePlante;
    public List<string> JoueurAttaque;
    public List<string> EnnemiMeurt;
    public List<string> EnnemiSeFaitBouger;
    public List<string> EnnemiAttaque;



    private void Start()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        GameObject temp;

        for (int i = 0; i < poolSize; i++)
        {
            temp = Instantiate(prefabToPool, transform);
            temp.SetActive(false);
            pool.Add(temp.GetComponent<BarkDialogue>());
        }
    }

    /*private void Update()
    {
        if (Input.GetButtonDown("Jump")){
            GetObjectFromPool(testLock, JoueurAttaque);
        }
    }*/

    public BarkDialogue GetObjectFromPool(GameObject ObjectToTrack, List<string> StateBark)
    {
        if (pool.Count > 0)
        {
            BarkDialogue pulledObject = pool[0];
            pool.RemoveAt(0);
            objectsInUse.Add(pulledObject);
            pulledObject.gameObject.SetActive(true);
            pulledObject.transform.DOScale(new Vector3(1f, 1f, 1f), 0.5f).SetEase(Ease.InOutSine, 0.5f);

            pulledObject.tracker = ObjectToTrack;

            pulledObject.StartCoroutine(pulledObject.DisplayLine(StateBark[Random.Range(0, StateBark.Count)]));
            return pulledObject;
        }
        return null;
    }

    public void ReturnObjectToPool(BarkDialogue objectToReturn)
    {
        objectToReturn.transform.position = transform.position;
        objectToReturn.transform.rotation = transform.rotation;
        objectToReturn.transform.parent = transform;

        objectToReturn.tracker = null;

        // Ici on reset tout autre paramètre modifié par le cycle de vie de l'objet
        objectToReturn.gameObject.SetActive(false);
        objectsInUse.Remove(objectToReturn);
        pool.Add(objectToReturn);
    }
}
