namespace Plus.HabboHotel.Roleplay.Cooldowns
{
    /// <summary>
    /// One entry per rate-limited roleplay action. Each kind is an independent timer — a user on
    /// robbery cooldown can still swing, dive a bin, or use a vendor.
    /// </summary>
    public enum RpCooldownKind
    {
        /// <summary>Global anti-spam throttle applied to every chat command.</summary>
        Command,

        /// <summary>A landed swing (:hit).</summary>
        Combat,

        /// <summary>A swing that never connected — a shorter beat than a landed hit.</summary>
        CombatWhiff,

        /// <summary>Robbing another user (:rob).</summary>
        Robbery,

        /// <summary>Dumpster diving a trash bin.</summary>
        TrashSearch,

        /// <summary>Using an rp_vendor machine.</summary>
        RpVendor,

        /// <summary>Using an rp_item. Scoped by rp_item definition id, duration from its own
        /// "cooldown" attribute — one timer per item type, not one for all items.</summary>
        RpItem
    }
}