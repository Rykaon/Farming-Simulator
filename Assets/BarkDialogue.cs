using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class BarkDialogue : MonoBehaviour
{
    [SerializeField] private float typingSpeed = 0.04f;

    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Vector3 offset;

    public GameObject tracker;

    private RectTransform rectTransform;

    [SerializeField] private BarkManager barkManager;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        barkManager = GameObject.Find("CanvasBark").GetComponent<BarkManager>();
    }

    private void Update()
    {
        if (tracker != null)
        {
            // Obtenir la position de la cible dans le monde
            Vector3 positionCible = tracker.transform.position + offset;

            // Convertir la position de la cible en espace de la caméra
            Vector3 positionEcran = Camera.main.WorldToScreenPoint(positionCible);

            // Convertir la position de l'écran en position locale du panneau
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform.parent as RectTransform, positionEcran, null, out Vector2 positionLocale);

            // Mettre à jour la position du panneau
            rectTransform.localPosition = positionLocale;
        }
    }

    public IEnumerator DisplayLine(string line)
    {
        var indexLetter = 0;
        dialogueText.text = "";

        foreach (char letter in line.ToCharArray())
        {
            indexLetter++;

            AudioManager.instance.PlayVariation("DialogueBoop", 0.1f, 0.5f);

            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        yield return new WaitForSeconds(1f);

        transform.DOScale(new Vector3(0f, 0f, 0f), 0.5f).SetEase(Ease.InOutSine, 0.5f);

        yield return new WaitForSeconds(1f);

        barkManager.ReturnObjectToPool(this);
    }
}
