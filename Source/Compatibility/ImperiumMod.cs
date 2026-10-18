using Verse;

namespace Imperium.RimWorld;

public sealed class ImperiumMod : Mod
{
    public ImperiumMod(ModContentPack content) : base(content)
    {
        Log.Message("[Imperium: Living Worlds] Development scaffold loaded (0.1.0). No autonomous construction is enabled yet.");
    }
}
