using System;
using System.Drawing;
using System.Windows.Forms;

namespace Presentacion
{
    /// <summary>Pide una contraseña nueva (con confirmación) para restablecer la de otro usuario.</summary>
    internal class DialogoPassword : Form
    {
        private readonly TextBox txtNueva;
        private readonly TextBox txtConfirmar;
        private readonly Label lblError;

        public string NuevaContraseña
        {
            get { return txtNueva.Text; }
        }

        public DialogoPassword(string nombreUsuario)
        {
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.Text = "Restablecer contraseña";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Estilo.Fondo;
            this.ClientSize = new Size(420, 270);

            Label lblTitulo = new Label();
            lblTitulo.Text = "Nueva contraseña para: " + nombreUsuario;
            lblTitulo.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblTitulo.ForeColor = Estilo.Primario;
            lblTitulo.Location = new Point(20, 15);
            lblTitulo.Size = new Size(380, 24);
            lblTitulo.AutoEllipsis = true;
            this.Controls.Add(lblTitulo);

            Label lblNueva = new Label();
            lblNueva.Text = "Nueva contraseña (mínimo 6 caracteres):";
            lblNueva.Location = new Point(20, 55);
            lblNueva.AutoSize = true;
            this.Controls.Add(lblNueva);

            txtNueva = new TextBox();
            txtNueva.Location = new Point(20, 80);
            txtNueva.Size = new Size(380, 28);
            txtNueva.UseSystemPasswordChar = true;
            txtNueva.MaxLength = 50;
            this.Controls.Add(txtNueva);

            Label lblConfirmar = new Label();
            lblConfirmar.Text = "Confirmar contraseña:";
            lblConfirmar.Location = new Point(20, 120);
            lblConfirmar.AutoSize = true;
            this.Controls.Add(lblConfirmar);

            txtConfirmar = new TextBox();
            txtConfirmar.Location = new Point(20, 145);
            txtConfirmar.Size = new Size(380, 28);
            txtConfirmar.UseSystemPasswordChar = true;
            txtConfirmar.MaxLength = 50;
            this.Controls.Add(txtConfirmar);

            lblError = new Label();
            lblError.Location = new Point(20, 182);
            lblError.Size = new Size(380, 22);
            lblError.ForeColor = Estilo.Error;
            this.Controls.Add(lblError);

            Button btnAceptar = Estilo.CrearBoton("Aceptar", Estilo.Exito, 120, 36);
            btnAceptar.Location = new Point(150, 215);
            btnAceptar.Click += (s, e) =>
            {
                if (txtNueva.Text.Length < 6)
                {
                    lblError.Text = "La contraseña debe tener al menos 6 caracteres.";
                    return;
                }
                if (txtNueva.Text != txtConfirmar.Text)
                {
                    lblError.Text = "La confirmación no coincide.";
                    return;
                }
                this.DialogResult = DialogResult.OK;
            };
            this.Controls.Add(btnAceptar);

            Button btnCancelar = Estilo.CrearBoton("Cancelar", Estilo.Error, 120, 36);
            btnCancelar.Location = new Point(280, 215);
            btnCancelar.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            this.Controls.Add(btnCancelar);

            this.AcceptButton = btnAceptar;
            this.CancelButton = btnCancelar;
        }
    }
}
