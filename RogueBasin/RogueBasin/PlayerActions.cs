using SdlDotNet.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueBasin
{
    public class PlayerActions
    {
        private Running running;
        private Targetting targetting;

        Creature lastSpellTarget = null;

        public PlayerActions(Running running, Targetting targetting)
        {
            this.running = running;
            this.targetting = targetting;
        }

        public ActionResult DoNothing()
        {
            return Utility.TimeAdvancesOnMove(Game.Dungeon.Movement.PCMoveRelative(new Point(0, 0)));
        }

        public bool UseUtility()
        {
            return UseUtilityOrWeapon(true);
        }

        public bool UseWeapon()
        {
            return UseUtilityOrWeapon(false);
        }

        /// <summary>
        /// Use a utility
        /// </summary>
        private bool UseUtilityOrWeapon(bool isUtility)
        {

            Dungeon dungeon = Game.Dungeon;
            Player player = Game.Dungeon.Player;

            //Check we have a useable item

            IEquippableItem toUse = null;
            Item toUseItem = null;

            if (isUtility)
            {
                toUse = player.GetEquippedUtility();
                toUseItem = player.GetEquippedUtilityAsItem();
            }
            else
            {
                toUse = player.GetEquippedRangedWeapon();
                toUseItem = player.GetEquippedRangedWeaponAsItem();
            }

            if (toUse == null || !toUse.HasOperateAction())
            {
                Game.MessageQueue.AddMessage("Need an item that can be operated.");
                LogFile.Log.LogEntryDebug("Can't use " + toUseItem.SingleItemDescription, LogDebugLevel.Medium);
                return false;
            }

            //Use the item
            LogFile.Log.LogEntryDebug("Using " + toUseItem.SingleItemDescription, LogDebugLevel.Medium);
            bool success = toUse.OperateItem();

            if (success && toUse.DestroyedOnUse())
            {
                //Destroy the item
                player.UnequipAndDestroyItem(toUseItem);
                player.EquipNextUtility();
            };

            return success;

        }

        public ActionResult RunToTargettedDestination()
        {
            if (!Game.Dungeon.IsSquareSeenByPlayer(targetting.CurrentTarget) && !Screen.Instance.SeeAllMap)
            {
                return new ActionResult(false, false);
            }

            var canRunToTarget = Game.Dungeon.Movement.CanRunToTarget(targetting.CurrentTarget);

            if(canRunToTarget != Movement.RunToTargetStatus.OK)
            {
                return new ActionResult(false, false);
            }
            
            IEnumerable<Point> path = Game.Dungeon.Movement.GetPlayerRunningPath(targetting.CurrentTarget.MapCoord);

            return running.StartRunning(path);
        }

        public ActionResult ThrowTargettedUtility()
        {
            if (!Game.Dungeon.IsSquareInPlayerFOV(targetting.CurrentTarget))
            {
                return new ActionResult(false, false);
            }

            var player = Game.Dungeon.Player;
            var throwSuccessfully = ThrowTargettedUtility(targetting.CurrentTarget);
            player.ResetTurnsMoving();
            player.ResetTurnsSinceAction();
            return new ActionResult(throwSuccessfully, throwSuccessfully);
        }

        public ActionResult FireTargettedWeapon()
        {
            if (!Game.Dungeon.IsSquareInPlayerFOV(targetting.CurrentTarget))
            {
                return new ActionResult(false, false);
            }

            var player = Game.Dungeon.Player;
            var fireSuccessfully = FireTargettedWeapon(targetting.CurrentTarget);
            player.ResetTurnsMoving();
            player.ResetTurnsSinceAction();
            return new ActionResult(fireSuccessfully, fireSuccessfully);
        }


        private bool ThrowTargettedUtility(Location target)
        {
            Dungeon dungeon = Game.Dungeon;
            Player player = Game.Dungeon.Player;

            IEquippableItem toThrow = player.GetEquippedUtility();
            Item toThrowItem = player.GetEquippedUtilityAsItem();

            var canThrowToTarget = dungeon.Combat.CanThrowToTargetWithEquippedUtility(target);

            switch(canThrowToTarget)
            {
                case Combat.ThrowToTargetStatus.NoUtility:
                    Game.MessageQueue.AddMessage("No throwable utility!");
                    LogFile.Log.LogEntryDebug("No throwable utility", LogDebugLevel.Medium);
                    return false;

                case Combat.ThrowToTargetStatus.CantThrowBetweenLevels:
                    Game.MessageQueue.AddMessage("Can't throw between levels with " + toThrowItem.SingleItemDescription + ".");
                    LogFile.Log.LogEntryDebug("Can't throw between levels with " + toThrowItem.SingleItemDescription, LogDebugLevel.Medium);
                    return false;

                case Combat.ThrowToTargetStatus.OutOfRange:
                    Game.MessageQueue.AddMessage("Out of range!");
                    LogFile.Log.LogEntryDebug("Out of range for " + toThrowItem.SingleItemDescription, LogDebugLevel.Medium);
                    return false;

                case Combat.ThrowToTargetStatus.OK:
                    break;
            }
            
            //Actually do throwing action
            Point destinationSq = toThrow.ThrowItem(target.MapCoord);

            //Remove stealth
            RemoveEffectsDueToThrowing(player, toThrowItem);

            //Play any audio
            toThrow.ThrowAudio();

            //Destroy it if required
            if (toThrow.DestroyedOnThrow())
            {
                player.UnequipAndDestroyItem(toThrowItem);

                //Try to reequip another item 
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

        private bool FireTargettedWeapon(Location target)
        {
            Dungeon dungeon = Game.Dungeon;
            Player player = Game.Dungeon.Player;

            IEquippableItem weapon = player.GetEquippedRangedWeapon();
            Item weaponI = player.GetEquippedRangedWeaponAsItem();

            var fireResult = dungeon.Combat.CanFireOnTargetWithEquippedWeapon(target);

                switch(fireResult)
                {
                    case Combat.FireOnTargetStatus.CantFireBetweenLevels:
                        Game.MessageQueue.AddMessage("Can't fire between levels with " + weaponI.SingleItemDescription + ".");
                        LogFile.Log.LogEntryDebug("Can't fire between levels with " + weaponI.SingleItemDescription, LogDebugLevel.Medium);
                    return false;

                    case Combat.FireOnTargetStatus.CantTargetSelf:
                        Game.MessageQueue.AddMessage("Can't target self with " + weaponI.SingleItemDescription + ".");
                        LogFile.Log.LogEntryDebug("Can't target self with " + weaponI.SingleItemDescription, LogDebugLevel.Medium);
                    return false;

                case Combat.FireOnTargetStatus.NotEnoughAmmo:
                        Game.MessageQueue.AddMessage("Not enough ammo for " + weaponI.SingleItemDescription);
                        LogFile.Log.LogEntryDebug("Not enough ammo for " + weaponI.SingleItemDescription, LogDebugLevel.Medium);
                    return false;

                case Combat.FireOnTargetStatus.NoWeapon:
                        Game.MessageQueue.AddMessage("No weapon to fire");
                        LogFile.Log.LogEntryDebug("No weapon to fire", LogDebugLevel.Medium);
                    return false;

                case Combat.FireOnTargetStatus.OutOfRange:
                        Game.MessageQueue.AddMessage("Out of range!");
                        LogFile.Log.LogEntryDebug("Out of range for " + weaponI.SingleItemDescription, LogDebugLevel.Medium);
                    return false;

                case Combat.FireOnTargetStatus.OK:
                    break;
            }

            //Confirmed able to fire
            //Weapons may fail?
            bool success = weapon.FireItem(target.MapCoord);

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

            //If we were successful, store the target
            if (success)
            {
                //Spell target is the creature (monster or PC)

                SquareContents squareContents = dungeon.MapSquareContents(target);

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

        /// <summary>
        /// Examine using the target. Returns if time passes.
        /// </summary>
        /// <returns></returns>
        public bool Examine()
        {
            targetting.TargetExamine();
            return false;
        }

        /// <summary>
        /// Call when time moves on due to a PC action that isn't a move. This may cause some special moves to cancel.
        /// </summary>
        public void SpecialMoveNonMoveAction()
        {
            Game.Dungeon.PCActionNoMove();
        }
    }
}
