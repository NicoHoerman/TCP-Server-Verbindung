﻿using EandE_ServerModel.EandE.GameAndLogic;
using Shared.Contract;
using System.Linq;
using TCP_Server.DataProvider;
using TCP_Server.Enum;
using TCP_Server.Support;

namespace TCP_Server.Test
{
	public class ClientDisconnection
    {
        public Game _game;
        public ServerInfo _serverInfo;
        private ServerDataPackageProvider _dataPackageProvider;

        public ClientDisconnection(Game game, ServerInfo serverInfo,ServerDataPackageProvider dataPackageProvider)
        {
            _game = game;
            _serverInfo = serverInfo;
            _dataPackageProvider = dataPackageProvider;
        }

        public void Execute(ICommunication communication )
        {
            communication.Send(_dataPackageProvider.GetPackage("DeclinedInfo"));
            DisconnectClient();
            for (int i = 0; i <= _serverInfo._communications.Count - 1; i++)
                _serverInfo._communications[i].Send(_dataPackageProvider.GetPackage("DeclineUpdate"));
        }

        public void RemoveFromLobby()
        {
            _serverInfo._communicationsToRemove.ForEach(x => _serverInfo._lobbylist[0]._CurrentPlayerCount--);
            RemoveFromList();

            if (_game._isRunning)
                _game.State.ExecuteStateAction("close");
        }

        private void RemoveFromList()
        {
            _serverInfo._communicationsToRemove.ForEach(x => _serverInfo._communications.Remove(x));
        }

        public void DisconnectClient()
        {
            var currentCommunication = _serverInfo._communications.Last();
            currentCommunication.Stop();

            _serverInfo._communicationsToRemove.Add(currentCommunication);

            RemoveFromList();
            Core.ValidationStatus = ValidationEnum.WaitingForPlayer;
        }
    }
}
