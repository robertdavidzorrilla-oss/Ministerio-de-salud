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
    public partial class Main : Form
    {
        private readonly Usuario _usuarioLogueado;

        private Panel panelMenuLateral = null!;
        private Panel panelContenedor = null!;
        private Panel panelHeaderUsuario = null!;
        private Panel panelInferior = null!;
        private FlowLayoutPanel flowMenu = null!;
        private Label lblNombreUsuario = null!;
        private Label lblRolUsuario = null!;
        private Button btnEstablecimientos = null!;
        private Button btnUsuarios = null!;
        private Button btnReportes = null!;
        private Button btnCambiarPassword = null!;
        private Button btnCerrarSesion = null!;

        private Button? _botonActivo;

        /// <summary>True cuando el usuario eligió "Cerrar sesión" (Program vuelve a mostrar el Login).</summary>
        public bool CerrarSesionSolicitada { get; private set; }

        public Main(Usuario usuario)
        {
            _usuarioLogueado = usuario;
            InitializeComponent();
            ConfigurarInterfazPrincipal();
            AplicarPermisosPorRol();
        }

        private void ConfigurarInterfazPrincipal()
        {
            this.Text = "Ministerio de Salud - Red Nacional de Establecimientos";
            this.Size = new Size(1300, 750);
            this.MinimumSize = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Estilo.Fondo;

            // ORDEN DE DOCKING: WinForms acopla primero el control agregado al final.
            // Por eso el contenedor (Fill) se agrega PRIMERO y el menú (Left) después;
            // si fuera al revés, el contenido de los módulos quedaría tapado por el menú.
            panelContenedor = new Panel();
            panelContenedor.Dock = DockStyle.Fill;
            panelContenedor.BackColor = Estilo.Fondo;
            this.Controls.Add(panelContenedor);

            panelMenuLateral = new Panel();
            panelMenuLateral.Dock = DockStyle.Left;
            panelMenuLateral.Width = 280;
            panelMenuLateral.BackColor = Estilo.Primario;
            this.Controls.Add(panelMenuLateral);

            // ---- Zona de botones (Fill) ----
            flowMenu = new FlowLayoutPanel();
            flowMenu.Dock = DockStyle.Fill;
            flowMenu.FlowDirection = FlowDirection.TopDown;
            flowMenu.WrapContents = false;
            flowMenu.Padding = new Padding(0, 10, 0, 0);
            flowMenu.BackColor = Estilo.Primario;
            panelMenuLateral.Controls.Add(flowMenu);

            // ---- Zona inferior (Bottom): cerrar sesión ----
            panelInferior = new Panel();
            panelInferior.Dock = DockStyle.Bottom;
            panelInferior.Height = 72;
            panelInferior.Padding = new Padding(15, 10, 15, 12);
            panelInferior.BackColor = Estilo.Primario;
            panelMenuLateral.Controls.Add(panelInferior);

            btnCerrarSesion = CrearBotonMenu("🚪  Cerrar Sesión");
            btnCerrarSesion.Dock = DockStyle.Fill;
            btnCerrarSesion.Margin = new Padding(0);
            btnCerrarSesion.BackColor = Estilo.Error;
            btnCerrarSesion.FlatAppearance.MouseOverBackColor = Estilo.Oscurecer(Estilo.Error, 25);
            btnCerrarSesion.Click += BtnCerrarSesion_Click;
            panelInferior.Controls.Add(btnCerrarSesion);

            // ---- Cabecera con datos del usuario (Top) ----
            panelHeaderUsuario = new Panel();
            panelHeaderUsuario.Dock = DockStyle.Top;
            panelHeaderUsuario.Height = 120;
            panelHeaderUsuario.BackColor = Estilo.Secundario;
            panelMenuLateral.Controls.Add(panelHeaderUsuario);

            Label lblIconoUsuario = new Label();
            lblIconoUsuario.Text = "👤";
            lblIconoUsuario.Font = new Font("Segoe UI", 28, FontStyle.Regular);
            lblIconoUsuario.ForeColor = Color.White;
            lblIconoUsuario.Location = new Point(10, 25);
            lblIconoUsuario.Size = new Size(60, 60);
            lblIconoUsuario.TextAlign = ContentAlignment.MiddleCenter;
            panelHeaderUsuario.Controls.Add(lblIconoUsuario);

            lblNombreUsuario = new Label();
            lblNombreUsuario.Text = _usuarioLogueado.Nombre;
            lblNombreUsuario.ForeColor = Color.White;
            lblNombreUsuario.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblNombreUsuario.Location = new Point(75, 22);
            lblNombreUsuario.Size = new Size(195, 24);
            lblNombreUsuario.AutoEllipsis = true;
            panelHeaderUsuario.Controls.Add(lblNombreUsuario);

            lblRolUsuario = new Label();
            lblRolUsuario.Text = _usuarioLogueado.Rol;
            lblRolUsuario.ForeColor = Color.FromArgb(225, 235, 225);
            lblRolUsuario.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            lblRolUsuario.Location = new Point(75, 48);
            lblRolUsuario.Size = new Size(195, 20);
            lblRolUsuario.AutoEllipsis = true;
            panelHeaderUsuario.Controls.Add(lblRolUsuario);

            // Provincia: se muestra siempre (los administradores generales aparecen como "Nacional")
            Label lblProvincia = new Label();
            string nombreProvincia = string.IsNullOrWhiteSpace(_usuarioLogueado.NombreProvincia)
                ? "Nacional (Todas)"
                : _usuarioLogueado.NombreProvincia;
            lblProvincia.Text = "📍 " + nombreProvincia;
            lblProvincia.ForeColor = Color.FromArgb(200, 255, 200);
            lblProvincia.Font = new Font("Segoe UI", 9, FontStyle.Italic);
            lblProvincia.Location = new Point(75, 70);
            lblProvincia.Size = new Size(195, 20);
            lblProvincia.AutoEllipsis = true;
            panelHeaderUsuario.Controls.Add(lblProvincia);

            // ---- Botones del menú ----
            btnEstablecimientos = CrearBotonMenu("🏥  Establecimientos");
            btnEstablecimientos.Click += (s, e) => AbrirModuloEstablecimientos();
            flowMenu.Controls.Add(btnEstablecimientos);

            btnUsuarios = CrearBotonMenu("👥  Gestión de Usuarios");
            btnUsuarios.Click += (s, e) => AbrirModuloUsuarios();
            flowMenu.Controls.Add(btnUsuarios);

            btnReportes = CrearBotonMenu("📊  Reportes y Exportación");
            btnReportes.Click += (s, e) => AbrirModuloReportes();
            flowMenu.Controls.Add(btnReportes);

            btnCambiarPassword = CrearBotonMenu("🔐  Cambiar Contraseña");
            btnCambiarPassword.Click += (s, e) => AbrirCambiarPassword();
            flowMenu.Controls.Add(btnCambiarPassword);
        }

        private Button CrearBotonMenu(string texto)
        {
            Button btn = new Button();
            btn.Text = texto;
            btn.Font = new Font("Segoe UI", 11, FontStyle.Regular);
            btn.ForeColor = Color.White;
            btn.BackColor = Estilo.Primario;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Estilo.Hover;
            btn.FlatAppearance.MouseDownBackColor = Estilo.Activo;
            btn.Size = new Size(280, 52);
            btn.Margin = new Padding(0, 0, 0, 2);
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Padding = new Padding(25, 0, 0, 0);
            btn.Cursor = Cursors.Hand;
            btn.UseVisualStyleBackColor = false;
            return btn;
        }

        // Resalta la opción del menú que está abierta
        private void MarcarActivo(Button boton)
        {
            foreach (Control c in flowMenu.Controls)
            {
                if (c is Button b)
                {
                    b.BackColor = Estilo.Primario;
                    b.Font = new Font("Segoe UI", 11, FontStyle.Regular);
                }
            }

            boton.BackColor = Estilo.Activo;
            boton.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            _botonActivo = boton;
        }

        private void AplicarPermisosPorRol()
        {
            // El consultor solo consulta: no gestiona usuarios.
            // El administrador general y el provincial sí (el provincial solo sobre su provincia).
            if (_usuarioLogueado.Rol == Roles.UsuarioConsultor)
            {
                btnUsuarios.Visible = false;
            }
        }

        // Reemplaza el módulo mostrado liberando el anterior (Controls.Clear no libera recursos)
        private void MostrarModulo(Control modulo, Button boton)
        {
            foreach (Control anterior in panelContenedor.Controls.Cast<Control>().ToList())
            {
                panelContenedor.Controls.Remove(anterior);
                anterior.Dispose();
            }

            modulo.Dock = DockStyle.Fill;
            panelContenedor.Controls.Add(modulo);
            MarcarActivo(boton);
        }

        private void AbrirModuloEstablecimientos()
        {
            MostrarModulo(new EstablecimientosControl(_usuarioLogueado), btnEstablecimientos);
        }

        private void AbrirModuloUsuarios()
        {
            MostrarModulo(new UsuariosControl(_usuarioLogueado), btnUsuarios);
        }

        private void AbrirModuloReportes()
        {
            MostrarModulo(new ReportesControl(_usuarioLogueado), btnReportes);
        }

        private void AbrirCambiarPassword()
        {
            MostrarModulo(new CambiarPasswordControl(_usuarioLogueado), btnCambiarPassword);
        }

        private void BtnCerrarSesion_Click(object? sender, EventArgs e)
        {
            if (Estilo.Confirmar("¿Desea cerrar la sesión?"))
            {
                CerrarSesionSolicitada = true;
                this.Close(); // Program.Main vuelve a mostrar el Login
            }
        }

        private void Main_Load(object sender, EventArgs e)
        {
            // Pantalla inicial
            AbrirModuloEstablecimientos();
        }
    }
}
