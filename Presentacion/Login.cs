using Business;
using Clases;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Presentacion
{
    public partial class Login : Form
    {
        private TextBox txtUsuario;
        private TextBox txtPassword;
        private Button btnLogin;
        private Panel panelLateral;
        private Label lblTitulo;
        private Label lblSubtitulo;
        private PictureBox picCruzRoja;
        private readonly UsuarioBusiness _usuarioBusiness = new UsuarioBusiness();

        /// <summary>Usuario autenticado cuando el diálogo se cierra con DialogResult.OK.</summary>
        public Usuario? UsuarioAutenticado { get; private set; }

        public Login()
        {
            InitializeComponent();
            ConfigurarDiseñoPersonalizado();
        }

        private void Login_Load(object sender, EventArgs e)
        {

        }

        private void ConfigurarDiseñoPersonalizado()
        {
            // Configuración general de la ventana
            this.Text = "Ministerio de Salud - Iniciar Sesión";
            this.Size = new Size(850, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.White;

            // Panel lateral izquierdo (Verde Institucional)
            panelLateral = new Panel();
            panelLateral.Dock = DockStyle.Left;
            panelLateral.Width = 350;
            panelLateral.BackColor = Color.FromArgb(20, 90, 50); // Verde oscuro institucional
            this.Controls.Add(panelLateral);

            // Cruz médica roja (Dibujada o simulada en panel/picturebox)
            Panel cruzPanel = new Panel();
            cruzPanel.Size = new Size(60, 60);
            cruzPanel.Location = new Point(40, 50);
            cruzPanel.BackColor = Color.Red; // Cruz médica roja
            // Para darle forma de cruz perfecta se pueden usar dos sub-panels o un gráfico, 
            // por simplicidad colocamos un contenedor rojo distintivo:
            panelLateral.Controls.Add(cruzPanel);

            lblTitulo = new Label();
            lblTitulo.Text = "Ministerio de Salud";
            lblTitulo.ForeColor = Color.White;
            lblTitulo.Font = new Font("Segoe UI", 20, FontStyle.Bold);
            lblTitulo.Location = new Point(35, 130);
            lblTitulo.AutoSize = true;
            panelLateral.Controls.Add(lblTitulo);

            lblSubtitulo = new Label();
            lblSubtitulo.Text = "Red nacional de establecimientos de salud.\nAcceso seguro por provincia.";
            lblSubtitulo.ForeColor = Color.LightGray;
            lblSubtitulo.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            lblSubtitulo.Location = new Point(40, 180);
            lblSubtitulo.Size = new Size(270, 60);
            panelLateral.Controls.Add(lblSubtitulo);

            // Controles de Login (Lado derecho - Blanco)
            Label lblLoginTitle = new Label();
            lblLoginTitle.Text = "Iniciar sesión";
            lblLoginTitle.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            lblLoginTitle.ForeColor = Color.FromArgb(20, 90, 50);
            lblLoginTitle.Location = new Point(420, 70);
            lblLoginTitle.AutoSize = true;
            this.Controls.Add(lblLoginTitle);

            Label lblUserLabel = new Label();
            lblUserLabel.Text = "Usuario o correo institucional";
            lblUserLabel.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            lblUserLabel.Location = new Point(420, 140);
            lblUserLabel.AutoSize = true;
            this.Controls.Add(lblUserLabel);

            txtUsuario = new TextBox();
            txtUsuario.Location = new Point(420, 170);
            txtUsuario.Size = new Size(350, 30);
            txtUsuario.Font = new Font("Segoe UI", 11);
            this.Controls.Add(txtUsuario);

            Label lblPassLabel = new Label();
            lblPassLabel.Text = "Contraseña";
            lblPassLabel.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            lblPassLabel.Location = new Point(420, 230);
            lblPassLabel.AutoSize = true;
            this.Controls.Add(lblPassLabel);

            txtPassword = new TextBox();
            txtPassword.Location = new Point(420, 260);
            txtPassword.Size = new Size(350, 30);
            txtPassword.Font = new Font("Segoe UI", 11);
            txtPassword.PasswordChar = '●';
            this.Controls.Add(txtPassword);

            btnLogin = new Button();
            btnLogin.Text = "Iniciar Sesión";
            btnLogin.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnLogin.BackColor = Color.FromArgb(39, 174, 96); // Verde brillante
            btnLogin.ForeColor = Color.White;
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Location = new Point(420, 330);
            btnLogin.Size = new Size(350, 45);
            btnLogin.Cursor = Cursors.Hand;
            btnLogin.Click += BtnLogin_Click;
            this.Controls.Add(btnLogin);

            this.AcceptButton = btnLogin;      // Enter inicia sesión
            this.ActiveControl = txtUsuario;
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            string identificador = txtUsuario.Text.Trim();
            string passwordPlana = txtPassword.Text; // sin Trim: la contraseña se compara tal cual fue creada

            if (string.IsNullOrEmpty(identificador) || string.IsNullOrEmpty(passwordPlana))
            {
                MessageBox.Show("Por favor, complete todos los campos.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                btnLogin.Enabled = false;
                this.Cursor = Cursors.WaitCursor;

                // Valida correo/nombre Y contraseña (hash SHA-256) contra la base de datos
                Usuario? usuario = _usuarioBusiness.Autenticar(identificador, passwordPlana);

                if (usuario == null)
                {
                    MessageBox.Show("Credenciales incorrectas. Verifique sus datos.", "Error de Autenticación", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtPassword.Clear();
                    txtPassword.Focus();
                    return;
                }

                UsuarioAutenticado = usuario;
                this.DialogResult = DialogResult.OK; // cierra el login; Program abre el Main
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al conectar con el servidor: " + ex.Message, "Error crítico", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnLogin.Enabled = true;
                this.Cursor = Cursors.Default;
            }
        }

    }
}
