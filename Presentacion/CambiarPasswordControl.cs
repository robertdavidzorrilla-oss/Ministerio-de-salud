using Business;
using Clases;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Presentacion
{
    public class CambiarPasswordControl : UserControl
    {
        private readonly Usuario _usuarioActual;
        private readonly UsuarioBusiness _negocio = new UsuarioBusiness();

        private TextBox txtActual = null!;
        private TextBox txtNueva = null!;
        private TextBox txtConfirmar = null!;
        private Label lblMensaje = null!;
        private Button btnCambiar = null!;

        public CambiarPasswordControl(Usuario usuario)
        {
            _usuarioActual = usuario;
            ConfigurarControles();
        }

        private void ConfigurarControles()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Estilo.Fondo;

            // Zona de contenido (Fill) primero y el título (Top) después
            Panel zonaContenido = new Panel();
            zonaContenido.Dock = DockStyle.Fill;
            zonaContenido.BackColor = Estilo.Fondo;
            zonaContenido.Padding = new Padding(20);
            this.Controls.Add(zonaContenido);

            this.Controls.Add(Estilo.CrearPanelSuperior("🔐 Cambiar Contraseña", out FlowLayoutPanel _, 60));

            Panel tarjeta = new Panel();
            tarjeta.Location = new Point(20, 20);
            tarjeta.Size = new Size(480, 380);
            tarjeta.BackColor = Color.White;
            tarjeta.BorderStyle = BorderStyle.FixedSingle;
            zonaContenido.Controls.Add(tarjeta);

            int y = 20;

            tarjeta.Controls.Add(CrearLabel("Contraseña actual:", y));
            y += 25;
            txtActual = CrearTextBox(y);
            tarjeta.Controls.Add(txtActual);
            y += 55;

            tarjeta.Controls.Add(CrearLabel("Nueva contraseña (mínimo 6 caracteres):", y));
            y += 25;
            txtNueva = CrearTextBox(y);
            tarjeta.Controls.Add(txtNueva);
            y += 55;

            tarjeta.Controls.Add(CrearLabel("Confirmar nueva contraseña:", y));
            y += 25;
            txtConfirmar = CrearTextBox(y);
            txtConfirmar.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    CambiarContraseña();
                }
            };
            tarjeta.Controls.Add(txtConfirmar);
            y += 50;

            lblMensaje = new Label();
            lblMensaje.Location = new Point(20, y);
            lblMensaje.Size = new Size(430, 40);
            lblMensaje.ForeColor = Estilo.Error;
            lblMensaje.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            tarjeta.Controls.Add(lblMensaje);
            y += 45;

            btnCambiar = Estilo.CrearBoton("💾 Cambiar contraseña", Estilo.Exito, 430, 42);
            btnCambiar.Location = new Point(20, y);
            btnCambiar.Click += (s, e) => CambiarContraseña();
            tarjeta.Controls.Add(btnCambiar);
        }

        private Label CrearLabel(string texto, int top)
        {
            Label lbl = new Label();
            lbl.Text = texto;
            lbl.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            lbl.ForeColor = Estilo.Primario;
            lbl.Location = new Point(20, top);
            lbl.AutoSize = true;
            return lbl;
        }

        private TextBox CrearTextBox(int top)
        {
            TextBox txt = new TextBox();
            txt.Location = new Point(20, top);
            txt.Size = new Size(430, 28);
            txt.Font = new Font("Segoe UI", 10);
            txt.UseSystemPasswordChar = true;
            txt.MaxLength = 50;
            return txt;
        }

        private void CambiarContraseña()
        {
            try
            {
                lblMensaje.Text = "";
                btnCambiar.Enabled = false;

                _negocio.CambiarContraseña(_usuarioActual, txtActual.Text, txtNueva.Text, txtConfirmar.Text);

                txtActual.Clear();
                txtNueva.Clear();
                txtConfirmar.Clear();
                Estilo.Info("✓ Su contraseña fue actualizada. Úsela la próxima vez que inicie sesión.");
            }
            catch (ArgumentException ex)
            {
                lblMensaje.Text = ex.Message;
            }
            catch (Exception ex)
            {
                lblMensaje.Text = ex.Message;
            }
            finally
            {
                btnCambiar.Enabled = true;
            }
        }
    }
}
