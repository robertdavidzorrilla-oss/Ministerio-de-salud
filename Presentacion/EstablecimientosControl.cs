using Business;
using Clases;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Presentacion
{
    public partial class EstablecimientosControl : UserControl
    {
        private readonly Usuario _usuarioActual;
        private readonly EstablecimientoBussines _negocio = new EstablecimientoBussines();
        private List<Establecimiento> _listaOriginal = new List<Establecimiento>();

        private DataGridView dgvEstablecimientos = null!;
        private Button btnNuevo = null!, btnEditar = null!, btnEliminar = null!, btnRefrescar = null!;
        private Label lblEstadoOperacion = null!;
        private TextBox txtBusqueda = null!;
        private Label lblTotalRegistros = null!;

        private bool EsConsultor => _usuarioActual.Rol == Roles.UsuarioConsultor;

        public EstablecimientosControl(Usuario usuario)
        {
            _usuarioActual = usuario;
            InitializeComponent();
            ConfigurarControles();
        }

        private void ConfigurarControles()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Estilo.Fondo;

            // ORDEN DE DOCKING: se acopla primero lo último agregado.
            // Por eso el grid (Fill) va PRIMERO y los paneles de arriba/abajo después.
            // (Antes el grid se agregaba en medio y los paneles superiores tapaban sus primeras filas.)
            dgvEstablecimientos = new DataGridView();
            Estilo.ConfigurarGrid(dgvEstablecimientos);
            dgvEstablecimientos.Columns.Add(Estilo.Columna("Nombre", "Nombre", 28));
            dgvEstablecimientos.Columns.Add(Estilo.Columna("NombreTipoEstablecimiento", "Tipo", 18));
            dgvEstablecimientos.Columns.Add(Estilo.Columna("NombreProvincia", "Provincia", 16));
            dgvEstablecimientos.Columns.Add(Estilo.Columna("Direccion", "Dirección", 38));
            dgvEstablecimientos.Columns.Add(Estilo.Columna("Telefono", "Teléfono", 14));
            dgvEstablecimientos.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && !EsConsultor)
                    EditarRegistroSeleccionado();
            };
            this.Controls.Add(dgvEstablecimientos);

            this.Controls.Add(Estilo.CrearBarraEstado(out lblEstadoOperacion));

            this.Controls.Add(Estilo.CrearPanelBusqueda(out txtBusqueda, out lblTotalRegistros));
            txtBusqueda.TextChanged += (s, e) => AplicarFiltro();

            this.Controls.Add(Estilo.CrearPanelSuperior("🏥 Gestión de Establecimientos de Salud", out FlowLayoutPanel botones));

            btnNuevo = Estilo.CrearBoton("➕ Nuevo", Estilo.Exito);
            btnNuevo.Click += (s, e) => AbrirMantenimiento(null);
            botones.Controls.Add(btnNuevo);

            btnEditar = Estilo.CrearBoton("✏️ Editar", Estilo.Azul);
            btnEditar.Click += (s, e) => EditarRegistroSeleccionado();
            botones.Controls.Add(btnEditar);

            btnEliminar = Estilo.CrearBoton("🗑️ Eliminar", Estilo.Error);
            btnEliminar.Click += (s, e) => EliminarRegistro();
            botones.Controls.Add(btnEliminar);

            btnRefrescar = Estilo.CrearBoton("🔄 Refrescar", Estilo.Advertencia);
            btnRefrescar.Click += (s, e) => CargarDatos();
            botones.Controls.Add(btnRefrescar);

            // Restricción por rol: los consultores solo consultan
            if (EsConsultor)
            {
                btnNuevo.Visible = false;
                btnEditar.Visible = false;
                btnEliminar.Visible = false;
            }
        }

        public void CargarDatos()
        {
            try
            {
                ActualizarEstado("⟳ Cargando datos...");
                _listaOriginal = _negocio.ListarEstablecimientosSegunRol(_usuarioActual);
                AplicarFiltro();
                ActualizarEstado("✓ Datos cargados correctamente");
            }
            catch (Exception ex)
            {
                ActualizarEstado("✗ Error al cargar datos", true);
                Estilo.MostrarError("Error al cargar los datos: " + ex.Message);
            }
        }

        private void AplicarFiltro()
        {
            string filtro = txtBusqueda.Text.Trim();

            List<Establecimiento> lista = string.IsNullOrEmpty(filtro)
                ? _listaOriginal.ToList()
                : _listaOriginal.Where(e =>
                    Estilo.Coincide(e.Nombre, filtro) ||
                    Estilo.Coincide(e.Direccion, filtro) ||
                    Estilo.Coincide(e.NombreProvincia, filtro) ||
                    Estilo.Coincide(e.NombreTipoEstablecimiento, filtro) ||
                    Estilo.Coincide(e.Telefono, filtro)).ToList();

            dgvEstablecimientos.DataSource = new BindingList<Establecimiento>(lista);
            lblTotalRegistros.Text = "Total: " + lista.Count;
        }

        private Establecimiento? ObtenerSeleccionado()
        {
            return dgvEstablecimientos.CurrentRow?.DataBoundItem as Establecimiento;
        }

        private void EditarRegistroSeleccionado()
        {
            Establecimiento? seleccionado = ObtenerSeleccionado();
            if (seleccionado == null)
            {
                Estilo.Aviso("Seleccione un establecimiento para editar.");
                return;
            }

            AbrirMantenimiento(seleccionado);
        }

        private void AbrirMantenimiento(Establecimiento? entidad)
        {
            bool esNuevo = entidad == null;

            using (MantenimientoEstablecimiento formMantenimiento = new MantenimientoEstablecimiento(entidad, _usuarioActual))
            {
                if (formMantenimiento.ShowDialog(this.FindForm()) == DialogResult.OK)
                {
                    CargarDatos();
                    Estilo.Info(esNuevo ? "✓ Establecimiento registrado con éxito." : "✓ Establecimiento actualizado con éxito.");
                }
            }
        }

        private void EliminarRegistro()
        {
            Establecimiento? seleccionado = ObtenerSeleccionado();
            if (seleccionado == null)
            {
                Estilo.Aviso("Seleccione un registro de la lista.");
                return;
            }

            if (!Estilo.Confirmar("¿Desea eliminar el establecimiento '" + seleccionado.Nombre + "'?\n\nEsta acción no se puede deshacer."))
                return;

            try
            {
                ActualizarEstado("⟳ Eliminando registro...");
                if (_negocio.Eliminar(seleccionado.IdEstablecimiento, _usuarioActual))
                {
                    Estilo.Info("✓ Establecimiento eliminado con éxito.");
                }
                else
                {
                    Estilo.Aviso("No se eliminó ningún registro (puede que ya no exista).");
                }
                CargarDatos();
            }
            catch (Exception ex)
            {
                ActualizarEstado("✗ Error al eliminar", true);
                Estilo.MostrarError("Error al eliminar: " + ex.Message);
            }
        }

        private void ActualizarEstado(string mensaje, bool esError = false)
        {
            lblEstadoOperacion.Text = mensaje;
            lblEstadoOperacion.ForeColor = esError ? Estilo.Error : Estilo.Exito;
        }

        private void EstablecimientosControl_Load(object? sender, EventArgs e)
        {
            // Se cargan los datos cuando el control ya está en pantalla
            CargarDatos();
        }
    }
}
