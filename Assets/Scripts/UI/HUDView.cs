using UnityEngine;
using UnityEngine.UI;
using ZeroGStacker.Core;
using ZeroGStacker.Gameplay;
using ZeroGStacker.Physics;

namespace ZeroGStacker.UI
{
    public class HUDView : MonoBehaviour
    {
        [Header("Text Displays")]
        [SerializeField] private Text heightText;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text comboText;
        [SerializeField] private Text stardustText;
        [SerializeField] private Text stabilizerCountText;
        [SerializeField] private Text dailyChallengeBadgeText;

        [Header("Stability Gauge")]
        [SerializeField] private Image leanGaugeFill;
        [SerializeField] private Image warningFlashOverlay;
        [SerializeField] private Text warningTimerText;

        [Header("Controls")]
        [SerializeField] private Button stabilizerButton;

        private void Start()
        {
            if (stabilizerButton != null)
            {
                stabilizerButton.onClick.AddListener(OnStabilizerClicked);
            }
        }

        public void UpdateHUD(RunState run, PlayerData player, TowerStack tower)
        {
            if (heightText != null) heightText.text = $"{run.height:F1} m";
            if (scoreText != null) scoreText.text = run.score.ToString();
            if (stardustText != null) stardustText.text = $"★ {player.stardust}";
            if (stabilizerCountText != null) stabilizerCountText.text = $"x{player.stabilizerCharges}";

            if (comboText != null)
            {
                if (run.comboMultiplier > 1.0f)
                {
                    comboText.gameObject.SetActive(true);
                    comboText.text = $"{run.comboMultiplier:F1}x COMBO";
                }
                else
                {
                    comboText.gameObject.SetActive(false);
                }
            }

            if (dailyChallengeBadgeText != null)
            {
                dailyChallengeBadgeText.gameObject.SetActive(run.isDailyChallenge);
                if (run.isDailyChallenge)
                {
                    dailyChallengeBadgeText.text = "DAILY SEED";
                }
            }

            // Stability gauge
            if (tower != null)
            {
                float absLean = Mathf.Abs(tower.LeanAngle);
                float ratio = Mathf.Clamp01(absLean / tower.CollapseThreshold);

                if (leanGaugeFill != null)
                {
                    leanGaugeFill.fillAmount = ratio;
                    leanGaugeFill.color = Color.Lerp(Color.cyan, Color.red, ratio);
                }

                if (warningFlashOverlay != null)
                {
                    warningFlashOverlay.gameObject.SetActive(tower.IsInGracePeriod);
                    if (tower.IsInGracePeriod)
                    {
                        float alpha = Mathf.PingPong(Time.time * 8f, 0.4f) + 0.1f;
                        warningFlashOverlay.color = new Color(1f, 0f, 0f, alpha);
                    }
                }

                if (warningTimerText != null)
                {
                    warningTimerText.gameObject.SetActive(tower.IsInGracePeriod);
                    if (tower.IsInGracePeriod)
                    {
                        float timeLeft = Mathf.Max(0f, GameConstants.DefaultGracePeriodDuration * (1f - tower.GraceProgress));
                        warningTimerText.text = $"CRITICAL LEAN! {timeLeft:F1}s";
                    }
                }
            }
        }

        private void OnStabilizerClicked()
        {
            GameManager.Instance?.UseStabilizerCharge();
        }
    }
}
