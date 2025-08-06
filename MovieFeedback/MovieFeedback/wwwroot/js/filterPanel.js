
function openFilterPanel() {
    document.getElementById('filterPanel').classList.remove('d-none');
}

function closeFilterPanel() {
    document.getElementById('filterPanel').classList.add('d-none');
}

function applyFilters() {
    // Просто закриваємо фільтр-панель
    closeFilterPanel();
}

function updateSearchButtonState() {
    const searchInput = document.getElementById('searchInput');
    const searchButton = document.getElementById('searchButton');
    if (searchInput.value.trim() === '') {
        searchButton.disabled = true;
    } else {
        searchButton.disabled = false;
    }
}

function submitSearch() {
    const searchInput = document.getElementById('searchInput');
    const query = searchInput.value.trim();
    if (query === '') {
        return;
    }

    const minRating = document.querySelector('input[name="minRating"]').value;
    const releaseYear = document.querySelector('input[name="releaseYear"]').value;
    const genreId = document.querySelector('select[name="genreId"]').value;
    const language = document.querySelector('select[name="language"]').value;
    const onlyFavorites = document.querySelector('input[name="onlyFavorites"]').checked;
    const onlyPopular = document.querySelector('input[name="onlyPopular"]').checked;
    const onlyRecent = document.querySelector('input[name="onlyRecent"]').checked;

    let url = `/Movie/Search?query=${encodeURIComponent(query)}`;

    if (minRating) url += `&minRating=${minRating}`;
    if (releaseYear) url += `&releaseYear=${releaseYear}`;
    if (genreId) url += `&genreId=${genreId}`;
    if (language) url += `&language=${language}`;
    if (onlyFavorites) url += `&onlyFavorites=true`;
    if (onlyPopular) url += `&onlyPopular=true`;
    if (onlyRecent) url += `&onlyRecent=true`;

    window.location.href = url;
}

document.addEventListener('DOMContentLoaded', function () {
    updateSearchButtonState();
    const searchInput = document.getElementById('searchInput');
    const searchButton = document.getElementById('searchButton');

    if (searchInput) {
        searchInput.addEventListener('input', updateSearchButtonState);

        searchInput.addEventListener('keydown', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                if (!searchButton.disabled) {
                    submitSearch();
                }
            }
        });
    }

    if (searchButton) {
        searchButton.addEventListener('click', submitSearch);
    }
});
