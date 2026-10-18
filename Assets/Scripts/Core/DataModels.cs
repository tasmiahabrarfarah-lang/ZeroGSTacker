using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZeroGStacker.Core
{
    public enum ShapeType
    {
        Cube,
        Sphere,
        Prism
    }

    public enum ShapeColor
    {
        Cyan,
        Magenta,
        ElectricYellow,
        NeonGreen,
        DeepViolet,
        SolarOrange
    }

    public enum HazardType
    {
        None,
        Asteroids,
        GravityFlip,
        SolarWind,
        UnstableShapes
    }

    public enum SkinID
    {
        DefaultNeon,
        CosmicHolo,
        CyberGrid,
        VoidObsidian,
        SolarFlare,
        SingularityGold
    }

    [Serializable]
    public class PlacedShape
    {
        public ShapeType shapeType;
        public ShapeColor colorType;
        public Color renderColor;
        public Vector2 localPosition;
        public Vector2 worldPosition;
        public float horizontalOffset; // x offset relative to tower base center
        public float radius;
        public float mass;
        public GameObject instance;
        public Rigidbody2D rb2d;
        public Collider2D col2d;

        public PlacedShape(ShapeType type, ShapeColor color, Color renderCol, Vector2 localPos, Vector2 worldPos, float hOffset, float rad, float m, GameObject go)
        {
            shapeType = type;
            colorType = color;
            renderColor = renderCol;
            localPosition = localPos;
            worldPosition = worldPos;
            horizontalOffset = hOffset;
            radius = rad;
            mass = m;
            instance = go;
            if (go != null)
            {
                rb2d = go.GetComponent<Rigidbody2D>();
                col2d = go.GetComponent<Collider2D>();
            }
        }
    }

    [Serializable]
    public class StackData
    {
        public List<PlacedShape> blocks = new List<PlacedShape>();
        public Vector2 centerOfMass;
        public float leanAngle;
        public float angularVelocity;
        public float height;
        public float totalLean;

        public void Clear()
        {
            blocks.Clear();
            centerOfMass = Vector2.zero;
            leanAngle = 0f;
            angularVelocity = 0f;
            height = 0f;
            totalLean = 0f;
        }
    }

    [Serializable]
    public class PlayerData
    {
        public int stardust = 100;
        public int streak = 1;
        public int stabilizerCharges = 2;
        public int streakFreezes = 1;
        public SkinID cosmeticSkin = SkinID.DefaultNeon;
        public List<string> unlockedSkins = new List<string> { SkinID.DefaultNeon.ToString() };
        public List<string> unlockedMilestones = new List<string>();
        public bool noAdsPurchased = false;
        public bool seasonPassPurchased = false;
        public string lastLoginDateUtc = "";
    }

    [Serializable]
    public class RunState
    {
        public int score;
        public float height;
        public int blocksStacked;
        public float comboMultiplier = 1.0f;
        public List<HazardType> hazardsActive = new List<HazardType>();
        public bool isDailyChallenge;
        public int dailySeed;
        public bool reviveUsed;
        public bool isGameOver;
        public int stardustEarnedThisRun;

        public void Reset(bool daily = false, int seed = 0)
        {
            score = 0;
            height = 0f;
            blocksStacked = 0;
            comboMultiplier = 1.0f;
            hazardsActive.Clear();
            isDailyChallenge = daily;
            dailySeed = seed;
            reviveUsed = false;
            isGameOver = false;
            stardustEarnedThisRun = 0;
        }
    }
}
