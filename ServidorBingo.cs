using System;
using System.Collections.Generic;
using System.Linq;
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
        private string nombreHost;

        public void IniciarServidor(int puerto, string nombreHost)
        {
            this.nombreHost = nombreHost; // Guardar el nombre del host

            servidor = new TcpListener(IPAddress.Any, puerto);
            servidor.Start();
            Console.WriteLine("Servidor iniciado en el puerto " + puerto);

            Thread hiloEscucha = new Thread(EscucharClientes);
            hiloEscucha.Start();

            // Agregamos el host manualmente a la lista
            EnviarListaJugadores();
        }
        public void DetenerServidor()
        {
            Console.WriteLine("El servidor se está cerrando...");

            // Avisar a todos los clientes que el servidor se cierra
            EnviarATodos("SERVIDOR_CERRADO");

            // Cerrar todas las conexiones activas
            foreach (TcpClient cliente in clientes.Keys.ToList())
            {
                if (cliente.Connected) {
                    cliente.Close();
                }
            }

            clientes.Clear(); // Vaciar la lista de clientes
            servidor.Stop();  // Detener el servidor
            Console.WriteLine("Servidor detenido.");
        }

        private void EscucharClientes()
        {
            while (true)
            {
                try
                {
                    TcpClient cliente = servidor.AcceptTcpClient();
                    Thread hiloCliente = new Thread(() => ManejarCliente(cliente));
                    hiloCliente.Start();
                }
                catch (SocketException ex)
                {
                    // Log or handle the exception as needed
                    Console.WriteLine($"SocketException: {ex.Message}");
                    break; // Optionally break the loop if needed
                }
            }
        }

        private void ManejarCliente(TcpClient cliente)
        {
            string nombreCliente = "";
            try
            {
                NetworkStream stream = cliente.GetStream();
                byte[] buffer = new byte[256];

                int bytesLeidos = stream.Read(buffer, 0, buffer.Length);
                nombreCliente = Encoding.UTF8.GetString(buffer, 0, bytesLeidos).Trim();

                // Si el nombre ya está en la lista, se rechaza
                if (!clientes.ContainsValue(nombreCliente))
                {
                    clientes.Add(cliente, nombreCliente);
                    Console.WriteLine($"Nuevo jugador conectado: {nombreCliente}");
                    EnviarListaJugadores();
                }

                // Escuchar mensajes del cliente
                while (true)
                {
                    bytesLeidos = stream.Read(buffer, 0, buffer.Length);
                    if (bytesLeidos == 0)
                    {
                        // Si `Read` devuelve 0, significa que el cliente se desconectó
                        throw new Exception("Cliente desconectado.");
                    }

                    string mensaje = Encoding.UTF8.GetString(buffer, 0, bytesLeidos);
                    EnviarATodos(mensaje);
                }
            }
            catch (Exception)
            {
                Console.WriteLine($"Jugador {nombreCliente} se ha desconectado.");
            }
            finally
            {
                if (cliente != null && clientes.ContainsKey(cliente))
                {
                    clientes.Remove(cliente);
                }
                cliente.Close();
                EnviarListaJugadores(); // Actualiza la lista sin el jugador desconectado
            }
        }

        private void EnviarListaJugadores()
        {
            string listaJugadores = "JUGADORES:" + string.Join(",", clientes.Values);
            EnviarATodos(listaJugadores);
        }

        public void EnviarATodos(string mensaje)
        {
            byte[] data = Encoding.UTF8.GetBytes(mensaje);
            List<TcpClient> clientesDesconectados = new List<TcpClient>();

            foreach (TcpClient cliente in clientes.Keys)
            {
                try
                {
                    if (cliente.Connected)
                    {
                        NetworkStream stream = cliente.GetStream();
                        stream.Write(data, 0, data.Length);
                    }
                    else
                    {
                        clientesDesconectados.Add(cliente);
                    }
                }
                catch (Exception)
                {
                    clientesDesconectados.Add(cliente);
                }
            }

            // Eliminar clientes desconectados de la lista
            foreach (var cliente in clientesDesconectados)
            {
                clientes.Remove(cliente);
            }
        }
    }
}
