using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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

namespace TicTacToe_Client_
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private static readonly Socket ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private const int port = 52841;
        public MainWindow()
        {
            InitializeComponent();
        }


        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            ConnectToServer();
            RequestLoop();
        }

        private void RequestLoop()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    ReceiveResponse();
                }
            });
        }

        private void ReceiveResponse()
        {
            var buffer = new byte[2048];
            int received = ClientSocket.Receive(buffer, SocketFlags.None);
            if (received == 0) return;
            var data = new byte[received];
            Array.Copy(buffer, data, received);
            string text = Encoding.ASCII.GetString(data);

            if (text == "YourTurn")
            {
                isMyTurn = true;
            }
            else
            {
                isMyTurn = false;
            }

            IntegrateToView(text);
        }

        private void IntegrateToView(string text)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var data = text.Split('\n');
                var row1 = data[0].Split('\t');
                var row2 = data[1].Split('\t');
                var row3 = data[2].Split('\t');

                b1.Content = row1[0];
                b2.Content = row1[1];
                b3.Content = row1[2];

                b4.Content = row2[0];
                b5.Content = row2[1];
                b6.Content = row2[2];

                b7.Content = row3[0];
                b8.Content = row3[1];
                b9.Content = row3[2];
            });
        }

        private void ConnectToServer()
        {
            while (!ClientSocket.Connected)
            {
                try
                {
                    ClientSocket.Connect(IPAddress.Parse("192.168.56.1"), port);
                }
                catch
                {

                }
            }

            //MessageBox.Show("Connected to game");

            var buffer = new byte[1024];
            int received = ClientSocket.Receive(buffer, SocketFlags.None);
            if (received == 0) return;

            var data = new byte[received];
            Array.Copy(buffer, data, received);

            string text = Encoding.ASCII.GetString(data);
            this.Title = "Player : " + text;
            this.player.Text = this.Title;
        }

        private bool isMyTurn = false;


        private void b1_Click(object sender, RoutedEventArgs e)
        {
            if (isMyTurn)
            {
                Task.Run(() =>
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        var bt = sender as Button;
                        string request = bt.Content.ToString() + player.Text.Split(' ')[2];
                        SendRequest(request);
                    });
                });
            }

        }

        private void SendRequest(string request)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(request);
            ClientSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
        }
    }
}
