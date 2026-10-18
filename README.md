# Zero-G Stacker

> **One-line pitch:** A one-tap zero-gravity stacking game: drifting shapes are pulled onto a swaying tower using gravity pulses; the game never ends, only your stack does.

---

## 🚀 Overview & Tech Stack
- **Engine:** Unity 6 (6000.1.12f1) 2D with Universal Render Pipeline (URP).
- **Physics:** Rigidbody2D + custom analytical stability math (torque, center of mass, critically damped spring sway).
- **Backend:** Firebase (Anonymous Auth, Firestore Leaderboards, Remote Config difficulty balancing).
- **Monetization:** AdMob primary with AppLovin MAX mediation layer, plus In-App Purchases (IAP).
- **Platforms:** Target iOS and Android (Portrait mobile orientation).
- **Procedural Assets:** 100% procedural primitive meshes, neon rim-light URP shader, procedural parallax starfields, and runtime mathematical sound synthesis. Zero external art/audio assets required.

---

## 🎮 Core Mechanics & Game Loop

### 1. Per-Tap Gravity Pulse (§4)
- **Input:** Screen tap or mouse click triggers a gravity pulse targeting the nearest drifting shape (or raycast shape).
- **Pull Physics:** The shape accelerates toward the stack-top anchor point over `pulseDuration (~0.35s)` using an `EaseInQuad` curve ($t^2$ gravitational pull).
- **Precision Evaluation:**
  - **`placementError < perfectThreshold` (~0.22m):** **"Perfect!"** placement. Radial particle burst, subtle camera shake, pitch-shifted chime rising with combo multiplier, combo increment (+0.5x, max 5x), and auto-correct nudge to exact center.
  - **`placementError <= stackMaxBounds`:** Normal placement. Placed at arrival position, adds to tower lean, combo resets to 1.0x.
  - **`placementError > stackMaxBounds`:** **Soft Fail**. Shape falls off into space with dynamic physics, streak resets to 1, no game over.

### 2. Stability Math & Tower Sway (§5)
- **Torque per Block:**
  $$\tau_i = m_i \cdot \Delta x_i \cdot g$$
- **Total Lean:**
  $$\text{TotalLean} = \frac{\sum \tau_i}{\max(h, 1.0)}$$
  *(Taller stacks are naturally more sensitive, ramping up difficulty dynamically)*.
- **Dynamic Sway:** Lean angle smoothly interpolates toward target angle using a critically damped spring (`Mathf.SmoothDampAngle`).
- **Collapse Condition:** Tower collapses if $|\text{leanAngle}| > \text{collapseThreshold}$ ($22^\circ$) continuously for more than $0.6\text{s}$. Counter-weight placements within the $0.6\text{s}$ grace period restore balance!
- **Stabilizer Charges:** Tap the stabilizer button to instantly zero out accumulated torque and realign the stack center.

---

## 📈 Endless Difficulty Curve (§6)
Height-driven progression exposed to **Firebase Remote Config**:

| Height Band | Drift Speed | Shape Pool | Active Hazards |
| :--- | :--- | :--- | :--- |
| **0–10m** | Base speed (`2.0`) | Cubes only | None |
| **10–25m** | +15% (`2.3`) | Cubes + Spheres | None |
| **25–50m** | +30% (`2.6`) | Cubes + Spheres + Prisms | Asteroid Drifters |
| **50–100m** | +50% (`3.0`) | All shapes, faster rotation | Gravity-Flip Zones |
| **100m+** | +70% (`3.4`), scaling +5%/10m | All + Unstable Wobblers | Solar Wind Gusts |

---

## 💰 Monetization Architecture (§7)
- **Rewarded Video:**
  1. *Game Over (first collapse):* Revive with stack intact (strictly once per run).
  2. *Game Over (after revive used):* 2x Stardust multiplier for the completed run.
  3. *Daily Chest:* Watch ad to open chest for 100–300 Stardust + chance to unlock cosmetic skins.
- **Interstitial Ads:** Frequency-capped (default every 3 runs, remote config tuned) and **strictly shown at natural breaks** (Game Over / Main Menu transition). Never during an active run.
- **In-App Purchases:**
  - Remove Ads ($2.99)
  - Stardust Bundles ($0.99, $2.49, $6.99)
  - Cosmetic Season Pass ($4.99)

---

## 🗓️ Retention Systems (§8)
- **Daily Seeded Challenge:** Generates a daily RNG seed from UTC date hash (`yyyy-MM-dd`). Every player experiences the exact same drifting shape sequence. Dedicated daily leaderboard resets at UTC midnight.
- **Streak Calendar:** Consecutive daily login rewards escalating across 7 days. Missed days are protected by purchasable/earnable **Streak Freezes**.
- **Milestone Badges:** Unlocked automatically at heights **25m, 50m, 100m, 250m, 500m**, granting exclusive cosmetic titles and skins.

---

## 🛠️ Editor Tools & Sandbox
Navigate to the top menu in Unity:
1. **`Zero-G Stacker > Build & Configure Main Scene`**: Automatically builds the entire scene hierarchy, connects all components, UI views, camera, shaders, and sets `Assets/Scenes/ZeroGStacker.unity` in EditorBuildSettings.
2. **`Zero-G Stacker > Remote Config Sandbox`**: Live editor window allowing designers to tweak drift speeds, gravity constants, collapse angles, and grace timers live during playtesting.
