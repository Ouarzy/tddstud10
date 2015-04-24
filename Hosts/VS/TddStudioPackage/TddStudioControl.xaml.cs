﻿/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OpenCover.Framework.Model;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using Microsoft.VisualStudio.Shell;

namespace R4nd0mApps.TddStud10.Hosts.VS
{
    /// <summary>
    /// Interaction logic for ProgressBarControl.xaml
    /// </summary>
    public partial class ProgressBarControl : UserControl
    {
        private static Color brightGreen = Color.FromArgb(0xFF, 0x01, 0xD3, 0x28);

        private bool animateColor = false;

        public ProgressBarControl()
        {
            InitializeComponent();

            // Animate from Blue to bright green by default
            StartColor = Colors.Blue;
            EndColor = brightGreen;
        }

        public Color StartColor { get; set; }
        public Color EndColor { get; set; }

        public bool AnimateColor 
        { 
            get
            {
                return animateColor;
            }
            set
            {
                animateColor = value;

                // Create or remove a drop shadow effect
                if (animateColor && progressBar.Effect == null)
                    progressBar.Effect = new DropShadowEffect();
                else if (!animateColor && progressBar.Effect != null)
                    progressBar.Effect = null;
            }
        }

        // Get/set the progress bar value
        public double Value
        {
            get
            {
                return progressBar.Value;
            }
            set
            {
                if (AnimateColor)
                {
                    progressBar.Foreground = new SolidColorBrush(Lerp(StartColor, EndColor, value));
                }
                else
                {
                    progressBar.Foreground = new SolidColorBrush(brightGreen);
                }

                progressBar.Value = value;
            }
        }

        // Get/set the progress bar text
        public string Text
        {
            get
            {
                return barText.Text;
            }
            set
            {
                barText.Text = value;
            }
        }

        // Calculate a linear interpolation between two colors
        private Color Lerp(Color first, Color second, double percentage)
        {
            byte a = (byte)((second.A - first.A) * percentage + first.A);
            byte r = (byte)((second.R - first.R) * percentage + first.R);
            byte g = (byte)((second.G - first.G) * percentage + first.G);
            byte b = (byte)((second.B - first.B) * percentage + first.B);

            return Color.FromArgb(a, r, g, b);
        }

        private void ProgressToolWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AdjustSize();
        }

        private void ProgressToolWindow_Loaded(object sender, RoutedEventArgs e)
        {
            AdjustSize();
        }

        // Adjust width and height to match tool window
        private void AdjustSize()
        {
            //rootViewbox.Width = this.ActualWidth;
            //rootViewbox.Height = this.ActualHeight;
            //progressBar.Width = this.ActualWidth - 24;
            //progressBar.Height = Math.Max(10, Math.Min(48, this.ActualHeight - 24));
            //viewbox.Width = progressBar.Width;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var solutionPath = ((Package.GetGlobalService(typeof(EnvDTE.DTE))) as EnvDTE.DTE).Solution.FullName;
            System.Threading.ThreadPool.QueueUserWorkItem(delegate
            {
                Engine.Instance = new Engine(solutionPath);
                Engine.Instance.DisplayFileSystemWatcherInfo(AddListLine);

                var serializer = new XmlSerializer(typeof(CoverageSession), new[] { typeof(Module), typeof(OpenCover.Framework.Model.File), typeof(Class) });
                var res = System.IO.File.ReadAllText(Engine.Instance.CoverageResults);
                CoverageSession coverageSession = null;
                StringReader reader = new StringReader(res);
                XmlTextReader xmlReader = new XmlTextReader(reader);
                try
                {
                    coverageSession = serializer.Deserialize(xmlReader) as CoverageSession;
                }
                finally
                {
                    xmlReader.Close();
                    reader.Close();
                }

                TestDetails testDetails = null;
                res = System.IO.File.ReadAllText(Engine.Instance.TestResults);
                reader = new StringReader(res);
                xmlReader = new XmlTextReader(reader);
                try
                {
                    testDetails = TestDetails.Serializer.Deserialize(xmlReader) as TestDetails;
                }
                finally
                {
                    xmlReader.Close();
                    reader.Close();
                }

                UpdateCoverageResults(coverageSession, testDetails);
            }, null);
        }

        public void AddListLine(string text)
        {
            Dispatcher.BeginInvoke(new Action(() => this.textBlock.Text += (text + "\n")));
        }

        /// <summary>
        /// Updates the coverage results for current OpenCover run on the UI thread.
        /// </summary>
        /// <param name="data">The CoverageSession data.</param>
        public void UpdateCoverageResults(CoverageSession data, TestDetails testDetails)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                CoverageData.Instance.UpdateCoverageResults(data, testDetails);
            }), null);
        }
    }

    internal class CoverageData
    {
        private static CoverageData _instance;
        public static CoverageData Instance 
        { 
            get
            {
                if (_instance == null)
                {
                    _instance = new CoverageData();
                }

                return _instance;
            } 
        }

        public CoverageSession CoverageSession { get; set; }

        public TestDetails TestDetails { get; set; }

        public void UpdateCoverageResults(CoverageSession data, TestDetails testDetails)
        {
            CoverageSession = data;
            TestDetails = testDetails;

            if (CoverageSession != null && testDetails != null)
            {
                if (NewCoverageDataAvailable != null)
                    NewCoverageDataAvailable(this, EventArgs.Empty);
            }
        }

        public event EventHandler NewCoverageDataAvailable;
    }
}