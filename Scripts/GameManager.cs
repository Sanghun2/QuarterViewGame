using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject menuCam;
    public GameObject gameCam;
    public Player player;
    public Boss boss;
    public int stage;
    public float playTime;
    public bool isBattle;
    public int enemyCntA;
    public int enemyCntB;
    public int enemyCntC;

    public GameObject menuPanel;
    public GameObject gamePanel;
    public Text maxScoreText;
    public Text scoreText;
    public Text stageText;
    public Text playTimeText;
    public Text playerHealthText;
    public Text playerAmmoText;
    public Text playerCoinText;

    public Image weapon1Img;
    public Image weapon2Img;
    public Image weapon3Img;
    public Image weaponRImg;

    public Text enemyAText;
    public Text enemyBText;
    public Text enemyCText;

    public RectTransform bossHealthGroup;
    public RectTransform bossHealthBar;

    void Awake()
    {
        string s = "0";
        if (PlayerPrefs.HasKey("MaxScore")) s = PlayerPrefs.GetInt("MaxScore").ToString();
        maxScoreText.text = s;
    }

    public void GameStart()
    {
        menuCam.SetActive(false);
        gameCam.SetActive(true);

        menuPanel.SetActive(false);
        gamePanel.SetActive(true);

        player.gameObject.SetActive(true);

        isBattle = true;
    }

    void Update()
    {
        if (isBattle)
        {
            playTime += Time.deltaTime;
        }
    }

    void LateUpdate()
    {
        scoreText.text = string.Format("{0:n0}", player.score);
        stageText.text = $"STAGE {stage}";

        int hour = (int)(playTime / 3600);
        int min = (int)((playTime - hour*3600 )/ 3600);
        int second = (int)(playTime % 60);
        playTimeText.text = $"{hour:00}:{min:00}:{second:00}";

        playerHealthText.text = $"{player.health} / {player.maxHealth}";
        playerCoinText.text = $"{player.coin} / {player.maxCoin}";

        if (player.equipWeapon == null || player.equipWeapon.type == Weapon.Type.Melee)
        {
            playerAmmoText.text = $"- / {player.maxAmmo}";
        }
        else
        {
            playerAmmoText.text = $"{player.equipWeapon.curAmmo} / {player.maxAmmo}";
        }

        weapon1Img.color = new Color(1, 1, 1, player.hasWeapons[0] ? 1 : 0); ;
        weapon2Img.color = new Color(1, 1, 1, player.hasWeapons[1] ? 1 : 0); ;
        weapon3Img.color = new Color(1, 1, 1, player.hasWeapons[2] ? 1 : 0); ;
        weaponRImg.color = new Color(1, 1, 1, player.hasGrenades > 0 ? 1 : 0); ;

        enemyAText.text = enemyCntA.ToString();
        enemyBText.text = enemyCntB.ToString();
        enemyCText.text = enemyCntC.ToString();

        bossHealthBar.localScale = new Vector3((float)boss.curHealth / boss.maxHealth, 1, 1);
    }
}
