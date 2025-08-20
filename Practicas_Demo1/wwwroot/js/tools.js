$(document).ready(function () {
    let tabla = $('#tablaTools').DataTable({
        "processing": true,
        "serverSide": false,
        "ajax": {
            "url": "/Tool/GetTools", 
            "type": "GET",
            "dataSrc": "data", 
            "error": function (xhr, error, thrown) {
                console.error('Error AJAX:', xhr);
                let errorMsg = 'No se pudieron cargar los materiales.';
                if (xhr.status === 404) {
                    errorMsg = 'Endpoint no encontrado. Verifica la ruta.';
                } else if (xhr.status === 500) {
                    errorMsg = 'Error interno del servidor.';
                }
                Swal.fire('Error', errorMsg, 'error');
            }
        },
        "columns": [
            { "data": "id" },
            { "data": "noPart" },
            { "data": "description" },
            { "data": "category" },
            { "data": "um" },
            { "data": "purshDate" },
            {
                "data": "price",
                "render": function (data, type) {
                    return type === 'display' ? '$' + parseFloat(data || 0).toFixed(2) : data;
                }
            },
            { "data": "tcStatus" },
            { "data": "productStatus" },
            {
                "data": null,
                "orderable": false,
                "searchable": false,
                "render": function (data, type, row) {
                    return `
                                 <button class="btn btn-warning btn-sm edit-btn" data-id="${row.id}" title="Editar">
                                    <i class="bi bi-pencil-square"></i>
                        </button>
                        <button class="btn btn-danger btn-sm delete-btn" data-id="${row.id}" title="Eliminar">
                            <i class="bi bi-trash-fill"></i>
                        </button>
                        `;
                }
            }
        ],
        "language": {
            "url": "https://cdn.datatables.net/plug-ins/1.11.5/i18n/es-ES.json"
        },
        "responsive": true,
        "paging": true,
        "lengthMenu": [10, 25, 50, 100],
        "pageLength": 10,
        "order": [[0, 'asc']]
    });

    // 📌 Redirigir a la vista de edición
    $('#tablaTools tbody').on('click', '.edit-btn', function (e) {
        e.preventDefault();
        const id = $(this).data('id');
        window.location.href = `/Tool/Edit/${id}`;
    });

    // 📌 Eliminar material
    $('#tablaTools tbody').on('click', '.delete-btn', function (e) {
        e.preventDefault();
        const id = $(this).data('id');

        Swal.fire({
            title: '¿Estás seguro?',
            text: "Esta acción no se puede deshacer",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Sí, eliminar',
            cancelButtonText: 'Cancelar'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/Tool/Delete',
                    method: 'POST',
                    data: { id: id },
                    success: function () {
                        Swal.fire('¡Eliminado!', 'El material ha sido eliminado correctamente.', 'success');
                        tabla.ajax.reload(null, false);
                    },
                    error: function (xhr) {
                        console.error('Error al eliminar:', xhr);
                        let errorMsg = 'No se pudo eliminar el material.';
                        if (xhr.responseJSON?.message) {
                            errorMsg = xhr.responseJSON.message;
                        } else if (xhr.status === 500) {
                            errorMsg = 'Error interno del servidor.';
                        } else if (xhr.status === 404) {
                            errorMsg = 'Material no encontrado.';
                        }
                        Swal.fire('Error', errorMsg, 'error');
                    }
                });
            }
        });
    });
});