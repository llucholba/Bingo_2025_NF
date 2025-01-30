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
            Random random = new Random();
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

        private void IniciarServidor()
        {
            servidor = new ServidorBingo();
            servidor.IniciarServidor(12345, txtNombreJugador.Text);
            lblEstadoConexion.Text = "Servidor iniciado...";
            esServidor = true;
        }
        private void ConectarComoCliente()
        {
            cliente = new ClienteBingo();
            cliente.Conectar(txtIPServidor.Text, 12345, txtNombreJugador.Text, RecibirMensaje);
            lblEstadoConexion.Text = "Conectado al servidor...";

            btnNuevoJuego.Visible = false;
            btnConectar.Visible = false;
        }
        private void RecibirMensaje(string mensaje)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => RecibirMensaje(mensaje)));
                return;
            }

            if (mensaje.StartsWith("JUGADORES:"))
            {
                string[] jugadores = mensaje.Substring(10).Split(',');
                lstJugadores.Items.Clear();
                foreach (var jugador in jugadores)
                {
                    lstJugadores.Items.Add(new ListViewItem(jugador));
                }
            }
            else
            {
                txtNuevaBola.Text = mensaje;
                actualizarBola(int.Parse(mensaje));
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
    }
}
