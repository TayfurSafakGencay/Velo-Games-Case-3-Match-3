using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverPanel : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _titleText;

    [SerializeField]
    private TextMeshProUGUI _scoreText;

    [SerializeField]
    private List<Image> _stars;

    private int _score;

    private void Start()
    {
        gameObject.SetActive(false);
    }
    
    private void SetStars(int starCount)
    {
        for (int i = 0; i < _stars.Count; i++)
        {
            _stars[i].enabled = starCount > i;
        }
    }

    public void OnGameWin(int star, int finalScore)
    {
        gameObject.SetActive(true);
        
        SetStars(star);

        _titleText.text = "You Win";
        _scoreText.text = finalScore.ToString();
    }

    public void OnGameLose(int star, int finalScore)
    {
        gameObject.SetActive(true);
        
        SetStars(star);

        _titleText.text = "You Lose";
        _scoreText.text = finalScore.ToString();
    }

    public void OnReplayLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnNextLevel()
    {
        
    }
}