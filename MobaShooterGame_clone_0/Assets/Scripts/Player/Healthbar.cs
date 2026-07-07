using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour
{
    public static Healthbar instance;
    public Slider slider;
    public TMP_Text healthText;
    public TMP_Text healthRegenText;
    public TMP_Text bulletStatusText;

    private void Awake()
    {
        instance = this;
    }
}
