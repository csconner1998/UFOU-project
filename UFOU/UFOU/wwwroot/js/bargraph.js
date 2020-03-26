var data = [
    {
        x: shapes.value.split(' '),
        y: values.value.split(' '),
        type: 'bar',
        marker: {
            color: 'rgb(66,33,99)',
            opacity: 0.75,
        }
    }
];

var layout =
{
    yaxis: {
        title: 'Number of Sightings'
    },
    xaxis: {
        title: 'Shapes'
    }
}

Plotly.newPlot('myDiv', data, layout);