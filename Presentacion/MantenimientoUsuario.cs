using Business;
using Clases;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Presentacion
{
    public class MantenimientoUsuario : Form
    {
        private readonly Usuario? _usuario;
        private readonly Usuario _usuarioActual;
        private readonly UsuarioBusiness _negocio = new UsuarioBusiness();

        private TextBox txtNombre = null!;
        private TextBox txtCorreo = null!;
        private TextBox txtPassword = null!;
        private Label lblPassword = null!;
        private ComboBox cmbRol = null!;
        private ComboBox cmbProvincia = null!;
        private Button btnGuardar = null!;
        private Label lblErrores = null!;

        private bool EsNuevo
        {
            get { return _usuario == null; }
        }

        public MantenimientoUsuario(Usuario? usuario, Usuario usuarioActual)
        {
            _usuario = usuario;
            _usuarioActual = usuarioActual;
            ConfigurarFormulario();
            CargarDatos();
        }

        private void ConfigurarFormulario()
        {
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.Text = EsNuevo ? "Nuevo Usuario" : "Editar Usuario";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Estilo.Fondo;
            // Alto del panel según los campos visibles (en edición no hay campo de contraseña)
            int altoPanel = EsNuevo ? 340 : 275;
            this.ClientSize = new Size(600, 60 + altoPanel + 100);

            Label lblTitulo = new Label();
            lblTitulo.Text = EsNuevo ? "➕ Nuevo Usuario" : "✏️ Editar Usuario";
            lblTitulo.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblTitulo.ForeColor = Estilo.Primario;
            lblTitulo.Location = new Point(20, 15);
            lblTitulo.AutoSize = true;
            this.Controls.Add(lblTitulo);

            Panel panel = new Panel();
            panel.Location = new Point(20, 60);
            panel.Size = new Size(560, altoPanel);
            panel.BackColor = Color.White;
            panel.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(panel);

            int y = 10;

            panel.Controls.Add(CrearLabel("Nombre:", y));
            y += 25;
            txtNombre = CrearTextBox(y, 50);
            panel.Controls.Add(txtNombre);
            y += 40;

            panel.Controls.Add(CrearLabel("Correo:", y));
            y += 25;
            txtCorreo = CrearTextBox(y, 100);
            panel.Controls.Add(txtCorreo);
            y += 40;

            lblPassword = CrearLabel("Contraseña inicial (mínimo 6 caracteres):", y);
            panel.Controls.Add(lblPassword);
            y += 25;
            txtPassword = CrearTextBox(y, 50);
            txtPassword.UseSystemPasswordChar = true;
            panel.Controls.Add(txtPassword);
            y += 40;

            // En edición la contraseña no se toca aquí (se usa "Restablecer contraseña")
            if (!EsNuevo)
            {
                lblPassword.Visible = false;
                txtPassword.Visible = false;
                y -= 65;
            }

            panel.Controls.Add(CrearLabel("Rol:", y));
            y += 25;
            cmbRol = CrearComboBox(y);
            cmbRol.SelectedIndexChanged += (s, e) => ActualizarEstadoProvincia();
            panel.Controls.Add(cmbRol);
            y += 40;

            panel.Controls.Add(CrearLabel("Provincia:", y));
            y += 25;
            cmbProvincia = CrearComboBox(y);
            panel.Controls.Add(cmbProvincia);

            lblErrores = new Label();
            lblErrores.Location = new Point(20, 60 + altoPanel + 10);
            lblErrores.Size = new Size(560, 22);
            lblErrores.ForeColor = Estilo.Error;
            this.Controls.Add(lblErrores);

            btnGuardar = Estilo.CrearBoton("💾 Guardar", Estilo.Exito, 130, 38);
            btnGuardar.Location = new Point(310, 60 + altoPanel + 42);
            btnGuardar.Click += (s, e) => GuardarRegistro();
            this.Controls.Add(btnGuardar);

            Button btnCancelar = Estilo.CrearBoton("❌ Cancelar", Estilo.Error, 130, 38);
            btnCancelar.Location = new Point(450, 60 + altoPanel + 42);
            btnCancelar.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            this.Controls.Add(btnCancelar);

            this.AcceptButton = btnGuardar;
            this.CancelButton = btnCancelar;
        }

        private Label CrearLabel(string texto, int top)
        {
            Label lbl = new Label();
            lbl.Text = texto;
            lbl.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            lbl.ForeColor = Estilo.Primario;
            lbl.Location = new Point(15, top);
            lbl.AutoSize = true;
            return lbl;
        }

        private TextBox CrearTextBox(int top, int maxLength)
        {
            TextBox txt = new TextBox();
            txt.Location = new Point(15, top);
            txt.Size = new Size(525, 28);
            txt.Font = new Font("Segoe UI", 10);
            txt.BorderStyle = BorderStyle.FixedSingle;
            txt.MaxLength = maxLength;
            return txt;
        }

        private ComboBox CrearComboBox(int top)
        {
            ComboBox cmb = new ComboBox();
            cmb.Location = new Point(15, top);
            cmb.Size = new Size(525, 28);
            cmb.Font = new Font("Segoe UI", 10);
            cmb.DropDownStyle = ComboBoxStyle.DropDownList;
            cmb.BackColor = Color.White;
            return cmb;
        }

        private void CargarDatos()
        {
            try
            {
                // Roles que el usuario actual puede asignar
                cmbRol.DataSource = _negocio.RolesAsignables(_usuarioActual).ToList();

                // Provincias desde la base de datos
                List<Provinciacs> provincias = _negocio.ObtenerProvincias();
                cmbProvincia.DataSource = provincias;
                cmbProvincia.DisplayMember = "Nombre";
                cmbProvincia.ValueMember = "IdProvincia";

                if (provincias.Count == 0)
                {
                    lblErrores.Text = "No hay provincias en la base de datos. Ejecute el script de datos iniciales.";
                }

                if (_usuario != null)
                {
                    txtNombre.Text = _usuario.Nombre;
                    txtCorreo.Text = _usuario.Correo;
                    cmbRol.SelectedItem = _usuario.Rol;
                    if (_usuario.IdProvincia.HasValue)
                        cmbProvincia.SelectedValue = _usuario.IdProvincia.Value;
                }

                // Provincial: la provincia queda fija en la suya
                if (_usuarioActual.Rol == Roles.AdministradorProvincial && _usuarioActual.IdProvincia.HasValue)
                {
                    cmbProvincia.SelectedValue = _usuarioActual.IdProvincia.Value;
                }

                ActualizarEstadoProvincia();
            }
            catch (Exception ex)
            {
                btnGuardar.Enabled = false;
                MessageBox.Show("Error al cargar datos: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // El administrador general no pertenece a una provincia; el provincial no puede cambiarla
        private void ActualizarEstadoProvincia()
        {
            string? rol = cmbRol.SelectedItem as string;

            if (rol == Roles.AdministradorGeneral)
            {
                cmbProvincia.Enabled = false;
                cmbProvincia.SelectedIndex = -1;
            }
            else if (_usuarioActual.Rol == Roles.AdministradorProvincial)
            {
                cmbProvincia.Enabled = false;
                if (_usuarioActual.IdProvincia.HasValue)
                    cmbProvincia.SelectedValue = _usuarioActual.IdProvincia.Value;
            }
            else
            {
                cmbProvincia.Enabled = true;
                if (cmbProvincia.SelectedIndex < 0 && cmbProvincia.Items.Count > 0)
                    cmbProvincia.SelectedIndex = 0;
            }
        }

        private void GuardarRegistro()
        {
            try
            {
                lblErrores.Text = "";

                string? rol = cmbRol.SelectedItem as string;
                if (rol == null)
                {
                    lblErrores.Text = "Debe seleccionar un rol.";
                    return;
                }

                Usuario obj = new Usuario();
                obj.IdUsuario = _usuario?.IdUsuario ?? 0;
                obj.Nombre = txtNombre.Text.Trim();
                obj.Correo = txtCorreo.Text.Trim();
                obj.Rol = rol;
                obj.IdProvincia = (rol != Roles.AdministradorGeneral && cmbProvincia.SelectedValue != null)
                    ? (int?)(int)cmbProvincia.SelectedValue
                    : null;

                if (!_negocio.Guardar(obj, EsNuevo ? txtPassword.Text : null, _usuarioActual))
                {
                    lblErrores.Text = "No se guardó ningún cambio en la base de datos.";
                    return;
                }

                this.DialogResult = DialogResult.OK;
            }
            catch (UnauthorizedAccessException ex)
            {
                lblErrores.Text = ex.Message;
                MessageBox.Show(ex.Message, "Acceso Denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (ArgumentException ex)
            {
                lblErrores.Text = ex.Message;
            }
            catch (InvalidOperationException ex)
            {
                lblErrores.Text = ex.Message;
            }
            catch (Exception ex)
            {
                lblErrores.Text = ex.Message;
                MessageBox.Show(ex.Message, "Error al guardar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
