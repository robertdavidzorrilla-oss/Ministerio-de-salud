using System;
using System.Drawing;
using System.Windows.Forms;

namespace Presentacion
{
    /// <summary>Colores y fábricas de controles compartidos por todas las pantallas.</summary>
    internal static class Estilo
    {
        public static readonly Color Primario = Color.FromArgb(0, 102, 51);
        public static readonly Color Secundario = Color.FromArgb(0, 153, 76);
        public static readonly Color Hover = Color.FromArgb(0, 128, 64);
        public static readonly Color Activo = Color.FromArgb(0, 80, 40);
        public static readonly Color Fondo = Color.FromArgb(240, 245, 242);
        public static readonly Color TextoOscuro = Color.FromArgb(30, 30, 30);
        public static readonly Color Exito = Color.FromArgb(39, 174, 96);
        public static readonly Color Advertencia = Color.FromArgb(243, 156, 18);
        public static readonly Color Error = Color.FromArgb(220, 53, 36);
        public static readonly Color Azul = Color.FromArgb(52, 152, 219);

        public static Color Oscurecer(Color color, int cantidad)
        {
            int r = Math.Max(0, Math.Min(255, color.R - cantidad));
            int g = Math.Max(0, Math.Min(255, color.G - cantidad));
            int b = Math.Max(0, Math.Min(255, color.B - cantidad));
            return Color.FromArgb(r, g, b);
        }

        public static Button CrearBoton(string texto, Color fondo, int ancho = 120, int alto = 40)
        {
            Button btn = new Button();
            btn.Text = texto;
            btn.Size = new Size(ancho, alto);
            btn.Margin = new Padding(0, 0, 10, 0);
            btn.BackColor = fondo;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Oscurecer(fondo, 20);
            btn.FlatAppearance.MouseDownBackColor = Oscurecer(fondo, 40);
            btn.Cursor = Cursors.Hand;
            btn.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btn.UseVisualStyleBackColor = false;
            return btn;
        }

        /// <summary>Panel superior con título y una fila de botones (FlowLayoutPanel).</summary>
        public static Panel CrearPanelSuperior(string titulo, out FlowLayoutPanel botones, int alto = 105)
        {
            Panel panel = new Panel();
            panel.Dock = DockStyle.Top;
            panel.Height = alto;
            panel.BackColor = Color.White;
            panel.Padding = new Padding(20, 8, 20, 8);

            botones = new FlowLayoutPanel();
            botones.Dock = DockStyle.Fill;
            botones.FlowDirection = FlowDirection.LeftToRight;
            botones.WrapContents = false;
            botones.BackColor = Color.White;

            Label lbl = new Label();
            lbl.Text = titulo;
            lbl.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lbl.ForeColor = Primario;
            lbl.Dock = DockStyle.Top;
            lbl.Height = 42;
            lbl.TextAlign = ContentAlignment.MiddleLeft;

            // Se agrega primero el que ocupa el espacio restante (Fill) y después el título (Top)
            panel.Controls.Add(botones);
            panel.Controls.Add(lbl);
            return panel;
        }

        public static Panel CrearPanelBusqueda(out TextBox txtBusqueda, out Label lblTotal)
        {
            Panel panel = new Panel();
            panel.Dock = DockStyle.Top;
            panel.Height = 56;
            panel.BackColor = Color.White;

            lblTotal = new Label();
            lblTotal.Text = "Total: 0";
            lblTotal.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblTotal.ForeColor = Primario;
            lblTotal.AutoSize = false;
            lblTotal.Width = 180;
            lblTotal.Dock = DockStyle.Right;
            lblTotal.TextAlign = ContentAlignment.MiddleCenter;
            panel.Controls.Add(lblTotal);

            Label lblBuscar = new Label();
            lblBuscar.Text = "🔍 Buscar:";
            lblBuscar.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblBuscar.ForeColor = Primario;
            lblBuscar.Location = new Point(20, 17);
            lblBuscar.AutoSize = true;
            panel.Controls.Add(lblBuscar);

            txtBusqueda = new TextBox();
            txtBusqueda.Location = new Point(115, 14);
            txtBusqueda.Size = new Size(350, 28);
            txtBusqueda.Font = new Font("Segoe UI", 10);
            panel.Controls.Add(txtBusqueda);

            return panel;
        }

        public static Panel CrearBarraEstado(out Label lblEstado)
        {
            Panel panel = new Panel();
            panel.Dock = DockStyle.Bottom;
            panel.Height = 34;
            panel.BackColor = Color.White;
            panel.Padding = new Padding(15, 0, 15, 0);

            lblEstado = new Label();
            lblEstado.Text = "✓ Listo";
            lblEstado.ForeColor = Exito;
            lblEstado.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            lblEstado.Dock = DockStyle.Fill;
            lblEstado.TextAlign = ContentAlignment.MiddleLeft;
            panel.Controls.Add(lblEstado);
            return panel;
        }

        public static void ConfigurarGrid(DataGridView dgv)
        {
            dgv.Dock = DockStyle.Fill;
            dgv.AutoGenerateColumns = false;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeRows = false;
            dgv.ReadOnly = true;
            dgv.MultiSelect = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.RowHeadersVisible = false;
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.None;

            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Primario;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 36;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            dgv.AlternatingRowsDefaultCellStyle.BackColor = Fondo;
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9);
            dgv.DefaultCellStyle.SelectionBackColor = Secundario;
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            dgv.RowTemplate.Height = 32;
        }

        public static DataGridViewTextBoxColumn Columna(string propiedad, string encabezado, float peso = 100)
        {
            DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
            col.Name = propiedad;
            col.DataPropertyName = propiedad;
            col.HeaderText = encabezado;
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            col.FillWeight = peso;
            col.SortMode = DataGridViewColumnSortMode.NotSortable;
            return col;
        }

        public static bool Coincide(string? texto, string filtro)
        {
            return texto != null && texto.Contains(filtro, StringComparison.OrdinalIgnoreCase);
        }

        public static void Info(string mensaje)
        {
            MessageBox.Show(mensaje, "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void Aviso(string mensaje)
        {
            MessageBox.Show(mensaje, "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void MostrarError(string mensaje)
        {
            MessageBox.Show(mensaje, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static bool Confirmar(string mensaje)
        {
            return MessageBox.Show(mensaje, "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }
    }
}
