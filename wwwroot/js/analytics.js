$(document).ready(function () {
    // Chart 1: Sales by Category
    $.ajax({
        url: '/Analytics/GetSalesByCategory',
        type: 'GET',
        success: function (data) {
            var ctx = document.getElementById('salesByCategoryChart').getContext('2d');
            var labels = data.map(item => item.category);
            var values = data.map(item => item.count);

            new Chart(ctx, {
                type: 'doughnut',
                data: {
                    labels: labels,
                    datasets: [{
                        data: values,
                        backgroundColor: [
                            '#FF6384', '#36A2EB', '#FFCE56', '#4BC0C0', '#9966FF'
                        ]
                    }]
                }
            });
        }
    });

    // Chart 2: Revenue by Month
    $.ajax({
        url: '/Analytics/GetRevenueByMonth',
        type: 'GET',
        success: function (data) {
            var ctx = document.getElementById('revenueByMonthChart').getContext('2d');
            var labels = data.map(item => item.month);
            var values = data.map(item => item.revenue);

            new Chart(ctx, {
                type: 'line',
                data: {
                    labels: labels,
                    datasets: [{
                        label: 'Revenue ($)',
                        data: values,
                        borderColor: '#36A2EB',
                        fill: false,
                        tension: 0.1
                    }]
                },
                options: {
                    scales: {
                        y: {
                            beginAtZero: true
                        }
                    }
                }
            });
        }
    });

    // Table: Top 5 Events
    $.ajax({
        url: '/Analytics/GetTopEvents',
        type: 'GET',
        success: function (data) {
            var tbody = $('#topEventsTable tbody');
            tbody.empty();

            data.forEach(function (item) {
                var row = `<tr>
                    <td>${item.title}</td>
                    <td>${item.ticketsSold}</td>
                    <td>$${item.revenue.toFixed(2)}</td>
                </tr>`;
                tbody.append(row);
            });
        }
    });
});
