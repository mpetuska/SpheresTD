﻿using UnityEngine;
using System.Collections;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {
    public static GameController controll;
    public string fileName = "Save_1";
    public GameObject loadErrorTooltip;

    public float sfxVolume = 1f;
    public float musicVolume = 1f;
    public AudioSource backgroundMusic;
    public bool muteSFX = false;
    public bool muteMusic = false;

    public float diffMultiplier = 1.2f;
    public int level = 0;
    public int hp = 10;
    public bool inGame = false;

	void Awake()
    {
        if (controll == null)
        {
            controll = this;
            DontDestroyOnLoad(transform.gameObject);            
            backgroundMusic = transform.GetComponent<AudioSource>();
            Screen.SetResolution(600, 900, false);            
        }
        else Destroy(transform.gameObject);
    }    

    public void Save()
    {
        GameObject nodes = GameObject.Find("Nodes");        
        int nodeCount = nodes.transform.childCount;
        string[] nodeInfo = new string[nodeCount];
        for (int i = 0; i < nodeCount; i++)
        {
            GameObject currentTurret = nodes.transform.GetChild(i).GetComponent<NodeController>().turret;
            if (currentTurret) nodeInfo[i] = currentTurret.GetComponent<TurretController>().type;
        }

        PlayerData playerData = new PlayerData();
        playerData.health = StatMaster.controll.hp;
        playerData.gold = StatMaster.controll.gold;
        playerData.level = StatMaster.controll.level;
        int wave = StatMaster.controll.wave - 1;        
        if (wave < 0) wave = 0;
        playerData.wave = wave;
        playerData.nodes = nodeInfo;
        playerData.difficultyMultiplier = diffMultiplier;

        BinaryFormatter bFormatter = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/" + fileName);
        Debug.Log("File saved at: " + Application.persistentDataPath);

        bFormatter.Serialize(file, playerData);
        file.Close();
    }

    public void Load(int levelIndex = 0)
    {
        if (File.Exists(Application.persistentDataPath + "/" + fileName) && levelIndex == 0)
        {            
            BinaryFormatter bFormatter = new BinaryFormatter();            
            FileStream file = File.Open(Application.persistentDataPath + "/" + fileName, FileMode.Open);
            PlayerData playerData = (PlayerData)bFormatter.Deserialize(file);
            file.Close();

            StartCoroutine(LoadLevel(playerData));                    
        }
        else if (levelIndex == 0)
        {
            PlayerData tempData = null;
            StartCoroutine(LoadLevel(tempData));
        }
        else StartCoroutine(LoadLevel(levelIndex));
    } 
    
    
    IEnumerator LoadLevel(PlayerData data = null)
    {
        if (data == null)
        {
            Debug.Log("No save file found!");
            GameObject tooltip = (GameObject)Instantiate(loadErrorTooltip, controll.transform);                    
            Destroy(tooltip, 2f);
            yield break;
        }

        Time.timeScale = 0;
        Fader.controll.FadeOut(0.1f);        
        while (!Fader.controll.stable) yield return null;
        controll.diffMultiplier = data.difficultyMultiplier;
        SceneManager.LoadScene(data.level + 1, LoadSceneMode.Single);
        while (Application.isLoadingLevel) yield return null;
        StatMaster stats = StatMaster.controll;
        stats.hp = data.health;     stats.hpValue.text = stats.hp.ToString();
        stats.gold = data.gold;     stats.goldValue.text = stats.gold.ToString();
        stats.level = data.level;   if (data.level == 0) stats.levelValue.text = "SURVIVAL"; else stats.levelValue.text = stats.level.ToString();
        stats.wave = data.wave;     stats.waveValue.text = stats.wave.ToString();

        for (int i = 0; i < GameObject.Find("Nodes").transform.childCount; i++)
        {
            GameObject node = GameObject.Find("Nodes").transform.GetChild(i).gameObject;
            node.GetComponent<NodeController>().BuildByType(data.nodes[i]);
        }
        Fader.controll.FadeIn(0.1f);
        while (!Fader.controll.stable) yield return null;
        Time.timeScale = 1;
        controll.inGame = true;
    }

    IEnumerator LoadLevel(int levelIndex = 1)
    {
        Time.timeScale = 0;
        Fader.controll.FadeOut(0.1f);
        while (!Fader.controll.stable) yield return null;
        SceneManager.LoadScene(levelIndex, LoadSceneMode.Single);
        while (Application.isLoadingLevel) yield return null;
        Debug.Log("Level " + (levelIndex - 1) + " loaded!");
        Fader.controll.FadeIn(0.1f);
        while (!Fader.controll.stable) yield return null;
        Time.timeScale = 1;
        controll.inGame = true;
    }

    public IEnumerator Starter()
    {
        Fader.controll.FadeOut(0.1f);
        while (!Fader.controll.stable) yield return null;
        SceneManager.LoadScene(1);
        while (Application.isLoadingLevel) yield return null;
        
        Fader.controll.FadeIn(0.1f);
    }
}

[Serializable]
public class PlayerData
{
    public int health, gold, level, wave;
    public string[] nodes; //Stores the turret indexes for each node. 
    public float difficultyMultiplier;
}