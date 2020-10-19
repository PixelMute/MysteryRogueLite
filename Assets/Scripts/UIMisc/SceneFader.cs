using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneFader : MonoBehaviour
{

    #region FIELDS
    public Image fadeOutUIImage;
    public float fadeSpeed = 0.8f;

    public enum FadeDirection
    {
        In, //Alpha = 1
        Out // Alpha = 0
    }
    #endregion

    #region MONOBHEAVIOR
    void OnEnable()
    {
        StartCoroutine(Fade(FadeDirection.Out));
    }
    #endregion

    #region FADE
    public IEnumerator Fade(FadeDirection fadeDirection)
    {
        return Fade(fadeDirection, fadeSpeed);
    }


    public IEnumerator Fade(FadeDirection fadeDirection, float speed)
    {
        float alpha = (fadeDirection == FadeDirection.Out) ? 1 : 0;
        float fadeEndValue = (fadeDirection == FadeDirection.Out) ? 0 : 1;
        if (fadeDirection == FadeDirection.Out)
        {
            while (alpha >= fadeEndValue)
            {
                SetColorImage(ref alpha, fadeDirection, speed);
                yield return null;
            }
            fadeOutUIImage.enabled = false;
        }
        else
        {
            fadeOutUIImage.enabled = true;
            while (alpha <= fadeEndValue)
            {
                SetColorImage(ref alpha, fadeDirection, speed);
                yield return null;
            }
        }
    }
    #endregion

    #region HELPERS
    public IEnumerator FadeAndLoadScene(FadeDirection fadeDirection, string sceneToLoad)
    {
        yield return Fade(fadeDirection);
        SceneManager.LoadScene(sceneToLoad);
    }

    public IEnumerator FadeOutAndBackIn()
    {
        yield return Fade(FadeDirection.In);
        yield return Fade(FadeDirection.Out);
    }

    private void SetColorImage(ref float alpha, FadeDirection fadeDirection, float speed)
    {
        fadeOutUIImage.color = new Color(fadeOutUIImage.color.r, fadeOutUIImage.color.g, fadeOutUIImage.color.b, alpha);
        alpha += Time.deltaTime * (1.0f / speed) * ((fadeDirection == FadeDirection.Out) ? -1 : 1);
    }
    #endregion
}