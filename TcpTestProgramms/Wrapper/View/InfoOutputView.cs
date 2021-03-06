﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wrapper.Contracts;
using Wrapper.Implementation;

namespace Wrapper.View
{
    public class InfoOutputView : IUpdateOutputView
    {
        public const int DEFAULT_POSITION_X = 0;
        public const int DEFAULT_POSITION_Y = 5;

        private readonly IOutputWrapper _outputWrapper;
        private int _posX;
        private int _posY;
        private string _info;

        public bool ViewEnabled { get; set; }

        public InfoOutputView(IOutputWrapper outputWrapper, int posX, int posY)
        {
            _outputWrapper = outputWrapper;
            _posX = posX;
            _posY = posY;
           
        }

        public InfoOutputView()
            : this(new OutputWrapper(), DEFAULT_POSITION_X, DEFAULT_POSITION_Y)
        { }

        public void SetUpdateContent(string content)
        {
			if (!(content == null || content.Length == 0))
			{
				ViewEnabled = true;
				_info = content;
			}
        }

        public void Show()
        {
            _outputWrapper.WriteOutput(_posX, _posY, _info, ConsoleColor.Magenta);
        }
    }
}
