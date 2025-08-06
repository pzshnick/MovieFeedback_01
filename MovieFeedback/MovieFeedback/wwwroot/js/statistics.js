// === Global Variables ===
let currentChart;
let currentChartType = 'ratings'; // default selected chart type

// === Helpers ===
function showNoDataMessage(message = "Please select a date range to display statistics.") {
    const messageDiv = document.getElementById('noDataMessage');
    messageDiv.innerText = message;
    messageDiv.style.display = 'block';
    document.getElementById('statisticsChart').style.display = 'none';
}
function hideNoDataMessage() {
    document.getElementById('noDataMessage').style.display = 'none';
    document.getElementById('statisticsChart').style.display = 'block';
}

function resetChart() {
    if (currentChart) {
        currentChart.destroy();
        currentChart = null;
    }
    showNoDataMessage();
}

// === Fetchers ===
async function fetchData(endpoint, fromDate, toDate) {
    const url = `${endpoint}?fromDate=${fromDate}&toDate=${toDate}`;
    const response = await fetch(url);
    if (!response.ok) {
        console.error(`Failed to fetch data from ${endpoint}.`);
        return null;
    }
    return await response.json();
}

// === Builders ===
function buildLineChart(labels, data, label) {
    const ctx = document.getElementById('statisticsChart').getContext('2d');
    currentChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: label,
                data: data,
                backgroundColor: 'rgba(138, 43, 226, 0.3)',
                borderColor: '#8e44ad',
                borderWidth: 2,
                tension: 0.3,
                pointRadius: 4,
                pointBackgroundColor: '#8e44ad'
            }]
        },
        options: defaultOptions()
    });
}

function buildBarChart(labels, data, label) {
    const ctx = document.getElementById('statisticsChart').getContext('2d');
    currentChart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: label,
                data: data,
                backgroundColor: '#8e44ad',
                borderColor: '#8e44ad',
                borderWidth: 1
            }]
        },
        options: defaultOptions()
    });
}

function buildRatingsWithAverageChart(dates, ratings, avgRating) {
    const ctx = document.getElementById('statisticsChart').getContext('2d');
    currentChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: dates,
            datasets: [
                {
                    label: 'Ratings History',
                    data: ratings,
                    backgroundColor: 'rgba(138, 43, 226, 0.4)',
                    borderColor: '#8e44ad',
                    borderWidth: 2,
                    tension: 0.3,
                    pointRadius: 4,
                    pointBackgroundColor: '#8e44ad',
                    fill: false
                },
                {
                    label: 'Average Rating',
                    data: new Array(dates.length).fill(avgRating),
                    borderColor: '#e0e0e0',
                    borderWidth: 2,
                    borderDash: [10, 5], // Пунктирна лінія
                    pointRadius: 0,
                    fill: false
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                x: {
                    ticks: { color: '#e0e0e0', font: { size: 12 } },
                    grid: { color: 'rgba(255,255,255,0.05)' }
                },
                y: {
                    min: 0,
                    max: 10,
                    ticks: { color: '#e0e0e0', font: { size: 12 } },
                    grid: { color: 'rgba(255,255,255,0.05)' }
                }
            },
            plugins: {
                legend: { labels: { color: '#e0e0e0' } }
            }
        }
    });
}

function defaultOptions() {
    return {
        responsive: true,
        maintainAspectRatio: false,
        scales: {
            x: {
                ticks: { color: '#e0e0e0', font: { size: 12 } },
                grid: { color: 'rgba(255,255,255,0.05)' }
            },
            y: {
                ticks: { color: '#e0e0e0', font: { size: 12 } },
                grid: { color: 'rgba(255,255,255,0.05)' }
            }
        },
        plugins: {
            legend: { labels: { color: '#e0e0e0' } }
        }
    };
}

// === Loaders for each chart ===
async function applyFilters() {
    const fromDate = document.getElementById('fromDate').value;
    const toDate = document.getElementById('toDate').value;

    if (!fromDate || !toDate) {
        showNoDataMessage('Please enter both dates.');
        resetChartWithoutMessage();
        return;
    }

    if (new Date(fromDate) > new Date(toDate)) {
        showNoDataMessage('The "From" date cannot be after the "To" date.');
        resetChartWithoutMessage();
        return;
    }

    hideNoDataMessage();

    if (currentChartType === 'ratings') {
        const data = await fetchData('/Stats/GetRatingsData', fromDate, toDate);
        if (!data || data.length === 0) {
            resetChart();
            return;
        }
        const labels = data.map(d => d.date.split('T')[0]);
        const ratings = data.map(d => d.averageRating);
        hideNoDataMessage();
        buildLineChart(labels, ratings, 'Ratings');
    }
    else if (currentChartType === 'favorites') {
        const data = await fetchData('/Stats/GetFavoritesData', fromDate, toDate);
        if (!data || data.length === 0) {
            resetChart();
            return;
        }
        const labels = data.map(d => d.date.split('T')[0]);
        const counts = data.map(d => d.count);
        hideNoDataMessage();
        buildBarChart(labels, counts, 'Favorites Activity');
    }
    else if (currentChartType === 'activity') {
        const data = await fetchData('/Stats/GetActivityData', fromDate, toDate);
        if (!data || data.length === 0) {
            resetChart();
            return;
        }
        const labels = data.map(d => d.date.split('T')[0].substring(0, 7));
        const counts = data.map(d => d.count);
        hideNoDataMessage();
        buildBarChart(labels, counts, 'Activity');
    }
    else if (currentChartType === 'average') {
        const data = await fetchData('/Stats/GetRatingsData', fromDate, toDate); // беремо оцінки теж
        if (!data || data.length === 0) {
            resetChart();
            return;
        }
        const labels = data.map(d => d.date.split('T')[0]);
        const ratings = data.map(d => d.averageRating);
        const avgRating = ratings.reduce((sum, r) => sum + r, 0) / ratings.length;

        hideNoDataMessage();
        buildRatingsWithAverageChart(labels, ratings, avgRating);
    }
}

// === Chart switching (menu) ===
function switchChart(type) {
    currentChartType = type;
    document.getElementById('chartTitle').innerText = {
        ratings: "Ratings History",
        favorites: "Favorite Movies",
        activity: "Activity Level",
        average: "Average Rating"
    }[type];

    // Знімаємо клас active з усіх кнопок
    const buttons = document.querySelectorAll('.list-group-item');
    buttons.forEach(btn => btn.classList.remove('active'));

    // Додаємо клас active до натиснутої кнопки
    const clickedButton = {
        ratings: buttons[0],
        favorites: buttons[1],
        activity: buttons[2],
        average: buttons[3]
    }[type];

    clickedButton.classList.add('active');

    resetChart();

    // Очищати дати
    document.getElementById('fromDate').value = '';
    document.getElementById('toDate').value = '';
}

// === INIT ===
window.onload = function () {
    showNoDataMessage();
};
