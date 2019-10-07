using System;
using Microsoft.Xna.Framework;
using SDVCommon;
using SDVCommon.Extensions;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;

namespace autotool
{
    public class ModEntry : Mod
    {
        private ModConfig _config;
        public bool IsActive { get; set; }

        public override void Entry(IModHelper helper)
        {
            _config = Helper.ReadConfig<ModConfig>();
            Helper.Events.Input.ButtonPressed += ButtonPressed;
        }

        private void ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            var player = Game1.player;
            if (!Context.IsWorldReady || !Context.IsPlayerFree || player.FarmerSprite.isOnToolAnimation()) return;

            // toggle mod on/off
            if (e.Button == _config.ToggleKey)
            {
                IsActive = !IsActive;
                var state = IsActive ? "enabled" : "disabled";
                Game1.chatBox.addMessage($"Autotool is now {state}", IsActive ? Color.LimeGreen : Color.Red);
            }

            // if mod active
            if (IsActive && e.Button.IsUseToolButton())
            {
                DoAutotool();
            }
        }

        private void DoAutotool()
        {
            var player = Game1.player;
            var loc = player.currentLocation;
            if (loc == null) return;

            if (player.IsMonsterInProximity(2.5f, out _))
            {
                player.EquipTool(ToolType.MeleeWeapon);
                return;
            }

            var target = player.GetToolLocation();
            var toolLocationVector = new Vector2((int) target.X / Game1.tileSize, (int) target.Y / Game1.tileSize);

            var props = TileUtils.GetTileProperties(toolLocationVector);

            var str = string.Join(", ", props.GetIndividualFlags());
            Monitor.Log(target.ToString() + " => " + str);


            if (props.HasFlag(TileProperties.Cuttable))
            {
                player.EquipTool(ToolType.Scythe);
                return;
            }

            if (props.HasFlag(TileProperties.Mineable))
            {
                player.EquipTool(ToolType.Pickaxe);
                return;
            }

            if (props.HasFlag(TileProperties.Choppable))
            {
                player.EquipTool(ToolType.Axe);
                return;
            }
                
            if (props.HasFlag(TileProperties.Waterable))
            {
                player.EquipTool(ToolType.WateringCan);
                return;
            }
            
            if (props.HasFlag(TileProperties.Diggable))
            {
                player.EquipTool(ToolType.Hoe);
                return;
            }
        }
    }
}