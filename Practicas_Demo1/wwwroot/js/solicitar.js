let materialesData = [];
let herramientasData = [];
let productosEnSolicitud = [];
let contadorProductos = 0;

$(document).ready(function () {
	$('#producto').select2({
		placeholder: 'Selecciona un producto...',
		width: '100%'
	});

	// Carga solo materiales al inicio
	cargarMateriales();
	actualizarContadores();
});

// Función para cargar materiales
function cargarMateriales() {
	fetch('/Employee/ObtenerMateriales')
		.then(response => {
			if (!response.ok) {
				throw new Error('Error en la respuesta: ' + response.status);
			}
			return response.json();
		})
		.then(data => {
			if (data.error) {
				mostrarError('Error al cargar materiales: ' + data.error);
				return;
			}

			materialesData = data;
			if (Array.isArray(data) && data.length > 0) {
				llenarDropdown(data, 'material');
			} else {
				mostrarDropdownVacio('No hay materiales disponibles');
			}
		})
		.catch(error => {
			mostrarError('Error de conexión al obtener materiales: ' + error.message);
		});
}

// Función para cargar herramientas
function cargarHerramientas() {
	fetch('/Employee/ObtenerHerramientas')
		.then(response => {
			if (!response.ok) {
				throw new Error('Error en la respuesta: ' + response.status);
			}
			return response.json();
		})
		.then(data => {
			if (data.error) {
				mostrarError('Error al cargar herramientas: ' + data.error);
				return;
			}

			herramientasData = data;
			if (Array.isArray(data) && data.length > 0) {
				llenarDropdown(data, 'herramienta');
			} else {
				mostrarDropdownVacio('No hay herramientas disponibles');
			}
		})
		.catch(error => {
			mostrarError('Error de conexión al obtener herramientas: ' + error.message);
		});
}

function llenarDropdown(data, tipo) {
	const select = $('#producto');
	select.empty();
	select.append('<option disabled selected>Selecciona un ' + tipo + '...</option>');

	data.forEach(item => {
		let texto = item.Description || item.description || 'Sin descripción';
		select.append(`<option value="${texto}">${texto}</option>`);
	});
}

// Función para mostrar dropdown vacío
function mostrarDropdownVacio(mensaje) {
	const select = $('#producto');
	select.empty();
	select.append(`<option disabled selected>${mensaje}</option>`);
}

// Función para mostrar errores
function mostrarError(mensaje) {
	const select = $('#producto');
	select.empty();
	select.append('<option disabled selected>Error al cargar datos</option>');

	if (typeof Swal !== 'undefined') {
		Swal.fire({
			icon: 'error',
			title: 'Error',
			text: mensaje
		});
	}
}

// Manejar cambio entre Material y Herramienta
$('input[name="tipo"]').change(function () {
	const tipoSeleccionado = $(this).attr('id');

	// Limpiar campos
	$('#producto').empty().append('<option disabled selected>Cargando...</option>');
	$('#unidadMedida').val('');
	$('#stock').val('');
	$('#stockDisponible').val('');
	$('#codigo').val('');

	if (tipoSeleccionado === 'option1') { // Material
		if (materialesData.length > 0) {
			llenarDropdown(materialesData, 'material');
		} else {
			cargarMateriales();
		}
	} else { // Herramienta
		if (herramientasData.length > 0) {
			llenarDropdown(herramientasData, 'herramienta');
		} else {
			cargarHerramientas();
		}
	}
});

// Manejar selección de producto
$('#producto').change(function () {
	const descripcion = $(this).val();
	const tipo = $('#option1').is(':checked') ? 'Material' : 'Herramienta';

	if (descripcion && descripcion !== '') {
		fetch(`/Employee/ObtenerInfoProducto?descripcion=${encodeURIComponent(descripcion)}&tipo=${encodeURIComponent(tipo)}`)
			.then(response => {
				if (!response.ok) {
					throw new Error('Error en la respuesta: ' + response.status);
				}
				return response.json();
			})
			.then(data => {
				if (data.error) {
					console.error('Error al obtener info del producto:', data.error);
					return;
				}

				$('#codigo').val(data.noPart || data.NoPart || '');
				$('#unidadMedida').val(data.um || data.UM || '');
				$('#stock').val(data.stock || data.Stock || '0');
				$('#stockDisponible').val(data.stock || data.Stock || '0');
			})
			.catch(error => {
				console.error('Error al obtener info del producto:', error);
			});
	}
});

// Agregar producto a la lista
$('#btnAgregarProducto').on('click', function (e) {
	e.preventDefault();

	const producto = $('#producto').val();
	const codigo = $('#codigo').val();
	const um = $('#unidadMedida').val();
	const cantidad = parseInt($('#cantidad').val());
	const comentario = $('#comentario').val();
	const stock = parseInt($('#stock').val());
	const tipo = $('#option1').is(':checked') ? "Material" : "Herramienta";

	// Validaciones
	if (!producto || producto.includes('Selecciona')) {
		mostrarAlerta('error', 'Producto requerido', 'Por favor selecciona un producto.');
		return;
	}

	if (!cantidad || cantidad <= 0) {
		mostrarAlerta('error', 'Cantidad requerida', 'Por favor ingresa una cantidad válida.');
		return;
	}

	if (cantidad > stock) {
		mostrarAlerta('warning', 'Cantidad limitada', 'Verificar en Toolcrib disponibilidad.');
		
	}

	// Verificar si el producto ya está en la lista
	const productoExistente = productosEnSolicitud.find(p => p.codigo === codigo);
	if (productoExistente) {
		mostrarAlerta('warning', 'Producto duplicado', 'Este producto ya está en la lista.');
		return;
	}

	// Agregar a la lista
	const nuevoProducto = {
		id: ++contadorProductos,
		codigo: codigo,
		descripcion: producto,
		cantidad: cantidad,
		um: um,
		comentario: comentario,
		tipo: tipo,
		stock: stock
	};

	productosEnSolicitud.push(nuevoProducto);
	agregarFilaTabla(nuevoProducto);
	limpiarFormulario();
	actualizarContadores();
});

function agregarFilaTabla(producto) {
	// Ocultar fila vacía si existe
	$('#filaVacia').hide();

	const fila = `
		<tr data-id="${producto.id}">
			<td class="align-middle">${producto.codigo}</td>
			<td class="align-middle">
				<div class="d-flex flex-column">
					<span>${producto.descripcion}</span>
					<small class="text-muted">${producto.tipo}</small>
				</div>
			</td>
			<td class="align-middle">
				<span class="badge bg-info">${producto.cantidad}</span>
			</td>
			<td class="align-middle">${producto.um}</td>
			<td class="align-middle">
				<small class="text-muted">${producto.comentario}</small>
			</td>
			<td class="align-middle">
				<button type="button" class="btn btn-sm btn-outline-danger" onclick="eliminarProducto(${producto.id})">
					<i class="fas fa-trash"></i>
				</button>
			</td>
		</tr>
	`;

	$('#productosAgregados').append(fila);
}

function eliminarProducto(id) {
	Swal.fire({
		title: '¿Eliminar producto?',
		text: 'Esta acción no se puede deshacer.',
		icon: 'warning',
		showCancelButton: true,
		confirmButtonColor: '#d33',
		cancelButtonColor: '#3085d6',
		confirmButtonText: 'Sí, eliminar',
		cancelButtonText: 'Cancelar'
	}).then((result) => {
		if (result.isConfirmed) {
			// Eliminar de la lista
			productosEnSolicitud = productosEnSolicitud.filter(p => p.id !== id);

			// Eliminar fila de la tabla
			$(`tr[data-id="${id}"]`).remove();

			// Mostrar fila vacía si no hay productos
			if (productosEnSolicitud.length === 0) {
				$('#filaVacia').show();
			}

			actualizarContadores();

			Swal.fire({
				title: 'Eliminado',
				text: 'El producto ha sido eliminado de la lista.',
				icon: 'success',
				timer: 2000,
				showConfirmButton: false
			});
		}
	});
}

function actualizarContadores() {
	const total = productosEnSolicitud.length;
	$('#contadorProductos').text(`${total} producto${total !== 1 ? 's' : ''}`);
	$('#totalProductosModal').text(total);

	// Habilitar/deshabilitar botones
	$('#btnFinalizarSolicitud').prop('disabled', total === 0);
	$('#btnLimpiarLista').prop('disabled', total === 0);
}

// Limpiar lista completa
$('#btnLimpiarLista').on('click', function () {
	if (productosEnSolicitud.length === 0) return;

	Swal.fire({
		title: '¿Limpiar toda la lista?',
		text: 'Se eliminarán todos los productos agregados.',
		icon: 'warning',
		showCancelButton: true,
		confirmButtonColor: '#d33',
		cancelButtonColor: '#3085d6',
		confirmButtonText: 'Sí, limpiar todo',
		cancelButtonText: 'Cancelar'
	}).then((result) => {
		if (result.isConfirmed) {
			productosEnSolicitud = [];
			$('#productosAgregados').empty().append(`
				<tr id="filaVacia" class="text-center">
					<td colspan="6" class="py-4 text-muted">
						<i class="fas fa-inbox fa-2x mb-2"></i>
						<p class="mb-0">No hay productos agregados</p>
					</td>
				</tr>
			`);
			actualizarContadores();

			Swal.fire({
				title: 'Lista limpiada',
				text: 'Todos los productos han sido eliminados.',
				icon: 'success',
				timer: 2000,
				showConfirmButton: false
			});
		}
	});
});

// Mostrar modal de confirmación
$('#btnFinalizarSolicitud').on('click', function () {
	if (productosEnSolicitud.length === 0) return;

	$('#modalConfirmacion').modal('show');
});

// Confirmar y enviar solicitud
$('#btnConfirmarEnvio').on('click', function () {
	$('#modalConfirmacion').modal('hide');
	enviarSolicitudCompleta();
});

async function enviarSolicitudCompleta() {
	if (productosEnSolicitud.length === 0) {
		mostrarAlerta('error', 'Lista vacía', 'No hay productos para enviar.');
		return;
	}

	// Mostrar loading
	Swal.fire({
		title: 'Enviando solicitud...',
		text: 'Por favor espera mientras procesamos tu solicitud.',
		icon: 'info',
		allowOutsideClick: false,
		allowEscapeKey: false,
		showConfirmButton: false,
		didOpen: () => {
			Swal.showLoading();
		}
	});

	try {
		// Preparar la lista de productos para enviar
		const productosParaEnviar = productosEnSolicitud.map(producto => ({
			folio: '', // Se asignará en el servidor
			codigo: producto.codigo,
			descripcion: producto.descripcion,
			um: producto.um,
			cantidad: producto.cantidad,
			comentario: producto.comentario
		}));

		// Enviar toda la lista de una vez al nuevo endpoint
		const response = await fetch('/Employee/EnviarSolicitudCompleta', {
			method: 'POST',
			headers: {
				'Content-Type': 'application/json'
			},
			body: JSON.stringify(productosParaEnviar)
		});

		if (!response.ok) {
			const errorData = await response.json();
			throw new Error(errorData.error || `Error del servidor: ${response.status}`);
		}

		const result = await response.json();

		// Éxito
		Swal.fire({
			icon: 'success',
			title: 'Solicitud enviada exitosamente',
			text: `Folio generado: ${result.folio}. Se enviaron ${result.productosEnviados} producto(s).`,
			showConfirmButton: true,
			confirmButtonText: 'Aceptar'
		}).then(() => {
			// Limpiar todo
			productosEnSolicitud = [];
			$('#productosAgregados').empty().append(`
				<tr id="filaVacia" class="text-center">
					<td colspan="6" class="py-4 text-muted">
						<i class="fas fa-inbox fa-2x mb-2"></i>
						<p class="mb-0">No hay productos agregados</p>
					</td>
				</tr>
			`);
			limpiarFormulario();
			actualizarContadores();
			$('#folio').text('Se generará al finalizar');
		});

	} catch (error) {
		Swal.fire({
			icon: 'error',
			title: 'Error al enviar solicitud',
			text: error.message
		});
	}
}

function limpiarFormulario() {
	$('#cantidad').val('');
	$('#comentario').val('Nuevo Ingreso');
	$('#unidadMedida').val('');
	$('#stock').val('');
	$('#stockDisponible').val('');
	$('#codigo').val('');

	// Resetear select2
	$('#producto').val(null).trigger('change');

	const tipoActual = $('#option1').is(':checked') ? 'material' : 'herramienta';
	if (tipoActual === 'material' && materialesData.length > 0) {
		llenarDropdown(materialesData, 'material');
	} else if (tipoActual === 'herramienta' && herramientasData.length > 0) {
		llenarDropdown(herramientasData, 'herramienta');
	}
}

function mostrarAlerta(tipo, titulo, mensaje) {
	Swal.fire({
		icon: tipo,
		title: titulo,
		text: mensaje,
		timer: tipo === 'success' ? 3000 : undefined,
		showConfirmButton: tipo !== 'success'
	});
}

// Limpiar formulario con botón reset
$('button[type="reset"]').click(function () {
	limpiarFormulario();
});