using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace FactorioToolAssistedSpeedrun.Services
{
    public static class WindowPositionManager
    {
        public static void Save(Window window, string keyPrefix)
        {
            Properties.Settings.Default[$"{keyPrefix}_Top"] = window.Top;
            Properties.Settings.Default[$"{keyPrefix}_Left"] = window.Left;
            Properties.Settings.Default[$"{keyPrefix}_Width"] = window.Width;
            Properties.Settings.Default[$"{keyPrefix}_Height"] = window.Height;
            Properties.Settings.Default.Save();
        }

        public static void Load(Window window, string keyPrefix)
        {
            if (Properties.Settings.Default[$"{keyPrefix}_Top"] is double top)
                window.Top = top;
            if (Properties.Settings.Default[$"{keyPrefix}_Left"] is double left)
                window.Left = left;
            if (Properties.Settings.Default[$"{keyPrefix}_Width"] is double width)
                window.Width = width;
            if (Properties.Settings.Default[$"{keyPrefix}_Height"] is double height)
                window.Height = height;
        }
    }
}