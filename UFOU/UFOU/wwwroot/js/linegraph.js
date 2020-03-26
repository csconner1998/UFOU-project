var data = {
    x: dateinput.value.split(" "),
    y: datenumber.value.split(" "),
    type: 'scatter',
    line: {
        color: 'rgb(66, 33, 99)',
        width: 4
        }
};
var layout = {
    title: 'Sightings per year',
    xaxis: {
        title: 'Year',
        type:"year",
        range: [lowest.value, highest.value],
        autorange: false
    },
    yaxis: {
        title: 'Number Of Sightings',
        autorange: true
    }
};



Plotly.newPlot('myDiv', [data], layout);


function add_favorite(e, report_id) {
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
            //e.preventDefault(),
            $.ajax({
                url: '/Favorite/AddFavorite',
                data:
                {
                    reportID: report_id,
                },
                method: 'POST'//e.srcElement.method
            }).done(function () {
                Swal.fire({
                    type: 'success',
                    title: 'Report has been added to favorites!',
                    showConfirmButton: false,
                    timer: 1500
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