using TMPro;
using UnityEngine;

public class LoadingText : MonoBehaviour
{
    public float TimeBetweenChange;
    private int count = 0;
    private float timeSinceChange = 0;
    private TextMeshProUGUI Text;

    private void Awake()
    {
        Text = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        timeSinceChange += Time.deltaTime;
        if (timeSinceChange >= TimeBetweenChange)
        {
            count = (count + 1) % 4;
            timeSinceChange = 0;
            switch (count)
            {
                case 0:
                    Text.text = "Loading";
                    break;
                case 1:
                    Text.text = "Loading.";
                    break;
                case 2:
                    Text.text = "Loading..";
                    break;
                case 3:
                    Text.text = "Loading...";
                    break;
            }
        }
    }
}
