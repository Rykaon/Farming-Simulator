using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NexusSystem : MonoBehaviour
{
    private Pathfinding pathfinding;
    private GridSystem gridSystem;
    public int lifePoints;

    public GameObject floatingText;

    [SerializeField] public Team team;

    public enum Team
    {
        Blue,
        Orange
    }

    void Start()
    {
        pathfinding = Pathfinding.instance;
        gridSystem = Camera.main.gameObject.GetComponent<GridSystem>();
    }

    public IEnumerator InstantiateFloatingText(int damage)
    {
        GameObject newFloatingText = Instantiate(floatingText, new Vector3(transform.position.x, 5f, transform.position.z), Quaternion.identity);
        newFloatingText.transform.LookAt(new Vector3(Camera.main.transform.GetChild(0).position.x, newFloatingText.transform.position.y, Camera.main.transform.GetChild(0).position.z));
        newFloatingText.transform.rotation = Quaternion.Euler(newFloatingText.transform.eulerAngles.x, newFloatingText.transform.eulerAngles.y - 180, newFloatingText.transform.eulerAngles.z);
        newFloatingText.GetComponent<TextMeshPro>().text = "-" + damage.ToString();
        yield return new WaitForSecondsRealtime(2f);
        Destroy(newFloatingText);
    }

    public void InflictDamage(int damage)
    {
        if (lifePoints > 0)
        {
            lifePoints -= damage;
            StartCoroutine(InstantiateFloatingText(damage));

            if (lifePoints <= 0)
            {
                lifePoints = 0;

                switch (team)
                {
                    case Team.Orange:
                        //Condition de victoire Blue et défaite Orange
                        Debug.Log("VICTOIRE DES BLEUS, DÉFAITE DES ORANGES");
                        break;

                    case Team.Blue:
                        //Condition de victoire Blue et défaite Orange
                        Debug.Log("VICTOIRE DES ORANGES, DÉFAITE DES BLEUS");
                        break;
                }
            }
        }
    }
}
