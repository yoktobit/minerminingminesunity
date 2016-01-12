using UnityEngine;
using System.Collections;

public class MinerSaveGame {

    private static MinerSaveGame _instance;
    public static MinerSaveGame Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new MinerSaveGame();
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

    public void Load()
    {
        minerData[0] = new MinerData();
        minerData[1] = new MinerData();
        minerData[2] = new MinerData();
    }

    public void Save()
    {

    }

    public MinerSaveGame()
    {
        minerData = new MinerData[3];
    }
}
