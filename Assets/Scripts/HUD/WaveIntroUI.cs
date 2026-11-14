using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// At the start of every wave a giant wave number smashes onto the screen, with a picture of each
// enemy coming and how many ("x10"), then fades away. About 5 seconds; the game keeps running.
public class WaveIntroUI : MonoBehaviour
{
    [SerializeField] private WaveSpawner waves;
    [SerializeField] private CanvasGroup group;            // fades everything together
    [SerializeField] private RectTransform number;         // the giant number
    [SerializeField] private TMP_Text numberText;
    [SerializeField] private RectTransform enemyRow;       // the pictures go in here
    [SerializeField] private GameObject enemyTemplate;     // one picture: an Image, with a "Count" text child
    [SerializeField] private float holdSeconds = 4f;

    private readonly List<GameObject> shownEnemies = new List<GameObject>();
    private Coroutine playing;

    private void Start()
    {
        group.alpha = 0f;
        enemyTemplate.SetActive(false);

        // Works in any level scene: find the WaveManager if nobody dragged one in
        if (waves == null) waves = FindFirstObjectByType<WaveSpawner>();
        if (waves != null) waves.WaveStarted += Show;
    }

    private void OnDestroy()
    {
        if (waves != null) waves.WaveStarted -= Show;
    }

    private void Show(int wave, List<WavePlan.EnemyCount> enemies)
    {
        numberText.text = wave.ToString();

        foreach (GameObject old in shownEnemies) Destroy(old);
        shownEnemies.Clear();
        foreach (WavePlan.EnemyCount enemy in enemies)
        {
            GameObject picture = Instantiate(enemyTemplate, enemyRow);
            picture.SetActive(true);
            picture.GetComponent<Image>().sprite = EnemyBase.IconFor(enemy.prefab);
            picture.GetComponentInChildren<TMP_Text>().text = "x" + enemy.count;
            shownEnemies.Add(picture);
        }

        if (playing != null) StopCoroutine(playing);
        playing = StartCoroutine(Play());
    }

    private IEnumerator Play()
    {
        // Smash: starts huge and see-through, slams down to full size...
        for (float t = 0f; t < 0.25f; t += Time.deltaTime)
        {
            float k = t / 0.25f;
            number.localScale = Vector3.one * Mathf.Lerp(4f, 1f, k * k);
            group.alpha = k;
            yield return null;
        }
        number.localScale = Vector3.one;
        group.alpha = 1f;

        // ...and shakes from the impact
        Vector2 home = number.anchoredPosition;
        for (float t = 0f; t < 0.3f; t += Time.deltaTime)
        {
            number.anchoredPosition = home + Random.insideUnitCircle * 22f * (1f - t / 0.3f);
            yield return null;
        }
        number.anchoredPosition = home;

        yield return new WaitForSeconds(holdSeconds);

        for (float t = 0f; t < 0.5f; t += Time.deltaTime)
        {
            group.alpha = 1f - t / 0.5f;
            yield return null;
        }
        group.alpha = 0f;
        playing = null;
    }
}
