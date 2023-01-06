using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace ServidorPantalla
{
    public class Servidor : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        List<TcpClient> clientes = new List<TcpClient>();

        List<string> encuestas = new List<string>();
        TcpListener listener;

        Dispatcher dispatcher;

        public ICommand IniciarCommand { get; set; }
        public ICommand DetenerCommand { get; set; }

        private string conexion;
        public string Conexion
        {
            get { return conexion; }
            set
            {
                conexion = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Conexion"));
            }
        }
        public void IniciarServidor()
        {
            if (listener == null)
            {
                Task.Run(() =>
                {
                    try
                    {
                        IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, 5550);
                        listener = new TcpListener(endPoint);
                        listener.Start();

                        while (listener != null)
                        {
                            TcpClient tcp = listener.AcceptTcpClient();
                            clientes.Add(tcp);
                            Recibir(tcp);
                        }
                    }
                    catch (Exception) { }
                });
                conexion = "Servidor conectado";
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
            }
        }

        public void Recibir(TcpClient cliente)
        {
            Task.Run(() =>
            {
                NetworkStream ns = cliente.GetStream();

                while (cliente.Connected)
                {

                    if (cliente.Available > 0)
                    {
                        byte[] buffer = new byte[cliente.Available];
                        ns.Read(buffer, 0, buffer.Length);

                        string mensaje = Encoding.UTF8.GetString(buffer);

                        dispatcher.Invoke(() =>
                        {
                            encuestas.Add(mensaje);

                        });
                    }
                    Task.Delay(50);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
                }
            });
        }
        public void DetenerServidor()
        {
            if (listener != null)
            {
                listener.Stop();
                listener = null;

                foreach (var cliente in clientes)
                {
                    cliente.Close();
                }
                clientes.Clear();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
                conexion = "Servidor desconectado";
            }
        }
        public Servidor()
        {
            dispatcher = Dispatcher.CurrentDispatcher;
            IniciarCommand = new RelayCommand(IniciarServidor);
            DetenerCommand = new RelayCommand(DetenerServidor);
        }
    }
}

