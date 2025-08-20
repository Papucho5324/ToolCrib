let materialesData = [];

$(document).ready(function () {
	$('#producto').select2({
		placeholder: 'Selecciona un material...',
		width: '100%'
	});

	cargarMateriales();
});

// Función para cargar materiales
function cargarMateriales() {
	fetch('/OfficeSup/ObtenerMateriales')
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
				llenarDropdown(data);
			} else {
				mostrarDropdownVacio('No hay materiales disponibles');
			}
		})
		.catch(error => {
			mostrarError('Error de conexión al obtener materiales: ' + error.message);
		});
}

function llenarDropdown(data) {
	const select = $('#producto');
	select.empty();
	select.append('<option disabled selected>Selecciona un material...</option>');

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

	// Opcional: mostrar SweetAlert si está disponible
	if (typeof Swal !== 'undefined') {
		Swal.fire({
			icon: 'error',
			title: 'Error',
			text: mensaje
		});
	}
}

// Manejar selección de producto
$('#producto').change(function () {
	const descripcion = $(this).val();
	const tipo = 'Material'; // Siempre será Material

	if (descripcion && descripcion !== '') {
		// Obtener información del producto
		fetch(`/OfficeSup/ObtenerInfoProducto?descripcion=${encodeURIComponent(descripcion)}&tipo=${encodeURIComponent(tipo)}`)
			.then(response => {
				if (!response.ok) {
					throw new Error('Error en la respuesta: ' + response.status);
				}
				return response.json();
			})
			.then(data => {
				if (data.error) {
					// Considera si quieres mostrar un error al usuario aquí también, por ejemplo con Swal.
					return;
				}

				$('#codigo').val(data.noPart || data.NoPart || '');
				$('#unidadMedida').val(data.um || data.UM || '');
				$('#stock').val(data.stock || data.Stock || '0');
			})
			.catch(error => {
				// Considera si quieres mostrar un error al usuario aquí también, por ejemplo con Swal.
			});
	}
});

// Manejar envío de solicitud
$('#btnEnviarSolicitud').on('click', function (e) {
	e.preventDefault();

	const producto = $('#producto').val();
	const codigo = $('#codigo').val();
	const um = $('#unidadMedida').val();
	const cantidad = $('#cantidad').val();
	const comentario = $('#comentario').val();
	const tipo = "Material"; // Siempre será Material

	if (!producto || producto === 'Selecciona un material...') {
		Swal.fire({
			icon: 'error',
			title: 'Producto requerido',
			text: 'Por favor selecciona un material.'
		});
		return;
	}

	if (!cantidad || cantidad <= 0) {
		Swal.fire({
			icon: 'error',
			title: 'Cantidad requerida',
			text: 'Por favor ingresa una cantidad válida.'
		});
		return;
	}

	const model = {
		folio: $('#folioHidden').val(),
		codigo: codigo,
		descripcion: producto,
		um: um,
		cantidad: parseInt(cantidad),
		comentario: comentario
	};

	fetch('/OfficeSup/EnviarYFinalizarSolicitud', {
		method: 'POST',
		headers: {
			'Content-Type': 'application/json'
		},
		body: JSON.stringify(model)
	})
		.then(res => {
			if (!res.ok) {
				throw new Error('Error en la respuesta: ' + res.status);
			}
			return res.json();
		})
		.then(data => {
			Swal.fire({
				icon: 'success',
				title: 'Solicitud enviada',
				text: data.mensaje || 'Se ha enviado la solicitud correctamente.'
			});

			// Limpiar campos
			$('#cantidad').val('');
			$('#comentario').val('Nuevo Ingreso');
			$('#producto').val('');
			$('#unidadMedida').val('');
			$('#stock').val('');
			$('#codigo').val('');
		})
		.catch(err => {
			Swal.fire({
				icon: 'error',
				title: 'Error',
				text: 'Ocurrió un error al enviar la solicitud: ' + err.message
			});
		});
});

// Limpiar formulario
$('button[type="reset"]').click(function () {
	$('#cantidad').val('');
	$('#comentario').val('Nuevo Ingreso');
	$('#unidadMedida').val('');
	$('#stock').val('');
	$('#codigo').val('');

	// Recargar dropdown de materiales
	if (materialesData.length > 0) {
		llenarDropdown(materialesData);
	}
});