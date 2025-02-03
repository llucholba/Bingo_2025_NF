using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bingo_2025_NF
{
    public partial class frmBingo : Form
    {
        public frmBingo()
        {
            InitializeComponent();
        }

        private TextBox[] txtBolillas;
        public List<int> bolillas = new List<int>();
        Random bolAleatoria = new Random();
        public int contadorBolillas = 1;

        private ServidorBingo servidor;
        private ClienteBingo cliente;
        private bool esServidor;

        private void frmBingo_Load(object sender, EventArgs e)
        {
            txtBolillas = new TextBox[]
            {
                txtBola1, txtBola2, txtBola3, txtBola4, txtBola5, txtBola6, txtBola7, txtBola8, txtBola9, txtBola10,
                txtBola11, txtBola12, txtBola13, txtBola14, txtBola15, txtBola16, txtBola17, txtBola18, txtBola19, txtBola20,
                txtBola21, txtBola22, txtBola23, txtBola24, txtBola25, txtBola26, txtBola27, txtBola28, txtBola29, txtBola30,
                txtBola31, txtBola32, txtBola33, txtBola34, txtBola35, txtBola36, txtBola37, txtBola38, txtBola39, txtBola40,
                txtBola41, txtBola42, txtBola43, txtBola44, txtBola45, txtBola46, txtBola47, txtBola48, txtBola49, txtBola50,
                txtBola51, txtBola52, txtBola53, txtBola54, txtBola55, txtBola56, txtBola57, txtBola58, txtBola59, txtBola60,
                txtBola61, txtBola62, txtBola63, txtBola64, txtBola65, txtBola66, txtBola67, txtBola68, txtBola69, txtBola70,
                txtBola71, txtBola72, txtBola73, txtBola74, txtBola75, txtBola76, txtBola77, txtBola78, txtBola79, txtBola80,
                txtBola81, txtBola82, txtBola83, txtBola84, txtBola85, txtBola86, txtBola87, txtBola88, txtBola89, txtBola90
            };

            for (int i = 1; i <= 90; i++)
            {
                bolillas.Add(i);
            }

            barajarArrayBolillas();

            txtNuevaBola.Text = "";
        }
        private void frmBingo_Shown(object sender, EventArgs e)
        {
            btnNuevoJuego.Focus();
        }

        private void btnNuevoJuego_Click(object sender, EventArgs e)
        {
            DialogResult nuevoBingo = MessageBox.Show("¿Iniciar juego nuevo?", "Juego Nuevo", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (nuevoBingo == DialogResult.Yes)
            {
                btnNuevaBola.Visible = true;

                resetearColoresTxtBolas();

                txtNuevaBola.Text = "";
                picBola.Image = null;
                txtContador.Text = "";
                contadorBolillas = 1;

                // Re-iniciamos el juego y barajamos las bolillas
                bolillas.Clear();
                for (int i = 1; i <= 90; i++)
                {
                    bolillas.Add(i);
                }
                barajarArrayBolillas();

                GenerarCarton();

                // Aquí debemos enviar un mensaje para que los clientes generen su cartón
                if (esServidor)
                {
                    // El mensaje "INICIAR_CARTON" indicará a los clientes que deben generar su cartón
                    servidor.EnviarATodos("INICIAR_CARTON");
                }

                picEvento.Image = null;
            }
        }
        private void btnNuevaBola_Click(object sender, EventArgs e)
        {
            if (bolillas.Count > 0)
            {
                int index = bolAleatoria.Next(bolillas.Count);
                int numeroAzar = bolillas[index];
                txtNuevaBola.Text = numeroAzar.ToString();

                // Actualizamos la bola con el número seleccionado
                actualizarBola(numeroAzar);

                // Al mostrar la bola, marcar el cartón si el número coincide
                if (cartonesConBolas.ContainsKey(numeroAzar))
                {
                    // Obtener el TextBox correspondiente a ese número y cambiar el color
                    TextBox txtBolaMarcada = cartonesConBolas[numeroAzar];
                    txtBolaMarcada.BackColor = Color.Yellow; // O cualquier color que quieras usar
                    txtBolaMarcada.ForeColor = Color.Red; // Cambiar color de texto para destacarlo
                }

                // Eliminamos el número seleccionado para evitar que se repita
                bolillas.RemoveAt(index);

                txtContador.Text = contadorBolillas.ToString();
                contadorBolillas++;

                if (esServidor)
                {
                    servidor.EnviarATodos(numeroAzar.ToString());
                }

                // Verificar si el cartón está completo
                if (cartonCompleto())
                {
                    // Obtener la ruta base del proyecto
                    string rutaProyecto = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.FullName;

                    // Construir la ruta de la imagen "cartel_bingo.png"
                    string rutaImagen = Path.Combine(rutaProyecto, "Resources", "Imagenes", "Carteles", "cartel_bingo.png");

                    // Verificar si la imagen existe antes de cargarla
                    if (File.Exists(rutaImagen))
                    {
                        picEvento.Image = Image.FromFile(rutaImagen);
                    }
                    else
                    {
                        picEvento.Image = null; // En caso de que no se encuentre la imagen
                    }
                }

                if (bolillas.Count == 0)
                {
                    picEvento.Image = null;

                    DialogResult finBingo = MessageBox.Show("¿Deseas Jugar otra vez?", "Juego Finalizado", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (finBingo == DialogResult.Yes)
                    {
                        // Reiniciamos la lista de bolillas
                        bolillas.Clear();
                        for (int i = 1; i <= 90; i++)
                        {
                            bolillas.Add(i);
                        }

                        barajarArrayBolillas();

                        resetearColoresTxtBolas();

                        txtNuevaBola.Text = "";
                        picBola.Image = null;

                        txtContador.Text = "";
                        contadorBolillas = 1;

                        GenerarCarton();
                    }
                    else
                    {
                        btnNuevaBola.Visible = false;
                    }
                }
            }
        }

        private void barajarArrayBolillas()
        {
            for (int i = 0; i < bolillas.Count; i++)
            {
                int j = bolAleatoria.Next(i, bolillas.Count);
                int temp = bolillas[i];
                bolillas[i] = bolillas[j];
                bolillas[j] = temp;
            }
        }
        private void resetearColoresTxtBolas()
        {
            foreach (var txtBola in txtBolillas)
            {
                txtBola.ForeColor = Color.DimGray;
            }
        }
        private void actualizarBola(int numeroAzar)
        {
            // Obtener la ruta raíz del proyecto
            string rutaProyecto = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.FullName;

            // Construir la ruta de la imagen
            string rutaImagen = Path.Combine(rutaProyecto, "Resources", "Imagenes", "Bolas", $"bola_{numeroAzar:D2}.png");

            // Verificar si la imagen existe antes de cargarla
            if (File.Exists(rutaImagen))
            {
                picBola.Image = Image.FromFile(rutaImagen);
            }
            else
            {
                picBola.Image = null; // En caso de que no se encuentre la imagen
            }

            // Actualizamos el color del TextBox correspondiente
            txtBolillas[numeroAzar - 1].ForeColor = Color.Yellow; // -1 porque el índice del array comienza en 0
        }

        private Dictionary<int, TextBox> cartonesConBolas = new Dictionary<int, TextBox>();
        private void GenerarCarton()
        {
            // Limpiar el TableLayoutPanel antes de agregar los nuevos controles
            tablePanelCarton.Controls.Clear();
            tablePanelCarton.BackColor = Color.LightGray;
            cartonesConBolas.Clear(); // Limpiar el diccionario de referencias

            // Lista de 15 números aleatorios únicos para el cartón
            List<int> numerosCarton = new List<int>();

            // Generar 15 números aleatorios entre 1 y 90 sin repetirse
            Random random = new Random(Guid.NewGuid().GetHashCode());
            while (numerosCarton.Count < 15)
            {
                int numeroAleatorio = random.Next(1, 91); // Generamos un número entre 1 y 90
                if (!numerosCarton.Contains(numeroAleatorio))
                {
                    numerosCarton.Add(numeroAleatorio);
                }
            }

            // Barajamos los números generados para que se distribuyan aleatoriamente
            numerosCarton = numerosCarton.OrderBy(x => random.Next()).ToList();

            // Llenar el TableLayoutPanel con los números
            int contador = 0;
            for (int fila = 0; fila < 3; fila++) // 3 filas
            {
                for (int columna = 0; columna < 5; columna++) // 5 columnas
                {
                    // Crear un TextBox para cada celda
                    TextBox txtBola = new TextBox();
                    txtBola.ReadOnly = true;
                    txtBola.TextAlign = HorizontalAlignment.Center;
                    txtBola.Font = new Font("Segoe UI", 16, FontStyle.Bold);
                    txtBola.BackColor = Color.White;

                    // Asignamos un número aleatorio a la celda
                    int numero = numerosCarton[contador];
                    txtBola.Text = numero.ToString();
                    contador++;

                    // Añadimos el TextBox a la celda correspondiente en el TableLayoutPanel
                    tablePanelCarton.Controls.Add(txtBola, columna, fila);

                    // Guardamos el número y su correspondiente TextBox en el diccionario
                    cartonesConBolas.Add(numero, txtBola);
                }
            }
        }

        private bool cartonCompleto()
        {
            // Verificar si todos los TextBox del cartón tienen el fondo amarillo
            foreach (var txtBola in cartonesConBolas.Values)
            {
                if (txtBola.BackColor != Color.Yellow) // Si algún TextBox no está marcado, el cartón no está completo
                {
                    return false;
                }
            }

            return true; // Si todos los TextBox están marcados, el cartón está completo
        }

        /*Para enviar el cartón del cliente a otros jugadores y así todos puedan ver el cartón de cada jugador
        //private void ProcesarCarton(string mensaje)
        //{
        //    // Quitamos "CARTON:" y separamos por '|' para obtener el nombre y los números.
        //    string datos = mensaje.Substring(7); // Remueve "CARTON:"
        //    string[] partes = datos.Split('|');
        //    if (partes.Length != 2) return; // Error en el formato
        //    string jugador = partes[0];
        //    List<int> numerosCarton = partes[1].Split(',').Select(int.Parse).ToList();

        //    // Actualizamos o agregamos el cartón del jugador
        //    cartonesJugadores[jugador] = numerosCarton;

        //    // Actualizamos la interfaz
        //    ActualizarUICartones();
        //}
        //private void ActualizarUICartones()
        //{
        //    flpCartones.Controls.Clear();
        //    foreach (var kv in cartonesJugadores)
        //    {
        //        string jugador = kv.Key;
        //        List<int> numeros = kv.Value;

        //        // Creamos un panel para este cartón
        //        Panel pnlCarton = new Panel();
        //        pnlCarton.BorderStyle = BorderStyle.FixedSingle;
        //        pnlCarton.Width = 220; // Ajusta según sea necesario
        //        pnlCarton.Height = 120; // Ajusta según sea necesario

        //        // Agregamos una etiqueta con el nombre del jugador
        //        Label lblJugador = new Label();
        //        lblJugador.Text = jugador;
        //        lblJugador.Dock = DockStyle.Top;
        //        lblJugador.TextAlign = ContentAlignment.MiddleCenter;
        //        pnlCarton.Controls.Add(lblJugador);

        //        // Creamos un TableLayoutPanel para el cartón
        //        TableLayoutPanel tlp = new TableLayoutPanel();
        //        tlp.RowCount = 3;
        //        tlp.ColumnCount = 5;
        //        tlp.Dock = DockStyle.Fill;
        //        tlp.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;

        //        int index = 0;
        //        for (int i = 0; i < 3; i++)
        //        {
        //            for (int j = 0; j < 5; j++)
        //            {
        //                Label lblNum = new Label();
        //                lblNum.Text = numeros[index].ToString();
        //                lblNum.Dock = DockStyle.Fill;
        //                lblNum.TextAlign = ContentAlignment.MiddleCenter;
        //                // Opcional: configurar color y fuente para resaltar los números marcados, etc.
        //                tlp.Controls.Add(lblNum, j, i);
        //                index++;
        //            }
        //        }

        //        pnlCarton.Controls.Add(tlp);
        //        flpCartones.Controls.Add(pnlCarton);
        //    }
        //}
        */

        private void IniciarServidor()
        {
            servidor = new ServidorBingo();
            servidor.IniciarServidor(12345, txtNombreJugador.Text);

            esServidor = true;

            // Conectar el host como cliente también
            ConectarComoCliente();
        }
        private void ConectarComoCliente()
        {
            cliente = new ClienteBingo();
            cliente.Conectar(txtIPServidor.Text, 12345, txtNombreJugador.Text, RecibirMensaje);
            lblEstadoConexion.Text = "Conectado al servidor...";

            btnConectar.Visible = false;

            if (!esServidor)
            {
                btnNuevoJuego.Visible = false;
                btnDesconectar.Visible = true;
            }
            else
            {
                lblEstadoConexion.Text = "Servidor iniciado...";
                btnCerrarServidor.Visible = true;
            }
        }

        //private Dictionary<string, List<int>> cartonesJugadores = new Dictionary<string, List<int>>();
        private void RecibirMensaje(string mensaje)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => RecibirMensaje(mensaje)));
                return;
            }
            //Listar los jugadores de la partida
            if (mensaje.StartsWith("JUGADORES:"))
            {
                string[] jugadores = mensaje.Substring(10).Split(',');
                lstJugadores.Items.Clear();
                foreach (var jugador in jugadores)
                {
                    lstJugadores.Items.Add(new ListViewItem(jugador));
                }
            }
            //Generar cartón en cada jugador conectado (cliente)
            else if (mensaje == "INICIAR_CARTON")
            {
                // Se recibe el mensaje de inicio de cartón: se genera el cartón local
                GenerarCarton();

                // Opcional: Luego de generar el cartón, el cliente puede enviar su cartón al servidor para sincronizarlo
                // Por ejemplo, suponiendo que GenerarCarton() genera el cartón y almacena la lista de números en una variable 'numerosCarton'
                // cliente.EnviarCarton(numerosCarton);
            }

            /*Recibir cartón de otro jugador para que así, todos los jugadores puedan ver todos los cartones
            //else if (mensaje.StartsWith("CARTON:"))
            //{
            //    // Procesamos el mensaje de cartón.
            //    // Formato esperado: CARTON:Jugador1|5,12,19,...,87
            //    ProcesarCarton(mensaje);
            //}
            */
            
            //Nueva Bola
            else
            {
                //MessageBox.Show("Número recibido en el cliente: " + mensaje);
                txtNuevaBola.Text = mensaje;
                actualizarBola(int.Parse(mensaje));
                //int numero;
                //if (int.TryParse(mensaje, out numero))
                //{
                //    actualizarBola(numero);
                //}
                //else
                //{
                //    Console.WriteLine("Error al convertir el mensaje a número.");
                //}
            }
        }

        private void btnConectar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombreJugador.Text) || string.IsNullOrWhiteSpace(txtIPServidor.Text))
            {
                MessageBox.Show("Debe ingresar un nombre y un IP", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (txtIPServidor.Text == "localhost")
            {
                IniciarServidor();
            }
            else
            {
                ConectarComoCliente();
            }
        }

        private void btnDesconectar_Click(object sender, EventArgs e)
        {
            if (cliente != null)
            {
                DialogResult cerrarServidor = MessageBox.Show("¿Desconectar este cliente?", "Desconectar cliente", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (cerrarServidor == DialogResult.Yes)
                {
                    cliente.Desconectar();
                    MessageBox.Show("Te has desconectado del servidor.");
                }
            }
        }

        private void btnCerrarServidor_Click(object sender, EventArgs e)
        {
            if (servidor != null)
            {
                DialogResult cerrarServidor = MessageBox.Show("¿Deseas cerrar el servidor?", "Desconectar servidor", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (cerrarServidor == DialogResult.Yes)
                {
                    servidor.DetenerServidor();
                    MessageBox.Show("El servidor se ha cerrado.");
                }
            }
        }
    }
}
