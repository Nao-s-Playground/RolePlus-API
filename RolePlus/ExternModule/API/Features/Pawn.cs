﻿// -----------------------------------------------------------------------
// <copyright file="Pawn.cs" company="NaoUnderscore">
// Copyright (c) NaoUnderscore. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace RolePlus.ExternModule.API.Features
{
#nullable enable

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using Exiled.API.Features.Items;
    using Exiled.CustomItems;
    using Exiled.CustomItems.API.Features;
    using Hints;
    using MapEditorReborn.Commands.ModifyingCommands.Position;
    using PlayerRoles;
    using RolePlus.ExternModule.API.Enums;
    using UnityEngine;
    using UnityEngine.Experimental.Playables;
    using AbilityBehaviour = CustomAbilities.AbilityBehaviour;
    using CharacterMeshComponent = CustomMeshes.CharacterMeshComponent;
    using CustomAbility = CustomAbilities.CustomAbility;
    using CustomEscape = CustomEscapes.CustomEscape;
    using CustomRole = CustomRoles.CustomRole;
    using CustomTeam = CustomTeams.CustomTeam;
    using Display = CustomHud.Display;
    using EBehaviour = RolePlus.ExternModule.API.Engine.Framework.EBehaviour;
    using EscapeBehaviour = CustomEscapes.EscapeBehaviour;
    using Hint = CustomHud.Hint;
    using HudBehaviour = CustomHud.HudBehaviour;
    using RoleBehaviour = CustomRoles.RoleBehaviour;

    /// <summary>
    /// Represents the in-game player, by encapsulating a ReferenceHub.
    /// <para>
    /// <see cref="Pawn"/> implements more features in addition to <see cref="Player"/>'s existing ones.
    /// <br>This is treated as a <see cref="Player"/>, which means it can be used along with existing methods asking for a <see cref="Player"/> as parameter.</br>
    /// <para>Nullable context is enabled in order to prevent users to pass or interact with <see langword="null"/> references.</para>
    /// </para>
    /// </summary>
    public class Pawn : Player
    {
        private HudBehaviour? _hudBehaviour;
        private RoleBehaviour? _roleBehaviour;
        private EscapeBehaviour? _escapeBehaviour;
        private CharacterMeshComponent? _characterMeshComponent;
        private CustomRole? _customRole;
        private CustomTeam? _customTeam;
        private CustomEscape? _customEscape;

        /// <summary>
        /// Initializes a new instance of the <see cref="Pawn"/> class.
        /// </summary>
        /// <param name="referenceHub">The <see cref="ReferenceHub"/> of the player to be encapsulated.</param>
        public Pawn(ReferenceHub referenceHub)
            : base(referenceHub)
        {
            foreach (KeyValuePair<Player, HashSet<CustomAbility>> kvp in CustomAbility.Manager)
            {
                if (kvp.Key != this)
                    continue;

                foreach (CustomAbility ability in kvp.Value)
                    AbilityManager.Add(ability, GetComponent(ability.BehaviourComponent).Cast<AbilityBehaviour>());
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pawn"/> class.
        /// </summary>
        /// <param name="gameObject">The <see cref="GameObject"/> of the player.</param>
        public Pawn(GameObject gameObject)
            : base(gameObject)
        {
            foreach (KeyValuePair<Player, HashSet<CustomAbility>> kvp in CustomAbility.Manager)
            {
                if (kvp.Key != this)
                    continue;

                foreach (CustomAbility ability in kvp.Value)
                    AbilityManager.Add(ability, GetComponent(ability.BehaviourComponent).Cast<AbilityBehaviour>());
            }
        }

        /// <summary>
        /// Gets the pawn's ability manager.
        /// </summary>
        public Dictionary<CustomAbility, AbilityBehaviour> AbilityManager { get; private set; } = new();

        /// <summary>
        /// Gets all pawn's <see cref="EBehaviour"/>'s.
        /// </summary>
        public IEnumerable<EBehaviour> Behaviours => ComponentsInChildren.Where(cmp => cmp is EBehaviour).Cast<EBehaviour>();

        /// <summary>
        /// Gets the pawn's <see cref="CustomRoles.CustomRole"/>.
        /// </summary>
        public CustomRole? CustomRole => _customRole ??= CustomRole.Get(this);

        /// <summary>
        /// Gets the pawn's <see cref="CustomTeams.CustomTeam"/>.
        /// </summary>
        public CustomTeam? CustomTeam => _customTeam ??= CustomTeam.Get(this);

        /// <summary>
        /// Gets the pawn's <see cref="CustomEscapes.CustomEscape"/>.
        /// </summary>
        public CustomEscape? CustomEscape => _customEscape ??= CustomEscape.Get(this);

        /// <summary>
        /// Gets the pawn's <see cref="CustomMeshes.CharacterMeshComponent"/>.
        /// </summary>
        public CharacterMeshComponent CharacterMeshComponent => _characterMeshComponent ??= GetComponent<CharacterMeshComponent>();

        /// <summary>
        /// Gets the pawn's <see cref="HudBehaviour"/>.
        /// </summary>
        public HudBehaviour Hud => _hudBehaviour ??= GetComponent<HudBehaviour>();

        /// <summary>
        /// Gets the pawn's <see cref="CustomRoles.RoleBehaviour"/>.
        /// </summary>
        public RoleBehaviour RoleBehaviour => _roleBehaviour ??= GetComponent<RoleBehaviour>();

        /// <summary>
        /// Gets the pawn's <see cref="CustomEscapes.EscapeBehaviour"/>.
        /// </summary>
        public EscapeBehaviour EscapeBehaviour => _escapeBehaviour ??= GetComponent<EscapeBehaviour>();

        /// <summary>
        /// Gets the pawn's custom abilities.
        /// </summary>
        public IEnumerable<CustomAbility> CustomAbilities => AbilityManager.Keys;

        /// <summary>
        /// Gets the pawn's <see cref="AbilityBehaviour"/>.
        /// </summary>
        public IEnumerable<AbilityBehaviour> AbilityBehaviours => AbilityManager.Values;

        /// <summary>
        /// Gets the pawn's <see cref="Display"/> instances.
        /// </summary>
        public SortedList<DisplayLocation, Display> Displays => Hud.Displays;

        /// <summary>
        /// Gets a value indicating whether the pawn has a <see cref="CustomRoles.CustomRole"/>.
        /// </summary>
        public bool HasCustomRole => CustomRole.Players.Contains(this);

        /// <summary>
        /// Gets a value indicating whether the pawn is any SCP, including custom ones.
        /// </summary>
        public new bool IsScp
        {
            get
            {
                if (IsCustomScp)
                    return true;

                Team? team = Role?.Team;
                return team.HasValue && team.GetValueOrDefault() == Team.SCPs;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the pawn is any custom SCP.
        /// </summary>
        public bool IsCustomScp => CustomRole is not null && CustomRole.IsScp;

        /// <summary>
        /// Gets a <see cref="IEnumerable{T}"/> of <see cref="object"/> containing all custom items in the pawn's inventory.
        /// </summary>
        /// <returns>A <see cref="IEnumerable{T}"/> of <see cref="object"/> which contains all found custom items.</returns>
        public IEnumerable<CustomItem> CustomItems
        {
            get
            {
                foreach (Item item in Items)
                {
                    if (!CustomItem.TryGet(item, out CustomItem? customItem) || customItem is null)
                        continue;

                    yield return customItem;
                }
            }
        }

        /// <summary>
        /// Gets a <see cref="IEnumerable{T}"/> of <see cref="EffectType"/>.
        /// </summary>
        /// <returns>A <see cref="IEnumerable{T}"/> of <see cref="EffectType"/>.</returns>
        public IEnumerable<EffectType> EffectTypes
        {
            get
            {
                foreach (object effect in Enum.GetValues(typeof(EffectType)))
                {
                    if (!Enum.TryParse(effect.ToString(), out EffectType effectType) || !TryGetEffect(effectType, out _))
                        continue;

                    yield return effectType;
                }
            }
        }

        /// <summary>
        /// Add a <see cref="CustomItem"/> of the specified type to the pawn's inventory.
        /// </summary>
        /// <param name="customItem">The item to be added.</param>
        /// <returns><see langword="true"/> if the item has been given to the pawn; otherwise, <see langword="false"/>.</returns>
        public bool AddItem(object customItem)
        {
            if (IsInventoryFull)
                return false;

            try
            {
                uint value = (uint)customItem;
                CustomItem.TryGive(this, value);
                return true;
            }
            catch
            {
                if (customItem is CustomItem instance)
                    return CustomItem.TryGive(this, instance.Id);

                return false;
            }
        }

        /// <summary>
        /// Adds a <see cref="IEnumerable{T}"/> of <see cref="object"/> containing all the custom items to the pawn's inventory.
        /// </summary>
        /// <param name="customItems">The custom items to be added.</param>
        public void AddItem(IEnumerable<object> customItems)
        {
            foreach (object customItemType in customItems)
            {
                if (!AddItem(customItemType))
                    continue;
            }
        }

        /// <summary>
        /// Tries to get a <see cref="CustomItem"/> from the given <see cref="Item"/> instance.
        /// </summary>
        /// <typeparam name="T">The type of the <see cref="CustomItem"/> to look for.</typeparam>
        /// <param name="customItem">The <see cref="CustomItem"/> result.</param>
        /// <returns><see langword="true"/> if pawn owns the specified <see cref="CustomItem"/>; otherwise, <see langword="false"/>.</returns>
        public bool TryGetCustomItem<T>(out T? customItem)
            where T : CustomItem
        {
            customItem = null;
            foreach (Item item in Items)
            {
                if (!CustomItem.TryGet(item, out CustomItem? tmp) || tmp is null || tmp.GetType() != typeof(T))
                    continue;

                customItem = (T)tmp;
            }

            return customItem is not null;
        }

        /// <summary>
        /// Gets the pawn's <see cref="CustomRoles.CustomRole"/>.
        /// </summary>
        /// <param name="customRole">The <see cref="CustomRoles.CustomRole"/> result.</param>
        /// <returns>The found <see cref="CustomRoles.CustomRole"/>, or <see langword="null"/> if not found.</returns>
        public bool TryGetCustomRole(out CustomRole? customRole) => (customRole = CustomRole) is not null;

        /// <summary>
        /// Sets the pawn's role.
        /// </summary>
        /// <param name="role">The role to be set.</param>
        /// <param name="shouldKeepPosition"><inheritdoc cref="CustomRole.ShouldKeepPosition"/></param>
        public void SetRole(object role, bool shouldKeepPosition = false)
        {
            if (role is RoleType id)
            {
                Role.Set(id);
                return;
            }

            CustomRole.UnsafeSpawn(this, role, shouldKeepPosition);
        }

        /// <summary>
        /// Safely drops an item.
        /// </summary>
        /// <param name="item">The item to be dropped.</param>
        public void SafeDropItem(Item item)
        {
            if (TryGetCustomItem(out CustomItem? customItem))
            {
                RemoveItem(item, false);
                customItem?.Spawn(Position, this);
                return;
            }

            DropItem(item);
        }

        /// <summary>
        /// Gets a value indicating whether the pawn can look to the specified <see cref="Player"/>.
        /// </summary>
        /// <param name="player">The target.</param>
        /// <param name="maxDistance">The maximum distance before dropping the check.</param>
        /// <returns><see langword="true"/> if the pawn can look to <paramref name="player"/>; otherwise, <see langword="false"/>.</returns>
        public bool CanLookToPlayer(Player player, float maxDistance)
        {
            if (IsDead || Role == RoleType.Scp079 || player.IsDead || player.Role == RoleType.Scp079)
                return false;

            float num = Vector3.Dot(CameraTransform.forward, player.Position - CameraTransform.position);
            return num >= 0f && num * num / (player.Position - CameraTransform.position).sqrMagnitude > 0.4225f &&
                Physics.Raycast(CameraTransform.position, player.Position - CameraTransform.position, out RaycastHit raycastHit, maxDistance, -117407543) &&
                raycastHit.transform.name == player.GameObject.name;
        }

        /// <summary>
        /// Shows a hint to the specified player.
        /// </summary>
        /// <param name="hint">The hint to display..</param>
        /// <param name="displayLocation">The display location.</param>
        /// <param name="overrideQueue">A value indicating whether the queue should be cleared before adding the hint.</param>
        public void ShowHint(Hint hint, DisplayLocation displayLocation = DisplayLocation.MiddleBottom, bool overrideQueue = false) => Hud?.Show(hint, displayLocation, overrideQueue);

        /// <summary>
        /// Shows a hint to the specified player.
        /// </summary>
        /// <param name="hint">The hint to display..</param>
        /// <param name="replace">The <see cref="string"/> to replace..</param>
        /// <param name="newValue">The <see cref="string"/> replacement.</param>
        /// <param name="displayLocation">The display location.</param>
        /// <param name="overrideQueue">A value indicating whether the queue should be cleared before adding the hint.</param>
        public void ShowHint(Hint hint, string replace, string newValue, DisplayLocation displayLocation = DisplayLocation.MiddleBottom, bool overrideQueue = false) =>
            Hud?.Show(hint, replace, newValue, displayLocation, overrideQueue);

        /// <summary>
        /// Shows a hint to the specified player.
        /// </summary>
        /// <param name="hint">The hint to display..</param>
        /// <param name="replace">The <see cref="string"/> to replace..</param>
        /// <param name="newValue">The <see cref="string"/> replacement.</param>
        /// <param name="displayLocation">The display location.</param>
        /// <param name="overrideQueue">A value indicating whether the queue should be cleared before adding the hint.</param>
        public void ShowHint(Hint hint, string[] replace, string[] newValue, DisplayLocation displayLocation = DisplayLocation.MiddleBottom, bool overrideQueue = false) =>
            Hud?.Show(hint, replace, newValue, displayLocation, overrideQueue);
    }
}
