// Event listener para el formulario de decisión
document.getElementById('decisionForm').addEventListener('submit', function (e) {
    const btn = document.activeElement;
    const isRechazo = btn && btn.name === "Estado" && btn.value === "RECHAZADO";
    const isAprobacion = btn && btn.name === "Estado" && btn.value === "APROBADO";

    // Solo validar comentario si es rechazo
    if (isRechazo) {
        const motivoSelect = document.getElementById('comentarioSelect').value;

        if (!motivoSelect || motivoSelect.trim() === "") {
            e.preventDefault();
            Swal.fire({
                icon: 'warning',
                title: 'Motivo requerido',
                text: 'Debes seleccionar un motivo para rechazar el folio.',
            });
            return;
        }

        // Asegurar que el comentario se envíe correctamente
        document.getElementById('comentarioInput').value = motivoSelect;

        // Para rechazo, permitir envío directo sin confirmación
        return true;
    }

    // Para aprobación, mostrar confirmación y limpiar el comentario
    if (isAprobacion) {
        e.preventDefault();
        document.getElementById('comentarioInput').value = "";

        Swal.fire({
            title: '¿Confirmar aprobación?',
            text: '¿Estás seguro de aprobar este folio?',
            icon: 'question',
            showCancelButton: true,
            confirmButtonColor: '#198754',
            cancelButtonColor: '#6c757d',
            confirmButtonText: 'Sí, aprobar',
            cancelButtonText: 'Cancelar'
        }).then((result) => {
            if (result.isConfirmed) {
                const form = document.getElementById('decisionForm');

                // Insertar input hidden manual para Estado
                const estadoInput = document.createElement("input");
                estadoInput.type = "hidden";
                estadoInput.name = "Estado";
                estadoInput.value = "APROBADO";

                form.appendChild(estadoInput);

                // Remover cualquier onsubmit
                form.onsubmit = null;

                form.submit();
            }

        });
    }
});

// Event listener para cuando se oculta el modal
const modal = document.getElementById('folioModal');
modal.addEventListener('hidden.bs.modal', function () {
    // Limpiar el contenido del modal
    document.getElementById('detalleFolioBody').innerHTML = '';
    document.getElementById('comentariosRechazo').style.display = 'none';
    document.getElementById('btnFinalizarRechazo').style.display = 'none';

    // Limpiar valores de los campos
    document.getElementById('comentarioSelect').value = '';
    document.getElementById('comentarioInput').value = '';

    // Remover el atributo required del select cuando se oculta
    document.getElementById('comentarioSelect').removeAttribute('required');

    // Resetear botones
    document.getElementById('botonesDecision').style.display = 'none';
    document.getElementById('btnCerrarModal').style.display = 'none';
});

// Event listener para cuando se muestra el modal
modal.addEventListener('show.bs.modal', function (event) {
    const button = event.relatedTarget;
    const folio = button.getAttribute('data-folio');
    const status = (button.getAttribute('data-status') || '').trim().toUpperCase();

    // Configurar valores básicos
    document.getElementById('folioInput').value = folio;
    document.getElementById('estadoInfo').innerHTML = `Estado actual: <span class="badge bg-secondary">${status}</span>`;

    const botones = document.getElementById('botonesDecision');
    const cerrar = document.getElementById('btnCerrarModal');

    // Mostrar u ocultar botones según estado
    if (status === "PENDIENTE") {
        botones.style.display = 'flex';
        cerrar.style.display = 'none';
    } else {
        botones.style.display = 'none';
        cerrar.style.display = 'inline-block';
        Swal.fire({
            icon: 'info',
            title: 'Folio ya procesado',
            text: `Este folio ya fue ${status.toLowerCase()}, por lo que no puede editarse.`,
            confirmButtonText: 'Entendido'
        });
    }

    // Cargar detalles del folio
    fetch(`/Supervisor/DetallesFolio?folio=${folio}`)
        .then(response => response.json())
        .then(data => {
            let html = "";
            if (data.length === 0) {
                html = `<tr><td colspan="6" class="text-center text-muted">No hay datos del folio.</td></tr>`;
            } else {
                data.forEach(item => {
                    html += `
                        <tr>
                            <td>${item.codeTc}</td>
                            <td>${item.descripcion}</td>
                            <td>${item.um}</td>
                            <td>${item.cantidad}</td>
                            <td>${item.remark}</td>
                            <td>${item.location}</td>
                        </tr>
                    `;
                });
            }
            document.getElementById('detalleFolioBody').innerHTML = html;
        })
        .catch(error => {
            document.getElementById('detalleFolioBody').innerHTML = `<tr><td colspan="6" class="text-danger">Error al cargar detalles.</td></tr>`;
            console.error(error);
        });
});

// Función para mostrar el formulario de rechazo
function mostrarRechazo() {
    document.getElementById('comentariosRechazo').style.display = 'block';
    document.getElementById('btnFinalizarRechazo').style.display = 'inline-block';

    // Asegurar que el select tenga el event listener
    const select = document.getElementById('comentarioSelect');

    // Agregar el atributo required solo cuando se muestra
    select.setAttribute('required', 'required');

    // Remover event listeners previos y agregar uno nuevo
    select.removeEventListener('change', actualizarComentario);
    select.addEventListener('change', actualizarComentario);

    // Limpiar valores iniciales
    select.value = '';
    document.getElementById('comentarioInput').value = '';

    // Forzar el focus en el select para mejor UX
    setTimeout(() => {
        select.focus();
    }, 100);
}

// Función separada para actualizar el comentario
function actualizarComentario() {
    const select = document.getElementById('comentarioSelect');
    const input = document.getElementById('comentarioInput');
    input.value = select.value;
}