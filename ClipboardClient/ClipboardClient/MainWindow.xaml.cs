using ClipboardSyncAgent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ClipboardClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static String PreviousClipboardContent = "";

        static ClipBoardClient ClipboardSyncAgent = new ClipBoardClient();

        Brush RedColor = Brushes.Red;
        Brush GreenColor = Brushes.Green;
        Brush YellowColor;

        static bool ContainsAudio
        {
            get
            {
                return Clipboard.ContainsAudio();
            }
        }
        static bool ContainsText {
            get {
                return Clipboard.ContainsText();
            }
        }
        static bool ContainsImage
        {
            get
            {
                return Clipboard.ContainsImage();
            }
        }
        static bool ContainsFileList
        {
            get
            {
                return Clipboard.ContainsFileDropList();
            }
        }

        static DispatcherTimer ClipboardTicker;

        public MainWindow()
        {
            InitializeComponent();

            YellowColor = new BrushConverter().ConvertFromString("#FFF0B234") as Brush;


            ClipboardSyncAgent.OnClipBoardBroadcastRecieved += OnClipBoardBroadcastRecieved;
            ClipboardSyncAgent.OnConnectionStatusChanged += OnConnectionStatusChanged;

            ClipboardSyncAgent.ConnectToServer();

            

            ClipboardTicker = new DispatcherTimer(DispatcherPriority.Normal);
            ClipboardTicker.Interval = TimeSpan.FromMilliseconds(500);
            ClipboardTicker.Tick += ClipboardCheckTick;
            ClipboardTicker.Start();
        }

        private void OnConnectionStatusChanged(object source, Microsoft.AspNet.SignalR.Client.ConnectionState e)
        {
            if (e == Microsoft.AspNet.SignalR.Client.ConnectionState.Connected)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    StatusIndicator.Fill = GreenColor;
                }), DispatcherPriority.Normal);
            }
            else if (e == Microsoft.AspNet.SignalR.Client.ConnectionState.Disconnected)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    StatusIndicator.Fill = RedColor;
                }), DispatcherPriority.Normal);

            }
            else if (e == Microsoft.AspNet.SignalR.Client.ConnectionState.Reconnecting || e == Microsoft.AspNet.SignalR.Client.ConnectionState.Connecting)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    StatusIndicator.Fill = YellowColor;
                }), DispatcherPriority.Normal);
            }
        }

        private void OnClipBoardBroadcastRecieved(object source, ClipboardBroadcastRecievedArgs e)
        {
            SetClipboardText(e.NewClipboardText);
        }

        private void ClipboardCheckTick(object sender, EventArgs e)
        {
            GetClipboardContent();
        }

        private void ClearClipboard()
        {
            ContentBlock.Text = String.Empty;
            Clipboard.Clear();
        }

        private void GetClipboardContent()
        {
            var clipData = Clipboard.GetDataObject();

            if (clipData != null)
            {
                if (ContainsText)
                {

                    var canGetData = clipData.GetDataPresent("Text");
                    var isString = canGetData ? clipData.GetData("Text") is string : false;
                    var outString = isString ? clipData.GetData("Text").ToString() : "";

                    if (PreviousClipboardContent != outString)
                    {
                        ClipboardSyncAgent.SendClipBoardToServer(outString);
                    }

                    PreviousClipboardContent = outString;

                    ContentBlock.Text = $"Clipboard text = {outString}";
                }
            }
            else
            {
                if (PreviousClipboardContent != String.Empty)
                {
                    ClipboardSyncAgent.SendClipBoardToServer(String.Empty);
                }

                PreviousClipboardContent = String.Empty;
                ContentBlock.Text = $"Clipboard text is blank";
            }
        }

        private void SetClipboardText(string text)
        {
            ClearClipboard();
            Clipboard.SetText(text);
        }

        private void displayBtn_Click(object sender, RoutedEventArgs e)
        {
            GetClipboardContent();
        }

        private void clearBtn_Click(object sender, RoutedEventArgs e)
        {
            ClearClipboard();
        }

        private void setBtn_Click(object sender, RoutedEventArgs e)
        {
            SetClipboardText("Hello World");
            GetClipboardContent();
        }

        private void StatusIndicator_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.HeightChanged)
            {
                (sender as Ellipse).Width = e.NewSize.Height;
            }
        }
    }
}
