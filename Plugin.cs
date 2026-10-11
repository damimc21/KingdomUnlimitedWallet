using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
#if MONO
using BepInEx.Unity.Mono;
#else
using BepInEx.Unity.IL2CPP;
using Il2CppInterop.Runtime.Attributes;
#endif
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace KingdomUnlimitedWallet
{
    [BepInPlugin(Id, "Contadores sin limite", "1.2.0")]
#if MONO
    public class Plugin : BaseUnityPlugin
#else
    public class Plugin : BasePlugin
#endif
    {
        public const string Id = "com.codex.kingdom.unlimitedwallet";
        internal static ManualLogSource LogSource;
        internal static ConfigEntry<int> FontSize;
        internal static ConfigEntry<bool> ExtraResources;
        internal static bool Verify;

#if MONO
        private void Awake()
#else
        public override void Load()
#endif
        {
#if MONO
            LogSource = base.Logger;
#else
            LogSource = Log;
#endif
            FontSize = Config.Bind("Pantalla", "TamanoTexto", 24, new ConfigDescription("Tamano de los contadores (escala con la resolucion).",
                new AcceptableValueRange<int>(14, 48)));
            ExtraResources = Config.Bind("Pantalla", "OtrosRecursos", true, "Mostrar otros recursos de la bolsa si tienes alguno.");
            Verify = File.Exists(Path.Combine(Paths.PluginPath, "wallet-verify.flag"));
            // Verification runs pause simulation before the existing campaign advances.
            var harmony = new Harmony(Id);
            var patches = new List<(MethodInfo Target, string Prefix)>();
#if MONO
            const string applyData = "Persistent.IBehaviour.ApplyData";
#else
            const string applyData = "Persistent_IBehaviour_ApplyData";
#endif
            foreach (var method in new[] { "Awake", "Start", "SetCurrency", "AddCurrency", "SuckCurrency", "TryToGrabCurrency", applyData })
                patches.Add((RequireMethod(typeof(Wallet), method), nameof(EnsureCapacity)));
            // These methods only build the physical purse. Wallet amounts remain authoritative.
            foreach (var method in new[] { "ShowCurrency", "StartShow", "AddCurrency", "RemoveCurrency", "SpawnCurrency", "CurrencyFell" })
                patches.Add((RequireMethod(typeof(CurrencyBag), method), nameof(SkipBagVisual)));
            patches.Add((RequireMethod(typeof(Player), "CoinFellFromBag"), nameof(SkipBagVisual)));
            try
            {
                foreach (var patch in patches)
                    harmony.Patch(patch.Target, prefix: new HarmonyMethod(typeof(Plugin), patch.Prefix));
#if MONO
                gameObject.AddComponent<CounterHud>();
#else
                AddComponent<CounterHud>();
#endif
            }
            catch
            {
                harmony.UnpatchSelf();
                throw;
            }
            LogSource.LogInfo("Contadores sin limite cargados: cartera original, sin fisica ni desbordamiento de bolsa.");
        }

        private static MethodInfo RequireMethod(Type type, string name) =>
            AccessTools.Method(type, name) ?? throw new MissingMethodException(type.FullName, name);

        private static bool SkipBagVisual() => false;

        private static void EnsureCapacity(Wallet __instance)
        {
            // Villagers, bankers and enemies keep their original capacities.
            if (__instance.GetComponent<Player>() != null)
                __instance.TotalCapacity = int.MaxValue;
        }
    }

    public class CounterHud : MonoBehaviour
    {
        private Player[] players = Array.Empty<Player>();
        private float nextRefresh;
        private GameObject hudRoot;
        private readonly Dictionary<string, ResourceRow> rows = new();
        private readonly Dictionary<CurrencyType, IconAsset> icons = new();
        private Font nativeFont;
        // Kingdom's visible digits are baked at ten pixels. Some builds report
        // a larger line height, so the text rectangle must accommodate both.
        private const float GlyphHeight = 10;
#if !MONO
        private bool tested;
#endif

        private sealed class ResourceRow
        {
            internal GameObject Root;
            internal RectTransform Rect;
            internal Image Icon;
            internal Text Number;
        }

        private sealed class IconAsset
        {
            internal Sprite Sprite;
            internal Color Tint;
        }

#if !MONO
        public CounterHud(IntPtr pointer) : base(pointer) { }
#endif

        public void Update()
        {
            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + 0.3f;
            try
            {
                // Keep this API for Mono editions predating FindObjectsByType.
#pragma warning disable CS0618
                players = UnityEngine.Object.FindObjectsOfType<Player>()
                    .Where(p => p != null && p.wallet != null && p.gameObject.activeInHierarchy && p.hasLocalAuthority)
                    .OrderBy(p => p.playerId).ToArray();
#pragma warning restore CS0618
                foreach (var p in players)
                {
                    p.wallet.TotalCapacity = int.MaxValue;
                    var bag = p.GetCurrencyBag();
                    if (bag != null) bag.HideImmediate();
                }
                UpdateHud();
#if !MONO
                if (Plugin.Verify && players.Length > 0 && Managers.Inst != null && Managers.Inst.game != null && Managers.Inst.game.state == Game.State.Playing)
                {
                    Time.timeScale = 0;
                    if (!tested && NetworkBigBoss.HasWorldAuth)
                    {
                        tested = true;
                        VerifyWallet(players[0]);
                    }
                }
#endif
            }
            catch (Exception error) { Plugin.LogSource.LogError(error); }
        }

        private void UpdateHud()
        {
            if (players.Length == 0 || Managers.Inst == null || Managers.Inst.currency == null)
            {
                if (hudRoot != null) hudRoot.SetActive(false);
                return;
            }
            if (hudRoot == null) CreateHud();
            hudRoot.SetActive(true);
            var font = Language.current?.regularFont?.font;
            if (font != null && nativeFont != font)
            {
                nativeFont = font;
                Plugin.LogSource.LogInfo($"HUD: fuente del juego = {font.name}");
            }
            foreach (var row in rows.Values) row.Root.SetActive(false);
            float rowHeight = Math.Max(34, Plugin.FontSize.Value + 12);
            for (int n = 0; n < players.Length; n++)
            {
                var player = players[n];
                if (player == null || player.wallet == null) continue;
                int index = 0;
                // Each local monarch gets a separate screen corner without extra text.
                bool left = n % 2 != 0;
                float top = 24 + n / 2 * rowHeight * 8;
                UpdateRow(player, CurrencyType.Coins, left, top + index++ * rowHeight, rowHeight);
                UpdateRow(player, CurrencyType.Gems, left, top + index++ * rowHeight, rowHeight);
                if (!Plugin.ExtraResources.Value) continue;
                foreach (var type in CurrencyManager.AllCurrencyTypes)
                {
                    if (type == CurrencyType.Coins || type == CurrencyType.Gems || type == CurrencyType.Crown) continue;
                    if (player.wallet.GetCurrency(type) > 0)
                        UpdateRow(player, type, left, top + index++ * rowHeight, rowHeight);
                }
            }
            // Keep icons and digits in aligned columns, including when amounts grow.
            var visible = rows.Values.Where(r => r.Root.activeSelf).ToArray();
            if (visible.Length > 0)
            {
                float width = visible.Max(r => r.Rect.sizeDelta.x);
                foreach (var row in visible) row.Rect.sizeDelta = new Vector2(width, row.Rect.sizeDelta.y);
            }
        }

        private void CreateHud()
        {
            hudRoot = new GameObject("Kingdom Resource Counters");
            hudRoot.transform.SetParent(transform, false);
            var canvas = hudRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            canvas.pixelPerfect = true;
            var scaler = hudRoot.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1;
        }

#if !MONO
        [HideFromIl2Cpp]
#endif
        private IconAsset FindIcon(CurrencyType type)
        {
            if (!Managers.Inst.currency.TryGetData(type, out var config)) return null;
            SpriteRenderer renderer = null;
            Color tint = Color.white;
            if (config.Prefab != null)
            {
                var prefab = BiomeData.GetPrefabSwap(config.Prefab);
                if (prefab == null) prefab = config.Prefab;
                renderer = prefab.GetComponentsInChildren<SpriteRenderer>(true).FirstOrDefault(r => r.sprite != null);
                if (renderer != null) tint = renderer.color;
            }
            if ((renderer == null || renderer.sprite == null) && config.BagPrefab != null)
            {
                var prefab = BiomeData.GetPrefabSwap(config.BagPrefab);
                if (prefab == null) prefab = config.BagPrefab;
                renderer = prefab.GetComponent<SpriteRenderer>();
                if (prefab.colors != null && prefab.colors.Length > 0) tint = prefab.colors[0];
            }
            if (renderer == null || renderer.sprite == null)
            {
                var indicator = config.Indicator;
                if (indicator != null) renderer = indicator.Renderer;
                if (renderer != null) tint = renderer.color;
            }
            if (renderer == null || renderer.sprite == null) return null;
            tint.a = 1;
            var icon = new IconAsset { Sprite = renderer.sprite, Tint = tint };
            // Re-read biome swaps each refresh; Unity's Image handles packed sprite atlases.
            if (!icons.TryGetValue(type, out var previous) || previous.Sprite != icon.Sprite)
            {
                icons[type] = icon;
                Plugin.LogSource.LogInfo($"HUD: {type} = sprite {icon.Sprite.name}, {icon.Sprite.rect.width}x{icon.Sprite.rect.height}");
            }
            return icon;
        }

        private void UpdateRow(Player player, CurrencyType type, bool left, float top, float height)
        {
            string key = player.playerId + ":" + type;
            if (!rows.TryGetValue(key, out var row))
            {
                row = new ResourceRow();
                row.Root = new GameObject("Counter " + key);
                row.Root.transform.SetParent(hudRoot.transform, false);
                row.Rect = row.Root.AddComponent<RectTransform>();
                var iconObj = new GameObject("Native currency icon");
                iconObj.transform.SetParent(row.Root.transform, false);
                row.Icon = iconObj.AddComponent<Image>();
                row.Icon.raycastTarget = false;
                row.Icon.preserveAspect = true;
                var numberObj = new GameObject("Amount");
                numberObj.transform.SetParent(row.Root.transform, false);
                row.Number = numberObj.AddComponent<Text>();
                row.Number.raycastTarget = false;
                row.Number.alignment = TextAnchor.MiddleLeft;
                row.Number.horizontalOverflow = HorizontalWrapMode.Overflow;
                row.Number.verticalOverflow = VerticalWrapMode.Overflow;
                row.Number.color = new Color(1, .95f, .83f);
                row.Number.fontStyle = FontStyle.Normal;
                var outline = numberObj.AddComponent<Shadow>();
                outline.effectColor = new Color(.1f, .08f, .07f, .9f);
                outline.effectDistance = new Vector2(1, -1);
                rows[key] = row;
            }
            row.Root.SetActive(true);
            var corner = new Vector2(left ? 0 : 1, 1);
            row.Rect.anchorMin = row.Rect.anchorMax = row.Rect.pivot = corner;
            row.Rect.anchoredPosition = new Vector2(left ? 28 : -28, -top);
            row.Number.font = nativeFont;
            int nativeSize = Math.Max(1, Language.current.regularFont.size);
            // Some Windows font assets report dynamic=True but keep ten-pixel
            // glyphs. Request enough native line space before scaling the glyphs.
            if (nativeFont != null && nativeFont.dynamic) nativeSize = Math.Max(nativeSize, 24);
            row.Number.fontSize = nativeSize;
            // Scale the visible bitmap digits, keeping the game's native font settings.
            float textScale = Plugin.FontSize.Value / GlyphHeight;
            row.Number.transform.localScale = new Vector3(textScale, textScale, 1);
            row.Number.text = player.wallet.GetCurrency(type).ToString("N0", CultureInfo.GetCultureInfo("es-ES"));
            float numberWidth = Math.Max(32, row.Number.preferredWidth * textScale + 4);
            float iconSize = 28;
            row.Rect.sizeDelta = new Vector2(iconSize + 10 + numberWidth, height);
            var icon = FindIcon(type);
            row.Icon.enabled = icon != null;
            if (icon != null) { row.Icon.sprite = icon.Sprite; row.Icon.color = icon.Tint; }
            var iconRect = row.Icon.rectTransform;
            iconRect.anchorMin = iconRect.anchorMax = iconRect.pivot = new Vector2(0, .5f);
            // gem_spin has transparent space above the visible diamond. Compensate
            // for that padding so its visual centre lines up with the digits.
            iconRect.anchoredPosition = new Vector2(0, type == CurrencyType.Gems ? 4 : 0);
            iconRect.sizeDelta = new Vector2(iconSize, iconSize);
            var numberRect = row.Number.rectTransform;
            numberRect.anchorMin = numberRect.anchorMax = numberRect.pivot = new Vector2(0, .5f);
            numberRect.anchoredPosition = new Vector2(iconSize + 10, 0);
            // Unity can clip glyphs when the rectangle is shorter than the font's
            // preferred line height, even with vertical overflow enabled.
            numberRect.sizeDelta = new Vector2(numberWidth / textScale,
                Math.Max(height / textScale, row.Number.preferredHeight + 2));
        }

        public void OnDestroy()
        {
            if (hudRoot != null) UnityEngine.Object.Destroy(hudRoot);
        }

#if !MONO
        private static void Require(bool condition, string message)
        {
            if (!condition) throw new Exception("VERIFY FAIL: " + message);
            Plugin.LogSource.LogInfo("VERIFY PASS: " + message);
        }

        private void VerifyWallet(Player player)
        {
            var wallet = player.wallet;
            var snapshot = wallet.Persistent_IBehaviour_RetrieveData();
            try
            {
                Plugin.LogSource.LogInfo($"VERIFY original: coins={player.coins}, gems={player.gems}, player={player.playerId}");
                foreach (var type in new[] { CurrencyType.Coins, CurrencyType.Gems })
                {
                    wallet.SetCurrency(type, 1500);
                    Require(wallet.GetCurrency(type) == 1500, type + " accepts 1500 (above vanilla capacity)");
                    var pickup = wallet.PrepareCurrencyObjectBeforeMoving(type);
                    Require(wallet.SuckCurrency(pickup, false), type + " native pickup succeeds");
                    Require(wallet.GetCurrency(type) == 1501, type + " pickup increases counter to 1501");
                    var paid = wallet.MoveCurrencyObject(type, player.transform, Vector3.zero, true);
                    Require(paid != null && wallet.GetCurrency(type) == 1500, type + " native payment deducts one");
                }
                var saved = wallet.Persistent_IBehaviour_RetrieveData();
                wallet.SetCurrency(CurrencyType.Coins, 0);
                wallet.SetCurrency(CurrencyType.Gems, 0);
                wallet.Persistent_IBehaviour_ApplyData(saved);
                Require(wallet.Coins == 1500 && wallet.Gems == 1500, "native persistence round trip retains separate counts");
                var bag = player.GetCurrencyBag();
                Require(bag == null || bag._currencyObjects.Count == 0, "no physical currency objects in purse");
                wallet.SetCurrency(CurrencyType.Coins, 100000);
                wallet.SetCurrency(CurrencyType.Gems, 100000);
                Require(wallet.Coins == 100000 && wallet.Gems == 100000, "100000 coins and 100000 gems accepted independently");
            }
            catch (Exception error) { Plugin.LogSource.LogError(error); }
            finally
            {
                wallet.Persistent_IBehaviour_ApplyData(snapshot);
                Plugin.LogSource.LogInfo($"VERIFY restored: coins={player.coins}, gems={player.gems}");
            }
        }
#endif
    }
}
