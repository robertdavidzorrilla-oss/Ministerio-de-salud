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
    public class UsuariosControl : UserControl
    {
        private readonly Usuario _usuarioActual;
        private readonly UsuarioBusiness _negocio = new UsuarioBusiness();
        private List<Usuario> _listaOriginal = new List<Usuario>();

        private DataGridView dgvUsuarios = null!;
        private Label lblEstadoOperacion = null!;
        private TextBox txtBusqueda = null!;
        private Label lblTotalRegistros = null!;

        public UsuariosControl(Usuario usuario)
        {
            _usuarioActual = usuario;
            ConfigurarControles();
            this.Load += (s, e) => CargarDatos();
        }

        private void ConfigurarControles()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Estilo.Fondo;

            // Grid (Fill) primero; los paneles de borde después (ver nota en EstablecimientosControl)
            dgvUsuarios = new DataGridView();
            Estilo.ConfigurarGrid(dgvUsuarios);
            dgvUsuarios.Columns.Add(Estilo.Columna("Nombre", "Nombre", 28));
            dgvUsuarios.Columns.Add(Estilo.Columna("Correo", "Correo", 34));
            dgvUsuarios.Columns.Add(Estilo.Columna("Rol", "Rol", 22));
            dgvUsuarios.Columns.Add(Estilo.Columna("NombreProvincia", "Provincia", 22));
            dgvUsuarios.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0)
                    EditarSeleccionado();
            };
            this.Controls.Add(dgvUsuarios);

            this.Controls.Add(Estilo.CrearBarraEstado(out lblEstadoOperacion));

            this.Controls.Add(Estilo.CrearPanelBusqueda(out txtBusqueda, out lblTotalRegistros));
            txtBusqueda.TextChanged += (s, e) => AplicarFiltro();

            this.Controls.Add(Estilo.CrearPanelSuperior("👥 Gestión de Usuarios", out FlowLayoutPanel botones));

            Button btnNuevo = Estilo.CrearBoton("➕ Nuevo", Estilo.Exito);
            btnNuevo.Click += (s, e) => AbrirMantenimiento(null);
            botones.Controls.Add(btnNuevo);

            Button btnEditar = Estilo.CrearBoton("✏️ Editar", Estilo.Azul);
            btnEditar.Click += (s, e) => EditarSeleccionado();
            botones.Controls.Add(btnEditar);

            Button btnEliminar = Estilo.CrearBoton("🗑️ Eliminar", Estilo.Error);
            btnEliminar.Click += (s, e) => EliminarSeleccionado();
            botones.Controls.Add(btnEliminar);

            Button btnPassword = Estilo.CrearBoton("🔑 Contraseña", Color.FromArgb(142, 68, 173), 130);
            btnPassword.Click += (s, e) => RestablecerContraseñaSeleccionado();
            botones.Controls.Add(btnPassword);

            Button btnRefrescar = Estilo.CrearBoton("🔄 Refrescar", Estilo.Advertencia);
            btnRefrescar.Click += (s, e) => CargarDatos();
            botones.Controls.Add(btnRefrescar);
        }

        public void CargarDatos()
        {
            try
            {
                ActualizarEstado("⟳ Cargando datos...");
                _listaOriginal = _negocio.ListarSegunRol(_usuarioActual);
                AplicarFiltro();
                ActualizarEstado("✓ Datos cargados correctamente");
            }
            catch (Exception ex)
            {
                ActualizarEstado("✗ Error al cargar datos", true);
                Estilo.MostrarError("Error al cargar los usuarios: " + ex.Message);
            }
        }

        private void AplicarFiltro()
        {
            string filtro = txtBusqueda.Text.Trim();

            List<Usuario> lista = string.IsNullOrEmpty(filtro)
                ? _listaOriginal.ToList()
                : _listaOriginal.Where(u =>
                    Estilo.Coincide(u.Nombre, filtro) ||
                    Estilo.Coincide(u.Correo, filtro) ||
                    Estilo.Coincide(u.Rol, filtro) ||
                    Estilo.Coincide(u.NombreProvincia, filtro)).ToList();

            dgvUsuarios.DataSource = new BindingList<Usuario>(lista);
            lblTotalRegistros.Text = "Total: " + lista.Count;
        }

        private Usuario? ObtenerSeleccionado()
        {
            return dgvUsuarios.CurrentRow?.DataBoundItem as Usuario;
        }

        private void EditarSeleccionado()
        {
            Usuario? seleccionado = ObtenerSeleccionado();
            if (seleccionado == null)
            {
                Estilo.Aviso("Seleccione un usuario para editar.");
                return;
            }

            if (!_negocio.PuedeGestionar(_usuarioActual, seleccionado))
            {
                Estilo.Aviso("No tiene permisos para modificar a este usuario.");
                return;
            }

            AbrirMantenimiento(seleccionado);
        }

        private void AbrirMantenimiento(Usuario? entidad)
        {
            bool esNuevo = entidad == null;

            using (MantenimientoUsuario form = new MantenimientoUsuario(entidad, _usuarioActual))
            {
                if (form.ShowDialog(this.FindForm()) == DialogResult.OK)
                {
                    CargarDatos();
                    Estilo.Info(esNuevo ? "✓ Usuario registrado con éxito." : "✓ Usuario actualizado con éxito.");
                }
            }
        }

        private void EliminarSeleccionado()
        {
            Usuario? seleccionado = ObtenerSeleccionado();
            if (seleccionado == null)
            {
                Estilo.Aviso("Seleccione un usuario de la lista.");
                return;
            }

            if (!Estilo.Confirmar("¿Desea eliminar al usuario '" + seleccionado.Nombre + "'?\n\nEsta acción no se puede deshacer."))
                return;

            try
            {
                ActualizarEstado("⟳ Eliminando usuario...");
                if (_negocio.Eliminar(seleccionado, _usuarioActual))
                    Estilo.Info("✓ Usuario eliminado con éxito.");
                else
                    Estilo.Aviso("No se eliminó ningún registro (puede que ya no exista).");

                CargarDatos();
            }
            catch (Exception ex)
            {
                ActualizarEstado("✗ Error al eliminar", true);
                Estilo.MostrarError(ex.Message);
            }
        }

        private void RestablecerContraseñaSeleccionado()
        {
            Usuario? seleccionado = ObtenerSeleccionado();
            if (seleccionado == null)
            {
                Estilo.Aviso("Seleccione un usuario para cambiarle la contraseña.");
                return;
            }

            if (!_negocio.PuedeGestionar(_usuarioActual, seleccionado))
            {
                Estilo.Aviso("No tiene permisos para cambiar la contraseña de este usuario.");
                return;
            }

            using (DialogoPassword dialogo = new DialogoPassword(seleccionado.Nombre))
            {
                if (dialogo.ShowDialog(this.FindForm()) != DialogResult.OK)
                    return;

                try
                {
                    if (_negocio.RestablecerContraseña(seleccionado, dialogo.NuevaContraseña, _usuarioActual))
                        Estilo.Info("✓ Contraseña actualizada con éxito.");
                    else
                        Estilo.Aviso("No se actualizó la contraseña.");
                }
                catch (Exception ex)
                {
                    Estilo.MostrarError(ex.Message);
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
