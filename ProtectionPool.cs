using System;

namespace KingdomUnlimitedWallet
{
    // Native attack damage is measured in resource units, never in whole HUD icons.
    internal sealed class ProtectionPool
    {
        internal int Maximum { get; private set; }
        internal int Current { get; private set; }
        internal ProtectionPool(int maximum, int? current = null)
        {
            Maximum = Math.Max(1, maximum);
            Current = Math.Max(0, Math.Min(Maximum, current ?? Maximum));
        }
        internal void Resize(int maximum)
        {
            Maximum = Math.Max(1, maximum);
            Current = Math.Min(Current, Maximum);
        }
        internal void Damage(int amount) => Current = (int)Math.Max(0L, (long)Current - Math.Max(0, amount));
        internal void Recover(int amount) => Current = (int)Math.Min(Maximum, (long)Current + Math.Max(0, amount));
        internal void CrownLost() => Current = 0;
    }
}
