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
    [BepInPlugin(Id, "Contadores sin limite", "1.3.2")]
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
        internal static ConfigEntry<bool> UseProtection, LoseResources;
        internal static ConfigEntry<int> MaximumProtection, RecoveryPerResource;
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
            UseProtection = Config.Bind("Dificultad", "Escudos", false, "Vida independiente del dinero. F2: configurar. Un jugador y cooperativo local; online conserva el comportamiento original.");
            LoseResources = Config.Bind("Dificultad", "PerderRecursos", true, "Descontar y soltar recursos al recibir dano. Sin escudos y sin perdida, los golpes normales no penalizan; el robo especial de corona se conserva.");
            MaximumProtection = Config.Bind("Dificultad", "ResistenciaMaxima", 60, new ConfigDescription("Puntos de proteccion. Cada ataque consume su dano original, no un escudo entero por golpe.", new AcceptableValueRange<int>(1, 1000)));
            RecoveryPerResource = Config.Bind("Dificultad", "RecuperacionPorRecurso", 1, new ConfigDescription("Puntos recuperados por moneda, gema u otro recurso elegible realmente recogido. El dinero acumulado no cura.", new AcceptableValueRange<int>(1, 20)));
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
                CrownProtection.Install(harmony);
                WalletSettings.Install(harmony);
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
            LogSource.LogInfo("Contadores sin limite 1.3.2 cargados. F2: escudos y dificultad. Sin fisica ni desbordamiento de bolsa.");
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
        private IconAsset protectionIcon;
        private Texture2D protectionTexture;
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
            internal Text Separator;
            internal Text Maximum;
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
                CrownProtection.Tick(players);
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
            float rowHeight = Math.Max(36, Plugin.FontSize.Value + 14);
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
                if (CrownProtection.Enabled)
                {
                    var protection = CrownProtection.For(player);
                    if (protection != null)
                    {
                        if (protectionIcon == null) CreateProtectionIcon();
                        UpdateRow(player, CurrencyType.Crown, left, top + index++ * rowHeight, rowHeight,
                            protection.Current.ToString(), protectionIcon, protection.Maximum.ToString());
                    }
                }
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

#if !MONO
        [HideFromIl2Cpp]
#endif
        private void UpdateRow(Player player, CurrencyType type, bool left, float top, float height,
            string displayedAmount = null, IconAsset displayedIcon = null, string maximumAmount = null)
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
                row.Number = CreateNumber(row.Root, "Amount");
                row.Separator = CreateNumber(row.Root, "Protection separator");
                row.Maximum = CreateNumber(row.Root, "Protection maximum");
                rows[key] = row;
            }
            row.Root.SetActive(true);
            var corner = new Vector2(left ? 0 : 1, 1);
            row.Rect.anchorMin = row.Rect.anchorMax = row.Rect.pivot = corner;
            row.Rect.anchoredPosition = new Vector2(left ? 28 : -28, -top);
            // Scale the visible bitmap digits, keeping the game's native font settings.
            float textScale = Plugin.FontSize.Value / GlyphHeight;
            float iconSize = 28;
            float numberLeft = iconSize + 10;
            float amountWidth = PositionNumber(row.Number,
                displayedAmount ?? player.wallet.GetCurrency(type).ToString("N0", CultureInfo.GetCultureInfo("es-ES")),
                textScale, height, numberLeft);
            row.Separator.gameObject.SetActive(maximumAmount != null);
            row.Maximum.gameObject.SetActive(maximumAmount != null);
            if (maximumAmount != null)
            {
                // Kingdom's slash is much taller than its digits. Scale only the
                // separator, retaining full-size current and maximum values.
                float separatorWidth = PositionNumber(row.Separator, "/", textScale * .55f,
                    height, numberLeft + amountWidth + 3, -1);
                float maximumWidth = PositionNumber(row.Maximum, maximumAmount, textScale,
                    height, numberLeft + amountWidth + 3 + separatorWidth + 3);
                FitSeparator(row.Separator, row.Maximum);
                amountWidth += 3 + separatorWidth + 3 + maximumWidth;
            }
            float numberWidth = Math.Max(32, amountWidth + 4);
            row.Rect.sizeDelta = new Vector2(iconSize + 10 + numberWidth, height);
            var icon = displayedIcon ?? FindIcon(type);
            row.Icon.enabled = icon != null;
            if (icon != null) { row.Icon.sprite = icon.Sprite; row.Icon.color = icon.Tint; }
            var iconRect = row.Icon.rectTransform;
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0, .5f);
            iconRect.pivot = new Vector2(.5f, .5f);
            // gem_spin has transparent space above the visible diamond. Compensate
            // for that padding so its visual centre lines up with the digits.
            // Centre narrower sprites (especially the shield) within the same
            // icon column, and align their visible centres with the digit bodies.
            iconRect.anchoredPosition = new Vector2(iconSize / 2, -2 + (type == CurrencyType.Gems ? 4 : 0));
            iconRect.sizeDelta = new Vector2(iconSize, iconSize);
        }

        private static Text CreateNumber(GameObject parent, string name)
        {
            var numberObj = new GameObject(name);
            numberObj.transform.SetParent(parent.transform, false);
            var number = numberObj.AddComponent<Text>();
            number.raycastTarget = false;
            number.alignment = TextAnchor.MiddleLeft;
            number.horizontalOverflow = HorizontalWrapMode.Overflow;
            number.verticalOverflow = VerticalWrapMode.Overflow;
            number.color = new Color(1, .95f, .83f);
            number.fontStyle = FontStyle.Normal;
            var outline = numberObj.AddComponent<Shadow>();
            outline.effectColor = new Color(.1f, .08f, .07f, .9f);
            outline.effectDistance = new Vector2(1, -1);
            return number;
        }

        private float PositionNumber(Text number, string value, float scale, float height, float left, float y = 0)
        {
            number.font = nativeFont;
            int nativeSize = Math.Max(1, Language.current.regularFont.size);
            // Some font assets report dynamic=True but retain bitmap glyph sizes.
            if (nativeFont != null && nativeFont.dynamic) nativeSize = Math.Max(nativeSize, 24);
            number.fontSize = nativeSize;
            number.transform.localScale = new Vector3(scale, scale, 1);
            number.text = value;
            float width = number.preferredWidth * scale;
            var numberRect = number.rectTransform;
            numberRect.anchorMin = numberRect.anchorMax = numberRect.pivot = new Vector2(0, .5f);
            numberRect.anchoredPosition = new Vector2(left, y);
            // Unity can clip glyphs when the rectangle is shorter than the font's
            // preferred line height, even with vertical overflow enabled.
            numberRect.sizeDelta = new Vector2(width / scale + 2,
                Math.Max(height / scale, number.preferredHeight + 2));
            return width;
        }

        private static void FitSeparator(Text separator, Text digits)
        {
            // Fit the visible glyph, not the font's padded line rectangle. Keep
            // the narrow horizontal scale while aligning both ends to the digits.
            if (!GlyphBounds(digits, out float digitBottom, out float digitTop) ||
                !GlyphBounds(separator, out float slashBottom, out float slashTop)) return;
            float scaleY = digits.transform.localScale.y * (digitTop - digitBottom) / (slashTop - slashBottom);
            var scale = separator.transform.localScale;
            separator.transform.localScale = new Vector3(scale.x, scaleY, 1);
            var position = separator.rectTransform.anchoredPosition;
            position.y = digits.rectTransform.anchoredPosition.y + digitBottom * digits.transform.localScale.y - slashBottom * scaleY;
            separator.rectTransform.anchoredPosition = position;
        }

        private static bool GlyphBounds(Text text, out float bottom, out float top)
        {
            var generator = text.cachedTextGenerator;
            generator.Populate(text.text, text.GetGenerationSettings(text.rectTransform.rect.size));
            var vertices = generator.verts;
            bottom = float.PositiveInfinity;
            top = float.NegativeInfinity;
            // vertexCount excludes the dummy quad appended to the verts list.
            for (int i = 0; i < generator.vertexCount; i++)
            {
                float y = vertices[i].position.y / text.pixelsPerUnit;
                bottom = Math.Min(bottom, y);
                top = Math.Max(top, y);
            }
            return top > bottom;
        }

        public void OnDestroy()
        {
            WalletSettings.Close();
            CrownProtection.Flush();
            if (hudRoot != null) UnityEngine.Object.Destroy(hudRoot);
            if (protectionIcon?.Sprite != null) UnityEngine.Object.Destroy(protectionIcon.Sprite);
            if (protectionTexture != null) UnityEngine.Object.Destroy(protectionTexture);
        }

        private void CreateProtectionIcon()
        {
            // A tiny code-drawn pixel shield: no external asset or art dependency.
            string[] mask = { "..GGGGGGG..", ".GLLLLLLLG.", "GLBBBBBBBLG", "GLBBBBBBBLG", "GLBBBLBBBLG",
                "GLBBLLLBBLG", "GLBBBLBBBLG", ".GLBBBBBLG.", ".GLBBBBBLG.", "..GLBBBLG..", "...GLBLG...", "....GLG....", ".....G....." };
            protectionTexture = new Texture2D(11, 13, TextureFormat.RGBA32, false);
            protectionTexture.filterMode = FilterMode.Point;
            var pixels = new Color[11 * 13];
            for (int y = 0; y < 13; y++) for (int x = 0; x < 11; x++)
            {
                char p = mask[12 - y][x];
                pixels[y * 11 + x] = p == 'G' ? new Color(.62f, .43f, .19f) : p == 'L' ? new Color(.96f, .83f, .53f)
                    : p == 'B' ? new Color(.27f, .55f, .66f) : Color.clear;
            }
            protectionTexture.SetPixels(pixels); protectionTexture.Apply();
            protectionIcon = new IconAsset { Sprite = Sprite.Create(protectionTexture, new Rect(0, 0, 11, 13), new Vector2(.5f, .5f), 1), Tint = Color.white };
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
