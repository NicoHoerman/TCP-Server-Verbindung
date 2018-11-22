﻿using Newtonsoft.Json;
using Shared.Communications;
using Shared.Contract;
using Shared.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using TCP_Client.DTO;
using TCP_Client.PROTOCOLS;
using TCP_Client.UDP;
using Wrapper;
using Wrapper.Contracts;
using Wrapper.Implementation;

namespace TCP_Client.Actions
{
    public class InputAction
    {
        private int someInt;
        private bool isConnected = false;
        private bool Searched = false;
        private System.Timers.Timer timer;

        public string AfterConnectMsg { get; set; } = string.Empty;

        public Dictionary<string, Action<string,ICommunication>> _inputActions;

        private ProtocolAction _ActionHandler;
        private Dictionary<ClientView, IView> _views;
        private readonly IErrorView _errorView;
        private readonly IHelpOutputView _helpOutputView;
        public readonly IInputView _inputView;
        
        private OutputWrapper _OutputWrapper;  

        private UdpClientUnit _UdpListener;

        public InputAction(ProtocolAction protocolAction, Dictionary<ClientView, IView> views)
        {

            _ActionHandler = protocolAction;
            _views = views;
            _errorView = views[ClientView.Error] as IErrorView; // Potential null exception error.
            _helpOutputView = views[ClientView.HelpOutput] as IHelpOutputView; //Potenzieller Null Ausnahmen Fehler
            _inputView = views[ClientView.Input] as IInputView;
            _OutputWrapper = new OutputWrapper();

            _inputActions = new Dictionary<string, Action<string,ICommunication>>
            {
                { "/help", OnInputHelpAction },
                { "/rolldice", OnInputRollDiceAction },
                { "/closegame", OnCloseGameAction },
                {"/someInt" ,OnIntAction },
                {"/search", OnSearchAction },
                {"/startgame", OnStartGameAction }
            };

            _UdpListener = new UdpClientUnit();

        }

       

        public void ParseAndExecuteCommand(string input,ICommunication communication)
        {
            string receivedInput = input;
            if(input == "/someInt")
            {
                _errorView.SetContent(input, "Error: " + "This command does not exist or isn't enabled at this time");
                return;
            }
            if (input.All(char.IsDigit))
            {
                input = "/someInt";
            }

            if (_inputActions.TryGetValue(input, out var action) == false)
            {
                //log
                _errorView.SetContent(input, "Error: " + "This command does not exist or isn't enabled at this time");
                return;
            }

            action(receivedInput,communication);
        }

        #region Input actions

        private void OnInputHelpAction(string input, ICommunication communication)
        {
            if (!isConnected)
            {
                _errorView.SetContent(input, "Error: " + "This command does not exist or isn't enabled at this time");
                return;
            }
            if (communication.IsMaster == true)
                _helpOutputView.SetHelp("Your master help\ncould be standing here");
            else
                _helpOutputView.SetHelp("Your normal help\ncould be standing here");

            var dataPackage = new DataPackage
            {

                Header = ProtocolActionEnum.GetHelp, //"Client_wants_to_get_help",
                Payload = JsonConvert.SerializeObject(new PROT_HELP
                {

                })

            };
            dataPackage.Size = dataPackage.ToByteArray().Length;

            communication.Send(dataPackage);
        }

        private void OnInputRollDiceAction(string input, ICommunication communication)
        {
            if (!isConnected)
            {
                _errorView.SetContent(input, "Error: " + "This command does not exist or isn't enabled at this time");
                return;
            }

            var dataPackage = new DataPackage
            {
                Header = ProtocolActionEnum.RollDice, //"Client_wants_to_rolldice",
                Payload = JsonConvert.SerializeObject(new PROT_ROLLDICE
                {

                })
            };
            dataPackage.Size = dataPackage.ToByteArray().Length;

            communication.Send(dataPackage);
        }

        private void OnIntAction(string input, ICommunication communication)
        {

            if (isConnected | !Searched)
            {
                _errorView.SetContent(input, "Error: " + "This command does not exist or isn't enabled at this time");
                return;
            }
            int chosenServerId = Int32.Parse(input);
            if (_ActionHandler._serverDictionary.Count >= chosenServerId)
            {
                BroadcastDTO current = _ActionHandler.GetServer(chosenServerId-1);
                communication._client.Connect(IPAddress.Parse(current._Server_ip), current._Server_Port);
                AfterConnectMsg = $"Server {chosenServerId} chosen";
                isConnected = true;
                communication.SetNWStream();
                var dataPackage = new DataPackage
                {
                    Header = ProtocolActionEnum.OnConnection, 
                    Payload = JsonConvert.SerializeObject(new PROT_CONNECTION
                    {

                    })
                };
                dataPackage.Size = dataPackage.ToByteArray().Length;

                communication.Send(dataPackage);
            }
            else
            {
                _errorView.SetContent(input, "There is no server with this ID");
            }

            _inputView.SetInputLine("Type a command:", 16);

        }

        private void OnSearchAction(string input, ICommunication communication)
        {
            if (isConnected)
            {
                _errorView.SetContent(input, "Error: " + "This command does not exist or isn't enabled at this time");
                return;
            }

            _OutputWrapper.WriteOutput(0, 1, "Searching...", ConsoleColor.DarkGray);                                                                                   

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            _UdpListener._closed = false;
            _UdpListener.SendRequest();

            while (stopwatch.ElapsedMilliseconds < 3000)
            {
                if (_UdpListener._DataList.Count > 0)
                {
                    communication.AddPackage(_UdpListener._DataList.First());
                    _UdpListener._DataList.RemoveAt(0);
                }
            }
            stopwatch.Stop();
            _UdpListener.StopListening();
            Searched = true;
            _inputView.SetInputLine("Enter the server number you want to connect to.",49);

            //_UdpListener._closed = false;
        }

        private void OnCloseGameAction(string obj,ICommunication communication)
        {
            throw new NotImplementedException();
        }

        private void OnStartGameAction(string arg1, ICommunication arg2)
        {
            throw new NotImplementedException();
        }
        #endregion


    }
}
