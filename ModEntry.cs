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

    public class ModEntry : Mod
    {
        private static int HOTBAR_SIZE = 12;

        private int currentSlot = 0;

        public override void Entry(IModHelper helper)
        {
            Helper.Events.Input.ButtonPressed += new EventHandler<ButtonPressedEventArgs>(this.ButtonPressed);
        }

        private void ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            var player = Game1.player;
            if (player == null) return;
            if (!Context.IsWorldReady || !Context.IsPlayerFree || player.FarmerSprite.isOnToolAnimation()) return;
            if (!SButtonExtensions.IsUseToolButton(e.Button)) return;

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

            if (loc.doesTileHaveProperty((int)toolLocationVector.X, (int)toolLocationVector.Y, "Diggable", "Back") != null)
            {
                Monitor.Log($"We should use the hoe here! {toolLocationVector.ToString()}", LogLevel.Info);
                this.SwitchTo(ToolType.Hoe);
            }
            if (loc.doesTileHaveProperty((int)toolLocationVector.X, (int)toolLocationVector.Y, "Water", "Back") != null)
            {
                Monitor.Log($"We should use the watering can here! {toolLocationVector.ToString()}", LogLevel.Info);
                this.SwitchTo(ToolType.WateringCan);
            }

            var objects = loc.objects;
            if (objects != null)
            {
                if (objects.ContainsKey(toolLocationVector))
                {
                    if (objects[toolLocationVector].name.Equals("Artifact Spot"))
                    {
                        Monitor.Log($"We should use the hoe here! {toolLocationVector.ToString()}", LogLevel.Info);
                        this.SwitchTo(ToolType.Hoe);
                    }
                    if (objects[toolLocationVector].name.Equals("Stone"))
                    {
                        Monitor.Log($"We should use the pickaxe here! {toolLocationVector.ToString()}", LogLevel.Info);
                        this.SwitchTo(ToolType.Pickaxe);
                    }
                    if (objects[toolLocationVector].name.Equals("Twig"))
                    {
                        Monitor.Log($"We should use the axe here! {toolLocationVector.ToString()}", LogLevel.Info);
                        this.SwitchTo(ToolType.Axe);
                    }
                    if (objects[toolLocationVector].name.Equals("Weeds"))
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
                        if ((terrainFeatures[toolLocationVector] as HoeDirt).crop != null && (((terrainFeatures[toolLocationVector] as HoeDirt).crop.harvestMethod.Value == 1 && (terrainFeatures[toolLocationVector] as HoeDirt).crop.fullyGrown.Value) || (terrainFeatures[toolLocationVector] as HoeDirt).crop.dead.Value))
                        {
                            Monitor.Log($"We should use the scythe/sword here! {toolLocationVector.ToString()}", LogLevel.Info);
                            this.SwitchTo(ToolType.Scythe);
                        }
                        else
                        {
                            Monitor.Log($"We should use the watering can here! {toolLocationVector.ToString()}", LogLevel.Info);
                            this.SwitchTo(ToolType.WateringCan);
                        }
                    }
                    if (terrainFeatures[toolLocationVector] is GiantCrop)
                    {
                        Monitor.Log($"We should use the axe here! {toolLocationVector.ToString()}", LogLevel.Info);
                        this.SwitchTo(ToolType.Axe);
                    }
                    if (terrainFeatures[toolLocationVector] is Tree)
                    {
                        Monitor.Log($"We should use the axe here! {toolLocationVector.ToString()}", LogLevel.Info);
                        this.SwitchTo(ToolType.Axe);
                    }
                    if (terrainFeatures[toolLocationVector] is Grass)
                    {
                        Monitor.Log($"We should use the scythe here! {toolLocationVector.ToString()}", LogLevel.Info);
                        this.SwitchTo(ToolType.Scythe);
                    }
                }
            }
        }

        private void GameTicked(object sender, EventArgs e)
        {
            var player = Game1.player;
            if (!Context.IsWorldReady || !Context.IsPlayerFree || player.FarmerSprite.isOnToolAnimation()) return;
            this.currentSlot = player.CurrentToolIndex;
        }


        private bool IsMonsterInProximity()
        {
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
                    type == ToolType.MeleeWeapon && isSword(item)
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
            return item != null && item.DisplayName.ToLower().EndsWith("hoe");
        }

        private bool isPickaxe(Item item)
        {
            return item != null && item.DisplayName.ToLower().EndsWith("pickaxe");
        }

        private bool isAxe(Item item)
        {
            return item != null && item.DisplayName.ToLower().EndsWith("axe");
        }

        private bool isWateringCan(Item item)
        {
            return item != null && item.DisplayName.ToLower().EndsWith("watering can");
        }

        private bool isFishingPole(Item item)
        {
            return item != null && (item.DisplayName.ToLower().EndsWith("pole") || item.DisplayName.ToLower().EndsWith("rod"));
        }

        private bool isScythe(Item item)
        {
            return item != null && (item.DisplayName.ToLower().EndsWith("scythe"));
        }

        private bool isSword(Item item)
        {
            return item != null && item is MeleeWeapon;
        }

    }
}
