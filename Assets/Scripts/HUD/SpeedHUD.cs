using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Sketch spot A: a Galactic Racer style speedometer.
public class SpeedHUD : MonoBehaviour
{
    [SerializeField] private PlayerCombat player;
    [SerializeField] private TMP_Text speedValue;
    [SerializeField] private Image ring;                  // Image Type: Filled, Radial 360
    [SerializeField] private float topSpeed = 24f;        // m/s that fills the ring (a full sprint)
    [SerializeField] private float maxFill = 0.5f;        // 0.5 = half a circle
    [SerializeField] private float numberScale = 3.6f;    // m/s to km/h
    [SerializeField] private Color walkColor = new Color(1f, 0.85f, 0.45f);
    [SerializeField] private Color sprintColor = new Color(1f, 0.4f, 0.1f);

    private int shownSpeed = -1;

    private void Start()
    {
        // Works in any level scene: find the player if nobody dragged one in
        if (player == null) player = FindFirstObjectByType<PlayerCombat>();
    }

    private void Update()
    {
        float speed = player.CurrentSpeed;
        ring.fillAmount = Mathf.Clamp01(speed / topSpeed) * maxFill;

        Color color = player.IsSprinting ? sprintColor : walkColor;
        ring.color = color;
        speedValue.color = color;

        int number = Mathf.RoundToInt(speed * numberScale);
        if (number != shownSpeed)
        {
            shownSpeed = number;
            speedValue.text = number.ToString();
        }
    }
}
