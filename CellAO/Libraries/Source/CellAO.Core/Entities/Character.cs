﻿#region License

// Copyright (c) 2005-2013, CellAO Team
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
// 
//     * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//     * Neither the name of the CellAO Team nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
// EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
// PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
// PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
// LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

#endregion

namespace CellAO.Core.Entities
{
    #region Usings ...

    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CellAO.Core.Inventory;
    using CellAO.Core.Nanos;
    using CellAO.Core.Network;
    using CellAO.Core.Textures;
    using CellAO.Core.Vector;
    using CellAO.Database.Dao;
    using CellAO.Database.Entities;
    using CellAO.Enums;
    using CellAO.Interfaces;
    using CellAO.Stats;

    using SmokeLounge.AOtomation.Messaging.GameData;
    using SmokeLounge.AOtomation.Messaging.Messages;
    using SmokeLounge.AOtomation.Messaging.Messages.N3Messages;

    using ZoneEngine.Core;

    using Quaternion = CellAO.Core.Vector.Quaternion;
    using Vector3 = SmokeLounge.AOtomation.Messaging.GameData.Vector3;

    #endregion

    /// <summary>
    /// </summary>
    public class Character : Dynel, ICharacter
    {
        #region Fields

        /// <summary>
        /// </summary>
        public Dictionary<int, int> SocialTab = new Dictionary<int, int>();

        /// <summary>
        /// </summary>
        public List<AOTextures> Textures = new List<AOTextures>();

        /// <summary>
        /// Caching Mesh layer structure
        /// </summary>
        private MeshLayers meshLayer = new MeshLayers();

        /// <summary>
        /// </summary>
        private MoveDirections moveDirection;

        /// <summary>
        /// </summary>
        private DateTime predictionTime;

        /// <summary>
        /// Caching Mesh layer for social tab items
        /// </summary>
        private MeshLayers socialMeshLayer = new MeshLayers();

        /// <summary>
        /// </summary>
        private SpinOrStrafeDirections spinDirection;

        /// <summary>
        /// </summary>
        private SpinOrStrafeDirections strafeDirection;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// </summary>
        /// <param name="zoneClient">
        /// </param>
        /// <param name="identity">
        /// </param>
        public Character(IZoneClient zoneClient, Identity identity)
            : base(identity)
        {
            this.Client = zoneClient;
            this.ActiveNanos = new List<IActiveNano>();

            this.UploadedNanos = new List<IUploadedNanos>();
            foreach (int nano in UploadedNanosDao.ReadNanos(identity.Instance))
            {
                this.UploadedNanos.Add(new UploadedNano() { NanoId = nano });
            }

            this.BaseInventory = new PlayerInventory(this);
            this.Stats.AfterStatChangedEvent += this.StatsAfterStatChangedEvent;

            this.SocialTab = new Dictionary<int, int>
                             {
                                 { 0, 0 }, 
                                 { 1, 0 }, 
                                 { 2, 0 }, 
                                 { 3, 0 }, 
                                 { 4, 0 }, 
                                 { 38, 0 }, 
                                 { 1004, 0 }, 
                                 { 1005, 0 }, 
                                 { 64, 0 }, 
                                 { 32, 0 }, 
                                 { 1006, 0 }, 
                                 { 1007, 0 }
                             };

            this.meshLayer.AddMesh(0, this.Stats[StatIds.headmesh].Value, 0, 4);
            this.socialMeshLayer.AddMesh(0, this.Stats[StatIds.headmesh].Value, 0, 4);

            this.BaseInventory.Read();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// </summary>
        public List<IActiveNano> ActiveNanos { get; private set; }

        /// <summary>
        /// </summary>
        public IZoneClient Client { get; private set; }

        /// <summary>
        /// </summary>
        public Coordinate Coordinates { get; set; }

        /// <summary>
        /// </summary>
        public Identity FightingTarget { get; set; }

        /// <summary>
        /// </summary>
        public Quaternion Heading { get; set; }

        /// <summary>
        /// </summary>
        public IInventoryPage MainInventory { get; private set; }

        /// <summary>
        /// </summary>
        public MeshLayers MeshLayer
        {
            get
            {
                return this.meshLayer;
            }

            private set
            {
                this.meshLayer = value;
            }
        }

        /// <summary>
        /// </summary>
        public MoveModes MoveMode { get; set; }

        /// <summary>
        /// </summary>
        public string OrganizationName
        {
            get
            {
                try
                {
                    return OrganizationDao.GetOrganizationData(this.Stats[StatIds.clan].Value).Name;
                }
                catch (Exception)
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// </summary>
        public TimeSpan PredictionDuration { get; private set; }

        /// <summary>
        /// </summary>
        public MoveModes PreviousMoveMode { get; set; }

        /// <summary>
        /// </summary>
        public Vector3 RawCoordinates { get; set; }

        /// <summary>
        /// </summary>
        public Quaternion RawHeading { get; set; }

        /// <summary>
        /// </summary>
        public Identity SelectedTarget { get; set; }

        /// <summary>
        /// </summary>
        public MeshLayers SocialMeshLayer
        {
            get
            {
                return this.socialMeshLayer;
            }

            private set
            {
                this.socialMeshLayer = value;
            }
        }

        /// <summary>
        /// </summary>
        public TradeSkillInfo TradeSkillSource { get; set; }

        /// <summary>
        /// </summary>
        public TradeSkillInfo TradeSkillTarget { get; set; }

        /// <summary>
        /// </summary>
        public List<IUploadedNanos> UploadedNanos { get; private set; }

        #endregion

        #region Properties

        /// <summary>
        /// </summary>
        private MoveModes moveMode
        {
            get
            {
                return (MoveModes)this.Stats[StatIds.currentmovementmode].Value;
            }

            set
            {
                this.Stats[StatIds.currentmovementmode].Value = (int)value;
            }
        }

        /// <summary>
        /// </summary>
        private MoveModes previousMoveMode
        {
            get
            {
                return (MoveModes)this.Stats[StatIds.prevmovementmode].Value;
            }

            set
            {
                this.Stats[StatIds.prevmovementmode].Value = (int)value;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void CalculateSkills()
        {
            // TODO: Reintroduce skill calculation (walk inventory and active nanos)

            // First, walk inventory and get buffs from there
            this.DoNotDoTimers = true;
            this.Stats.ClearModifiers();
            this.Textures.Clear();
            this.meshLayer.Clear();
            this.socialMeshLayer.Clear();
            this.meshLayer.AddMesh(0, (Int32)this.Stats[StatIds.headmesh].BaseValue, 0, 4);
            this.socialMeshLayer.AddMesh(0, (Int32)this.Stats[StatIds.headmesh].BaseValue, 0, 4);

            this.SocialTab = new Dictionary<int, int>
                             {
                                 { 0, 0 }, 
                                 { 1, 0 }, 
                                 { 2, 0 }, 
                                 { 3, 0 }, 
                                 { 4, 0 }, 
                                 { 38, 0 }, 
                                 { 1004, 0 }, 
                                 { 1005, 0 }, 
                                 { 64, 0 }, 
                                 { 32, 0 }, 
                                 { 1006, 0 }, 
                                 { 1007, 0 }
                             };

            this.BaseInventory.CalculateModifiers(this);
        }

        /// <summary>
        /// </summary>
        /// <param name="nanoId">
        /// </param>
        /// <returns>
        /// </returns>
        public bool HasNano(int nanoId)
        {
            return this.UploadedNanos.Any(x => x.NanoId == nanoId);
        }

        /// <summary>
        /// </summary>
        public void Save()
        {
            this.WriteStats();
            this.BaseInventory.Write();
            CharacterDao.UpdatePosition(this.GetDBCharacter());
        }

        /// <summary>
        /// </summary>
        /// <param name="messageBody">
        /// </param>
        /// <param name="announceToPlayfield">
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void Send(MessageBody messageBody, bool announceToPlayfield)
        {
            if (!announceToPlayfield)
            {
                this.Send(messageBody);
                return;
            }

            this.Playfield.Announce(messageBody);
        }

        /// <summary>
        /// </summary>
        /// <param name="message">
        /// </param>
        public void Send(SystemMessage message)
        {
            this.Playfield.Send(this.Client, message);
        }

        /// <summary>
        /// </summary>
        /// <param name="identity">
        /// </param>
        /// <returns>
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public bool SetFightingTarget(Identity identity)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// </summary>
        /// <param name="identity">
        /// </param>
        /// <returns>
        /// </returns>
        public bool SetTarget(Identity identity)
        {
            this.SelectedTarget = identity;
            return true;
        }

        /// <summary>
        /// </summary>
        public void StopMovement()
        {
            // This should be used to stop the interpolating and save last interpolated value of movement before teleporting
            this.RawCoordinates.X = this.Coordinates.x;
            this.RawCoordinates.Y = this.Coordinates.y;
            this.RawCoordinates.Z = this.Coordinates.z;
            this.RawHeading = this.Heading;

            this.spinDirection = SpinOrStrafeDirections.None;
            this.strafeDirection = SpinOrStrafeDirections.None;
            this.moveDirection = MoveDirections.None;
        }

        /// <summary>
        /// </summary>
        /// <param name="destination">
        /// </param>
        /// <param name="heading">
        /// </param>
        /// <param name="playfield">
        /// </param>
        public void Teleport(Coordinate destination, IQuaternion heading, Identity playfield)
        {
            this.Playfield.Teleport(this, destination, heading, playfield);
        }

        /// <summary>
        /// </summary>
        /// <param name="moveType">
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void UpdateMoveType(byte moveType)
        {
            this.predictionTime = DateTime.Now;

            /*
             * NV: Would be nice to have all other possible values filled out for this at some point... *Looks at Suiv*
             * More specifically, 10, 13 and 22 - 10 and 13 seem to be tied to spinning with mouse. 22 seems random (ping mabe?)
             * Also TODO: Tie this with CurrentMovementMode stat and persistance (ie, log off walking, log back on and still walking)
             * Values of CurrentMovementMode and their effects:
                0: slow moving feet not animating
                1: rooted cant sit
                2: walk
                3: run
                4: swim
                5: crawl
                6: sneak
                7: flying
                8: sitting
                9: rooted can sit
                10: same as 0
                11: sleeping
                12: lounging
                13: same as 0
                14: same as 0
                15: same as 0
                16: same as 0
             */
            switch (moveType)
            {
                case 1: // Forward Start
                    this.moveDirection = MoveDirections.Forwards;
                    break;
                case 2: // Forward Stop
                    this.moveDirection = MoveDirections.None;
                    break;

                case 3: // Reverse Start
                    this.moveDirection = MoveDirections.Backwards;
                    break;
                case 4: // Reverse Stop
                    this.moveDirection = MoveDirections.None;
                    break;

                case 5: // Strafe Right Start
                    this.strafeDirection = SpinOrStrafeDirections.Right;
                    break;
                case 6: // Strafe Stop (Right)
                    this.strafeDirection = SpinOrStrafeDirections.None;
                    break;

                case 7: // Strafe Left Start
                    this.strafeDirection = SpinOrStrafeDirections.Left;
                    break;
                case 8: // Strafe Stop (Left)
                    this.strafeDirection = SpinOrStrafeDirections.None;
                    break;

                case 9: // Turn Right Start
                    this.spinDirection = SpinOrStrafeDirections.Right;
                    break;
                case 10: // Mouse Turn Right Start
                    break;
                case 11: // Turn Stop (Right)
                    this.spinDirection = SpinOrStrafeDirections.None;
                    break;

                case 12: // Turn Left Start
                    this.spinDirection = SpinOrStrafeDirections.Left;
                    break;
                case 13: // Mouse Turn Left Start
                    break;
                case 14: // Turn Stop (Left)
                    this.spinDirection = SpinOrStrafeDirections.None;
                    break;

                case 15: // Jump Start

                    // NV: TODO: This!
                    break;
                case 16: // Jump Stop
                    break;

                case 17: // Elevate Up Start
                    break;
                case 18: // Elevate Up Stop
                    break;

                case 19: // ? 19 = 20 = 22 = 31 = 32
                    break;
                case 20: // ? 19 = 20 = 22 = 31 = 32
                    break;

                case 21: // Full Stop
                    break;

                case 22: // ? 19 = 20 = 22 = 31 = 32
                    break;

                case 23: // Switch To Frozen Mode
                    break;
                case 24: // Switch To Walk Mode
                    this.moveMode = MoveModes.Walk;
                    break;
                case 25: // Switch To Run Mode
                    this.moveMode = MoveModes.Run;
                    break;
                case 26: // Switch To Swim Mode
                    break;
                case 27: // Switch To Crawl Mode
                    this.previousMoveMode = this.moveMode;
                    this.moveMode = MoveModes.Crawl;
                    break;
                case 28: // Switch To Sneak Mode
                    this.previousMoveMode = this.moveMode;
                    this.moveMode = MoveModes.Sneak;
                    break;
                case 29: // Switch To Fly Mode
                    break;
                case 30: // Switch To Sit Ground Mode
                    this.previousMoveMode = this.moveMode;
                    this.moveMode = MoveModes.Sit;
                    break;

                case 31: // ? 19 = 20 = 22 = 31 = 32
                    break;
                case 32: // ? 19 = 20 = 22 = 31 = 32
                    break;

                case 33: // Switch To Sleep Mode
                    this.moveMode = MoveModes.Sleep;
                    break;
                case 34: // Switch To Lounge Mode
                    this.moveMode = MoveModes.Lounge;
                    break;

                case 35: // Leave Swim Mode
                    break;
                case 36: // Leave Sneak Mode
                    this.moveMode = this.previousMoveMode;
                    break;
                case 37: // Leave Sit Mode
                    this.moveMode = this.previousMoveMode;
                    break;
                case 38: // Leave Frozen Mode
                    break;
                case 39: // Leave Fly Mode
                    break;
                case 40: // Leave Crawl Mode
                    this.moveMode = this.previousMoveMode;
                    break;
                case 41: // Leave Sleep Mode
                    break;
                case 42: // Leave Lounge Mode
                    break;
                default:

                    // Console.WriteLine("Unknown MoveType: " + moveType);
                    break;
            }

            // Console.WriteLine((moveDirection != 0 ? moveMode.ToString() : "Stand") + "ing in the direction " + moveDirection.ToString() + (spinDirection != 0 ? " while spinning " + spinDirection.ToString() : "") + (strafeDirection != 0 ? " and strafing " + strafeDirection.ToString() : ""));
        }

        /// <summary>
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void WriteStats()
        {
            this.Stats.Write();
        }

        #endregion

        #region Methods

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        internal DBCharacter GetDBCharacter()
        {
            DBCharacter temp = new DBCharacter();
            temp.FirstName = this.FirstName;
            temp.LastName = this.LastName;

            temp.HeadingW = this.RawHeading.wf;
            temp.HeadingX = this.RawHeading.xf;
            temp.HeadingY = this.RawHeading.yf;
            temp.HeadingZ = this.RawHeading.zf;
            temp.X = this.RawCoordinates.X;
            temp.Y = this.RawCoordinates.Y;
            temp.Z = this.RawCoordinates.Z;

            temp.Id = this.Identity.Instance;
            temp.Name = this.Name;
            temp.Online = 1;
            temp.Playfield = this.Playfield.Identity.Instance;
            return temp;
        }

        /// <summary>
        /// </summary>
        /// <param name="messageBody">
        /// </param>
        internal void Send(MessageBody messageBody)
        {
            this.Playfield.Send(this.Client, messageBody);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender">
        /// </param>
        /// <param name="e">
        /// </param>
        private void StatsAfterStatChangedEvent(object sender, StatChangedEventArgs e)
        {
            if (DoNotDoTimers)
            {
                return;
            }

            uint valueToSend = e.Stat.SendBaseValue ? e.Stat.BaseValue : (uint)e.Stat.Value;

            var messageBody = new StatMessage()
                              {
                                  Identity = this.Identity, 
                                  Stats =
                                      new[]
                                      {
                                          new GameTuple<CharacterStat, uint>()
                                          {
                                              Value1 =
                                                  (CharacterStat)
                                                  e.Stat.StatId, 
                                              Value2 = valueToSend
                                          }
                                      }, 
                              };

            if (e.AnnounceToPlayfield)
            {
                this.Playfield.Announce(messageBody);
            }
            else
            {
                this.Send(messageBody);
            }
        }

        #endregion
    }
}