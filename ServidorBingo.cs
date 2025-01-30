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
        private Dictionary<TcpClient, string> clientes = new Dictionary<TcpClient, string>();
        private readonly TcpClient servidorCliente = new TcpClient(); // Instancia especial para el servidor

        public void IniciarServidor(int puerto, string nombreHost)
        {
            servidor = new TcpListener(IPAddress.Any, puerto);
            servidor.Start();
            Console.WriteLine("Servidor iniciado en el puerto " + puerto);

            // Limpiar lista y agregar el host a la lista de jugadores
            clientes.Clear();
            clientes.Add(servidorCliente, nombreHost);  // servidorCliente representa al propio servidor

            Thread hiloEscucha = new Thread(EscucharClientes);
            hiloEscucha.Start();

            // Enviar la lista de jugadores a todos (aunque aún no haya clientes)
            EnviarListaJugadores();
        }

        private void EscucharClientes()
        {
            while (true)
            {
                TcpClient cliente = servidor.AcceptTcpClient();
                Thread hiloCliente = new Thread(() => ManejarCliente(cliente));
                hiloCliente.Start();
            }
        }

        private void ManejarCliente(TcpClient cliente)
        {
            NetworkStream stream = cliente.GetStream();
            byte[] buffer = new byte[256];
            int bytesLeidos = stream.Read(buffer, 0, buffer.Length);
            string nombreCliente = Encoding.UTF8.GetString(buffer, 0, bytesLeidos).Trim();

            if (!clientes.ContainsValue(nombreCliente))
            {
                clientes.Add(cliente, nombreCliente);
                Console.WriteLine("Nuevo jugador conectado: " + nombreCliente);
                EnviarListaJugadores();
            }

            while ((bytesLeidos = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                string mensaje = Encoding.UTF8.GetString(buffer, 0, bytesLeidos);
                EnviarATodos(mensaje);
            }

            clientes.Remove(cliente);
            cliente.Close();
            EnviarListaJugadores();
        }

        private void EnviarListaJugadores()
        {
            string listaJugadores = "JUGADORES:" + string.Join(",", clientes.Values);
            EnviarATodos(listaJugadores);
        }

        public void EnviarATodos(string mensaje)
        {
            byte[] data = Encoding.UTF8.GetBytes(mensaje);
            foreach (TcpClient cliente in clientes.Keys)
            {
                if (cliente.Connected)
                {
                    NetworkStream stream = cliente.GetStream();
                    stream.Write(data, 0, data.Length);
                }
                else
                {
                    // Handle the disconnected client case
                    Console.WriteLine("Cliente desconectado.");
                }
            }
        }
    }
}
