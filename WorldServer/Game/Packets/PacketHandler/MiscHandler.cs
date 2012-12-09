﻿/*
 * Copyright (C) 2012 Arctium <http://>
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using Framework.Constants;
using Framework.Logging;
using Framework.Network.Packets;
using WorldServer.Network;
using WorldServer.Game.Managers;
using Framework.Database;

namespace WorldServer.Game.Packets.PacketHandler
{
    public class MiscHandler : Globals
    {
        public static void HandleMessageOfTheDay(ref WorldClass session)
        {
            PacketWriter motd = new PacketWriter(LegacyMessage.MessageOfTheDay);
            motd.WriteUInt32(3);

            motd.WriteCString("Arctium MoP test");
            motd.WriteCString("Welcome to our MoP server test.");
            motd.WriteCString("Your development team =)");
            session.Send(motd);
        }

        [Opcode(ClientMessage.Ping, "16309")]
        public static void HandlePong(ref PacketReader packet, ref WorldClass session)
        {
            uint sequence = packet.ReadUInt32();
            uint latency = packet.ReadUInt32();

            PacketWriter pong = new PacketWriter(JAMCCMessage.Pong);
            pong.WriteUInt32(sequence);

            session.Send(pong);
        }

        [Opcode(ClientMessage.LogDisconnect, "16309")]
        public static void HandleDisconnectReason(ref PacketReader packet, ref WorldClass session)
        {
            var pChar = session.Character;
            uint disconnectReason = packet.ReadUInt32();

            if (pChar != null)
                WorldMgr.DeleteSession(pChar.Guid);

            DB.Realms.Execute("UPDATE accounts SET online = 0 WHERE id = ?", session.Account.Id);

            Log.Message(LogType.DEBUG, "Account with Id {0} disconnected. Reason: {1}", session.Account.Id, disconnectReason);
        }

        public static void HandleUpdateClientCacheVersion(ref WorldClass session)
        {
            PacketWriter cacheVersion = new PacketWriter(LegacyMessage.UpdateClientCacheVersion);

            cacheVersion.WriteUInt32(0);

            session.Send(cacheVersion);
        }

        [Opcode(ClientMessage.LoadingScreenNotify, "16309")]
        public static void HandleLoadingScreenNotify(ref PacketReader packet, ref WorldClass session)
        {
            BitUnpack BitUnpack = new BitUnpack(packet);

            uint mapId = packet.ReadUInt32();
            bool loadingScreenState = BitUnpack.GetBit();

            Log.Message(LogType.DEBUG, "Loading screen for map '{0}' is {1}.", mapId, loadingScreenState ? "enabled" : "disabled");
        }

        [Opcode(ClientMessage.ViolenceLevel, "16309")]
        public static void HandleViolenceLevel(ref PacketReader packet, ref WorldClass session)
        {
            byte violenceLevel = packet.ReadUInt8();

            Log.Message(LogType.DEBUG, "Violence level from account '{0} (Id: {1})' is {2}.", session.Account.Name, session.Account.Id, violenceLevel);
        }

        [Opcode(ClientMessage.ActivePlayer, "16309")]
        public static void HandleActivePlayer(ref PacketReader packet, ref WorldClass session)
        {
            byte active = packet.ReadUInt8();    // Always 0

            Log.Message(LogType.DEBUG, "Player {0} (Guid: {1}) is active.", session.Character.Name, session.Character.Guid);
        }

        [Opcode(ClientMessage.ZoneUpdate, "16309")]
        public static void HandleZoneUpdate(ref PacketReader packet, ref WorldClass session)
        {
            var pChar = session.Character;

            uint zone = packet.ReadUInt32();    // Always 0

            ObjectMgr.SetZone(ref pChar, zone);
        }
    }
}
