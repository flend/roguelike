using SdlDotNet.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueBasin
{
    class PlayerActions
    {
        private Running running;
        private Targetting targetting;

        Creature lastSpellTarget = null;

        public PlayerActions(Running running, Targetting targetting)
        {
            this.running = running;
            this.targetting = targetting;
        }


        private ActionResult RunToTargettedDestination()
        {
            if (!Game.Dungeon.IsSquareSeenByPlayer(targetting.CurrentTarget) && !Screen.Instance.SeeAllMap)
            {
                return new ActionResult(false, false);
            }

            var player = Game.Dungeon.Player;

            IEnumerable<Point> path = player.GetPlayerRunningPath(targetting.CurrentTarget.MapCoord);

            if (!path.Any())
            {
                return new ActionResult(false, false);
            }

            return running.StartRunning(path);
        }

        private ActionResult ThrowTargettedUtility()
        {
            if (!Game.Dungeon.IsSquareInPlayerFOV(targetting.CurrentTarget))
            {
                return new ActionResult(false, false);
            }

            var player = Game.Dungeon.Player;
            var throwSuccessfully = ThrowTargettedUtility(targetting.CurrentTarget.MapCoord);
            player.ResetTurnsMoving();
            player.ResetTurnsSinceAction();
            return new ActionResult(throwSuccessfully, throwSuccessfully);
        }

        private ActionResult FireTargettedWeapon()
        {
            if (!Game.Dungeon.IsSquareInPlayerFOV(targetting.CurrentTarget))
            {
                return new ActionResult(false, false);
            }

            var player = Game.Dungeon.Player;
            var fireSuccessfully = FireTargettedWeapon(targetting.CurrentTarget.MapCoord);
            player.ResetTurnsMoving();
            player.ResetTurnsSinceAction();
            return new ActionResult(fireSuccessfully, fireSuccessfully);
        }


        private bool ThrowTargettedUtility(Point target)
        {
            Dungeon dungeon = Game.Dungeon;
            Player player = Game.Dungeon.Player;

            IEquippableItem toThrow = player.GetEquippedUtility();
            Item toThrowItem = player.GetEquippedUtilityAsItem();

            CreatureFOV currentFOV = Game.Dungeon.CalculateCreatureFOV(Game.Dungeon.Player);

            if (toThrow == null)
            {
                Game.MessageQueue.AddMessage("No throwable weapon!");
                LogFile.Log.LogEntryDebug("No throwable weapon", LogDebugLevel.Medium);
                return false;
            }

            int range = toThrow.RangeThrow();

            //Check we are in range of target (not done above)
            if (!Utility.TestRangeFOVForWeapon(Game.Dungeon.Player, target, range, currentFOV))
            {
                Game.MessageQueue.AddMessage("Out of range!");
                LogFile.Log.LogEntryDebug("Out of range for " + toThrowItem.SingleItemDescription, LogDebugLevel.Medium);

                return false;
            }

            //Actually do throwing action
            Point destinationSq = toThrow.ThrowItem(target);

            //Remove stealth
            RemoveEffectsDueToThrowing(player, toThrowItem);

            //Play any audio
            toThrow.ThrowAudio();

            //Destroy it if required
            if (toThrow.DestroyedOnThrow())
            {
                player.UnequipAndDestroyItem(toThrowItem);

                //Try to reequip another item 
                //player.EquipInventoryItemType(toThrow.GetType());
                player.EquipNextUtility();

                return true;
            }

            if (destinationSq != null)
            {
                //Drop the item at the end point

                Point dropTarget = destinationSq;

                //If there is a creature at the end point, try to find a free area
                SquareContents squareContents = dungeon.MapSquareContents(player.LocationLevel, destinationSq);

                //Is there a creature here? If so, try to find another location
                if (squareContents.monster != null)
                {
                    //Get surrounding squares
                    List<Point> freeSqs = dungeon.GetWalkableAdjacentSquaresFreeOfCreatures(player.LocationLevel, destinationSq);

                    if (freeSqs.Count > 0)
                    {
                        dropTarget = freeSqs[Game.Random.Next(freeSqs.Count)];
                    }
                }

                player.UnequipAndDropItem(toThrowItem, player.LocationLevel, dropTarget);
            }

            //Time only goes past if successfully thrown
            return true;
        }

        private bool FireTargettedWeapon(Point target)
        {

            Dungeon dungeon = Game.Dungeon;
            Player player = Game.Dungeon.Player;

            IEquippableItem weapon = player.GetEquippedRangedWeapon();
            Item weaponI = player.GetEquippedRangedWeaponAsItem();

            if (weapon == null)
            {
                Game.MessageQueue.AddMessage("No weapon to fire");
                LogFile.Log.LogEntryDebug("No weapon to fire", LogDebugLevel.Medium);
            }

            if (target.x == player.LocationMap.x && target.y == player.LocationMap.y)
            {
                Game.MessageQueue.AddMessage("Can't target self with " + weaponI.SingleItemDescription + ".");
                LogFile.Log.LogEntryDebug("Can't target self with " + weaponI.SingleItemDescription, LogDebugLevel.Medium);
                return false;
            }

            //Check ammo
            if (weapon.RemainingAmmo() < 1)
            {
                Game.MessageQueue.AddMessage("Not enough ammo for " + weaponI.SingleItemDescription);
                LogFile.Log.LogEntryDebug("Not enough ammo for " + weaponI.SingleItemDescription, LogDebugLevel.Medium);

                return false;
            }

            //Check we are in range of target (not done above)
            int range = weapon.RangeFire();
            CreatureFOV currentFOV = Game.Dungeon.CalculateCreatureFOV(Game.Dungeon.Player);

            if (!Utility.TestRangeFOVForWeapon(Game.Dungeon.Player, target, range, currentFOV))
            {
                Game.MessageQueue.AddMessage("Out of range!");
                LogFile.Log.LogEntryDebug("Out of range for " + weaponI.SingleItemDescription, LogDebugLevel.Medium);

                return false;
            }

            //Actually do firing action
            bool success = weapon.FireItem(target);

            if (success)
            {
                RemoveEffectsDueToFiringWeapon(player);
            }

            //Ditch empty weapons
            /*
            if (weapon.RemainingAmmo() < 1)
            {
                Game.MessageQueue.AddMessage("This " + (weapon as Item).SingleItemDescription + " is all out of ammo! Ditching it!");
                LogFile.Log.LogEntryDebug("Out of range for " + weaponI.SingleItemDescription, LogDebugLevel.Medium);
                player.UnequipAndDestroyItem(weapon as Item);
                player.GivePistol();
            }*/

            //Store details for a recast

            //If we successful, store the target
            if (success)
            {
                //Spell target is the creature (monster or PC)

                SquareContents squareContents = dungeon.MapSquareContents(player.LocationLevel, target);

                //Is there a creature here? If so, store
                if (squareContents.monster != null)
                    lastSpellTarget = squareContents.monster;

                if (squareContents.player != null)
                    lastSpellTarget = squareContents.player;
            }

            if (success)
            {
                weapon.FireAudio();
            }

            //Time only goes past if successful
            return success;
        }


        private void RemoveEffectsDueToFiringWeapon(Player player)
        {
            //player.RemoveEffect(typeof(PlayerEffects.StealthBoost));
            //player.RemoveEffect(typeof(PlayerEffects.SpeedBoost));
            player.CancelBoostDueToAttack();
            player.CancelStealthDueToAttack();
        }

        private void RemoveEffectsDueToThrowing(Player player, Item toThrow)
        {
            //Some items permit stealth
            if (toThrow is Items.SoundGrenade)
                return;

            if (toThrow is Items.AcidGrenade)
                return;

            player.RemoveEffect(typeof(PlayerEffects.StealthBoost));
        }

    }
}
