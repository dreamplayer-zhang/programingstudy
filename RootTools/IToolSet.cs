﻿using System.Windows.Controls;

namespace RootTools
{
    public interface IToolSet
    {
        string p_id { get; set; }
        void ThreadStop();
    }
}
