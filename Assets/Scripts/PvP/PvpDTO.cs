using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace pvp
{
    public class Player
    {
        public string socketId;
        public string roomId;
        public float[] position;
        public float[] rotation;
        public float[] turretRotation;
        public float hp;
        public float hpfull;

        public Player()
        {
            position = new float[] { 0, 0, 0 };
            rotation = new float[] { 0, 0, 0 };
            turretRotation = new float[] { 0, 0, 0 };
        }

        public void SetPosition(float x, float y, float z)
        {
            position[0] = x;
            position[1] = y;
            position[2] = z;
        }

        public void SetRotaion(float x, float y, float z)
        {
            rotation[0] = x;
            rotation[1] = y;
            rotation[2] = z;
        }

        public void SetTurretRotation(float x, float y, float z)
        {
            turretRotation[0] = x;
            turretRotation[1] = y;
            turretRotation[2] = z;
        }
    }

    public class PlayerInit
    {
        public string socketId;
        public string roomId;
        public float[] position;
        public float[] rotation;
        public string mainColor;
        public string subColor;
        public int weaponId;
    }

    public class UnitInit
    {
        public string roomId;
        public int instanceId;
        public int prefabId;
        public string name;
        public float hitPointFull;
        public float[] position;
        public float[] rotation;
    }

    public class UnitData
    {
        public int instanceID;
        public float[] position;
        public float[] rotation;

        public void SetPosition(float x, float y, float z)
        {
            position[0] = x;
            position[1] = y;
            position[2] = z;
        }

        public void SetRotation(float x, float y, float z)
        {
            rotation[0] = x;
            rotation[1] = y;
            rotation[2] = z; 
        }
    }

    public class UnitList
    {
        public string roomId;
        public UnitData[] units;

        public UnitList()
        {
            roomId = string.Empty;
            units = new UnitData[]{};
        }
    }

    public class PlayerFire
    {
        public string socketId;
        public string roomId;
        public float[] turretRotation;
    }

    public class UnitHealth
    {
        public string roomId;
        public bool isEnemy;
        public int instanceID;
        public string name;
        public float hitPoint;
    }

    public class ClearUnit
    {
        public string roomId;
        public bool isEnemy;
        public int instanceID;
        public string name;
    }

    public class CollectibleInit
    {
        public string roomId;
        public int collectibleIndex;
        public float[] position;
    }

    public class AttackPlayer
    {
        public string socketId;
        public string roomId;
        public AttackInstance attackInstance;
    }

    public class PlayerDestroy
    {
        public string socketId;
        public string roomId;
    }
}

