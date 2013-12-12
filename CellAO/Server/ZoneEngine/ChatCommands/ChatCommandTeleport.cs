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

namespace ZoneEngine.ChatCommands
{
    #region Usings ...

    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using CellAO.Core.Entities;
    using CellAO.Core.Vector;

    using SmokeLounge.AOtomation.Messaging.GameData;

    using ZoneEngine.Core;
    using ZoneEngine.Core.Playfields;

    #endregion

    /// <summary>
    /// </summary>
    public class ChatCommandteleport : AOChatCommand
    {
        #region Public Methods and Operators

        /// <summary>
        /// </summary>
        /// <param name="args">
        /// </param>
        /// <returns>
        /// </returns>
        public override bool CheckCommandArguments(string[] args)
        {
            var check = new List<Type> { typeof(float), typeof(float), typeof(int) };
            bool check1 = CheckArgumentHelper(check, args);

            check.Clear();
            check.Add(typeof(float));
            check.Add(typeof(float));
            check.Add(typeof(string));
            check.Add(typeof(float));
            check.Add(typeof(int));
            check1 |= CheckArgumentHelper(check, args);

            return check1;
        }

        /// <summary>
        /// </summary>
        /// <param name="client">
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public override void CommandHelp(ZoneClient client)
        {
            // No help needed, no arguments can be given
            client.SendChatText("Teleports you");
            client.SendChatText("Usage: /tp [float] [float] [int] (X, Z, Playfield)");
            client.SendChatText("Or:    /tp [float] [float] y [float] [int] (X, Z, Y, Playfield)");
            return;
        }

        /// <summary>
        /// </summary>
        /// <param name="client">
        /// </param>
        /// <param name="target">
        /// </param>
        /// <param name="args">
        /// </param>
        public override void ExecuteCommand(ZoneClient client, Identity target, string[] args)
        {
            var check = new List<Type> { typeof(float), typeof(float), typeof(int) };

            var coord = new Coordinate();
            int pf = client.Character.Playfield.Identity.Instance;
            if (CheckArgumentHelper(check, args))
            {
                coord = new Coordinate(
                    float.Parse(args[1], NumberStyles.Any, CultureInfo.InvariantCulture), 
                    client.Character.Coordinates.y, 
                    float.Parse(args[2], NumberStyles.Any, CultureInfo.InvariantCulture));
                pf = int.Parse(args[3]);
            }

            check.Clear();
            check.Add(typeof(float));
            check.Add(typeof(float));
            check.Add(typeof(string));
            check.Add(typeof(float));
            check.Add(typeof(int));

            if (CheckArgumentHelper(check, args))
            {
                coord = new Coordinate(
                    float.Parse(args[1], NumberStyles.Any, CultureInfo.InvariantCulture), 
                    float.Parse(args[4], NumberStyles.Any, CultureInfo.InvariantCulture), 
                    float.Parse(args[2], NumberStyles.Any, CultureInfo.InvariantCulture));
                pf = int.Parse(args[5]);
            }

            if (!Playfields.ValidPlayfield(pf))
            {
                client.SendFeedback(110, 188845972);
                return;
            }

            client.Playfield.Teleport(
                (Character)client.Character, 
                coord, 
                client.Character.Heading, 
                new Identity() { Type = IdentityType.Playfield, Instance = pf });
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        public override int GMLevelNeeded()
        {
            return 1;
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        public override List<string> ListCommands()
        {
            var temp = new List<string> { "tp", "teleport" };
            return temp;
        }

        #endregion
    }
}