namespace Plus.HabboHotel.Rooms.Chat.Commands
{
    /// <summary>
    /// The buckets the overlay's command list groups by. Assigned in CommandManager's constructor
    /// from the Register* block a command is declared in, so every command has exactly one.
    ///
    /// "All" is not listed here — it is a client-side view over everything the user can see, not
    /// a category a command belongs to.
    /// </summary>
    public static class CommandCategory
    {
        /// <summary>Available to everyone. Only ever surfaces under the "All" tab.</summary>
        public const string General = "General";

        public const string Roleplay = "Roleplay";

        /// <summary>Requires a subscription above rank 1.</summary>
        public const string Vip = "VIP";

        /// <summary>Moderator and administrator commands. Requires rank above 3.</summary>
        public const string Staff = "Staff";
    }
}
