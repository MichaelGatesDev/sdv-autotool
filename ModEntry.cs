using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;

namespace autotool
{
    enum ToolType
    {
        Hoe,
        Pickaxe,
        Axe,
        WateringCan,
        FishingPole,
        Scythe,
        MeleeWeapon,
        Other
    }

    class ModConfig
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

    public class ModEntry : Mod
    {
        private ModConfig Config;
        public bool IsActive { get; set; } = false;

        private static int HOTBAR_SIZE = 12;

        private int currentSlot = 0;

        public override void Entry(IModHelper helper)
        {
            this.Config = this.Helper.ReadConfig<ModConfig>();
            Helper.Events.Input.ButtonPressed += new EventHandler<ButtonPressedEventArgs>(this.ButtonPressed);
        }

        private void ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            var player = Game1.player;
            if (!Context.IsWorldReady || !Context.IsPlayerFree || player.FarmerSprite.isOnToolAnimation()) return;

            // toggle mod on/off
            if (e.Button == this.Config.ToggleKey)
            {
                this.IsActive = !this.IsActive;
                var state = IsActive ? "enabled" : "disabled";
                Monitor.Log($"Autotool mod is now {state}");
            }

            // if mod off
            if (this.IsActive && SButtonExtensions.IsUseToolButton(e.Button))
            {
                DoAutotool();
            }
        }

        private void GameTicked(object sender, EventArgs e)
        {
            var player = Game1.player;
            if (!Context.IsWorldReady || !Context.IsPlayerFree || player.FarmerSprite.isOnToolAnimation()) return;
            this.currentSlot = player.CurrentToolIndex;
        }


        private void DoAutotool()
        {
            var player = Game1.player;
            var loc = player.currentLocation;
            if (loc == null) return;

            // monsters take priority
            if (IsMonsterInProximity())
            {
                Monitor.Log($"There's a monster nearby", LogLevel.Info);
                this.SwitchTo(ToolType.MeleeWeapon);
                return;
            }

            var target = player.GetToolLocation();
            if (target == null) return;

            var toolLocationVector = new Vector2((int)target.X / Game1.tileSize, (int)target.Y / Game1.tileSize);

            if (loc.doesTileHaveProperty((int)toolLocationVector.X, (int)toolLocationVector.Y, "Diggable", "Back") != null && Config.AutoHoe)
            {
                Monitor.Log($"We should use the hoe here! {toolLocationVector.ToString()}", LogLevel.Info);
                this.SwitchTo(ToolType.Hoe);
            }
            if (loc.doesTileHaveProperty((int)toolLocationVector.X, (int)toolLocationVector.Y, "Water", "Back") != null && Config.AutoWateringCan)
            {
                Monitor.Log($"We should use the watering can here! {toolLocationVector.ToString()}", LogLevel.Info);
                this.SwitchTo(ToolType.WateringCan);
            }

            var objects = loc.objects;
            if (objects != null)
            {
                if (objects.ContainsKey(toolLocationVector))
                {
                    if (objects[toolLocationVector].name.Equals("Artifact Spot") && Config.AutoHoe)
                    {
                        Monitor.Log($"We should use the hoe here! {toolLocationVector.ToString()}", LogLevel.Info);
                        this.SwitchTo(ToolType.Hoe);
                    }
                    if (objects[toolLocationVector].name.Equals("Stone") && Config.AutoPickaxe)
                    {
                        Monitor.Log($"We should use the pickaxe here! {toolLocationVector.ToString()}", LogLevel.Info);
                        this.SwitchTo(ToolType.Pickaxe);
                    }
                    if (objects[toolLocationVector].name.Equals("Twig") && Config.AutoAxe)
                    {
                        Monitor.Log($"We should use the axe here! {toolLocationVector.ToString()}", LogLevel.Info);
                        this.SwitchTo(ToolType.Axe);
                    }
                    if (objects[toolLocationVector].name.Equals("Weeds") && Config.AutoScythe)
                    {
                        Monitor.Log($"We should use the scythe/sword here! {toolLocationVector.ToString()}", LogLevel.Info);
                        this.SwitchTo(ToolType.Scythe);
                    }
                }
            }
            var terrainFeatures = loc.terrainFeatures;
            if (terrainFeatures != null)
            {
                if (terrainFeatures.ContainsKey(toolLocationVector))
                {
                    if (terrainFeatures[toolLocationVector] is HoeDirt)
                    {
                        if ((terrainFeatures[toolLocationVector] as HoeDirt).crop != null && (((terrainFeatures[toolLocationVector] as HoeDirt).crop.harvestMethod.Value == 1 && (terrainFeatures[toolLocationVector] as HoeDirt).crop.fullyGrown.Value) || (terrainFeatures[toolLocationVector] as HoeDirt).crop.dead.Value) && Config.AutoScythe)
                        {
                            Monitor.Log($"We should use the scythe/sword here! {toolLocationVector.ToString()}", LogLevel.Info);
                            this.SwitchTo(ToolType.Scythe);
                        }
                        else if (Config.AutoWateringCan)
                        {
                            Monitor.Log($"We should use the watering can here! {toolLocationVector.ToString()}", LogLevel.Info);
                            this.SwitchTo(ToolType.WateringCan);
                        }
                    }
                    if (terrainFeatures[toolLocationVector] is GiantCrop && Config.AutoAxe)
                    {
                        Monitor.Log($"We should use the axe here! {toolLocationVector.ToString()}", LogLevel.Info);
                        this.SwitchTo(ToolType.Axe);
                    }
                    if (terrainFeatures[toolLocationVector] is Tree && Config.AutoAxe)
                    {
                        Monitor.Log($"We should use the axe here! {toolLocationVector.ToString()}", LogLevel.Info);
                        this.SwitchTo(ToolType.Axe);
                    }
                    if (terrainFeatures[toolLocationVector] is Grass && Config.AutoScythe)
                    {
                        Monitor.Log($"We should use the scythe here! {toolLocationVector.ToString()}", LogLevel.Info);
                        this.SwitchTo(ToolType.Scythe);
                    }
                }
            }
        }

        private bool IsMonsterInProximity()
        {
            var player = Game1.player;
            return false;
        }

        private bool SwitchTo(ToolType type)
        {
            var player = Game1.player;
            var items = player.Items;
            for (var i = 0; i < HOTBAR_SIZE; i++)
            {
                var item = player.Items[i];
                if (
                    type == ToolType.Axe && isAxe(item) ||
                    type == ToolType.FishingPole && isFishingPole(item) ||
                    type == ToolType.Hoe && isHoe(item) ||
                    type == ToolType.Pickaxe && isPickaxe(item) ||
                    type == ToolType.WateringCan && isWateringCan(item) ||
                    type == ToolType.Scythe && isScythe(item) ||
                    type == ToolType.MeleeWeapon && isMeleeWeapon(item)
                )
                {
                    player.CurrentToolIndex = i;
                    return true;
                }
            }
            return false;
        }

        private void PrintHotbar()
        {
            var hotbar = this.GetHotbar();
            for (var i = 0; i < hotbar.Count; i++)
            {
                var item = hotbar[i];
                var name = item == null ? "air" : item.Name;
                var category = item == null ? "none" : item.getCategoryName();
                Monitor.Log($"Item at slot {i} is {name} of category {category}");
            }
        }

        private List<Item> GetHotbar()
        {
            var result = new List<Item>();
            var player = Game1.player;
            var items = player.Items;
            for (var i = 0; i < HOTBAR_SIZE - 1; i++)
            {
                var item = player.Items[i];
                result.Add(item);
            }
            return result;
        }

        private bool isHoe(Item item)
        {
            var name = item == null ? "" : item.DisplayName.ToLower();
            return item != null && (name.Equals("hoe") || name.Contains(" hoe"));
        }

        private bool isPickaxe(Item item)
        {
            var name = item == null ? "" : item.DisplayName.ToLower();
            return item != null && (name.Equals("pickaxe") || name.Contains(" pickaxe"));
        }

        private bool isAxe(Item item)
        {
            var name = item == null ? "" : item.DisplayName.ToLower();
            return item != null && (name.Equals("axe") || name.Contains(" axe"));
        }

        private bool isWateringCan(Item item)
        {
            var name = item == null ? "" : item.DisplayName.ToLower();
            return item != null && (name.Equals("watering can") || name.Contains(" watering can"));
        }

        private bool isFishingPole(Item item)
        {
            var name = item == null ? "" : item.DisplayName.ToLower();
            return item != null && (name.EndsWith("pole") || name.EndsWith("rod"));
        }

        private bool isScythe(Item item)
        {
            var name = item == null ? "" : item.DisplayName.ToLower();
            return item != null && (name.Equals("scythe") || name.Contains(" scythe"));
        }

        private bool isMeleeWeapon(Item item)
        {
            return item != null && item is MeleeWeapon;
        }

    }
}
