using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private Button newRun, continueRun;
    [SerializeField] private Color activate, desactivate;
    [SerializeField] private TextMeshProUGUI txt;
    [SerializeField] private Image blackScreen;

    public Coroutine loadRoutine = null;
    public bool hasRun = false;

    private void Awake()
    {
        string path = Application.persistentDataPath + "/player.data";
        if (!File.Exists(path))
        {
            continueRun.interactable = false;
            txt.color = desactivate;
            hasRun = false;
        }
        else
        {
            hasRun = true;
        }
    }

    public void LoadRun()
    {
        loadRoutine = StartCoroutine(Load());
    }

    private IEnumerator Load()
    {
        blackScreen.DOFade(1f, 1f);
        yield return new WaitForSecondsRealtime(1.5f);
        SceneManager.LoadScene("RunScene");
    }

    public void NewRun()
    {
        string path = Application.persistentDataPath + "/player.data";
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        LoadRun();
    }
}
