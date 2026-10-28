using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using BepInEx;
using Coatsink.Common;
using HarmonyLib;
using UnityEngine;

namespace KingdomUnlimitedWallet
{
    internal static class CrownProtection
    {
        private static readonly Dictionary<string, ProtectionPool> pools = new();
        private static readonly Dictionary<string, bool> crowns = new();
        private static readonly HashSet<string> resetCampaigns = new();
        private static string statePath;
        private static bool loaded, dirty, onlineNotice;
        private static float nextSave;
        internal static bool Available => !NetworkBigBoss.IsOnline && NetworkBigBoss.HasWorldAuth;
        internal static bool Enabled => Plugin.UseProtection.Value && Available;

        internal static void Install(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(Player), "OnReceiveDamageHandler"),
                prefix: new HarmonyMethod(typeof(CrownProtection), nameof(ReceiveDamage)));
            harmony.Patch(AccessTools.Method(typeof(Wallet), "SuckCurrency"),
                prefix: new HarmonyMethod(typeof(CrownProtection), nameof(BeforePickup)),
                postfix: new HarmonyMethod(typeof(CrownProtection), nameof(AfterPickup)));
            harmony.Patch(AccessTools.Method(typeof(Player), nameof(Player.LoseCrown)),
                postfix: new HarmonyMethod(typeof(CrownProtection), nameof(CrownLost)));
            harmony.Patch(AccessTools.Method(typeof(GlobalSaveData), nameof(GlobalSaveData.CreateNewCampaign)),
                postfix: new HarmonyMethod(typeof(CrownProtection), nameof(NewCampaign)));
        }

        private static string Key(Player player)
        {
            var campaign = CampaignSaveData.current;
            var global = GlobalSaveData.loaded;
            if (campaign == null || global == null) return null;
            // Island DateTime is reconstructed by Unity on load. Use serialized
            // campaign fields and explicitly invalidate a slot when it is replaced.
            return global.currentCampaign + ":" + global.currentChallenge + ":" + campaign.realStartDateTime + ":" +
                campaign.reign + ":" + player.playerId;
        }

        private static void NewCampaign(int campaignIndex, int challengeId)
        {
            resetCampaigns.Add(campaignIndex + ":" + challengeId + ":");
            Load();
            ApplyCampaignResets();
        }

        private static void ApplyCampaignResets()
        {
            if (!loaded || resetCampaigns.Count == 0) return;
            var removed = new List<string>();
            foreach (string key in pools.Keys)
                foreach (string prefix in resetCampaigns)
                    if (key.StartsWith(prefix, StringComparison.Ordinal)) { removed.Add(key); break; }
            foreach (string key in removed) { pools.Remove(key); crowns.Remove(key); }
            resetCampaigns.Clear(); dirty = true;
        }

        internal static ProtectionPool For(Player player)
        {
            Load();
            var key = Key(player);
            if (key == null) return null;
            if (!pools.TryGetValue(key, out var pool))
            {
                pool = new ProtectionPool(Plugin.MaximumProtection.Value);
                pools.Add(key, pool);
                dirty = true;
            }
            if (pool.Maximum != Plugin.MaximumProtection.Value)
            {
                pool.Resize(Plugin.MaximumProtection.Value);
                dirty = true;
            }
            // Retrieving a dropped crown restores one point, not a full refill.
            if (crowns.TryGetValue(key, out bool hadCrown) && !hadCrown && player.hasCrown && pool.Current == 0)
            {
                pool.Recover(1);
                dirty = true;
            }
            crowns[key] = player.hasCrown;
            return pool;
        }

        internal static void Tick(IEnumerable<Player> players)
        {
            if (NetworkBigBoss.IsOnline && (Plugin.UseProtection.Value || !Plugin.LoseResources.Value) && !onlineNotice)
            {
                onlineNotice = true;
                Plugin.LogSource.LogWarning("Las opciones de dificultad se aplican a un jugador y cooperativo local. En partidas online se mantiene el dano original.");
            }
            if (Enabled) foreach (var player in players) For(player);
            if (Time.unscaledTime >= nextSave)
            {
                nextSave = Time.unscaledTime + 1;
                Flush();
            }
        }

        private static bool ReceiveDamage(Player __instance, int damageMultiplier, GameObject damager, DamageSource source)
        {
            if (!Available || (!Plugin.UseProtection.Value && Plugin.LoseResources.Value)) return true;
            // Keep the game's special crown-stealing, petrification and passenger rules.
            if (source == DamageSource.Stealer || __instance.IsPetrified || __instance.passenger != null) return true;
            if (damageMultiplier <= 0) return false;
            var pool = Plugin.UseProtection.Value ? For(__instance) : null;
            if (Plugin.UseProtection.Value && pool == null) return true;
            Vector2 direction = Direction(__instance, damager);
            if (pool != null)
            {
                pool.Damage(damageMultiplier);
                dirty = true;
            }
            if (Plugin.LoseResources.Value)
            {
                var wallet = __instance.wallet;
                for (int i = 0; i < damageMultiplier && wallet.HasDroppableCurrency; i++)
                {
                    var type = wallet.FirstAvailableDroppableCurrency();
                    bool water = type.CanDropInWater() && UnityEngine.Random.value < .5f;
                    var force = Vector2.Scale(direction, new Vector2(UnityEngine.Random.Range(.5f, 4f), UnityEngine.Random.Range(3f, 5f)));
                    var dropped = wallet.DropCurrency(type, force, water ? PickUpPolicy.Blocked : PickUpPolicy.Nobody);
                    if (water && dropped != null) dropped.DropInWater();
                }
            }
            if (pool != null && pool.Current == 0 && __instance.hasCrown)
            {
                bool knockedByPlayer = damager != null && damager.TryGetComponent<IUnitControllable>(out var unit) && unit.PlayerId != -1;
                __instance.LoseCrown(direction * UnityEngine.Random.Range(3f, 6f), false, knockedByPlayer);
            }
            if (source == DamageSource.Ogre || source == DamageSource.BoulderEnemy)
#if MONO
                SingletonMonoBehaviour<CameraMarshaller>.Inst.GetCameraForPlayerID(__instance.playerId)?.Shake(__instance.transform.position);
#else
                SingletonMonoBehaviour<CameraMarshaller>.Inst.GetCameraForPlayerID(__instance.playerId)?.Shake(
                    __instance.transform.position, 1, 1, new Il2CppSystem.Nullable<int>());
#endif
            return false;
        }

        private static Vector2 Direction(Player player, GameObject damager)
        {
            if (damager == null) return new Vector2(player.transform.localScale.x, 1);
            if (damager.TryGetComponent<Rigidbody2D>(out var body))
            {
#if MONO
#pragma warning disable CS0618 // The pre-Unity-6 Mono builds expose velocity.
                float x = body.velocity.x;
#pragma warning restore CS0618
#else
                float x = body.linearVelocity.x;
#endif
                if (Mathf.Abs(x) > .2f) return new Vector2(Mathf.Sign(x), 1);
            }
            return new Vector2(damager.transform.localScale.x, 1);
        }

        private static void BeforePickup(Wallet __instance, DroppableCurrency currency, out int __state)
        {
            __state = -1;
            if (!Enabled || currency == null || !currency.CurrencyType.GoesIntoBag() || !currency.CurrencyType.CanBeDropped()) return;
            var player = __instance.GetComponent<Player>();
            if (player == null || !player.hasCrown) return;
            For(player); // Capture the full initial state before collection changes anything.
            __state = __instance.GetCurrency(currency.CurrencyType);
        }

        private static void AfterPickup(Wallet __instance, DroppableCurrency currency, bool __result, int __state)
        {
            if (__state < 0 || !__result || !Enabled || currency == null || __instance.GetCurrency(currency.CurrencyType) <= __state) return;
            var player = __instance.GetComponent<Player>();
            if (player == null || !player.hasCrown) return;
            var pool = For(player);
            if (pool == null) return;
            pool.Recover(Plugin.RecoveryPerResource.Value);
            dirty = true;
        }

        private static void CrownLost(Player __instance)
        {
            if (!Enabled || __instance.hasCrown) return;
            For(__instance)?.CrownLost();
            var key = Key(__instance);
            if (key != null) crowns[key] = false;
            dirty = true;
        }

        private static string SaveDirectory()
        {
#if MONO
            var agent = AccessTools.Field(typeof(Filer), "agent")?.GetValue(null);
            return agent == null ? null : AccessTools.Field(agent.GetType(), "folder")?.GetValue(agent) as string;
#else
            return Filer.agent?.TryCast<Filer.DotNetAgent>()?.folder;
#endif
        }

        private static void Load()
        {
            string folder = SaveDirectory();
            if (string.IsNullOrEmpty(folder)) return;
            string path = Path.Combine(folder, "unlimited-wallet-protection-v1.txt");
            if (loaded && statePath == path) return;
            // A native user/profile switch can select a different save agent folder.
            statePath = path;
            pools.Clear(); crowns.Clear(); dirty = false;
            loaded = true;
            try
            {
                foreach (string line in File.Exists(statePath) ? File.ReadAllLines(statePath) : Array.Empty<string>())
                {
                    var parts = line.Split('\t');
                    if (parts.Length == 3 && int.TryParse(parts[1], out int value) && int.TryParse(parts[2], out int maximum)
                        && maximum > 0 && maximum <= 1000 && value >= 0 && value <= maximum)
                        pools[parts[0]] = new ProtectionPool(maximum, value);
                }
            }
            catch (Exception error) { Plugin.LogSource.LogWarning("No se pudo leer la proteccion guardada: " + error.Message); }
            ApplyCampaignResets();
        }

        internal static void Flush()
        {
            if (!loaded || !dirty || !Game.SavingEnabled || statePath == null) return;
            try
            {
                var lines = new List<string> { "# Unlimited Wallet protection v1; local campaign state" };
                foreach (var entry in pools)
                    lines.Add(entry.Key + "\t" + entry.Value.Current.ToString(CultureInfo.InvariantCulture) + "\t" + entry.Value.Maximum.ToString(CultureInfo.InvariantCulture));
                string temp = statePath + ".tmp";
                File.WriteAllLines(temp, lines);
                if (File.Exists(statePath)) File.Replace(temp, statePath, null);
                else File.Move(temp, statePath);
                dirty = false;
            }
            catch (Exception error) { Plugin.LogSource.LogWarning("No se pudo guardar la proteccion: " + error.Message); }
        }
    }
}
