using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections;

public class TimerUI : MonoBehaviour
{
    public float timeRemaining = 10;
    public bool timerIsRunning = false;
    public TextMeshProUGUI timeText;
    private bool timeIsRunningOut;

    Coroutine boomBoom;


    public void DisplayTime(float timeToDisplay)
    {
        timeToDisplay += 1;

        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

        timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        if (timeToDisplay <= 10)
        {
            timeIsRunningOut = true;
        }
    }

    public void ToggleBoomBoom(bool isActive)
    {
        if (isActive)
        {
            boomBoom = StartCoroutine(BoomBoom());
        }
        else
        {
            if (boomBoom != null)
            {
                timeIsRunningOut = false;
                timeText.color = Color.white;
                StopCoroutine(boomBoom);
            }
        }
        
    }

    public IEnumerator BoomBoom()
    {
        transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f),0.5f).SetEase(Ease.InOutSine);
        if (timeIsRunningOut)
        {
            timeText.DOColor(Color.red,0.1f).SetEase(Ease.InOutSine);
        }
        yield return new WaitForSecondsRealtime(0.5f);


        transform.DOScale(new Vector3(1f, 1f, 1f), 0.5f).SetEase(Ease.InOutSine);
        if (timeIsRunningOut)
        {
            timeText.DOColor(Color.white, 0.1f).SetEase(Ease.InOutSine);
        }
        yield return new WaitForSecondsRealtime(0.5f);

        boomBoom = StartCoroutine(BoomBoom());
    }
}