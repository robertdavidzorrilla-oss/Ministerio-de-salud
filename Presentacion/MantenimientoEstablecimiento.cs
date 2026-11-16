using Business;
using Clases;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Presentacion
{
    public partial class MantenimientoEstablecimiento : Form
    {
        private readonly Establecimiento? _establecimiento;
        private readonly Usuario _usuarioActual;
        private readonly EstablecimientoBussines _negocio = new EstablecimientoBussines();

        private TextBox txtNombre = null!;
        private TextBox txtDireccion = null!;
        private TextBox txtTelefono = null!;
        private ComboBox cmbProvincia = null!;
        private ComboBox cmbTipoEstablecimiento = null!;
        private Button btnGuardar = null!;
        private Button btnCancelar = null!;
        private Label lblErrores = null!;

        public MantenimientoEstablecimiento(Establecimiento? establecimiento, Usuario usuario)
        {
            _establecimiento = establecimiento;
            _usuarioActual = usuario;
            ConfigurarFormulario();
            CargarDatos();
        }

        private void ConfigurarFormulario()
        {
            bool esEdicion = _establecimiento != null;

            // Escalado por DPI para que no se desfasen los controles en pantallas con zoom
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = AutoScaleMode.Dpi;

            this.Text = esEdicion ? "Editar Establecimiento" : "Nuevo Establecimiento";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Estilo.Fondo;
            // ClientSize (no Size): antes los botones quedaban fuera del área visible de la ventana
            this.ClientSize = new Size(600, 520);

            Label lblTitulo = new Label();
            lblTitulo.Text = esEdicion ? "✏️ Editar Establecimiento" : "➕ Nuevo Establecimiento";
            lblTitulo.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblTitulo.ForeColor = Estilo.Primario;
            lblTitulo.Location = new Point(20, 15);
            lblTitulo.AutoSize = true;
            this.Controls.Add(lblTitulo);

            Panel panelPrincipal = new Panel();
            panelPrincipal.Location = new Point(20, 60);
            panelPrincipal.Size = new Size(560, 355);
            panelPrincipal.BackColor = Color.White;
            panelPrincipal.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(panelPrincipal);

            int posY = 10;

            panelPrincipal.Controls.Add(CrearLabel("Nombre del Establecimiento:", posY));
            posY += 25;
            txtNombre = CrearTextBox(posY, 50);
            panelPrincipal.Controls.Add(txtNombre);
            posY += 40;

            panelPrincipal.Controls.Add(CrearLabel("Dirección:", posY));
            posY += 25;
            txtDireccion = CrearTextBox(posY, 255);
            panelPrincipal.Controls.Add(txtDireccion);
            posY += 40;

            panelPrincipal.Controls.Add(CrearLabel("Teléfono (máx. 12 caracteres, ej. 809-555-1234):", posY));
            posY += 25;
            txtTelefono = CrearTextBox(posY, 12);
            panelPrincipal.Controls.Add(txtTelefono);
            posY += 40;

            panelPrincipal.Controls.Add(CrearLabel("Provincia:", posY));
            posY += 25;
            cmbProvincia = CrearComboBox(posY);
            panelPrincipal.Controls.Add(cmbProvincia);
            posY += 40;

            panelPrincipal.Controls.Add(CrearLabel("Tipo de Establecimiento:", posY));
            posY += 25;
            cmbTipoEstablecimiento = CrearComboBox(posY);
            panelPrincipal.Controls.Add(cmbTipoEstablecimiento);

            lblErrores = new Label();
            lblErrores.Location = new Point(20, 422);
            lblErrores.Size = new Size(560, 22);
            lblErrores.ForeColor = Estilo.Error;
            lblErrores.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            this.Controls.Add(lblErrores);

            btnGuardar = Estilo.CrearBoton("💾 Guardar", Estilo.Exito, 130, 38);
            btnGuardar.Location = new Point(310, 460);
            btnGuardar.Click += (s, e) => GuardarRegistro();
            this.Controls.Add(btnGuardar);

            btnCancelar = Estilo.CrearBoton("❌ Cancelar", Estilo.Error, 130, 38);
            btnCancelar.Location = new Point(450, 460);
            btnCancelar.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            this.Controls.Add(btnCancelar);

            this.AcceptButton = btnGuardar;   // Enter = guardar
            this.CancelButton = btnCancelar;  // Esc = cancelar
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
            txt.MaxLength = maxLength; // mismo límite que la columna en SQL
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
                // Provincias: se leen de la tabla Provincias de la base de datos
                List<Provinciacs> provincias = _negocio.ObtenerProvincias();
                cmbProvincia.DataSource = provincias;
                cmbProvincia.DisplayMember = "Nombre";
                cmbProvincia.ValueMember = "IdProvincia";

                List<TipoEstablecimiento> tipos = _negocio.ObtenerTiposEstablecimiento();
                cmbTipoEstablecimiento.DataSource = tipos;
                cmbTipoEstablecimiento.DisplayMember = "Nombre";
                cmbTipoEstablecimiento.ValueMember = "IdTipoEstablecimiento";

                if (provincias.Count == 0 || tipos.Count == 0)
                {
                    lblErrores.Text = provincias.Count == 0
                        ? "No hay provincias en la base de datos. Ejecute el script de datos iniciales."
                        : "No hay tipos de establecimiento en la base de datos. Ejecute el script de datos iniciales.";
                    btnGuardar.Enabled = false;
                }

                if (_establecimiento != null)
                {
                    txtNombre.Text = _establecimiento.Nombre;
                    txtDireccion.Text = _establecimiento.Direccion;
                    txtTelefono.Text = _establecimiento.Telefono;
                    cmbProvincia.SelectedValue = _establecimiento.IdProvincia;
                    cmbTipoEstablecimiento.SelectedValue = _establecimiento.IdTipoEstablecimiento;
                }

                // Administrador provincial: la provincia queda fija
                if (_usuarioActual.Rol == Roles.AdministradorProvincial && _usuarioActual.IdProvincia.HasValue)
                {
                    cmbProvincia.SelectedValue = _usuarioActual.IdProvincia.Value;
                    cmbProvincia.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                btnGuardar.Enabled = false;
                MessageBox.Show("Error al cargar datos: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GuardarRegistro()
        {
            try
            {
                lblErrores.Text = "";

                if (string.IsNullOrWhiteSpace(txtNombre.Text))
                {
                    lblErrores.Text = "El nombre es requerido.";
                    txtNombre.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtDireccion.Text))
                {
                    lblErrores.Text = "La dirección es requerida.";
                    txtDireccion.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtTelefono.Text))
                {
                    lblErrores.Text = "El teléfono es requerido.";
                    txtTelefono.Focus();
                    return;
                }

                if (cmbProvincia.SelectedValue == null)
                {
                    lblErrores.Text = "Debe seleccionar una provincia.";
                    cmbProvincia.Focus();
                    return;
                }

                if (cmbTipoEstablecimiento.SelectedValue == null)
                {
                    lblErrores.Text = "Debe seleccionar un tipo de establecimiento.";
                    cmbTipoEstablecimiento.Focus();
                    return;
                }

                // Se trabaja sobre una copia: si el guardado falla no se altera la fila de la lista
                Establecimiento obj = new Establecimiento();
                obj.IdEstablecimiento = _establecimiento?.IdEstablecimiento ?? 0;
                obj.Nombre = txtNombre.Text.Trim();
                obj.Direccion = txtDireccion.Text.Trim();
                obj.Telefono = txtTelefono.Text.Trim();
                obj.IdProvincia = (int)cmbProvincia.SelectedValue;
                obj.IdTipoEstablecimiento = (int)cmbTipoEstablecimiento.SelectedValue;

                if (!_negocio.Guardar(obj, _usuarioActual))
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
            catch (Exception ex)
            {
                lblErrores.Text = ex.Message;
                MessageBox.Show(ex.Message, "Error al guardar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
