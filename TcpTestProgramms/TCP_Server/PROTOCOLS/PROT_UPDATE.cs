﻿using Shared.Contracts;

namespace TCP_Server.PROTOCOLS
{
    public class PROT_UPDATE : IProtocol
    {       
        public string _infoOutput;
		public string _lobbyDisplay;
		public string _commandList;
		public int _diceResult;
		public int _lastPlayer;
		public int _currentplayer;
		public string _turnstate;
		public int _pawn1loacation;
		public int _pawn2location;
    }
}
