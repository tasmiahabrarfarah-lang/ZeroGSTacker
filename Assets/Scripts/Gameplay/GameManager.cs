using System;
using UnityEngine;
using UnityEngine.InputSystem;
using ZeroGStacker.Audio;
using ZeroGStacker.Backend;
using ZeroGStacker.Core;
using ZeroGStacker.Hazards;
using ZeroGStacker.Monetization;
using ZeroGStacker.Physics;
using ZeroGStacker.UI;
using ZeroGStacker.Visuals;

namespace ZeroGStacker.Gameplay
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager instance;
        public static GameManager Instance => instance;

        [Header("Components")]
        [SerializeField] private TowerStack towerStack;
        [SerializeField] private ShapeSpawner shapeSpawner;
        [SerializeField] private GravityPulseController pulseController;
        [SerializeField] private HazardManager hazardManager;
        [SerializeField] private CameraController cameraController;
        [SerializeField] private UIManager uiManager;

        [Header("State")]
        private PlayerData playerData = new PlayerData();
        private RunState runState = new RunState();
        private bool isGameActive = false;

        public PlayerData Player => playerData;
        public RunState Run => runState;
        public TowerStack Tower => towerStack;
        public bool IsGameActive => isGameActive;

        public event Action<RunState> OnRunStateUpdated;
        public event Action<PlayerData> OnPlayerDataUpdated;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;

            LoadPlayerData();
        }

        private void Start()
        {
            // Subscribe to Firebase Remote Config updates
            if (FirebaseManager.Instance != null)
            {
                FirebaseManager.Instance.OnRemoteConfigLoaded += ApplyAllRemoteConfigs;
                ApplyAllRemoteConfigs();
            }

            // Check login streak calendar
            if (StreakCalendarService.Instance != null)
            {
                StreakCalendarService.Instance.CheckDailyStreak(playerData);
            }

            // Wire tower callbacks
            if (towerStack != null)
            {
                towerStack.OnTowerCollapsed += HandleTowerCollapsed;
            }

            // Wire pulse callbacks
            if (pulseController != null)
            {
                pulseController.OnBlockPlaced += HandleBlockPlaced;
                pulseController.OnBlockMissed += HandleBlockMissed;
            }

            // Start in Menu state
            if (uiManager != null)
            {
                uiManager.ShowMainMenu();
            }
        }

        private void OnDestroy()
        {
            if (FirebaseManager.Instance != null)
            {
                FirebaseManager.Instance.OnRemoteConfigLoaded -= ApplyAllRemoteConfigs;
            }
        }

        public void ApplyAllRemoteConfigs()
        {
            if (FirebaseManager.Instance == null) return;

            var fb = FirebaseManager.Instance;

            // Section 6: Endless Difficulty Curve remote tuning
            float baseSpeed = fb.GetRemoteFloat(GameConstants.KeyDiffSpeedBase, 2.0f);
            float m10 = fb.GetRemoteFloat(GameConstants.KeyDiffSpeedMult10_25, 1.15f);
            float m25 = fb.GetRemoteFloat(GameConstants.KeyDiffSpeedMult25_50, 1.30f);
            float m50 = fb.GetRemoteFloat(GameConstants.KeyDiffSpeedMult50_100, 1.50f);
            float m100 = fb.GetRemoteFloat(GameConstants.KeyDiffSpeedMult100Plus, 1.70f);
            float scalePer10 = fb.GetRemoteFloat(GameConstants.KeyDiffScalingPer10Units, 0.05f);

            if (shapeSpawner != null)
            {
                shapeSpawner.ApplyRemoteConfig(baseSpeed, m10, m25, m50, m100, scalePer10);
            }

            // Hazard thresholds
            float astH = fb.GetRemoteFloat(GameConstants.KeyHazardAsteroidHeight, 25.0f);
            float flipH = fb.GetRemoteFloat(GameConstants.KeyHazardGravityFlipHeight, 50.0f);
            float windH = fb.GetRemoteFloat(GameConstants.KeyHazardSolarWindHeight, 100.0f);

            if (hazardManager != null)
            {
                hazardManager.ApplyRemoteConfig(astH, flipH, windH);
            }

            // Stability parameters
            float grav = fb.GetRemoteFloat(GameConstants.KeyStabilityGravity, GameConstants.DefaultGravityConstant);
            float collapseThresh = fb.GetRemoteFloat(GameConstants.KeyStabilityCollapseThreshold, GameConstants.DefaultCollapseThreshold);
            float graceDur = fb.GetRemoteFloat(GameConstants.KeyStabilityGraceDuration, GameConstants.DefaultGracePeriodDuration);
            float perfThresh = fb.GetRemoteFloat(GameConstants.KeyStabilityPerfectThreshold, GameConstants.DefaultPerfectThreshold);

            if (towerStack != null)
            {
                towerStack.ApplyRemoteConfig(grav, collapseThresh, graceDur, GameConstants.DefaultLeanSensitivity);
            }
            if (pulseController != null)
            {
                pulseController.ApplyRemoteConfig(GameConstants.DefaultPulseDuration, perfThresh);
            }
        }

        public void StartNewRun(bool isDailyChallenge = false)
        {
            isGameActive = true;
            int seed = 0;

            if (isDailyChallenge && DailyChallengeService.Instance != null)
            {
                seed = DailyChallengeService.Instance.GetDailySeed();
                shapeSpawner?.SetDailySeed(seed);
            }
            else
            {
                shapeSpawner?.ClearAll();
            }

            runState.Reset(isDailyChallenge, seed);
            towerStack?.ResetTower();
            hazardManager?.ResetHazards();
            cameraController?.ResetCamera();

            // Populate initial drifting shapes
            if (shapeSpawner != null && towerStack != null)
            {
                shapeSpawner.MaintainShapeCount(towerStack.GetTopAnchorPoint(), towerStack.Height);
            }

            if (uiManager != null)
            {
                uiManager.ShowHUD();
            }

            OnRunStateUpdated?.Invoke(runState);
        }

        private void Update()
        {
            if (!isGameActive) return;

            // Check tap / click input
            HandlePlayerInput();

            // Maintain drifting shapes
            if (shapeSpawner != null && towerStack != null && !towerStack.IsCollapsed)
            {
                shapeSpawner.MaintainShapeCount(towerStack.GetTopAnchorPoint(), towerStack.Height);
            }
        }

        private void HandlePlayerInput()
        {
            bool tapTriggered = false;
            Vector2 tapPos = Vector2.zero;

            // Touch input
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                tapTriggered = true;
                tapPos = Touchscreen.current.primaryTouch.position.ReadValue();
            }
            // Mouse input
            else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                tapTriggered = true;
                tapPos = Mouse.current.position.ReadValue();
            }

            if (tapTriggered)
            {
                // Verify not tapping on UI
                if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                {
                    pulseController?.TryFirePulse(tapPos);
                }
            }
        }

        private void HandleBlockPlaced(PlacedShape shape, bool isPerfect)
        {
            runState.blocksStacked++;
            runState.height = towerStack.Height;

            int earnedStardust = 2;

            if (isPerfect)
            {
                // +comboMultiplier, max 5x
                runState.comboMultiplier = Mathf.Min(runState.comboMultiplier + 0.5f, 5.0f);
                int scorePoints = Mathf.RoundToInt(100 * runState.comboMultiplier);
                runState.score += scorePoints;
                earnedStardust = Mathf.RoundToInt(5 * runState.comboMultiplier);

                ProceduralSoundSynthesizer.Instance?.PlayPerfectPlacement(Mathf.RoundToInt(runState.comboMultiplier * 2));
            }
            else
            {
                runState.comboMultiplier = 1.0f;
                runState.score += 50;
            }

            runState.stardustEarnedThisRun += earnedStardust;
            playerData.stardust += earnedStardust;

            // Check milestone unlocks
            MilestoneService.Instance?.CheckMilestones(runState.height, playerData);

            SavePlayerData();
            OnRunStateUpdated?.Invoke(runState);
            OnPlayerDataUpdated?.Invoke(playerData);
        }

        private void HandleBlockMissed()
        {
            // Soft fail: streak/combo resets, run continues
            runState.comboMultiplier = 1.0f;
            OnRunStateUpdated?.Invoke(runState);
        }

        private void HandleTowerCollapsed()
        {
            isGameActive = false;
            runState.isGameOver = true;

            ProceduralSoundSynthesizer.Instance?.PlayCollapse();
            ScreenShaker.Instance?.Shake(0.6f, 0.35f);

            // Record completed run for interstitial ad frequency capping
            AdMobMaxMediationManager.Instance?.RecordRunCompleted();

            // Submit scores to leaderboards
            string lbId = runState.isDailyChallenge && DailyChallengeService.Instance != null
                ? DailyChallengeService.Instance.GetDailyLeaderboardId()
                : "global_height";

            FirebaseManager.Instance?.SubmitLeaderboardScoreAsync(lbId, runState.height, runState.score);

            SavePlayerData();

            // Show Game Over UI
            uiManager?.ShowGameOver(runState);
        }

        public void UseStabilizerCharge()
        {
            if (playerData.stabilizerCharges <= 0 || towerStack == null || towerStack.IsCollapsed) return;

            playerData.stabilizerCharges--;
            towerStack.StabilizeTower();
            ProceduralSoundSynthesizer.Instance?.PlayStabilizer();
            ScreenShaker.Instance?.Shake(0.2f, 0.12f);

            SavePlayerData();
            OnPlayerDataUpdated?.Invoke(playerData);
        }

        public void ReviveWithStack()
        {
            if (runState.reviveUsed) return;

            runState.reviveUsed = true;
            runState.isGameOver = false;
            isGameActive = true;

            towerStack?.ReviveStack();
            uiManager?.ShowHUD();
            OnRunStateUpdated?.Invoke(runState);
        }

        public void ApplyDoubleStardustReward()
        {
            playerData.stardust += runState.stardustEarnedThisRun;
            runState.stardustEarnedThisRun *= 2;
            SavePlayerData();
            OnPlayerDataUpdated?.Invoke(playerData);
            OnRunStateUpdated?.Invoke(runState);
        }

        public void SavePlayerData()
        {
            string json = JsonUtility.ToJson(playerData);
            PlayerPrefs.SetString("ZGS_PlayerData", json);
            PlayerPrefs.Save();
        }

        private void LoadPlayerData()
        {
            string json = PlayerPrefs.GetString("ZGS_PlayerData", "");
            if (!string.IsNullOrEmpty(json))
            {
                playerData = JsonUtility.FromJson<PlayerData>(json);
            }
            if (playerData.stabilizerCharges < 2) playerData.stabilizerCharges = 2;
        }
    }
}
