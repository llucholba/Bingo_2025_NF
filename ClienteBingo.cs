using System;
//using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Bingo_2025_NF
{
    public class ClienteBingo
    {
        private TcpClient cliente;
        private NetworkStream stream;
        //private string nombre;
        private Action<string> onMensajeRecibido;

        public void Conectar(string ipServidor, int puerto, string nombreJugador, Action<string> callback)
        {
            cliente = new TcpClient();
            cliente.Connect(ipServidor, puerto);
            stream = cliente.GetStream();
            onMensajeRecibido = callback;
            //nombre = nombreJugador;

            //byte[] data = Encoding.UTF8.GetBytes(nombre);
            byte[] data = Encoding.UTF8.GetBytes(nombreJugador);
            stream.Write(data, 0, data.Length);

            Thread hiloEscucha = new Thread(EscucharServidor);
            hiloEscucha.Start();
        }

        public void Desconectar()
        {
            if (cliente != null && cliente.Connected)
            {
                try
                {
                    cliente.Close();
                    Console.WriteLine("Te has desconectado del servidor.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error al desconectar: " + ex.Message);
                }
            }
        }

        private void EscucharServidor()
        {
            byte[] buffer = new byte[256];
            int bytesLeidos;

            while ((bytesLeidos = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                string mensaje = Encoding.UTF8.GetString(buffer, 0, bytesLeidos);
                onMensajeRecibido?.Invoke(mensaje);
            }
        }

        public void EnviarMensaje(string mensaje)
        {
            byte[] data = Encoding.UTF8.GetBytes(mensaje);
            stream.Write(data, 0, data.Length);
        }



        /*Para enviar el cartón del cliente a otros jugadores y así todos puedan ver el cartón de cada jugador
        //public void EnviarCarton(List<int> numerosCarton)
        //{
        //    // Formateamos el mensaje incluyendo el nombre del jugador
        //    // Ejemplo: "CARTON:Jugador1|5,12,19,...,87"
        //    string mensaje = $"CARTON:{nombre}|{string.Join(",", numerosCarton)}";
        //    EnviarMensaje(mensaje);
        //}
        */
    }
}
