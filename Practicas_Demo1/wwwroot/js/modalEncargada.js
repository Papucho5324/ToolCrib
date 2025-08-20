document.addEventListener('DOMContentLoaded', function () {
    const folioModal = document.getElementById('folioModal');
    if (folioModal) {

        // Función para obtener la clase de color del badge según el estado
        function getStatusColor(status) {
            switch (status) {
                case 'PENDIENTE':
                    return 'bg-warning text-dark';
                case 'APROBADO':
                    return 'bg-success';
                case 'RECHAZADO':
                    return 'bg-danger';
                default:
                    return 'bg-secondary';
            }
        }

        // Evento que se ejecuta cuando el modal se muestra
        folioModal.addEventListener('show.bs.modal', function (event) {
            const button = event.relatedTarget;
            const folio = button.getAttribute('data-folio');
            const status = (button.getAttribute('data-status') || '').trim().toUpperCase();

            // Obtén la clase de color dinámicamente
            const statusColorClass = getStatusColor(status);

            const folioInput = document.getElementById('folioInput');
            const estadoInfo = document.getElementById('estadoInfo');
            if (folioInput) folioInput.value = folio;
            if (estadoInfo) estadoInfo.innerHTML = `Estado actual: <span class="badge ${statusColorClass}">${status}</span>`;

            const botonesDecision = document.getElementById('botonesDecision');
            const btnFinalizarRechazo = document.getElementById('btnFinalizarRechazo');
            const btnCerrarModal = document.getElementById('btnCerrarModal');
            const btnRechazarInicial = document.querySelector('button[onclick="mostrarRechazo()"]');
            const aprobarBtn = document.querySelector('button[name="Estado"][value="APROBADO"]');

            if (botonesDecision && btnCerrarModal) {
                // Lógica de visibilidad ajustada para el rol de encargada
                // Permite procesar folios PENDIENTES o APROBADOS
                if (status === "PENDIENTE" || status === "APROBADO") {
                    botonesDecision.style.display = 'flex';
                    btnCerrarModal.style.display = 'none';
                    if (btnFinalizarRechazo) btnFinalizarRechazo.style.display = 'none';
                    if (btnRechazarInicial) btnRechazarInicial.style.display = 'inline-block';
                    if (aprobarBtn) aprobarBtn.style.display = 'inline-block';
                } else if (status === "RECHAZADO") {
                    // Si el folio está rechazado, ya no se puede modificar
                    botonesDecision.style.display = 'none';
                    btnCerrarModal.style.display = 'inline-block';
                    Swal.fire({
                        icon: 'info',
                        title: 'Folio no editable',
                        text: `Este folio fue ${status.toLowerCase()}, por lo que no puede ser editado.`,
                        confirmButtonText: 'Entendido'
                    });
                }
            }

            // Lógica para cargar los detalles del folio
            fetch(`/FolioStatus/DetallesFolio?folio=${folio}`)
                .then(response => {
                    if (!response.ok) {
                        throw new Error('Network response was not ok');
                    }
                    return response.json();
                })
                .then(data => {
                    let html = "";
                    const detalleFolioBody = document.getElementById('detalleFolioBody');
                    if (!data || data.length === 0) {
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
                                </tr>`;
                        });
                    }
                    if (detalleFolioBody) detalleFolioBody.innerHTML = html;
                })
                .catch(error => {
                    console.error('Error al cargar los detalles:', error);
                    const detalleFolioBody = document.getElementById('detalleFolioBody');
                    if (detalleFolioBody) detalleFolioBody.innerHTML = `<tr><td colspan="6" class="text-danger">Error al cargar detalles.</td></tr>`;
                });
        });

        // Evento que se ejecuta cuando el modal se oculta
        folioModal.addEventListener('hidden.bs.modal', function () {
            const detalleFolioBody = document.getElementById('detalleFolioBody');
            const comentariosRechazo = document.getElementById('comentariosRechazo');
            const comentarioSelect = document.getElementById('comentarioSelect');
            const comentarioInput = document.getElementById('comentarioInput');

            if (detalleFolioBody) detalleFolioBody.innerHTML = '';
            if (comentariosRechazo) comentariosRechazo.style.display = 'none';
            if (comentarioSelect) comentarioSelect.value = '';
            if (comentarioInput) comentarioInput.value = '';
            if (comentarioSelect) comentarioSelect.removeAttribute('required');
        });
    }

    // --- Lógica de Manejo de la Decisión del Formulario ---
    const decisionForm = document.getElementById('decisionForm');
    if (decisionForm) {
        decisionForm.addEventListener('submit', function (e) {
            const btn = document.activeElement;
            const isRechazo = btn && btn.name === "Estado" && btn.value === "RECHAZADO";
            const isAprobacion = btn && btn.name === "Estado" && btn.value === "APROBADO";

            if (isRechazo) {
                const motivoSelect = document.getElementById('comentarioSelect').value;
                if (!motivoSelect || motivoSelect.trim() === "") {
                    e.preventDefault();
                    Swal.fire({
                        icon: 'warning',
                        title: 'Motivo requerido',
                        text: 'Debes seleccionar un motivo para rechazar el folio.'
                    });
                    return;
                }
            }

            if (isAprobacion) {
                e.preventDefault();

                const form = this; // Captura el formulario explícitamente

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
                        form.submit(); // ✅ Esto sí funcionará correctamente
                    }
                });
            }

        });
    }

    // Función para mostrar los campos de rechazo
    window.mostrarRechazo = function () {
        const comentariosDiv = document.getElementById('comentariosRechazo');
        const select = document.getElementById('comentarioSelect');
        const btnFinalizarRechazo = document.getElementById('btnFinalizarRechazo');
        const btnRechazarInicial = document.querySelector('button[onclick="mostrarRechazo()"]');
        const aprobarBtn = document.querySelector('button[name="Estado"][value="APROBADO"]');

        if (comentariosDiv) comentariosDiv.style.display = 'block';
        if (select) select.setAttribute('required', 'required');
        if (btnFinalizarRechazo) btnFinalizarRechazo.style.display = 'inline-block';
        if (btnRechazarInicial) btnRechazarInicial.style.display = 'none';
        if (aprobarBtn) aprobarBtn.style.display = 'none';

        if (select) {
            select.addEventListener('change', function () {
                document.getElementById('comentarioInput').value = this.value;
            });
        }
    };
});