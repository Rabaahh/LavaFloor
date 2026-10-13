using UnityEngine;

namespace DontTouchTheFloor
{
    /// <summary>
    /// Displays the minimal UI title, controls reminder, and Level Complete announcement.
    /// Implemented via OnGUI for submission-safe, zero-dependency immediate-mode rendering.
    /// </summary>
    public class GameUI : MonoBehaviour
    {
        private GUIStyle titleStyle;
        private GUIStyle subtitleStyle;
        private GUIStyle bannerTitleStyle;
        private GUIStyle bannerSubtitleStyle;
        private GUIStyle infoStyle;
        private GUIStyle buttonStyle;
        private bool stylesInitialized = false;

        private void InitStyles()
        {
            if (stylesInitialized) return;

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            titleStyle.normal.textColor = new Color(1.0f, 0.9f, 0.4f);

            subtitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleCenter
            };
            subtitleStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f);

            bannerTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 32,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            bannerTitleStyle.normal.textColor = new Color(0.2f, 1.0f, 0.4f);

            bannerSubtitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleCenter
            };
            bannerSubtitleStyle.normal.textColor = Color.white;

            infoStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.UpperLeft
            };
            infoStyle.normal.textColor = new Color(0.95f, 0.95f, 0.95f);

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            stylesInitialized = true;
        }

        private void OnGUI()
        {
            InitStyles();

            // 1. Top Title: DON'T TOUCH THE FLOOR
            float screenW = Screen.width;
            float topBoxW = 420f;
            float topBoxH = 65f;
            float topBoxX = (screenW - topBoxW) * 0.5f;

            GUI.Box(new Rect(topBoxX, 15f, topBoxW, topBoxH), GUIContent.none);
            GUI.Label(new Rect(topBoxX, 18f, topBoxW, 30f), "DON'T TOUCH THE FLOOR", titleStyle);
            GUI.Label(new Rect(topBoxX, 48f, topBoxW, 25f), "Cross the bedroom on furniture to reach the exit door!", subtitleStyle);

            // 2. Bottom-left Controls & Stats
            float infoW = 340f;
            float infoH = 80f;
            GUI.Box(new Rect(15f, Screen.height - infoH - 15f, infoW, infoH), GUIContent.none);

            string fallText = GameManager.Instance != null ? $"Lava Falls: {GameManager.Instance.fallCount}" : "Lava Falls: 0";
            string infoContent = $"Controls: WASD / Arrows to Move | Space to Jump\nMouse: Look around | Esc: Release cursor\n{fallText} | Press R to Reset";
            GUI.Label(new Rect(25f, Screen.height - infoH - 10f, infoW - 20f, infoH - 10f), infoContent, infoStyle);

            // 3. Level Complete Banner
            if (GameManager.Instance != null && GameManager.Instance.isLevelComplete)
            {
                float modalW = 480f;
                float modalH = 210f;
                float modalX = (Screen.width - modalW) * 0.5f;
                float modalY = (Screen.height - modalH) * 0.5f;

                GUI.Box(new Rect(modalX, modalY, modalW, modalH), GUIContent.none);
                GUI.Label(new Rect(modalX, modalY + 20f, modalW, 40f), "LEVEL COMPLETE!", bannerTitleStyle);
                GUI.Label(new Rect(modalX, modalY + 70f, modalW, 30f), "You escaped the room without touching the floor!", bannerSubtitleStyle);
                GUI.Label(new Rect(modalX, modalY + 105f, modalW, 25f), $"Total Lava Falls: {GameManager.Instance.fallCount}", subtitleStyle);

                if (GUI.Button(new Rect(modalX + 140f, modalY + 145f, 200f, 40f), "Play Again (R)", buttonStyle))
                {
                    GameManager.Instance.RestartLevel();
                }
            }
        }
    }
}
