using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ZeroGStacker.Audio;
using ZeroGStacker.Backend;
using ZeroGStacker.Core;
using ZeroGStacker.Gameplay;
using ZeroGStacker.Hazards;
using ZeroGStacker.Monetization;
using ZeroGStacker.Physics;
using ZeroGStacker.UI;
using ZeroGStacker.Visuals;

namespace ZeroGStacker.Editor
{
    public class ZeroGStackerEditorTools : EditorWindow
    {
        [MenuItem("Zero-G Stacker/Remote Config Sandbox")]
        public static void OpenSandbox()
        {
            GetWindow<ZeroGStackerEditorTools>("Zero-G Sandbox");
        }

        private float speedBase = 2.0f;
        private float mult10 = 1.15f;
        private float mult25 = 1.30f;
        private float mult50 = 1.50f;
        private float mult100 = 1.70f;
        private float gravity = 9.81f;
        private float collapseThresh = 22.0f;
        private float graceDur = 0.6f;

        private void OnGUI()
        {
            GUILayout.Label("Firebase Remote Config Dynamic Sandbox", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            speedBase = EditorGUILayout.FloatField("Base Drift Speed (0-10)", speedBase);
            mult10 = EditorGUILayout.FloatField("Speed Mult (10-25)", mult10);
            mult25 = EditorGUILayout.FloatField("Speed Mult (25-50)", mult25);
            mult50 = EditorGUILayout.FloatField("Speed Mult (50-100)", mult50);
            mult100 = EditorGUILayout.FloatField("Speed Mult (100+)", mult100);

            EditorGUILayout.Space();
            GUILayout.Label("Stability Math (§5)", EditorStyles.boldLabel);
            gravity = EditorGUILayout.FloatField("Gravity Constant", gravity);
            collapseThresh = EditorGUILayout.FloatField("Collapse Angle Threshold", collapseThresh);
            graceDur = EditorGUILayout.FloatField("Grace Duration (s)", graceDur);

            EditorGUILayout.Space();
            if (GUILayout.Button("Push Updates to Live Game"))
            {
                if (FirebaseManager.Instance != null)
                {
                    FirebaseManager.Instance.SetRemoteOverride(GameConstants.KeyDiffSpeedBase, speedBase.ToString());
                    FirebaseManager.Instance.SetRemoteOverride(GameConstants.KeyDiffSpeedMult10_25, mult10.ToString());
                    FirebaseManager.Instance.SetRemoteOverride(GameConstants.KeyDiffSpeedMult25_50, mult25.ToString());
                    FirebaseManager.Instance.SetRemoteOverride(GameConstants.KeyDiffSpeedMult50_100, mult50.ToString());
                    FirebaseManager.Instance.SetRemoteOverride(GameConstants.KeyDiffSpeedMult100Plus, mult100.ToString());
                    FirebaseManager.Instance.SetRemoteOverride(GameConstants.KeyStabilityGravity, gravity.ToString());
                    FirebaseManager.Instance.SetRemoteOverride(GameConstants.KeyStabilityCollapseThreshold, collapseThresh.ToString());
                    FirebaseManager.Instance.SetRemoteOverride(GameConstants.KeyStabilityGraceDuration, graceDur.ToString());
                }
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ApplyAllRemoteConfigs();
                }
                Debug.Log("[Sandbox] Remote config values pushed to active session!");
            }

            if (GUILayout.Button("Reset Player Save Data"))
            {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
                Debug.Log("[Sandbox] PlayerPrefs cleared.");
            }
        }

        [MenuItem("Zero-G Stacker/Build & Configure Main Scene")]
        public static void BuildMainScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Camera & Audio Listener
            var camGo = new GameObject("Main Camera");
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.03f, 0.03f, 0.08f, 1f); // Deep Cosmic Navy/Black
            cam.orthographic = true;
            cam.orthographicSize = 7.0f;
            camGo.AddComponent<AudioListener>();
            camGo.tag = "MainCamera";
            camGo.transform.position = new Vector3(0f, 3.5f, -10f);

            var camCtrl = camGo.AddComponent<CameraController>();
            camGo.AddComponent<ScreenShaker>();

            // 2. Parallax Starfield Background
            var starfieldGo = new GameObject("ParallaxStarfield");
            starfieldGo.AddComponent<ProceduralStarfield>();

            // 3. Audio Synthesizer
            var audioGo = new GameObject("AudioSynthesizer");
            audioGo.AddComponent<ProceduralSoundSynthesizer>();

            // 4. Managers Root
            var managersGo = new GameObject("Managers");
            managersGo.AddComponent<FirebaseManager>();
            managersGo.AddComponent<DailyChallengeService>();
            managersGo.AddComponent<StreakCalendarService>();
            managersGo.AddComponent<MilestoneService>();
            managersGo.AddComponent<AdMobMaxMediationManager>();
            managersGo.AddComponent<IAPManager>();

            // 5. Tower Stack & Base
            var towerGo = new GameObject("TowerStack");
            var towerStack = towerGo.AddComponent<TowerStack>();

            var basePlatform = new GameObject("BasePlatform");
            basePlatform.transform.SetParent(towerGo.transform, false);
            basePlatform.transform.position = new Vector3(0f, -2.5f, 0f);

            var baseMf = basePlatform.AddComponent<MeshFilter>();
            var baseMr = basePlatform.AddComponent<MeshRenderer>();
            baseMf.sharedMesh = ProceduralShapeGenerator.GetOrCreateMesh(ShapeType.Cube, 1.2f);
            var baseMat = ProceduralShapeGenerator.CreateNeonMaterial(new Color(0.3f, 0.4f, 0.6f, 1f));
            baseMr.material = baseMat;
            basePlatform.transform.localScale = new Vector3(2.4f, 0.4f, 1f);

            var pivotGo = new GameObject("StackPivot");
            pivotGo.transform.SetParent(towerGo.transform, false);
            pivotGo.transform.position = new Vector3(0f, -2.3f, 0f);

            // Reflection or serialized field setup for TowerStack
            var soTower = new SerializedObject(towerStack);
            soTower.FindProperty("basePlatform").objectReferenceValue = basePlatform.transform;
            soTower.FindProperty("pivotRoot").objectReferenceValue = pivotGo.transform;
            soTower.ApplyModifiedProperties();

            // 6. Shape Spawner
            var spawnerGo = new GameObject("ShapeSpawner");
            var spawner = spawnerGo.AddComponent<ShapeSpawner>();

            // 7. Pulse VFX
            var pulseVfxGo = new GameObject("PulseVFX");
            var pulseVfx = pulseVfxGo.AddComponent<PulseVFX>();

            // 8. Gravity Pulse Controller
            var pulseCtrlGo = new GameObject("GravityPulseController");
            var pulseCtrl = pulseCtrlGo.AddComponent<GravityPulseController>();

            var soPulse = new SerializedObject(pulseCtrl);
            soPulse.FindProperty("towerStack").objectReferenceValue = towerStack;
            soPulse.FindProperty("shapeSpawner").objectReferenceValue = spawner;
            soPulse.FindProperty("pulseVFX").objectReferenceValue = pulseVfx;
            soPulse.ApplyModifiedProperties();

            // 9. Hazard Manager
            var hazardGo = new GameObject("HazardManager");
            var hazardMgr = hazardGo.AddComponent<HazardManager>();
            var soHazard = new SerializedObject(hazardMgr);
            soHazard.FindProperty("towerStack").objectReferenceValue = towerStack;
            soHazard.ApplyModifiedProperties();

            // Configure Camera Controller
            var soCam = new SerializedObject(camCtrl);
            soCam.FindProperty("towerStack").objectReferenceValue = towerStack;
            soCam.ApplyModifiedProperties();

            // 10. UI Canvas & EventSystem
            var eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<EventSystem>();
            eventSystemGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            // Construct UI Screens hierarchy
            var uiMgrGo = new GameObject("UIManager");
            uiMgrGo.transform.SetParent(canvasGo.transform, false);
            var uiMgr = uiMgrGo.AddComponent<UIManager>();

            // Main Menu Screen
            var menuScreen = CreateUIContainer(canvasGo.transform, "MainMenuScreen");
            var menuTitle = CreateUIText(menuScreen.transform, "Title", "ZERO-G STACKER", 72, FontStyle.Bold, new Vector2(0, 500));
            var menuSubtitle = CreateUIText(menuScreen.transform, "Subtitle", "PULSE DRIFTING SHAPES INTO BALANCE", 28, FontStyle.Normal, new Vector2(0, 420));
            var playEndlessBtn = CreateUIButton(menuScreen.transform, "PlayEndlessBtn", "ENDLESS RUN", new Vector2(0, 100), new Vector2(480, 110), new Color(0f, 0.85f, 0.95f, 1f));
            var playDailyBtn = CreateUIButton(menuScreen.transform, "PlayDailyBtn", "DAILY SEEDED CHALLENGE", new Vector2(0, -40), new Vector2(480, 95), new Color(1f, 0.85f, 0.1f, 1f));
            var shopBtn = CreateUIButton(menuScreen.transform, "ShopBtn", "COSMETICS & IAP", new Vector2(0, -160), new Vector2(480, 85), new Color(0.7f, 0.3f, 1f, 1f));
            var chestBtn = CreateUIButton(menuScreen.transform, "ChestBtn", "DAILY CHEST (FREE)", new Vector2(0, -270), new Vector2(480, 85), new Color(0.2f, 0.9f, 0.4f, 1f));
            var lbBtn = CreateUIButton(menuScreen.transform, "LeaderboardBtn", "LEADERBOARDS", new Vector2(0, -380), new Vector2(480, 85), new Color(0.4f, 0.6f, 0.9f, 1f));
            var stardustText = CreateUIText(menuScreen.transform, "MenuStardust", "★ 100", 38, FontStyle.Bold, new Vector2(-360, 850));
            var streakText = CreateUIText(menuScreen.transform, "MenuStreak", "🔥 Day 1", 38, FontStyle.Bold, new Vector2(360, 850));

            // HUD Screen
            var hudScreen = CreateUIContainer(canvasGo.transform, "HUDScreen");
            hudScreen.SetActive(false);
            var hudView = hudScreen.AddComponent<HUDView>();
            var heightText = CreateUIText(hudScreen.transform, "HeightText", "0.0 m", 64, FontStyle.Bold, new Vector2(0, 780));
            var scoreText = CreateUIText(hudScreen.transform, "ScoreText", "0", 42, FontStyle.Normal, new Vector2(0, 710));
            var comboText = CreateUIText(hudScreen.transform, "ComboText", "1.5x COMBO", 44, FontStyle.Bold, new Vector2(0, 640));
            var hudStardust = CreateUIText(hudScreen.transform, "HudStardust", "★ 100", 36, FontStyle.Bold, new Vector2(-380, 860));
            var dailyBadge = CreateUIText(hudScreen.transform, "DailyBadge", "DAILY SEED", 30, FontStyle.Bold, new Vector2(380, 860));

            // Stability Gauge
            var gaugeBg = CreateUIImage(hudScreen.transform, "GaugeBg", new Vector2(0, -780), new Vector2(440, 28), new Color(0.15f, 0.15f, 0.25f, 0.8f));
            var gaugeFill = CreateUIImage(gaugeBg.transform, "GaugeFill", Vector2.zero, new Vector2(440, 28), Color.cyan);
            gaugeFill.type = Image.Type.Filled;
            gaugeFill.fillMethod = Image.FillMethod.Horizontal;
            gaugeFill.fillOrigin = (int)Image.OriginHorizontal.Left;

            var warnText = CreateUIText(hudScreen.transform, "WarningText", "CRITICAL LEAN!", 36, FontStyle.Bold, new Vector2(0, -720));
            var warnOverlay = CreateUIImage(hudScreen.transform, "WarnOverlay", Vector2.zero, new Vector2(1080, 1920), new Color(1f, 0f, 0f, 0f));
            warnOverlay.raycastTarget = false;
            warnText.gameObject.SetActive(false);
            warnOverlay.gameObject.SetActive(false);

            var stabBtn = CreateUIButton(hudScreen.transform, "StabilizerBtn", "⚡ STABILIZE", new Vector2(0, -860), new Vector2(340, 80), new Color(0.1f, 0.8f, 1f, 1f));
            var stabCount = CreateUIText(stabBtn.transform, "StabCount", "x2", 28, FontStyle.Bold, new Vector2(120, 0));

            var soHud = new SerializedObject(hudView);
            soHud.FindProperty("heightText").objectReferenceValue = heightText;
            soHud.FindProperty("scoreText").objectReferenceValue = scoreText;
            soHud.FindProperty("comboText").objectReferenceValue = comboText;
            soHud.FindProperty("stardustText").objectReferenceValue = hudStardust;
            soHud.FindProperty("stabilizerCountText").objectReferenceValue = stabCount;
            soHud.FindProperty("dailyChallengeBadgeText").objectReferenceValue = dailyBadge;
            soHud.FindProperty("leanGaugeFill").objectReferenceValue = gaugeFill;
            soHud.FindProperty("warningFlashOverlay").objectReferenceValue = warnOverlay;
            soHud.FindProperty("warningTimerText").objectReferenceValue = warnText;
            soHud.FindProperty("stabilizerButton").objectReferenceValue = stabBtn;
            soHud.ApplyModifiedProperties();

            // Game Over Screen
            var gameOverScreen = CreateUIContainer(canvasGo.transform, "GameOverScreen");
            gameOverScreen.SetActive(false);
            var goBg = CreateUIImage(gameOverScreen.transform, "DarkOverlay", Vector2.zero, new Vector2(1080, 1920), new Color(0f, 0f, 0.05f, 0.88f));
            var goTitle = CreateUIText(gameOverScreen.transform, "GameOverTitle", "TOWER COLLAPSED", 68, FontStyle.Bold, new Vector2(0, 480));
            var goHeight = CreateUIText(gameOverScreen.transform, "FinalHeight", "Height: 48.2 m", 46, FontStyle.Bold, new Vector2(0, 360));
            var goScore = CreateUIText(gameOverScreen.transform, "FinalScore", "Score: 1,420", 38, FontStyle.Normal, new Vector2(0, 290));
            var goStardust = CreateUIText(gameOverScreen.transform, "EarnedStardust", "+42 Stardust", 36, FontStyle.Bold, new Vector2(0, 220));

            var reviveBtn = CreateUIButton(gameOverScreen.transform, "ReviveBtn", "REVIVE STACK (AD)", new Vector2(0, 60), new Vector2(460, 95), new Color(0.15f, 0.9f, 0.35f, 1f));
            var doubleStardustBtn = CreateUIButton(gameOverScreen.transform, "DoubleStardustBtn", "2X STARDUST (AD)", new Vector2(0, 60), new Vector2(460, 95), new Color(1f, 0.75f, 0.1f, 1f));
            var restartBtn = CreateUIButton(gameOverScreen.transform, "RestartBtn", "TRY AGAIN", new Vector2(0, -65), new Vector2(460, 85), new Color(0.2f, 0.7f, 1f, 1f));
            var menuBtn = CreateUIButton(gameOverScreen.transform, "MenuBtn", "MAIN MENU", new Vector2(0, -170), new Vector2(460, 80), new Color(0.5f, 0.5f, 0.6f, 1f));

            var goView = gameOverScreen.AddComponent<GameOverView>();
            var soGov = new SerializedObject(goView);
            soGov.FindProperty("finalHeightText").objectReferenceValue = goHeight;
            soGov.FindProperty("finalScoreText").objectReferenceValue = goScore;
            soGov.FindProperty("stardustEarnedText").objectReferenceValue = goStardust;
            soGov.FindProperty("reviveButton").objectReferenceValue = reviveBtn;
            soGov.FindProperty("doubleStardustButton").objectReferenceValue = doubleStardustBtn;
            soGov.FindProperty("restartButton").objectReferenceValue = restartBtn;
            soGov.FindProperty("menuButton").objectReferenceValue = menuBtn;
            soGov.ApplyModifiedProperties();

            // Shop Modal
            var shopModal = CreateUIContainer(canvasGo.transform, "ShopModal");
            shopModal.SetActive(false);
            var shopBg = CreateUIImage(shopModal.transform, "ShopBg", Vector2.zero, new Vector2(900, 1400), new Color(0.08f, 0.08f, 0.15f, 0.96f));
            var shopTitle = CreateUIText(shopModal.transform, "ShopTitle", "COSMIC SHOP", 56, FontStyle.Bold, new Vector2(0, 580));
            var removeAdsBtn = CreateUIButton(shopModal.transform, "RemoveAdsBtn", "REMOVE ADS ($2.99)", new Vector2(0, 420), new Vector2(620, 85), new Color(0.3f, 0.8f, 0.95f, 1f));
            var removeAdsStatus = CreateUIText(removeAdsBtn.transform, "Status", "$2.99", 26, FontStyle.Bold, new Vector2(230, 0));
            var packSmall = CreateUIButton(shopModal.transform, "PackSmall", "500 STARDUST ($0.99)", new Vector2(0, 310), new Vector2(620, 80), new Color(0.9f, 0.7f, 0.2f, 1f));
            var packMed = CreateUIButton(shopModal.transform, "PackMed", "1500 STARDUST ($2.49)", new Vector2(0, 210), new Vector2(620, 80), new Color(0.9f, 0.7f, 0.2f, 1f));
            var packLarge = CreateUIButton(shopModal.transform, "PackLarge", "5000 STARDUST ($6.99)", new Vector2(0, 110), new Vector2(620, 80), new Color(0.9f, 0.7f, 0.2f, 1f));
            var passBtn = CreateUIButton(shopModal.transform, "PassBtn", "SEASON PASS ($4.99)", new Vector2(0, -10), new Vector2(620, 95), new Color(0.85f, 0.25f, 0.95f, 1f));
            var passStatus = CreateUIText(passBtn.transform, "Status", "LOCKED", 26, FontStyle.Bold, new Vector2(230, 0));
            var shopClose = CreateUIButton(shopModal.transform, "CloseBtn", "CLOSE", new Vector2(0, -560), new Vector2(300, 75), new Color(0.5f, 0.5f, 0.6f, 1f));

            var shopComp = shopModal.AddComponent<ShopModal>();
            var soShop = new SerializedObject(shopComp);
            soShop.FindProperty("removeAdsButton").objectReferenceValue = removeAdsBtn;
            soShop.FindProperty("stardustSmallButton").objectReferenceValue = packSmall;
            soShop.FindProperty("stardustMediumButton").objectReferenceValue = packMed;
            soShop.FindProperty("stardustLargeButton").objectReferenceValue = packLarge;
            soShop.FindProperty("seasonPassButton").objectReferenceValue = passBtn;
            soShop.FindProperty("removeAdsStatusText").objectReferenceValue = removeAdsStatus;
            soShop.FindProperty("seasonPassStatusText").objectReferenceValue = passStatus;
            soShop.FindProperty("closeButton").objectReferenceValue = shopClose;
            soShop.ApplyModifiedProperties();

            // Daily Chest Modal
            var chestModal = CreateUIContainer(canvasGo.transform, "DailyChestModal");
            chestModal.SetActive(false);
            var chestBg = CreateUIImage(chestModal.transform, "ChestBg", Vector2.zero, new Vector2(850, 1000), new Color(0.08f, 0.08f, 0.15f, 0.96f));
            var chestTitle = CreateUIText(chestModal.transform, "ChestTitle", "DAILY COSMIC CHEST", 52, FontStyle.Bold, new Vector2(0, 380));
            var chestRewardText = CreateUIText(chestModal.transform, "RewardText", "Open chest for 100-300 Stardust\nand chance of rare cosmetic skin!", 32, FontStyle.Normal, new Vector2(0, 150));
            var openChestBtn = CreateUIButton(chestModal.transform, "OpenChestBtn", "OPEN (WATCH AD)", new Vector2(0, -100), new Vector2(520, 95), new Color(0.2f, 0.9f, 0.4f, 1f));
            var chestClose = CreateUIButton(chestModal.transform, "CloseBtn", "LATER", new Vector2(0, -360), new Vector2(280, 70), new Color(0.5f, 0.5f, 0.6f, 1f));

            var chestComp = chestModal.AddComponent<DailyChestModal>();
            var soChest = new SerializedObject(chestComp);
            soChest.FindProperty("openChestButton").objectReferenceValue = openChestBtn;
            soChest.FindProperty("closeButton").objectReferenceValue = chestClose;
            soChest.FindProperty("rewardDisplayText").objectReferenceValue = chestRewardText;
            soChest.ApplyModifiedProperties();

            // Leaderboard Modal
            var lbModal = CreateUIContainer(canvasGo.transform, "LeaderboardModal");
            lbModal.SetActive(false);
            var lbBg = CreateUIImage(lbModal.transform, "LbBg", Vector2.zero, new Vector2(900, 1400), new Color(0.08f, 0.08f, 0.15f, 0.96f));
            var lbTitle = CreateUIText(lbModal.transform, "LbTitle", "COSMIC RECORDS", 52, FontStyle.Bold, new Vector2(0, 580));
            var tabGlobal = CreateUIButton(lbModal.transform, "TabGlobal", "GLOBAL ALL-TIME", new Vector2(-200, 480), new Vector2(360, 65), new Color(0.25f, 0.5f, 0.9f, 1f));
            var tabDaily = CreateUIButton(lbModal.transform, "TabDaily", "DAILY SEEDED", new Vector2(200, 480), new Vector2(360, 65), new Color(0.9f, 0.6f, 0.1f, 1f));
            var lbContent = CreateUIText(lbModal.transform, "Content", "Loading records...", 32, FontStyle.Normal, new Vector2(0, 60));
            lbContent.alignment = TextAnchor.UpperCenter;
            lbContent.rectTransform.sizeDelta = new Vector2(800, 700);
            var lbClose = CreateUIButton(lbModal.transform, "CloseBtn", "BACK", new Vector2(0, -560), new Vector2(280, 70), new Color(0.5f, 0.5f, 0.6f, 1f));

            var lbComp = lbModal.AddComponent<LeaderboardView>();
            var soLb = new SerializedObject(lbComp);
            soLb.FindProperty("titleText").objectReferenceValue = lbTitle;
            soLb.FindProperty("listContentText").objectReferenceValue = lbContent;
            soLb.FindProperty("tabGlobalButton").objectReferenceValue = tabGlobal;
            soLb.FindProperty("tabDailyButton").objectReferenceValue = tabDaily;
            soLb.FindProperty("closeButton").objectReferenceValue = lbClose;
            soLb.ApplyModifiedProperties();

            // Wire UIManager
            var soUi = new SerializedObject(uiMgr);
            soUi.FindProperty("mainMenuScreen").objectReferenceValue = menuScreen;
            soUi.FindProperty("hudScreen").objectReferenceValue = hudScreen;
            soUi.FindProperty("gameOverScreen").objectReferenceValue = gameOverScreen;
            soUi.FindProperty("shopModal").objectReferenceValue = shopModal;
            soUi.FindProperty("dailyChestModal").objectReferenceValue = chestModal;
            soUi.FindProperty("leaderboardModal").objectReferenceValue = lbModal;
            soUi.FindProperty("hudView").objectReferenceValue = hudView;
            soUi.FindProperty("gameOverView").objectReferenceValue = goView;
            soUi.FindProperty("shopView").objectReferenceValue = shopComp;
            soUi.FindProperty("playEndlessButton").objectReferenceValue = playEndlessBtn;
            soUi.FindProperty("playDailyButton").objectReferenceValue = playDailyBtn;
            soUi.FindProperty("openShopButton").objectReferenceValue = shopBtn;
            soUi.FindProperty("openChestButton").objectReferenceValue = chestBtn;
            soUi.FindProperty("openLeaderboardButton").objectReferenceValue = lbBtn;
            soUi.FindProperty("menuStardustText").objectReferenceValue = stardustText;
            soUi.FindProperty("menuStreakText").objectReferenceValue = streakText;
            soUi.ApplyModifiedProperties();

            // 11. GameManager Root
            var gmGo = new GameObject("GameManager");
            var gm = gmGo.AddComponent<GameManager>();
            var soGm = new SerializedObject(gm);
            soGm.FindProperty("towerStack").objectReferenceValue = towerStack;
            soGm.FindProperty("shapeSpawner").objectReferenceValue = spawner;
            soGm.FindProperty("pulseController").objectReferenceValue = pulseCtrl;
            soGm.FindProperty("hazardManager").objectReferenceValue = hazardMgr;
            soGm.FindProperty("cameraController").objectReferenceValue = camCtrl;
            soGm.FindProperty("uiManager").objectReferenceValue = uiMgr;
            soGm.ApplyModifiedProperties();

            // Save scene to Assets/Scenes/ZeroGStacker.unity
            string scenePath = "Assets/Scenes/ZeroGStacker.unity";
            EditorSceneManager.SaveScene(scene, scenePath);

            // Register in EditorBuildSettings
            EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(scenePath, true)
            };

            Debug.Log($"[ZeroGStacker] Successfully built and saved scene to {scenePath} and updated EditorBuildSettings!");
        }

        private static GameObject CreateUIContainer(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            return go;
        }

        private static Text CreateUIText(Transform parent, string name, string text, int fontSize, FontStyle style, Vector2 anchoredPos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var txt = go.AddComponent<Text>();
            txt.text = text;
            txt.fontSize = fontSize;
            txt.fontStyle = style;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.raycastTarget = false;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (txt.font == null) txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(700, 100);
            return txt;
        }

        private static Button CreateUIButton(Transform parent, string name, string label, Vector2 anchoredPos, Vector2 size, Color btnColor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var img = go.AddComponent<Image>();
            img.color = btnColor;
            img.raycastTarget = true;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;

            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            var txtGo = new GameObject("Text");
            txtGo.transform.SetParent(go.transform, false);
            var txt = txtGo.AddComponent<Text>();
            txt.text = label;
            txt.fontSize = (int)(size.y * 0.38f);
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = new Color(0.04f, 0.04f, 0.08f, 1f);
            txt.raycastTarget = false;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (txt.font == null) txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            var txtRt = txtGo.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.sizeDelta = Vector2.zero;

            return btn;
        }

        private static Image CreateUIImage(Transform parent, string name, Vector2 anchoredPos, Vector2 size, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = color;
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            return img;
        }

        [MenuItem("Zero-G Stacker/Build Windows Standalone")]
        public static void BuildWindowsPlayer()
        {
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = new[] { "Assets/Scenes/ZeroGStacker.unity" };
            buildPlayerOptions.locationPathName = "Builds/ZeroGStacker.exe";
            buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
            buildPlayerOptions.options = BuildOptions.None;

            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            var summary = report.summary;

            if (summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"[Build] Windows Standalone build succeeded: {summary.totalSize} bytes at Builds/ZeroGStacker.exe");
            }
            else
            {
                Debug.LogError($"[Build] Windows Standalone build failed: {summary.result}");
            }
        }
    }
}

