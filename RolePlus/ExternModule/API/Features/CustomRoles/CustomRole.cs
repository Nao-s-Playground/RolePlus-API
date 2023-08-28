﻿// -----------------------------------------------------------------------
// <copyright file="CustomRole.cs" company="NaoUnderscore">
// Copyright (c) NaoUnderscore. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace RolePlus.ExternModule.API.Features.CustomRoles
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Exiled.API.Enums;
    using Exiled.API.Extensions;
    using Exiled.API.Features;
    using Exiled.API.Features.Core;
    using Exiled.Events.EventArgs.Player;
  
    using MEC;
  
    using PlayerRoles;

    using Respawning;

    using RolePlus.ExternModule.API.Engine.Framework.Interfaces;
    using RolePlus.ExternModule.API.Enums;
    using RolePlus.ExternModule.API.Features.Attributes;
    using RolePlus.ExternModule.API.Features.CustomEscapes;
    using RolePlus.ExternModule.Events.EventArgs;
    using RolePlus.Internal;

    using UnityEngine;

    /// <summary>
    /// A tool to easily manage custom roles.
    /// </summary>
    public abstract class CustomRole : TypeCastObject<CustomRole>, IAddittiveBehaviour
    {
        private static readonly List<CustomRole> _registered = new();

        internal static readonly Dictionary<Player, CustomRole> PlayersValue = new();

        /// <summary>
        /// Gets a <see cref="List{T}"/> which contains all registered <see cref="CustomRole"/>'s.
        /// </summary>
        public static IEnumerable<CustomRole> Registered => _registered;

        /// <summary>
        /// Gets all players and their respective <see cref="CustomRole"/>.
        /// </summary>
        public static IReadOnlyDictionary<Player, CustomRole> Manager => PlayersValue;

        /// <summary>
        /// Gets all players belonging to a <see cref="CustomRole"/>.
        /// </summary>
        public static IEnumerable<Player> Players => Manager.Keys.ToHashSet();

        /// <summary>
        /// Gets the <see cref="CustomRole"/>'s <see cref="Type"/>.
        /// </summary>
        public abstract Type BehaviourComponent { get; }

        /// <summary>
        /// Gets the <see cref="CustomEscape"/>'s <see cref="Type"/>.
        /// </summary>
        public virtual Type EscapeBehaviourComponent { get; }

        /// <summary>
        /// Gets a the <see cref="CustomRole"/>'s name.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets or sets the <see cref="CustomRole"/>'s id.
        /// </summary>
        public virtual uint Id { get; protected set; }

        /// <summary>
        /// Gets the <see cref="CustomEscapes.EscapeSettings"/>.
        /// </summary>
        public virtual List<EscapeSettings> EscapeSettings { get; } = new();

        /// <summary>
        /// Gets the <see cref="RoleSettings"/>.
        /// </summary>
        public virtual RoleSettings Settings { get; } = RoleSettings.Default;

        /// <summary>
        /// Gets the <see cref="CustomRole"/>'s <see cref="RoleType"/>.
        /// </summary>
        public virtual RoleType Role { get; }

        /// <summary>
        /// Gets a <see cref="Dictionary{TKey, TValue}"/> containing all the events, and the relative probability, on which spawn the custom role.
        /// </summary>
        public virtual Dictionary<string, int> SpawnOnEvents { get; } = new();

        /// <summary>
        /// Gets a value indicating whether or not the <see cref="CustomRole"/> is a custom SCP.
        /// </summary>
        public virtual bool IsScp { get; }

        /// <summary>
        /// Gets the relative spawn probability of the <see cref="CustomRole"/>.
        /// </summary>
        public virtual int SpawnProbability { get; }

        /// <summary>
        /// Gets a value representing the maximum instances of the <see cref="CustomRole"/> that can be automatically assigned.
        /// </summary>
        public virtual int MaxInstances => IsScp ? 1 : -1;

        /// <summary>
        /// Gets the <see cref="CustomRole"/>'s respawn <see cref="Team"/> .
        /// </summary>
        public virtual SpawnableTeamType RespawnTeam => SpawnableTeamType.None;

        /// <summary>
        /// Gets the <see cref="CustomRole"/>'s respawn <see cref="RoleType"/> .
        /// </summary>
        public virtual RoleType RespawnRole { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="CustomRole"/> is enabled.
        /// </summary>
        public virtual bool IsEnabled { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="CustomRole"/> is used for custom teams.
        /// </summary>
        public virtual bool IsTeamUnit { get; }

        /// <summary>
        /// Gets a value indicating whether the player should be spawned in their current position.
        /// </summary>
        public virtual bool ShouldKeepPosition { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="CustomRole"/> is registered.
        /// </summary>
        public virtual bool IsRegistered => Registered.Contains(this);

        /// <summary>
        /// Gets a <see cref="IEnumerable{T}"/> of <see cref="Player"/> containing all players owning this <see cref="CustomRole"/>.
        /// </summary>
        public IEnumerable<Player> Owners => Player.Get(x => TryGet(x, out CustomRole customRole) && customRole.Id == Id);

        /// <summary>
        /// Compares two operands: <see cref="CustomRole"/> and <see cref="object"/>.
        /// </summary>
        /// <param name="left">The <see cref="CustomRole"/> to compare.</param>
        /// <param name="right">The <see cref="object"/> to compare.</param>
        /// <returns><see langword="true"/> if the values are equal.</returns>
        public static bool operator ==(CustomRole left, object right)
        {
            try
            {
                uint value = (uint)right;
                return left.Id == value;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Compares two operands: <see cref="CustomRole"/> and <see cref="object"/>.
        /// </summary>
        /// <param name="left">The <see cref="CustomRole"/> to compare.</param>
        /// <param name="right">The <see cref="object"/> to compare.</param>
        /// <returns><see langword="true"/> if the values are not equal.</returns>
        public static bool operator !=(CustomRole left, object right)
        {
            try
            {
                uint value = (uint)right;
                return left.Id != value;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Compares two operands: <see cref="object"/> and <see cref="CustomRole"/>.
        /// </summary>
        /// <param name="left">The <see cref="object"/> to compare.</param>
        /// <param name="right">The <see cref="CustomRole"/> to compare.</param>
        /// <returns><see langword="true"/> if the values are equal.</returns>
        public static bool operator ==(object left, CustomRole right) => right == left;

        /// <summary>
        /// Compares two operands: <see cref="object"/> and <see cref="CustomRole"/>.
        /// </summary>
        /// <param name="left">The <see cref="object"/> to compare.</param>
        /// <param name="right">The <see cref="CustomRole"/> to compare.</param>
        /// <returns><see langword="true"/> if the values are not equal.</returns>
        public static bool operator !=(object left, CustomRole right) => right != left;

        /// <summary>
        /// Compares two operands: <see cref="CustomRole"/> and <see cref="CustomRole"/>.
        /// </summary>
        /// <param name="left">The left <see cref="CustomRole"/> to compare.</param>
        /// <param name="right">The right <see cref="CustomRole"/> to compare.</param>
        /// <returns><see langword="true"/> if the values are equal.</returns>
        public static bool operator ==(CustomRole left, CustomRole right) => left.Id == right.Id;

        /// <summary>
        /// Compares two operands: <see cref="CustomRole"/> and <see cref="CustomRole"/>.
        /// </summary>
        /// <param name="left">The left <see cref="CustomRole"/> to compare.</param>
        /// <param name="right">The right <see cref="CustomRole"/> to compare.</param>
        /// <returns><see langword="true"/> if the values are not equal.</returns>
        public static bool operator !=(CustomRole left, CustomRole right) => left.Id != right.Id;

        /// <summary>
        /// Gets a <see cref="CustomRole"/> given the specified <paramref name="customRoleType"/>.
        /// </summary>
        /// <param name="customRoleType">The specified <see cref="Id"/>.</param>
        /// <returns>The <see cref="CustomRole"/> matching the search or <see langword="null"/> if not registered.</returns>
        public static CustomRole Get(object customRoleType) => Registered.FirstOrDefault(customRole => customRole == customRoleType && customRole.IsEnabled);

        /// <summary>
        /// Gets a <see cref="CustomRole"/> given the specified name.
        /// </summary>
        /// <param name="name">The specified name.</param>
        /// <returns>The <see cref="CustomRole"/> matching the search or <see langword="null"/> if not registered.</returns>
        public static CustomRole Get(string name) => Registered.FirstOrDefault(customRole => customRole.Name == name);

        /// <summary>
        /// Gets a <see cref="CustomRole"/> given the specified <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The specified <see cref="Type"/>.</param>
        /// <returns>The <see cref="CustomRole"/> matching the search or <see langword="null"/> if not found.</returns>
        public static CustomRole Get(Type type) => type.BaseType != typeof(RoleBehaviour) ? null : Registered.FirstOrDefault(customRole => customRole.BehaviourComponent == type);

        /// <summary>
        /// Gets a <see cref="CustomRole"/> given the specified <see cref="RoleBehaviour"/>.
        /// </summary>
        /// <param name="roleBuilder">The specified <see cref="RoleBehaviour"/>.</param>
        /// <returns>The <see cref="CustomRole"/> matching the search or <see langword="null"/> if not found.</returns>
        public static CustomRole Get(RoleBehaviour roleBuilder) => Get(roleBuilder.GetType());

        /// <summary>
        /// Gets a <see cref="CustomRole"/> from a <see cref="Player"/>.
        /// </summary>
        /// <param name="player">The <see cref="CustomRole"/> owner.</param>
        /// <returns>The <see cref="CustomRole"/> matching the search or <see langword="null"/> if not registered.</returns>
        public static CustomRole Get(Player player)
        {
            CustomRole customRole = default;

            foreach (KeyValuePair<Player, CustomRole> kvp in Manager)
            {
                if (kvp.Key != player)
                    continue;

                customRole = Get(kvp.Value.Id);
            }

            return customRole;
        }

        /// <summary>
        /// Tries to get a <see cref="CustomRole"/> given the specified <see cref="CustomRole"/>.
        /// </summary>
        /// <param name="customRoleType">The <see cref="object"/> to look for.</param>
        /// <param name="customRole">The found <see cref="CustomRole"/>, <see langword="null"/> if not registered.</param>
        /// <returns><see langword="true"/> if a <see cref="CustomRole"/> was found; otherwise, <see langword="false"/>.</returns>
        public static bool TryGet(object customRoleType, out CustomRole customRole) => (customRole = Get(customRoleType)) is not null;

        /// <summary>
        /// Tries to get a <see cref="CustomRole"/> given a specified name.
        /// </summary>
        /// <param name="name">The <see cref="CustomRole"/> name to look for.</param>
        /// <param name="customRole">The found <see cref="CustomRole"/>, <see langword="null"/> if not registered.</param>
        /// <returns><see langword="true"/> if a <see cref="CustomRole"/> was found; otherwise, <see langword="false"/>.</returns>
        public static bool TryGet(string name, out CustomRole customRole) => (customRole = Registered.FirstOrDefault(cRole => cRole.Name == name)) is not null;

        /// <summary>
        /// Tries to get the player's current <see cref="CustomRole"/>.
        /// </summary>
        /// <param name="player">The <see cref="Player"/> to search on.</param>
        /// <param name="customRole">The found <see cref="CustomRole"/>, <see langword="null"/> if not registered.</param>
        /// <returns><see langword="true"/> if a <see cref="CustomRole"/> was found; otherwise, <see langword="false"/>.</returns>
        public static bool TryGet(Player player, out CustomRole customRole) => (customRole = Get(player)) is not null;

        /// <summary>
        /// Tries to get the player's current <see cref="CustomRole"/>.
        /// </summary>
        /// <param name="roleBuilder">The <see cref="RoleBehaviour"/> to search for.</param>
        /// <param name="customRole">The found <see cref="CustomRole"/>, <see langword="null"/> if not registered.</param>
        /// <returns><see langword="true"/> if a <see cref="CustomRole"/> was found; otherwise, <see langword="false"/>.</returns>
        public static bool TryGet(RoleBehaviour roleBuilder, out CustomRole customRole) => (customRole = Get(roleBuilder.GetType())) is not null;

        /// <summary>
        /// Tries to get the player's current <see cref="CustomRole"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to search for.</param>
        /// <param name="customRole">The found <see cref="CustomRole"/>, <see langword="null"/> if not registered.</param>
        /// <returns><see langword="true"/> if a <see cref="CustomRole"/> was found; otherwise, <see langword="false"/>.</returns>
        public static bool TryGet(Type type, out CustomRole customRole) => (customRole = Get(type.GetType())) is not null;

        /// <summary>
        /// Tries to spawn the player as a specific <see cref="CustomRole"/>.
        /// </summary>
        /// <param name="player">The <see cref="Player"/> to be spawned.</param>
        /// <param name="customRole">The <see cref="CustomRole"/> to be set.</param>
        /// <returns><see langword="true"/> if the player was spawned; otherwise, <see langword="false"/>.</returns>
        public static bool SafeSpawn(Player player, CustomRole customRole)
        {
            if (customRole is null)
                return false;

            customRole.Spawn(player);

            return true;
        }

        /// <summary>
        /// Tries to spawn the player as a specific <see cref="CustomRole"/>.
        /// </summary>
        /// <param name="player">The <see cref="Player"/> to be spawned.</param>
        /// <param name="customRoleType">The <see cref="object"/> to be set.</param>
        /// <returns><see langword="true"/> if the player was spawned; otherwise, <see langword="false"/>.</returns>
        public static bool SafeSpawn(Player player, object customRoleType)
        {
            if (!TryGet(customRoleType, out CustomRole customRole))
                return false;

            SafeSpawn(player, customRole);

            return true;
        }

        /// <summary>
        /// Tries to spawn the player as a specific <see cref="CustomRole"/> by name.
        /// </summary>
        /// <param name="player">The <see cref="Player"/> to be spawned.</param>
        /// <param name="name">The <see cref="CustomRole"/> name to be set.</param>
        /// <returns>Returns a value indicating whether the <see cref="Player"/> was spawned or not.</returns>
        public static bool SafeSpawn(Player player, string name)
        {
            if (!TryGet(name, out CustomRole customRole))
                return false;

            SafeSpawn(player, customRole);

            return true;
        }

        /// <summary>
        /// Tries to force spawn the player as a specific <see cref="CustomRole"/>.
        /// </summary>
        /// <param name="player">The <see cref="Player"/> to be spawned.</param>
        /// <param name="customRole">The <see cref="CustomRole"/> to be set.</param>
        /// <param name="shouldKeepPosition">A value indicating whether the <see cref="Player"/> should be spawned in the same position.</param>
        /// <returns><see langword="true"/> if the player was spawned; otherwise, <see langword="false"/>.</returns>
        public static bool UnsafeSpawn(Player player, CustomRole customRole, bool shouldKeepPosition = false)
        {
            if (customRole is null)
                return false;

            customRole.ForceSpawn(player, shouldKeepPosition);

            return true;
        }

        /// <summary>
        /// Tries to force spawn the player as a specific <see cref="CustomRole"/>.
        /// </summary>
        /// <param name="player">The <see cref="Player"/> to be spawned.</param>
        /// <param name="customRoleType">The <see cref="object"/> to be set.</param>
        /// <param name="shouldKeepPosition">A value indicating whether the <see cref="Player"/> should be spawned in the same position.</param>
        /// <returns><see langword="true"/> if the player was spawned; otherwise, <see langword="false"/>.</returns>
        public static bool UnsafeSpawn(Player player, object customRoleType, bool shouldKeepPosition = false)
        {
            if (!TryGet(customRoleType, out CustomRole customRole))
                return false;

            UnsafeSpawn(player, customRole, shouldKeepPosition);

            return true;
        }

        /// <summary>
        /// Tries to force spawn the player as a specific <see cref="CustomRole"/> by name.
        /// </summary>
        /// <param name="player">The <see cref="Player"/> to be spawned.</param>
        /// <param name="name">The <see cref="CustomRole"/> name to be set.</param>
        /// <param name="shouldKeepPosition">A value indicating whether the <see cref="Player"/> should be spawned in the same position.</param>
        /// <returns><see langword="true"/> if the player was spawned; otherwise, <see langword="false"/>.</returns>
        public static bool UnsafeSpawn(Player player, string name, bool shouldKeepPosition = false)
        {
            if (!TryGet(name, out CustomRole customRole))
                return false;

            UnsafeSpawn(player, customRole, shouldKeepPosition);

            return true;
        }

        /// <summary>
        /// Enables all the custom roles present in the assembly.
        /// </summary>
        /// <returns>A <see cref="IEnumerable{T}"/> of <see cref="CustomRole"/> which contains all the enabled custom roles.</returns>
        public static List<CustomRole> EnableAll()
        {
            List<CustomRole> customRoles = new();
            foreach (Type type in Assembly.GetCallingAssembly().GetTypes())
            {
                CustomRoleAttribute attribute = type.GetCustomAttribute<CustomRoleAttribute>();
                if ((type.BaseType != typeof(CustomRole) && !type.IsSubclassOf(typeof(CustomRole))) || attribute is null)
                    continue;

                CustomRole customRole = Activator.CreateInstance(type) as CustomRole;

                if (!customRole.IsEnabled)
                    continue;

                if (customRole.TryRegister(attribute))
                    customRoles.Add(customRole);
            }

            if (customRoles.Count() != Registered.Count())
                Log.SendRaw($"[{Assembly.GetCallingAssembly().GetName().Name}] {customRoles.Count()} custom roles have been successfully registered!", ConsoleColor.Cyan);

            return customRoles;
        }

        /// <summary>
        /// Disables all the custom roles present in the assembly.
        /// </summary>
        /// <returns>A <see cref="IEnumerable{T}"/> of <see cref="CustomRole"/> which contains all the disabled custom roles.</returns>
        public static List<CustomRole> DisableAll()
        {
            List<CustomRole> customRoles = new();
            customRoles.AddRange(Registered.Where(customRole => customRole.TryUnregister()));

            Log.SendRaw($"[{Assembly.GetCallingAssembly().GetName().Name}] {customRoles.Count()} custom roles have been successfully unregistered!", ConsoleColor.Cyan);

            return customRoles;
        }

        /// <summary>
        /// Tries to register a <see cref="CustomRole"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the <see cref="CustomRole"/> was registered; otherwise, <see langword="false"/>.</returns>
        internal bool TryRegister(CustomRoleAttribute attribute = null)
        {
            if (!Registered.Contains(this))
            {
                if (attribute is not null && Id == 0)
                    Id = attribute.Id;

                if (Registered.Any(x => x.Id == Id))
                {
                    Log.Debug(
                        $"[CustomRoles] Couldn't register {Name}. " +
                        $"Another custom role has been registered with the same id:" +
                        $" {Registered.FirstOrDefault(x => x.Id == Id)}");

                    return false;
                }

                _registered.Add(this);

                if (SpawnOnEvents.Any())
                    Events.Handlers.Server.InvokingHandler += HandleSpawnOnEvents;

                Exiled.Events.Handlers.Player.Spawning += OverrideDefaultSpawnpoint;
                Exiled.Events.Handlers.Player.ChangingRole += OverrideDefaultInventory;

                return true;
            }

            Log.Debug($"[CustomRoles] Couldn't register {Name}. This custom role has been already registered.");

            return false;
        }

        /// <summary>
        /// Tries to unregister a <see cref="CustomRole"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the <see cref="CustomRole"/> was unregistered; otherwise, <see langword="false"/>.</returns>
        internal bool TryUnregister()
        {
            if (!Registered.Contains(this))
            {
                Log.Debug($"[CustomRoles] Couldn't unregister {Name}. This custom role hasn't been registered yet.");

                return false;
            }

            _registered.Remove(this);

            if (SpawnOnEvents.Any())
                Events.Handlers.Server.InvokingHandler -= HandleSpawnOnEvents;

            Exiled.Events.Handlers.Player.Spawning -= OverrideDefaultSpawnpoint;
            Exiled.Events.Handlers.Player.ChangingRole -= OverrideDefaultInventory;

            return true;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><see langword="true"/> if the object was equal; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object obj) => obj is CustomRole customRole && customRole == this;

        /// <summary>
        /// Returns a the 32-bit signed hash code of the current object instance.
        /// </summary>
        /// <returns>The 32-bit signed hash code of the current object instance.</returns>
        public override int GetHashCode() => base.GetHashCode();

        /// <summary>
        /// Spawns the player as a specific <see cref="CustomRole"/>.
        /// </summary>
        /// <param name="player">The <see cref="Player"/> to be spawned.</param>
        /// <returns><see langword="true"/> if the player was spawned; otherwise, <see langword="false"/>.</returns>
        public bool Spawn(Player player)
        {
            if (player.Role.Team is not Team.Dead)
                return false;

            Features.RespawnManager.RespawnQueue.Add(player);

            player.AddComponent(BehaviourComponent);
            PlayersValue.Remove(player);
            PlayersValue.Add(player, this);

            return true;
        }

        /// <summary>
        /// Force spawns the player as a specific <see cref="CustomRole"/>.
        /// </summary>
        /// <param name="player">The <see cref="Player"/> to be spawned.</param>
        /// <param name="shouldKeepPosition">A value indicating whether the <see cref="Player"/> should be spawned in the same position.</param>
        public void ForceSpawn(Player player, bool shouldKeepPosition = false)
        {
            PlayersValue.Remove(player);
            PlayersValue.Add(player, this);

            if (!player.IsAlive)
            {
                ForceSpawn_Internal(player, shouldKeepPosition);
                return;
            }

            player.SetRole(RoleType.Spectator);
            Timing.CallDelayed(0.1f, () => ForceSpawn_Internal(player, shouldKeepPosition));
        }

        /// <summary>
        /// Gets a value indicating whether a player can be spawned as a specific <see cref="CustomRole"/> given its probability.
        /// </summary>
        /// <returns><see langword="true"/> if the probability condition was satified; otherwise, <see langword="false"/>.</returns>
        public bool CanSpawnByProbability() => UnityEngine.Random.Range(0, 101) <= SpawnProbability;

        private void HandleSpawnOnEvents(InvokingHandlerEventArgs ev)
        {
            foreach (KeyValuePair<string, int> kvp in SpawnOnEvents)
            {
                string[] eventName = ev.Name.ToLower().SplitCamelCase().Split(' ');
                if ((kvp.Key.ToLower() != ev.Name.ToLower() &&
                    !eventName.Contains(kvp.Key.ToLower())) ||
                    !kvp.Value.EvaluateProbability())
                    continue;

                IEnumerable<Player> players = Player.Get(Team.Dead);
                if (players.IsEmpty())
                    return;

                SafeSpawn(Player.Get(Team.Dead).Random(), this);
            }
        }

        private void OverrideDefaultSpawnpoint(SpawningEventArgs ev)
        {
            if (!TryGet(ev.Player, out CustomRole customRole) ||
                customRole != this ||
                !ev.Player.GameObject.TryGetComponent(out RoleBehaviour builder))
                return;

            bool useCustomSpawnpoint = (bool)builder.GetType().BaseType
                .GetProperty("UseCustomSpawnpoint", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(builder);

            if (!useCustomSpawnpoint)
                return;

            ev.Position = builder.RandomSpawnpoint + (Vector3.up * 1.5f);
        }

        private void OverrideDefaultInventory(ChangingRoleEventArgs ev)
        {
            if (!TryGet(ev.Player, out CustomRole customRole) ||
                customRole != this ||
                !ev.Player.GameObject.TryGetComponent(out RoleBehaviour builder))
                return;

            ev.Items.Clear();
            ev.Ammo.Clear();

            InventoryManager inventory = (InventoryManager)builder.GetType().BaseType
                .GetProperty("Inventory", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(builder);

            if (inventory.Items is not null)
                ev.Items.AddRange(inventory.Items);

            if (inventory.CustomItems is not null)
                ev.Player.AddItem(inventory.CustomItems);

            if (inventory.AmmoBox is not null)
            {
                foreach (KeyValuePair<AmmoType, ushort> kvp in inventory.AmmoBox)
                    ev.Ammo?.Add(kvp.Key.GetItemType(), kvp.Value);
            }

            if (RoleBehaviour.StaticPlayers.Contains(ev.Player))
            {
                ev.SpawnFlags |= RoleSpawnFlags.AssignInventory;
                RoleBehaviour.StaticPlayers.Remove(ev.Player);
            }
        }

        private void ForceSpawn_Internal(Player player, bool shouldKeepPosition)
        {
            Features.RespawnManager.RespawnQueue.Add(player);

            if (shouldKeepPosition)
                RoleBehaviour.StaticPlayers.Add(player);

            player.AddComponent(BehaviourComponent, $"ECS-{Name}");
        }
    }
}
