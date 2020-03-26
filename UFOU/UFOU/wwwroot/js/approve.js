function add_favorite(report_id) {
    console.log("adding favorite");
    Swal.fire({
        title: 'Do you want to add this report to your favorites list?',
        icon: 'info',
        showCancelButton: true,
        confirmButtonColor: '#663399',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, add to favorites!'
    }).then((result) => {
        if (result.value) {
            $.ajax({
                method: 'POST',
                url: '/Favorite/Create',
                data:
                {
                    reportID: report_id,
                }
            }).done(function () {
                Swal.fire({
                    type: 'success',
                    title: 'Report has been added to favorites!',
                    showConfirmButton: true,
                })
            }).fail(function () {
                Swal.fire({
                    type: 'error',
                    title: 'Oops...',
                    text: 'Unable to add report to favorites',
                    showConfirmButton: true,
                })
            });
        }
    })
}