using StardewModdingAPI;

namespace autotool
{
    public class ModConfig
    {
        public SButton ToggleKey { get; set; } = SButton.RightControl;

        public bool AutoHoe { get; set; } = false;
        public bool AutoPickaxe { get; set; } = true;
        public bool AutoAxe { get; set; } = true;
        public bool AutoWateringCan { get; set; } = false;
        public bool AutoFishingPole { get; set; } = false;
        public bool AutoScythe { get; set; } = true;
        public bool AutoMelee { get; set; } = true;
    }
}