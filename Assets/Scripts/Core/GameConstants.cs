using System.Collections.Generic;
using UnityEngine;

namespace ZeroGStacker.Core
{
    public static class GameConstants
    {
        // 6-Color Vibrant Neon Palette
        public static readonly Color ColorCyan = new Color(0.0f, 0.94f, 1.0f, 1.0f);        // #00F0FF
        public static readonly Color ColorMagenta = new Color(1.0f, 0.0f, 0.5f, 1.0f);       // #FF007F
        public static readonly Color ColorElectricYellow = new Color(1.0f, 0.9f, 0.0f, 1.0f); // #FFE600
        public static readonly Color ColorNeonGreen = new Color(0.22f, 1.0f, 0.08f, 1.0f);  // #39FF14
        public static readonly Color ColorDeepViolet = new Color(0.54f, 0.17f, 0.89f, 1.0f);// #8A2BE2
        public static readonly Color ColorSolarOrange = new Color(1.0f, 0.37f, 0.12f, 1.0f); // #FF5F1F

        public static Color GetColor(ShapeColor colorType)
        {
            switch (colorType)
            {
                case ShapeColor.Cyan: return ColorCyan;
                case ShapeColor.Magenta: return ColorMagenta;
                case ShapeColor.ElectricYellow: return ColorElectricYellow;
                case ShapeColor.NeonGreen: return ColorNeonGreen;
                case ShapeColor.DeepViolet: return ColorDeepViolet;
                case ShapeColor.SolarOrange: return ColorSolarOrange;
                default: return ColorCyan;
            }
        }

        // Gameplay Defaults
        public const float DefaultPulseDuration = 0.35f;
        public const float DefaultPerfectThreshold = 0.22f;
        public const float DefaultStackBaseWidth = 2.4f;
        public const float DefaultBaseShapeRadius = 0.7f;
        public const float DefaultBaseShapeMass = 1.0f;

        // Stability Math Constants
        public const float DefaultGravityConstant = 9.81f;
        public const float DefaultLeanSensitivity = 1.25f;
        public const float DefaultMaxLeanAngle = 35.0f;
        public const float DefaultCollapseThreshold = 22.0f; // degrees
        public const float DefaultGracePeriodDuration = 0.6f; // seconds
        public const float DefaultSpringDampingTime = 0.30f; // critically damped spring

        // Remote Config Keys (Section 6)
        public const string KeyDiffSpeedBase = "diff_speed_base";
        public const string KeyDiffSpeedMult10_25 = "diff_speed_mult_10_25";
        public const string KeyDiffSpeedMult25_50 = "diff_speed_mult_25_50";
        public const string KeyDiffSpeedMult50_100 = "diff_speed_mult_50_100";
        public const string KeyDiffSpeedMult100Plus = "diff_speed_mult_100_plus";
        public const string KeyDiffScalingPer10Units = "diff_scaling_per_10_units";
        public const string KeyHazardAsteroidHeight = "hazard_asteroid_height";
        public const string KeyHazardGravityFlipHeight = "hazard_gravity_flip_height";
        public const string KeyHazardSolarWindHeight = "hazard_solar_wind_height";
        public const string KeyStabilityGravity = "stability_gravity_constant";
        public const string KeyStabilityCollapseThreshold = "stability_collapse_threshold";
        public const string KeyStabilityGraceDuration = "stability_grace_duration";
        public const string KeyStabilityPerfectThreshold = "stability_perfect_threshold";
        public const string KeyAdInterstitialInterval = "ad_interstitial_interval";
    }
}
