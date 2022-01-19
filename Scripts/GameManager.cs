using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject menuCam;
    public GameObject gameCam;
    public GameObject itemShop;
    public GameObject weaponShop;
    public GameObject startZone;
    public Player player;
    public Boss boss;
    public int stage;
    public float playTime;
    public bool isBattle;
    public int enemyCntA;
    public int enemyCntB;
    public int enemyCntC;
    public int enemyCntD;

    public GameObject menuPanel;
    public GameObject gamePanel;
    public GameObject overPanel;
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

    public Transform[] enemyZones;
    public GameObject[] enemies;
    public List<int> enemyList = new List<int>();

    public Text curScoreText;
    public Text bestText;

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

        isBattle = false;
    }

    public void StageStart()
    {
        itemShop.SetActive(false);
        weaponShop.SetActive(false);
        startZone.SetActive(false);

        foreach (var zone in enemyZones)
        {
            zone.gameObject.SetActive(true);
        }

        isBattle = true;
        StartCoroutine(InBattle());
    }

    public void StageEnd()
    {
        //플레이어 원위치 
        player.transform.position = Vector3.up * 1.95f;

        itemShop.SetActive(true);
        weaponShop.SetActive(true);
        startZone.SetActive(true);

        foreach (var zone in enemyZones)
        {
            zone.gameObject.SetActive(false);
        }

        isBattle = false;
        stage++;
    }

    public void GameOver()
    {
        gamePanel.SetActive(false);
        overPanel.SetActive(true);
        curScoreText.text = scoreText.text;

        int maxScore = PlayerPrefs.GetInt("MaxScore");
        if (maxScore < player.score)
        {
            bestText.gameObject.SetActive(true);
            PlayerPrefs.SetInt("MaxScore", player.score);
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(0);
    }

    IEnumerator InBattle()
    {
        if (stage % 5 == 0)
        {
            enemyCntD++;
            GameObject instantEnemy = Instantiate(enemies[3], enemyZones[0].position, enemyZones[0].rotation);
            Enemy enemy = instantEnemy.GetComponent<Enemy>();
            enemy.target = player.transform;
            boss = instantEnemy.GetComponent<Boss>();
        }
        else
        {
            for (int i = 0; i < stage; i++)
            {
                int ran = Random.Range(0, 3);
                enemyList.Add(ran);

                switch (ran)
                {
                    case 0:
                        enemyCntA++;
                        break;
                    case 1:
                        enemyCntB++;
                        break;
                    case 2:
                        enemyCntC++;
                        break;
                    default:
                        break;
                }
            }

            while (enemyList.Count > 0)
            {
                int ranZone = Random.Range(0, 4);
                GameObject instantEnemy = Instantiate(enemies[enemyList[0]], enemyZones[ranZone].position, enemyZones[ranZone].rotation);
                Enemy enemy = instantEnemy.GetComponent<Enemy>();
                enemy.target = player.transform;
                enemy.gameManager = this;
                enemyList.RemoveAt(0);
                yield return new WaitForSeconds(4f);
            }
        }

        //남은 몬스터의 수를 검사 
        while (enemyCntA + enemyCntB + enemyCntC + enemyCntD > 0)
        {
            yield return null;
        }

        yield return new WaitForSeconds(4f);
        boss = null;
        StageEnd();
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

        if (boss != null)
        {
            bossHealthGroup.anchoredPosition = Vector3.down * 30;
            bossHealthBar.localScale = new Vector3((float)boss.curHealth / boss.maxHealth, 1, 1);
        }
        else
        {
            bossHealthGroup.anchoredPosition = Vector3.up * 200;
        }
    }
}
