using System;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace KingdomUnlimitedWallet
{
    // Manual hit testing avoids creating a second EventSystem in the game's UI.
    internal static class WalletSettings
    {
        internal static bool IsOpen { get; private set; }
        private static GameObject root;
        private static RectTransform panel;
        private static readonly Text[] labels = new Text[5];
        private static readonly Image[] backgrounds = new Image[5];
        private static int selected, polledFrame = -1, suppressThrough = -1;
        private static float previousScale;
        private static bool previousCursor;
        private static CursorLockMode previousLock;

        internal static void Install(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(Game), "Update"), prefix: new HarmonyMethod(typeof(WalletSettings), nameof(Poll)));
#if MONO
            const string receiveInput = "IControllable.ReceiveInput";
#else
            const string receiveInput = "IControllable_ReceiveInput";
#endif
            harmony.Patch(AccessTools.Method(typeof(Player), receiveInput), prefix: new HarmonyMethod(typeof(WalletSettings), nameof(AllowInput)));
            harmony.Patch(AccessTools.Method(typeof(Game), "CheckForPause"), prefix: new HarmonyMethod(typeof(WalletSettings), nameof(AllowInput)));
            harmony.Patch(AccessTools.Method(typeof(CursorSystem), "Update"), prefix: new HarmonyMethod(typeof(WalletSettings), nameof(AllowCursor)));
        }

        private static bool AllowInput() => !IsOpen && Time.frameCount > suppressThrough && !Input.GetKeyDown(KeyCode.F2);
        private static bool AllowCursor() { if (!IsOpen) return true; OwnPause(); return false; }
        private static void OwnPause() { Time.timeScale = 0; Cursor.visible = true; Cursor.lockState = CursorLockMode.None; }

        internal static void Poll()
        {
            if (polledFrame == Time.frameCount) return;
            polledFrame = Time.frameCount;
            var game = Managers.Inst?.game;
            if (game == null || !game.InPlayableState || Level.isGenerating) { Close(); return; }
            if (Input.GetKeyDown(KeyCode.F2)) { if (IsOpen) Close(); else Open(); return; }
            if (!IsOpen) return;
            if (Input.GetKeyDown(KeyCode.Escape)) { Close(); return; }
            OwnPause();
            if (Input.GetKeyDown(KeyCode.UpArrow)) selected = (selected + 4) % 5;
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.Tab)) selected = (selected + 1) % 5;
            if (Input.GetKeyDown(KeyCode.LeftArrow)) Change(-1);
            if (Input.GetKeyDown(KeyCode.RightArrow)) Change(1);
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)) Change(1);
            if (Input.GetMouseButtonDown(0) && RectTransformUtility.ScreenPointToLocalPointInRectangle(panel, Input.mousePosition, null, out var point))
            {
                for (int i = 0; i < 5; i++)
                    if (Mathf.Abs(point.x) < 310 && Mathf.Abs(point.y - (90 - 52 * i)) < 23)
                    { selected = i; Change(point.x < 0 ? -1 : 1); break; }
            }
            Refresh();
        }

        internal static void Open()
        {
            if (IsOpen) return;
            if (root == null) Build();
            selected = 0;
            previousScale = Time.timeScale; previousCursor = Cursor.visible; previousLock = Cursor.lockState;
            Managers.Inst?.kingdom?.playerOne?.ReleaseInput(); Managers.Inst?.kingdom?.playerTwo?.ReleaseInput();
            IsOpen = true; root.SetActive(true); OwnPause(); Refresh();
        }

        internal static void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            if (root != null) root.SetActive(false);
            Time.timeScale = previousScale; Cursor.visible = previousCursor; Cursor.lockState = previousLock;
            suppressThrough = Time.frameCount + 1;
            CrownProtection.Flush();
        }

        private static void Change(int direction)
        {
            if (selected == 0) Plugin.UseProtection.Value = !Plugin.UseProtection.Value;
            else if (selected == 1) Plugin.LoseResources.Value = !Plugin.LoseResources.Value;
            else if (selected == 2) Plugin.MaximumProtection.Value = Mathf.Clamp(Plugin.MaximumProtection.Value + direction * 5, 1, 1000);
            else if (selected == 3) Plugin.RecoveryPerResource.Value = Mathf.Clamp(Plugin.RecoveryPerResource.Value + direction, 1, 20);
            else Close();
        }

        private static void Refresh()
        {
            if (!IsOpen) return;
            labels[0].text = "Escudos: " + (Plugin.UseProtection.Value ? "SI" : "NO");
            labels[1].text = "Perder recursos: " + (Plugin.LoseResources.Value ? "SI" : "NO");
            labels[2].text = "-   Resistencia maxima: " + Plugin.MaximumProtection.Value + "   +";
            labels[3].text = "-   Recuperacion por recurso: " + Plugin.RecoveryPerResource.Value + "   +";
            labels[4].text = "Cerrar";
            for (int i = 0; i < 5; i++) backgrounds[i].color = i == selected ? new Color(.30f, .25f, .16f, 1) : new Color(.12f, .14f, .15f, 1);
        }

        private static void Build()
        {
            root = new GameObject("Unlimited Wallet difficulty settings");
            UnityEngine.Object.DontDestroyOnLoad(root);
            var canvas = root.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 120;
            var scaler = root.AddComponent<CanvasScaler>(); scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight; scaler.matchWidthOrHeight = 1;
            var shade = new GameObject("Shade"); shade.transform.SetParent(root.transform, false);
            var image = shade.AddComponent<Image>(); image.color = new Color(0, 0, 0, .7f); image.raycastTarget = false;
            image.rectTransform.anchorMin = Vector2.zero; image.rectTransform.anchorMax = Vector2.one; image.rectTransform.offsetMin = image.rectTransform.offsetMax = Vector2.zero;
            var sheet = new GameObject("Difficulty"); sheet.transform.SetParent(root.transform, false);
            var background = sheet.AddComponent<Image>(); background.color = new Color(.07f, .09f, .10f, 1); background.raycastTarget = false;
            panel = background.rectTransform; panel.anchorMin = panel.anchorMax = panel.pivot = new Vector2(.5f, .5f); panel.sizeDelta = new Vector2(680, 470);
            Label(sheet.transform, "UNLIMITED WALLET", 195, 32);
            Label(sheet.transform, "Dificultad - un jugador / coop local", 150, 20);
            for (int i = 0; i < 5; i++)
            {
                var row = new GameObject("Setting " + i); row.transform.SetParent(sheet.transform, false);
                backgrounds[i] = row.AddComponent<Image>(); backgrounds[i].raycastTarget = false;
                var rect = backgrounds[i].rectTransform; rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
                rect.sizeDelta = new Vector2(620, 46); rect.anchoredPosition = new Vector2(0, 90 - 52 * i);
                labels[i] = Label(row.transform, "", 0, 22);
            }
            Label(sheet.transform, "F2 / Esc: cerrar  |  Flechas: ajustar", -195, 20);
            root.SetActive(false);
        }

        private static Text Label(Transform parent, string value, float y, float size)
        {
            var obj = new GameObject(value.Length > 0 ? value : "Value"); obj.transform.SetParent(parent, false);
            var text = obj.AddComponent<Text>(); text.raycastTarget = false; text.text = value;
            text.font = Language.current?.regularFont?.font; text.fontSize = 24; text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow; text.verticalOverflow = VerticalWrapMode.Overflow;
            text.color = new Color(1, .94f, .79f); text.transform.localScale = new Vector3(size / 10, size / 10, 1);
            var rect = text.rectTransform; rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = new Vector2(0, y); rect.sizeDelta = new Vector2(620 / (size / 10), Math.Max(48, text.preferredHeight + 2));
            return text;
        }
    }
}
