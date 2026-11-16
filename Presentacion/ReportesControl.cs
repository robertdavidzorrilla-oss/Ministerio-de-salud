using Business;
using Clases;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Presentacion
{
    public class ReportesControl : UserControl
    {
        private readonly Usuario _usuarioActual;
        private readonly EstablecimientoBussines _negocio = new EstablecimientoBussines();
        private readonly ReporteBusiness _reportes = new ReporteBusiness();

        private List<Establecimiento> _listaBase = new List<Establecimiento>();
        private List<Establecimiento> _listaFiltrada = new List<Establecimiento>();
        private bool _cargando;

        private DataGridView dgvReporte = null!;
        private ComboBox cmbProvincia = null!;
        private ComboBox cmbTipo = null!;
        private Label lblTotal = null!;
        private Label lblEstadoOperacion = null!;

        public ReportesControl(Usuario usuario)
        {
            _usuarioActual = usuario;
            ConfigurarControles();
            this.Load += (s, e) => CargarTodo();
        }

        private void ConfigurarControles()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Estilo.Fondo;

            // Grid (Fill) primero; los paneles de borde después
            dgvReporte = new DataGridView();
            Estilo.ConfigurarGrid(dgvReporte);
            dgvReporte.Columns.Add(Estilo.Columna("Nombre", "Nombre", 28));
            dgvReporte.Columns.Add(Estilo.Columna("NombreTipoEstablecimiento", "Tipo", 18));
            dgvReporte.Columns.Add(Estilo.Columna("NombreProvincia", "Provincia", 16));
            dgvReporte.Columns.Add(Estilo.Columna("Direccion", "Dirección", 38));
            dgvReporte.Columns.Add(Estilo.Columna("Telefono", "Teléfono", 14));
            this.Controls.Add(dgvReporte);

            this.Controls.Add(Estilo.CrearBarraEstado(out lblEstadoOperacion));

            // Panel de filtros
            Panel panelFiltros = new Panel();
            panelFiltros.Dock = DockStyle.Top;
            panelFiltros.Height = 60;
            panelFiltros.BackColor = Color.White;

            lblTotal = new Label();
            lblTotal.Text = "Total: 0";
            lblTotal.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblTotal.ForeColor = Estilo.Primario;
            lblTotal.AutoSize = false;
            lblTotal.Width = 180;
            lblTotal.Dock = DockStyle.Right;
            lblTotal.TextAlign = ContentAlignment.MiddleCenter;
            panelFiltros.Controls.Add(lblTotal);

            Label lblProv = new Label();
            lblProv.Text = "Provincia:";
            lblProv.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblProv.ForeColor = Estilo.Primario;
            lblProv.Location = new Point(20, 20);
            lblProv.AutoSize = true;
            panelFiltros.Controls.Add(lblProv);

            cmbProvincia = new ComboBox();
            cmbProvincia.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbProvincia.Font = new Font("Segoe UI", 10);
            cmbProvincia.Location = new Point(105, 16);
            cmbProvincia.Size = new Size(240, 28);
            cmbProvincia.SelectedIndexChanged += (s, e) => AplicarFiltros();
            panelFiltros.Controls.Add(cmbProvincia);

            Label lblTipoEst = new Label();
            lblTipoEst.Text = "Tipo:";
            lblTipoEst.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblTipoEst.ForeColor = Estilo.Primario;
            lblTipoEst.Location = new Point(370, 20);
            lblTipoEst.AutoSize = true;
            panelFiltros.Controls.Add(lblTipoEst);

            cmbTipo = new ComboBox();
            cmbTipo.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTipo.Font = new Font("Segoe UI", 10);
            cmbTipo.Location = new Point(420, 16);
            cmbTipo.Size = new Size(240, 28);
            cmbTipo.SelectedIndexChanged += (s, e) => AplicarFiltros();
            panelFiltros.Controls.Add(cmbTipo);

            this.Controls.Add(panelFiltros);

            this.Controls.Add(Estilo.CrearPanelSuperior("📊 Reportes y Exportación", out FlowLayoutPanel botones, 100));

            Button btnExportar = Estilo.CrearBoton("📥 Exportar CSV", Estilo.Exito, 150);
            btnExportar.Click += (s, e) => Exportar();
            botones.Controls.Add(btnExportar);

            Button btnRefrescar = Estilo.CrearBoton("🔄 Refrescar", Estilo.Advertencia);
            btnRefrescar.Click += (s, e) => CargarTodo();
            botones.Controls.Add(btnRefrescar);
        }

        private void CargarTodo()
        {
            try
            {
                _cargando = true;
                ActualizarEstado("⟳ Cargando datos...");

                // Provincias y tipos salen de la base de datos; se agrega la opción "todas"
                List<Provinciacs> provincias = _negocio.ObtenerProvincias();
                provincias.Insert(0, new Provinciacs { IdProvincia = 0, Nombre = "Todas las provincias" });
                cmbProvincia.DataSource = provincias;
                cmbProvincia.DisplayMember = "Nombre";
                cmbProvincia.ValueMember = "IdProvincia";

                List<TipoEstablecimiento> tipos = _negocio.ObtenerTiposEstablecimiento();
                tipos.Insert(0, new TipoEstablecimiento { IdTipoEstablecimiento = 0, Nombre = "Todos los tipos", Descripcion = "" });
                cmbTipo.DataSource = tipos;
                cmbTipo.DisplayMember = "Nombre";
                cmbTipo.ValueMember = "IdTipoEstablecimiento";

                // Solo el administrador general puede ver otras provincias
                if (_usuarioActual.Rol != Roles.AdministradorGeneral && _usuarioActual.IdProvincia.HasValue)
                {
                    cmbProvincia.SelectedValue = _usuarioActual.IdProvincia.Value;
                    cmbProvincia.Enabled = false;
                }

                _listaBase = _negocio.ListarEstablecimientosSegunRol(_usuarioActual);
            }
            catch (Exception ex)
            {
                ActualizarEstado("✗ Error al cargar datos", true);
                Estilo.MostrarError("Error al cargar el reporte: " + ex.Message);
                return;
            }
            finally
            {
                _cargando = false;
            }

            AplicarFiltros();
            ActualizarEstado("✓ Datos cargados correctamente");
        }

        private void AplicarFiltros()
        {
            if (_cargando)
                return;

            int idProvincia = cmbProvincia.SelectedValue is int p ? p : 0;
            int idTipo = cmbTipo.SelectedValue is int t ? t : 0;

            _listaFiltrada = _listaBase
                .Where(e => (idProvincia == 0 || e.IdProvincia == idProvincia)
                         && (idTipo == 0 || e.IdTipoEstablecimiento == idTipo))
                .ToList();

            dgvReporte.DataSource = new BindingList<Establecimiento>(_listaFiltrada);
            lblTotal.Text = "Total: " + _listaFiltrada.Count;
        }

        private void Exportar()
        {
            if (_listaFiltrada.Count == 0)
            {
                Estilo.Aviso("No hay datos para exportar con los filtros actuales.");
                return;
            }

            using (SaveFileDialog dialogo = new SaveFileDialog())
            {
                dialogo.Title = "Exportar establecimientos";
                dialogo.Filter = "Archivo CSV (*.csv)|*.csv";
                dialogo.FileName = "Establecimientos_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".csv";

                if (dialogo.ShowDialog(this.FindForm()) != DialogResult.OK)
                    return;

                try
                {
                    // Separador según la configuración regional, para que Excel abra las columnas bien
                    string separador = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
                    string csv = _reportes.GenerarCsvEstablecimientos(_listaFiltrada, separador);

                    // UTF-8 con BOM: Excel respeta las tildes y la ñ
                    File.WriteAllText(dialogo.FileName, csv, new UTF8Encoding(true));

                    ActualizarEstado("✓ Reporte exportado");
                    Estilo.Info("✓ Reporte exportado con " + _listaFiltrada.Count + " registros:\n" + dialogo.FileName);
                }
                catch (IOException ex)
                {
                    Estilo.MostrarError("No se pudo escribir el archivo (¿está abierto en Excel?): " + ex.Message);
                }
                catch (Exception ex)
                {
                    Estilo.MostrarError("Error al exportar: " + ex.Message);
                }
            }
        }

        private void ActualizarEstado(string mensaje, bool esError = false)
        {
            lblEstadoOperacion.Text = mensaje;
            lblEstadoOperacion.ForeColor = esError ? Estilo.Error : Estilo.Exito;
        }
    }
}
