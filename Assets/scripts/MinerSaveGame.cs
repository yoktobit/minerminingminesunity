using UnityEngine;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.IO;
using System;

[Serializable]
public class MinerSaveGame {

    private static MinerSaveGame _instance;
    public static MinerSaveGame Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = MinerSaveGame.Load();
            }
            return _instance;
        }
    }

    public MinerData[] minerData;

    private MinerData _current;
    public MinerData Current
    {
        get
        {
            if (_current == null)
            {
                if (minerData[0] == null)
                {
                    minerData[0] = new MinerData();
                }
                _current = minerData[0];
            }
            return _current;
        }
        set
        {
            _current = value;
        }
    }

    public static string saveGameFileName = Application.persistentDataPath + "/minerdata.dat";

    public static MinerSaveGame Load()
    {
        if (File.Exists(saveGameFileName))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream fs = File.Open(saveGameFileName, FileMode.Open);
            MinerSaveGame saveGame = (MinerSaveGame)formatter.Deserialize(fs);
            fs.Close();
            return saveGame;
        }
        else
        {
            MinerSaveGame newGame = new MinerSaveGame();
            newGame.minerData[0] = new MinerData();
            newGame.minerData[1] = new MinerData();
            newGame.minerData[2] = new MinerData();
            return newGame;
        }
    }

    public static void Save()
    {
        System.IO.FileStream fs = System.IO.File.Open(saveGameFileName, System.IO.FileMode.Create);
        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(fs, Instance);
        fs.Close();
    }

    public MinerSaveGame()
    {
        minerData = new MinerData[3];
    }
}
