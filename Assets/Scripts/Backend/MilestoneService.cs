using System;
using System.Collections.Generic;
using UnityEngine;
using ZeroGStacker.Core;

namespace ZeroGStacker.Backend
{
    [Serializable]
    public class MilestoneBadge
    {
        public float heightThreshold;
        public string title;
        public string description;
        public SkinID rewardSkin;
        public bool isUnlocked;

        public MilestoneBadge(float height, string t, string desc, SkinID skin)
        {
            heightThreshold = height;
            title = t;
            description = desc;
            rewardSkin = skin;
            isUnlocked = false;
        }
    }

    public class MilestoneService : MonoBehaviour
    {
        private static MilestoneService instance;
        public static MilestoneService Instance => instance;

        [SerializeField]
        private List<MilestoneBadge> badges = new List<MilestoneBadge>
        {
            new MilestoneBadge(25f, "Orbit Novice", "Reach 25m height", SkinID.CosmicHolo),
            new MilestoneBadge(50f, "Gravity Master", "Reach 50m height", SkinID.CyberGrid),
            new MilestoneBadge(100f, "Stratosphere Pioneer", "Reach 100m height", SkinID.VoidObsidian),
            new MilestoneBadge(250f, "Void Stacker", "Reach 250m height", SkinID.SolarFlare),
            new MilestoneBadge(500f, "Singularity Architect", "Reach 500m height", SkinID.SingularityGold)
        };

        public IReadOnlyList<MilestoneBadge> Badges => badges;

        public event Action<MilestoneBadge> OnMilestoneUnlocked;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
        }

        public void CheckMilestones(float currentHeight, PlayerData playerData)
        {
            for (int i = 0; i < badges.Count; i++)
            {
                var badge = badges[i];
                if (!badge.isUnlocked && currentHeight >= badge.heightThreshold)
                {
                    badge.isUnlocked = true;
                    if (!playerData.unlockedMilestones.Contains(badge.title))
                    {
                        playerData.unlockedMilestones.Add(badge.title);
                    }

                    string skinStr = badge.rewardSkin.ToString();
                    if (!playerData.unlockedSkins.Contains(skinStr))
                    {
                        playerData.unlockedSkins.Add(skinStr);
                    }

                    OnMilestoneUnlocked?.Invoke(badge);
                }
            }
        }
    }
}
