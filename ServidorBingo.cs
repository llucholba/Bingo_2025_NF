using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Bingo_2025_NF
{
    public class ServidorBingo
    {
        private TcpListener servidor;
        private List<TcpClient> clientes = new List<TcpClient>();

        public void IniciarServidor(int puerto)
        {
            servidor = new TcpListener(IPAddress.Any, puerto);
            servidor.Start();
            Console.WriteLine("Servidor iniciado en el puerto " + puerto);
            Thread hiloEscucha = new Thread(EscucharClientes);
            hiloEscucha.Start();
        }

        private void EscucharClientes()
        {
            while (true)
            {
                TcpClient cliente = servidor.AcceptTcpClient();
                clientes.Add(cliente);
                Console.WriteLine("Nuevo cliente conectado.");
                Thread hiloCliente = new Thread(() => ManejarCliente(cliente));
                hiloCliente.Start();
            }
        }

        private void ManejarCliente(TcpClient cliente)
        {
            NetworkStream stream = cliente.GetStream();
            byte[] buffer = new byte[256];
            int bytesLeidos;

            while ((bytesLeidos = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                string mensaje = Encoding.UTF8.GetString(buffer, 0, bytesLeidos);
                Console.WriteLine("Mensaje recibido: " + mensaje);

                // Enviar mensaje a todos los clientes
                EnviarATodos(mensaje);
            }

            clientes.Remove(cliente);
            cliente.Close();
        }

        public void EnviarATodos(string mensaje)
        {
            byte[] data = Encoding.UTF8.GetBytes(mensaje);
            foreach (TcpClient cliente in clientes)
            {
                NetworkStream stream = cliente.GetStream();
                stream.Write(data, 0, data.Length);
            }
        }
    }
}
