const state = {
    auth: JSON.parse(localStorage.getItem('hotelReservationAuth') || 'null'),
    amenities: [],
    ownerAccommodations: [],
    ownerManagement: null,
    ownerSelectedUnitId: null,
    lastBlockView: null,
    adminAccommodations: []
};

const roleNames = { 1: 'Customer', 2: 'Owner', 3: 'Admin' };
const accommodationTypes = { 1: 'Hotel', 2: 'Apartman' };
const approvalStatuses = { 1: 'Draft', 2: 'Pending', 3: 'Approved', 4: 'Rejected', 5: 'Suspended' };
const cancellationPolicies = { 1: 'Flexible', 2: 'Moderate', 3: 'Strict' };
const reservationStatuses = { 1: 'Confirmed', 2: 'Cancelled', 3: 'Completed' };
const amenityNames = {
    1: 'Wifi', 2: 'Parking', 3: 'Air conditioning', 4: 'Heating', 5: 'Pool',
    6: 'Breakfast', 7: 'Kitchen', 8: 'Balcony', 9: 'Sea view', 10: 'Bathtub',
    11: 'Washing machine', 12: 'Television', 13: 'Hair dryer', 14: 'Minibar',
    15: 'Safe', 16: 'Elevator', 17: 'Gym', 18: 'Spa', 19: 'Pet friendly',
    20: 'Wheelchair accessible'
};

function escapeHtml(value) {
    return String(value ?? '')
        .replaceAll('&', '&amp;')
        .replaceAll('<', '&lt;')
        .replaceAll('>', '&gt;')
        .replaceAll('"', '&quot;')
        .replaceAll("'", '&#039;');
}

function showMessage(text, type = 'success') {
    const box = document.getElementById('message');
    box.textContent = text;
    box.className = `message ${type}`;
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

function clearMessage() {
    const box = document.getElementById('message');
    box.textContent = '';
    box.className = 'message hidden';
}

async function apiFetch(url, options = {}) {
    const config = { method: 'GET', ...options };
    config.headers = { ...(options.headers || {}) };

    if (state.auth?.token) {
        config.headers.Authorization = `Bearer ${state.auth.token}`;
    }

    if (config.body && !(config.body instanceof FormData) && typeof config.body !== 'string') {
        config.headers['Content-Type'] = 'application/json';
        config.body = JSON.stringify(config.body);
    }

    const response = await fetch(url, config);

    if (response.status === 204) {
        if (!response.ok) throw new Error(`HTTP ${response.status}`);
        return null;
    }

    const contentType = response.headers.get('content-type') || '';
    const payload = contentType.includes('application/json')
        ? await response.json()
        : await response.text();

    if (!response.ok) {
        const message = typeof payload === 'string'
            ? payload
            : payload?.message || payload?.title || `HTTP ${response.status}`;
        throw new Error(message);
    }

    return payload;
}

function saveAuth(auth) {
    state.auth = auth;
    state.ownerAccommodations = [];
    state.ownerManagement = null;
    state.ownerSelectedUnitId = null;
    state.lastBlockView = null;
    state.adminAccommodations = [];

    if (auth) {
        localStorage.setItem('hotelReservationAuth', JSON.stringify(auth));
    } else {
        localStorage.removeItem('hotelReservationAuth');
    }

    updateSessionUi();
}

function updateSessionUi() {
    const box = document.getElementById('sessionBox');
    const authOnly = document.querySelectorAll('.auth-only');
    const ownerOnly = document.querySelectorAll('.owner-only');
    const adminOnly = document.querySelectorAll('.admin-only');

    authOnly.forEach(x => x.classList.toggle('hidden', !state.auth || state.auth.role === 3));
    ownerOnly.forEach(x => x.classList.toggle('hidden', state.auth?.role !== 2));
    adminOnly.forEach(x => x.classList.toggle('hidden', state.auth?.role !== 3));

    if (!state.auth) {
        box.innerHTML = '<strong>Niste prijavljeni</strong>';
        return;
    }

    box.innerHTML = `
        <strong>${escapeHtml(state.auth.firstName)} ${escapeHtml(state.auth.lastName)}</strong><br>
        ${escapeHtml(state.auth.email)} · ${roleNames[state.auth.role] || state.auth.role}<br>
        <button id="logoutButton">Odjava</button>
    `;

    document.getElementById('logoutButton').addEventListener('click', () => {
        saveAuth(null);
        showMessage('Odjavljeni ste.');
        showSection('searchSection');
    });
}

function showSection(id) {
    document.querySelectorAll('.panel').forEach(panel => panel.classList.remove('active-section'));
    document.getElementById(id).classList.add('active-section');
    clearMessage();

    if (id === 'myReservationsSection' && state.auth && state.auth.role !== 3) {
        loadMyReservations();
    }

    if (id === 'ownerSection' && state.auth?.role === 2) {
        loadOwnerAccommodations();
        loadOwnerReservations();
    }

    if (id === 'adminSection' && state.auth?.role === 3) {
        loadPending();
        loadAdminAccommodations();
        loadAccounts();
        loadAdminReservations();
    }
}

document.querySelectorAll('nav [data-section]').forEach(button => {
    button.addEventListener('click', () => showSection(button.dataset.section));
});

function setDefaultDates() {
    const checkIn = new Date();
    checkIn.setDate(checkIn.getDate() + 30);
    const checkOut = new Date(checkIn);
    checkOut.setDate(checkOut.getDate() + 3);

    const toDateInput = date => {
        const y = date.getFullYear();
        const m = String(date.getMonth() + 1).padStart(2, '0');
        const d = String(date.getDate()).padStart(2, '0');
        return `${y}-${m}-${d}`;
    };

    document.querySelector('#searchForm [name="checkIn"]').value = toDateInput(checkIn);
    document.querySelector('#searchForm [name="checkOut"]').value = toDateInput(checkOut);
}

function renderAmenityCheckboxes(containerId, selectedIds = []) {
    const selected = new Set((selectedIds || []).map(Number));
    document.getElementById(containerId).innerHTML = state.amenities.map(a => `
        <label>
            <input type="checkbox" value="${a.id}" ${selected.has(a.id) ? 'checked' : ''}>
            ${escapeHtml(amenityNames[a.type] || a.type)}
        </label>
    `).join('');
}

function checkedAmenityIds(containerId) {
    return [...document.querySelectorAll(`#${containerId} input[type="checkbox"]:checked`)]
        .map(x => Number(x.value));
}

function normalizePhotoPath(path) {
    const value = String(path || '').trim();
    if (!value) return '';

    if (/^(https?:)?\/\//i.test(value) || value.startsWith('data:') || value.startsWith('blob:')) {
        return value;
    }

    return value.startsWith('/') ? value : `/${value.replace(/^\.?\//, '')}`;
}

function handlePhotoError(img) {
    const item = img.closest('.photo-item');
    if (!item) return;

    const link = img.closest('.photo-link');
    if (link) link.classList.add('hidden');

    const fallback = item.querySelector('.photo-missing');
    if (fallback) fallback.classList.remove('hidden');
}

function renderPhotoGallery(photos, altText = 'Fotografija', options = {}) {
    const items = Array.isArray(photos) ? photos : [];
    if (!items.length) {
        return '<div class="empty-box">Nema fotografija.</div>';
    }

    const unitId = options.unitId || null;
    const editableUnit = options.editableUnit === true;
    const editableAccommodation = options.editableAccommodation === true;

    return `<div class="photo-gallery">${items.map((photo, index) => {
        const src = normalizePhotoPath(photo.path);

        return `
        <figure class="photo-item">
            ${photo.isPrimary ? '<span class="photo-badge">Primarna</span>' : ''}
            ${src ? `
                <a class="photo-link" href="${escapeHtml(src)}" target="_blank" rel="noopener" title="Otvori sliku u punoj veličini">
                    <img src="${escapeHtml(src)}" alt="${escapeHtml(altText)} ${index + 1}" loading="lazy" onerror="handlePhotoError(this)">
                </a>` : ''}
            <div class="photo-missing ${src ? 'hidden' : ''}">
                Slika nije pronađena na disku.<br>
                ${editableUnit || editableAccommodation ? 'Učitaj je ponovno.' : ''}
            </div>
            <figcaption>Slika ${index + 1}${photo.isPrimary ? ' · primarna' : ''}</figcaption>
            ${(editableUnit || editableAccommodation) ? `
                <div class="photo-actions">
                    ${src ? `<a class="small-link" href="${escapeHtml(src)}" target="_blank" rel="noopener">Otvori</a>` : ''}
                    ${editableUnit ? `
                        ${photo.isPrimary ? '' : `<button class="small" data-owner-unit-photo-action="primary" data-unit-id="${unitId}" data-photo-id="${photo.id}">Postavi primarnu</button>`}
                        <button class="small danger" data-owner-unit-photo-action="delete" data-unit-id="${unitId}" data-photo-id="${photo.id}">Obriši</button>
                    ` : ''}
                    ${editableAccommodation ? `
                        ${photo.isPrimary ? '' : `<button class="small" data-owner-photo-action="primary" data-photo-id="${photo.id}">Postavi primarnu</button>`}
                        <button class="small danger" data-owner-photo-action="delete" data-photo-id="${photo.id}">Obriši</button>
                    ` : ''}
                </div>` : ''}
        </figure>`;
    }).join('')}</div>`;
}

async function loadAmenities() {
    try {
        state.amenities = await apiFetch('/api/amenities');

        document.getElementById('searchAmenities').innerHTML = state.amenities.map(a => `
            <label>
                <input type="checkbox" name="amenityId" value="${a.id}">
                ${escapeHtml(amenityNames[a.type] || a.type)}
            </label>
        `).join('');

        renderAmenityCheckboxes('ownerAccommodationAmenities');
        renderAmenityCheckboxes('ownerUnitAmenities');
    } catch (error) {
        showMessage(`Ne mogu učitati amenities: ${error.message}`, 'error');
    }
}

// AUTH

document.getElementById('loginForm').addEventListener('submit', async event => {
    event.preventDefault();
    const data = new FormData(event.currentTarget);

    try {
        const result = await apiFetch('/api/Auth/login', {
            method: 'POST',
            body: {
                email: data.get('email'),
                password: data.get('password')
            }
        });
        saveAuth(result);
        showMessage(`Prijavljeni ste kao ${roleNames[result.role]}.`);
        showSection('searchSection');
    } catch (error) {
        showMessage(error.message, 'error');
    }
});

document.getElementById('registerForm').addEventListener('submit', async event => {
    event.preventDefault();
    const data = new FormData(event.currentTarget);

    try {
        const result = await apiFetch('/api/Auth/register', {
            method: 'POST',
            body: {
                firstName: data.get('firstName'),
                lastName: data.get('lastName'),
                email: data.get('email'),
                password: data.get('password'),
                role: Number(data.get('role'))
            }
        });
        saveAuth(result);
        showMessage('Registracija je uspjela i korisnik je prijavljen.');
        showSection('searchSection');
    } catch (error) {
        showMessage(error.message, 'error');
    }
});

// SEARCH

async function performSearch() {
    const form = document.getElementById('searchForm');
    const data = new FormData(form);
    const params = new URLSearchParams();

    params.set('City', data.get('city'));
    params.set('Country', data.get('country'));
    params.set('CheckIn', data.get('checkIn'));
    params.set('CheckOut', data.get('checkOut'));
    params.set('GuestCount', data.get('guestCount'));
    params.set('Sort', data.get('sort'));

    if (data.get('minPrice')) params.set('MinPrice', data.get('minPrice'));
    if (data.get('maxPrice')) params.set('MaxPrice', data.get('maxPrice'));
    if (data.get('type')) params.set('Type', data.get('type'));
    if (data.get('nameSearch')) params.set('NameSearch', data.get('nameSearch'));

    form.querySelectorAll('input[name="amenityId"]:checked')
        .forEach(input => params.append('AmenityIds', input.value));

    const results = await apiFetch(`/api/accommodations/search?${params}`);
    renderSearchResults(results);
}

document.getElementById('searchForm').addEventListener('submit', async event => {
    event.preventDefault();

    try {
        await performSearch();
    } catch (error) {
        showMessage(error.message, 'error');
    }
});

function renderSearchResults(results) {
    const container = document.getElementById('searchResults');

    if (!results.length) {
        container.innerHTML = '<div class="empty-box">Nema dostupnih smještaja za zadane filtere.</div>';
        return;
    }

    container.innerHTML = results.map(a => {
        const accommodationPhotos = Array.isArray(a.photos) && a.photos.length
            ? a.photos
            : a.primaryPhotoPath
                ? [{ path: a.primaryPhotoPath, isPrimary: true }]
                : [];

        return `
        <article class="card">
            <div class="unit-header">
                <div>
                    <h3>${escapeHtml(a.name)}</h3>
                    <div class="meta">
                        ${escapeHtml(a.city)}, ${escapeHtml(a.country)} · ${accommodationTypes[a.type] || a.type}<br>
                        Od ${a.fromPricePerNight} € / noć · od ${a.fromTotalPrice} € ukupno
                    </div>
                </div>
                <span class="badge">${accommodationPhotos.length} fotografija</span>
            </div>
            <p>${escapeHtml(a.description)}</p>

            <h4>Fotografije smještaja</h4>
            ${renderPhotoGallery(accommodationPhotos, a.name)}

            <h4>Dostupne jedinice</h4>
            ${a.availableUnits.map(u => `
                <div class="card unit-card">
                    <div class="unit-header">
                        <strong>${escapeHtml(u.name)}</strong>
                        <span class="badge">${u.pricePerNight} €/noć</span>
                    </div>
                    <div class="meta">
                        Kapacitet: ${u.capacity} · ${u.totalPrice} € ukupno ·
                        Otkazivanje: ${cancellationPolicies[u.cancellationPolicy] || u.cancellationPolicy}<br>
                        Sadržaji: ${u.amenityIds.map(id => amenityNames[id] || id).join(', ') || 'nema'}
                    </div>
                    <h4>Fotografije sobe</h4>
                    ${renderPhotoGallery(u.photos || [], u.name)}
                    ${state.auth && state.auth.role !== 3
                        ? `<div class="card-actions"><button class="primary" data-search-action="reserve" data-unit-id="${u.id}">Rezerviraj</button></div>`
                        : ''}
                </div>
            `).join('')}
            <div class="card-actions">
                <button data-search-action="reviews" data-accommodation-id="${a.id}">Prikaži recenzije</button>
            </div>
            <div id="reviews-${a.id}"></div>
        </article>`;
    }).join('');
}

document.getElementById('searchResults').addEventListener('click', async event => {
    const button = event.target.closest('button[data-search-action]');
    if (!button) return;

    try {
        if (button.dataset.searchAction === 'reserve') {
            if (!state.auth) throw new Error('Prvo se prijavite.');
            const form = document.getElementById('searchForm');
            const data = new FormData(form);
            const result = await apiFetch('/api/reservations', {
                method: 'POST',
                body: {
                    accommodationUnitId: Number(button.dataset.unitId),
                    checkInDate: data.get('checkIn'),
                    checkOutDate: data.get('checkOut'),
                    guestCount: Number(data.get('guestCount'))
                }
            });
            showMessage(`Rezervacija je kreirana. Ukupno: ${result.totalPrice} €.`);
            await performSearch();
        }

        if (button.dataset.searchAction === 'reviews') {
            await loadReviews(Number(button.dataset.accommodationId));
        }
    } catch (error) {
        showMessage(error.message, 'error');
    }
});

async function loadReviews(accommodationId) {
    const target = document.getElementById(`reviews-${accommodationId}`);
    const data = await apiFetch(`/api/accommodations/${accommodationId}/reviews`);

    target.innerHTML = `
        <h4>Recenzije (${data.reviewCount})</h4>
        <p>Prosjek: ${data.averageRating == null ? 'No reviews yet' : Number(data.averageRating).toFixed(2)}</p>
        ${data.reviews.length ? data.reviews.map(r => `
            <div class="card">
                <strong>${escapeHtml(r.reviewerName)}</strong> · ${r.rating}/5<br>
                <span>${escapeHtml(r.comment || '')}</span>
                ${state.auth?.role === 3 ? `<div class="card-actions"><button class="danger" data-review-delete-id="${r.id}" data-accommodation-id="${accommodationId}">Obriši recenziju</button></div>` : ''}
            </div>
        `).join('') : '<p>Nema recenzija.</p>'}
    `;
}

document.getElementById('searchResults').addEventListener('click', async event => {
    const button = event.target.closest('button[data-review-delete-id]');
    if (!button) return;

    try {
        await apiFetch(`/api/admin/reviews/${button.dataset.reviewDeleteId}`, { method: 'DELETE' });
        showMessage('Recenzija je obrisana.');
        await loadReviews(Number(button.dataset.accommodationId));
    } catch (error) {
        showMessage(error.message, 'error');
    }
});

// MY RESERVATIONS + REVIEWS

document.getElementById('loadMyReservations').addEventListener('click', loadMyReservations);

async function loadMyReservations() {
    try {
        const reservations = await apiFetch('/api/reservations/my');
        const rendered = [];

        for (const r of reservations) {
            let ownReview = null;
            if (r.status === 3) {
                try {
                    const reviews = await apiFetch(`/api/accommodations/${r.accommodationId}/reviews`);
                    ownReview = reviews.reviews.find(x => x.reservationId === r.id && x.accountId === state.auth?.id) || null;
                } catch { }
            }
            rendered.push(renderMyReservation(r, ownReview));
        }

        document.getElementById('myReservations').innerHTML = rendered.join('') || '<p>Nema rezervacija.</p>';
    } catch (error) {
        showMessage(error.message, 'error');
    }
}

function renderMyReservation(r, ownReview) {
    const reviewArea = r.status === 3
        ? ownReview
            ? `<div class="card-actions">
                    <span>${ownReview.rating}/5 — ${escapeHtml(ownReview.comment || '')}</span>
                    <button data-my-action="update-review" data-review-id="${ownReview.id}">Uredi review</button>
                    <button class="danger" data-my-action="delete-review" data-review-id="${ownReview.id}">Obriši review</button>
               </div>`
            : `<div class="card-actions"><button data-my-action="create-review" data-reservation-id="${r.id}">Dodaj review</button></div>`
        : '';

    return `
        <article class="card">
            <strong>${escapeHtml(r.accommodationName)} / ${escapeHtml(r.unitName)}</strong>
            <div class="meta">
                ${r.checkInDate} → ${r.checkOutDate} · ${r.guestCount} gostiju · ${r.totalPrice} €<br>
                Status: <span class="status">${reservationStatuses[r.status] || r.status}</span> · politika: ${cancellationPolicies[r.cancellationPolicyAtBooking] || r.cancellationPolicyAtBooking}
            </div>
            ${r.status === 1 ? `<div class="card-actions"><button class="danger" data-my-action="cancel" data-reservation-id="${r.id}">Otkaži</button></div>` : ''}
            ${reviewArea}
        </article>
    `;
}

document.getElementById('myReservations').addEventListener('click', async event => {
    const button = event.target.closest('button[data-my-action]');
    if (!button) return;

    try {
        const action = button.dataset.myAction;

        if (action === 'cancel') {
            if (!confirm('Otkazati rezervaciju?')) return;
            await apiFetch(`/api/reservations/${button.dataset.reservationId}/cancel`, { method: 'POST' });
            showMessage('Rezervacija je otkazana.');
        }

        if (action === 'create-review') {
            const rating = Number(prompt('Ocjena 1-5:', '5'));
            if (!Number.isInteger(rating) || rating < 1 || rating > 5) throw new Error('Ocjena mora biti od 1 do 5.');
            const comment = prompt('Komentar (opcionalno):', '') ?? '';
            await apiFetch('/api/reviews', {
                method: 'POST',
                body: { reservationId: Number(button.dataset.reservationId), rating, comment }
            });
            showMessage('Review je dodan.');
        }

        if (action === 'update-review') {
            const rating = Number(prompt('Nova ocjena 1-5:', '5'));
            if (!Number.isInteger(rating) || rating < 1 || rating > 5) throw new Error('Ocjena mora biti od 1 do 5.');
            const comment = prompt('Novi komentar:', '') ?? '';
            await apiFetch(`/api/reviews/${button.dataset.reviewId}`, {
                method: 'PUT',
                body: { rating, comment }
            });
            showMessage('Review je ažuriran.');
        }

        if (action === 'delete-review') {
            if (!confirm('Obrisati review?')) return;
            await apiFetch(`/api/reviews/${button.dataset.reviewId}`, { method: 'DELETE' });
            showMessage('Review je obrisan.');
        }

        await loadMyReservations();
    } catch (error) {
        showMessage(error.message, 'error');
    }
});

// OWNER

function selectedAccommodationId() {
    const id = Number(document.getElementById('ownerAccommodationSelect').value);
    if (!id) throw new Error('Prvo odaberi smještaj po nazivu.');
    return id;
}

function selectedUnitId(selectId = 'ownerUnitSelect') {
    const id = Number(document.getElementById(selectId).value);
    if (!id) throw new Error('Prvo odaberi sobu po nazivu.');
    return id;
}

document.getElementById('loadOwnerAccommodations').addEventListener('click', loadOwnerAccommodations);

document.getElementById('ownerAccommodationSelect').addEventListener('change', () => {
    state.ownerSelectedUnitId = null;
    state.lastBlockView = null;
    loadOwnerManagement();
});

async function loadOwnerAccommodations() {
    try {
        const previous = Number(document.getElementById('ownerAccommodationSelect').value) || null;
        const items = await apiFetch('/api/Accommodations/my');
        state.ownerAccommodations = items;

        document.getElementById('ownerAccommodations').innerHTML = items.map(a => `
            <article class="card">
                <strong>${escapeHtml(a.name)}</strong>
                <div class="meta">
                    ${escapeHtml(a.address)}, ${escapeHtml(a.city)}, ${escapeHtml(a.country)}<br>
                    ${accommodationTypes[a.type] || a.type} · ${approvalStatuses[a.approvalStatus] || a.approvalStatus} · Active: ${a.isActive ? 'DA' : 'NE'}
                    ${a.rejectionReason ? `<br>Razlog odbijanja: ${escapeHtml(a.rejectionReason)}` : ''}
                </div>
                <p>${escapeHtml(a.description)}</p>
                <div class="card-actions">
                    <button data-owner-accommodation-action="select" data-id="${a.id}">Upravljaj</button>
                    <button data-owner-accommodation-action="update" data-id="${a.id}">Uredi naziv/opis</button>
                    <button data-owner-accommodation-action="submit" data-id="${a.id}">Submit</button>
                    <button data-owner-accommodation-action="activate" data-id="${a.id}">Aktiviraj</button>
                    <button data-owner-accommodation-action="deactivate" data-id="${a.id}">Deaktiviraj</button>
                    <button class="danger" data-owner-accommodation-action="delete" data-id="${a.id}">Obriši</button>
                </div>
            </article>
        `).join('') || '<p>Nema smještaja.</p>';

        const select = document.getElementById('ownerAccommodationSelect');
        select.innerHTML = '<option value="">-- odaberi smještaj --</option>' + items.map(a =>
            `<option value="${a.id}">${escapeHtml(a.name)} (${accommodationTypes[a.type] || a.type})</option>`
        ).join('');

        const validPrevious = items.some(a => a.id === previous);
        if (validPrevious) select.value = String(previous);
        else if (items.length) select.value = String(items[0].id);

        if (select.value) await loadOwnerManagement();
        else clearOwnerManagement();
    } catch (error) {
        showMessage(error.message, 'error');
    }
}

function clearOwnerManagement() {
    state.ownerManagement = null;
    state.ownerSelectedUnitId = null;
    state.lastBlockView = null;
    document.getElementById('ownerSelectedInfo').innerHTML = '<div class="empty-box">Nema odabranog smještaja.</div>';
    document.getElementById('ownerPhotos').innerHTML = '';
    document.getElementById('ownerUnits').innerHTML = '';
    document.getElementById('ownerUnitSelect').innerHTML = '<option value="">-- nema sobe --</option>';
    document.getElementById('blockUnitSelect').innerHTML = '<option value="">-- nema sobe --</option>';
    document.getElementById('ownerSelectedUnitPhotos').innerHTML = '<div class="empty-box">Odaberi sobu.</div>';
    document.getElementById('blocksResult').innerHTML = '';
    renderAmenityCheckboxes('ownerAccommodationAmenities');
    renderAmenityCheckboxes('ownerUnitAmenities');
}

async function loadOwnerManagement(preferredUnitId = null) {
    const select = document.getElementById('ownerAccommodationSelect');
    const id = Number(select.value);
    if (!id) {
        clearOwnerManagement();
        return;
    }

    try {
        if (Number.isInteger(Number(preferredUnitId)) && Number(preferredUnitId) > 0) {
            state.ownerSelectedUnitId = Number(preferredUnitId);
        }

        const data = await apiFetch(`/api/Accommodations/${id}/manage`);
        state.ownerManagement = data;
        renderOwnerManagement();
    } catch (error) {
        showMessage(error.message, 'error');
    }
}

function renderOwnerManagement() {
    const a = state.ownerManagement;
    if (!a) return clearOwnerManagement();

    const hiddenApartmentUnit = a.type === 2 && a.units.length ? a.units[0] : null;

    document.getElementById('ownerSelectedInfo').innerHTML = `
        <article class="card">
            <div class="unit-header">
                <div>
                    <strong>${escapeHtml(a.name)}</strong>
                    <div class="meta">
                        ${escapeHtml(a.address)}, ${escapeHtml(a.city)}, ${escapeHtml(a.country)} ·
                        ${accommodationTypes[a.type] || a.type}
                    </div>
                </div>
                <div>
                    <span class="badge">${approvalStatuses[a.approvalStatus] || a.approvalStatus}</span>
                    <span class="badge">${a.isActive ? 'Aktivan' : 'Neaktivan'}</span>
                </div>
            </div>
            <div class="meta">
                Fotografije smještaja: ${a.photos.length} · Sadržaji: ${a.amenityIds.length} ·
                ${a.type === 1 ? `Sobe: ${a.units.length}` : 'Apartman koristi jednu skrivenu jedinicu'}
                ${a.rejectionReason ? `<br>Razlog odbijanja: ${escapeHtml(a.rejectionReason)}` : ''}
                ${hiddenApartmentUnit ? `<br>Kapacitet: ${hiddenApartmentUnit.capacity} · ${hiddenApartmentUnit.pricePerNight} €/noć · ${cancellationPolicies[hiddenApartmentUnit.cancellationPolicy] || hiddenApartmentUnit.cancellationPolicy}` : ''}
            </div>
        </article>
    `;

    document.getElementById('ownerPhotos').innerHTML = a.photos.length
        ? renderPhotoGallery(a.photos, a.name, { editableAccommodation: true })
        : '<div class="empty-box">Nema fotografija smještaja.</div>';

    renderAmenityCheckboxes('ownerAccommodationAmenities', a.amenityIds);

    const hotelUnits = a.type === 1 ? a.units : [];

    document.getElementById('ownerUnits').innerHTML = a.type !== 1
        ? '<div class="empty-box">Apartman ima jednu skrivenu jedinicu; hotelske sobe se ovdje ne dodaju.</div>'
        : hotelUnits.length
            ? hotelUnits.map(u => `
                <article class="card unit-card">
                    <div class="unit-header">
                        <strong>${escapeHtml(u.name)}</strong>
                        <div>
                            <span class="badge">${u.isActive ? 'Aktivna' : 'Neaktivna'}</span>
                            <span class="badge">${u.photos.length} slika</span>
                        </div>
                    </div>
                    <div class="meta">
                        Kapacitet: ${u.capacity} · ${u.pricePerNight} €/noć ·
                        ${cancellationPolicies[u.cancellationPolicy] || u.cancellationPolicy}<br>
                        Sadržaji: ${u.amenityIds.map(id => amenityNames[id] || id).join(', ') || 'nema'}
                    </div>
                    ${renderPhotoGallery(u.photos, u.name, { editableUnit: true, unitId: u.id })}
                    <div class="card-actions">
                        <button data-owner-unit-action="select" data-unit-id="${u.id}">Odaberi za uređivanje</button>
                        <button data-owner-unit-action="update" data-unit-id="${u.id}">Uredi podatke</button>
                        <button data-owner-unit-action="activate" data-unit-id="${u.id}">Aktiviraj</button>
                        <button data-owner-unit-action="deactivate" data-unit-id="${u.id}">Deaktiviraj</button>
                        <button class="danger" data-owner-unit-action="delete" data-unit-id="${u.id}">Obriši sobu</button>
                    </div>
                </article>
            `).join('')
            : '<div class="empty-box">Hotel još nema soba.</div>';

    const unitOptions = '<option value="">-- odaberi sobu --</option>' + hotelUnits.map(u =>
        `<option value="${u.id}">${escapeHtml(u.name)}</option>`
    ).join('');

    const validSelected = hotelUnits.some(u => u.id === Number(state.ownerSelectedUnitId));
    if (!validSelected) {
        state.ownerSelectedUnitId = hotelUnits.length ? hotelUnits[0].id : null;
    }

    const ownerUnitSelect = document.getElementById('ownerUnitSelect');
    const blockUnitSelect = document.getElementById('blockUnitSelect');
    ownerUnitSelect.innerHTML = unitOptions;
    blockUnitSelect.innerHTML = unitOptions;

    if (state.ownerSelectedUnitId) {
        ownerUnitSelect.value = String(state.ownerSelectedUnitId);
        blockUnitSelect.value = String(state.ownerSelectedUnitId);
    }

    renderSelectedUnitTools();
}

function renderSelectedUnitTools() {
    const unitId = Number(state.ownerSelectedUnitId);
    const unit = state.ownerManagement?.units.find(u => u.id === unitId);

    if (!unit || state.ownerManagement?.type !== 1) {
        document.getElementById('ownerSelectedUnitPhotos').innerHTML = '<div class="empty-box">Odaberi hotelsku sobu.</div>';
        renderAmenityCheckboxes('ownerUnitAmenities');
        return;
    }

    document.getElementById('ownerSelectedUnitPhotos').innerHTML =
        renderPhotoGallery(unit.photos, unit.name, { editableUnit: true, unitId: unit.id });

    renderAmenityCheckboxes('ownerUnitAmenities', unit.amenityIds || []);
}

document.getElementById('ownerAccommodations').addEventListener('click', async event => {
    const button = event.target.closest('button[data-owner-accommodation-action]');
    if (!button) return;

    const id = Number(button.dataset.id);
    const action = button.dataset.ownerAccommodationAction;
    const accommodation = state.ownerAccommodations.find(a => a.id === id);

    try {
        if (action === 'select') {
            document.getElementById('ownerAccommodationSelect').value = String(id);
            await loadOwnerManagement();
            showMessage(`Odabran je smještaj: ${accommodation?.name || ''}.`);
            return;
        }

        if (action === 'update') {
            const name = prompt('Novi naziv:', accommodation?.name || '');
            if (name == null) return;
            const description = prompt('Novi opis:', accommodation?.description || '');
            if (description == null) return;
            await apiFetch(`/api/Accommodations/${id}`, { method: 'PUT', body: { name, description } });
        } else if (action === 'delete') {
            if (!confirm(`Obrisati smještaj "${accommodation?.name || ''}"?`)) return;
            await apiFetch(`/api/Accommodations/${id}`, { method: 'DELETE' });
        } else {
            await apiFetch(`/api/Accommodations/${id}/${action}`, { method: 'POST' });
        }

        showMessage(`Akcija "${action}" je izvršena.`);
        await loadOwnerAccommodations();
    } catch (error) {
        showMessage(error.message, 'error');
    }
});

document.getElementById('createAccommodationType').addEventListener('change', event => {
    const apartment = event.target.value === '2';
    document.querySelectorAll('.apartment-field').forEach(x => x.classList.toggle('hidden', !apartment));
});

document.getElementById('createAccommodationForm').addEventListener('submit', async event => {
    event.preventDefault();
    const data = new FormData(event.currentTarget);
    const type = Number(data.get('type'));

    const body = {
        name: data.get('name'),
        description: data.get('description'),
        address: data.get('address'),
        city: data.get('city'),
        country: data.get('country'),
        type,
        capacity: type === 2 ? Number(data.get('capacity')) : null,
        pricePerNight: type === 2 ? Number(data.get('pricePerNight')) : null,
        cancellationPolicy: type === 2 ? Number(data.get('cancellationPolicy')) : null
    };

    try {
        const result = await apiFetch('/api/Accommodations', { method: 'POST', body });
        showMessage(`Smještaj "${result.name}" je kreiran.`);
        event.currentTarget.reset();
        document.querySelectorAll('.apartment-field').forEach(x => x.classList.add('hidden'));
        await loadOwnerAccommodations();
        document.getElementById('ownerAccommodationSelect').value = String(result.id);
        await loadOwnerManagement();
    } catch (error) {
        showMessage(error.message, 'error');
    }
});

// OWNER PHOTOS

document.getElementById('accommodationPhotoForm').addEventListener('submit', async event => {
    event.preventDefault();

    try {
        const id = selectedAccommodationId();
        const source = new FormData(event.currentTarget);
        const body = new FormData();
        body.append('File', source.get('file'));
        body.append('IsPrimary', source.get('isPrimary') === 'on' ? 'true' : 'false');

        await apiFetch(`/api/Accommodations/${id}/photos`, { method: 'POST', body });
        event.currentTarget.reset();
        showMessage('Fotografija je dodana.');
        await loadOwnerManagement();
    } catch (error) {
        showMessage(error.message, 'error');
    }
});

document.getElementById('ownerPhotos').addEventListener('click', async event => {
    const button = event.target.closest('button[data-owner-photo-action]');
    if (!button) return;

    try {
        const accommodationId = selectedAccommodationId();
        const photoId = Number(button.dataset.photoId);
        const action = button.dataset.ownerPhotoAction;

        if (action === 'primary') {
            await apiFetch(`/api/Accommodations/${accommodationId}/photos/${photoId}/primary`, { method: 'POST' });
            showMessage('Primarna fotografija je promijenjena.');
        } else {
            if (!confirm('Obrisati ovu fotografiju?')) return;
            await apiFetch(`/api/Accommodations/${accommodationId}/photos/${photoId}`, { method: 'DELETE' });
            showMessage('Fotografija je obrisana.');
        }

        await loadOwnerManagement();
    } catch (error) {
        showMessage(error.message, 'error');
    }
});

// OWNER AMENITIES

document.getElementById('saveAccommodationAmenities').addEventListener('click', async () => {
    try {
        const id = selectedAccommodationId();
        const amenityIds = checkedAmenityIds('ownerAccommodationAmenities');
        await apiFetch(`/api/accommodations/${id}/amenities`, {
            method: 'PUT',
            body: { amenityIds }
        });
        showMessage('Sadržaji smještaja su spremljeni.');
        await loadOwnerManagement();
    } catch (error) {
        showMessage(error.message, 'error');
    }
});

// OWNER UNITS

const roomPhotoInput = document.querySelector('#createUnitForm [name="photos"]');
roomPhotoInput.addEventListener('change', () => {
    const select = document.querySelector('#createUnitForm [name="primaryPhotoIndex"]');
    const files = Array.from(roomPhotoInput.files || []);
    select.innerHTML = files.length
        ? files.map((f, index) => `<option value="${index}">${index + 1}. ${escapeHtml(f.name)}</option>`).join('')
        : '<option value="0">Prva slika</option>';
});

document.getElementById('createUnitForm').addEventListener('submit', async event => {
    event.preventDefault();

    try {
        const accommodationId = selectedAccommodationId();
        if (state.ownerManagement?.type !== 1) throw new Error('Sobe se mogu ručno dodavati samo hotelu.');

        const form = event.currentTarget;
        const data = new FormData(form);
        const photos = Array.from(form.querySelector('[name="photos"]')?.files || []);
        if (photos.length === 0) throw new Error('Odaberi barem jednu fotografiju sobe.');

        const primaryPhotoIndex = Number(data.get('primaryPhotoIndex') || 0);
        if (primaryPhotoIndex < 0 || primaryPhotoIndex >= photos.length) {
            throw new Error('Odabrana primarna slika nije valjana.');
        }

        const body = new FormData();
        body.append('Name', data.get('name'));
        body.append('Capacity', data.get('capacity'));
        body.append('PricePerNight', data.get('pricePerNight'));
        body.append('CancellationPolicy', data.get('cancellationPolicy'));
        body.append('PrimaryPhotoIndex', String(primaryPhotoIndex));
        photos.forEach(file => body.append('Photos', file, file.name));

        const result = await apiFetch(`/api/accommodations/${accommodationId}/units`, {
            method: 'POST',
            body
        });

        showMessage(`Soba "${result.name}" je kreirana.`);
        form.reset();
        document.querySelector('#createUnitForm [name="primaryPhotoIndex"]').innerHTML = '<option value="0">Prva slika</option>';
        state.ownerSelectedUnitId = result.id;
        await loadOwnerManagement(result.id);
    } catch (error) {
        showMessage(error.message, 'error');
    }
});

document.getElementById('ownerUnits').addEventListener('click', async event => {
    const button = event.target.closest('button[data-owner-unit-action]');
    if (!button) return;

    try {
        const accommodationId = selectedAccommodationId();
        const unitId = Number(button.dataset.unitId);
        const unit = state.ownerManagement?.units.find(u => u.id === unitId);
        if (!unit) throw new Error('Soba nije pronađena.');

        const action = button.dataset.ownerUnitAction;

        if (action === 'select') {
            state.ownerSelectedUnitId = unitId;
            document.getElementById('ownerUnitSelect').value = String(unitId);
            document.getElementById('blockUnitSelect').value = String(unitId);
            renderSelectedUnitTools();
            showMessage(`Odabrana je soba: ${unit.name}.`);
            return;
        }

        if (action === 'update') {
            const name = prompt('Naziv sobe:', unit.name);
            if (name == null) return;
            const capacity = Number(prompt('Kapacitet:', String(unit.capacity)));
            const pricePerNight = Number(prompt('Cijena po noći:', String(unit.pricePerNight)));
            const cancellationPolicy = Number(prompt('Otkazivanje: 1 Flexible, 2 Moderate, 3 Strict', String(unit.cancellationPolicy)));

            if (!name.trim() || !Number.isInteger(capacity) || capacity <= 0 || pricePerNight <= 0 || ![1, 2, 3].includes(cancellationPolicy)) {
                throw new Error('Podaci sobe nisu valjani.');
            }

            await apiFetch(`/api/accommodations/${accommodationId}/units/${unitId}`, {
                method: 'PUT',
                body: { name, capacity, pricePerNight, cancellationPolicy }
            });
        } else if (action === 'delete') {
            if (!confirm(`Obrisati sobu "${unit.name}"?`)) return;
            await apiFetch(`/api/accommodations/${accommodationId}/units/${unitId}`, { method: 'DELETE' });
        } else {
            await apiFetch(`/api/accommodations/${accommodationId}/units/${unitId}/${action}`, { method: 'POST' });
        }

        if (action === 'delete' && state.ownerSelectedUnitId === unitId) {
            state.ownerSelectedUnitId = null;
        } else {
            state.ownerSelectedUnitId = unitId;
        }

        showMessage(`Akcija sobe "${action}" je izvršena.`);
        await loadOwnerManagement(state.ownerSelectedUnitId);
    } catch (error) {
        showMessage(error.message, 'error');
    }
});

document.getElementById('ownerUnitSelect').addEventListener('change', () => {
    state.ownerSelectedUnitId = Number(document.getElementById('ownerUnitSelect').value) || null;
    if (state.ownerSelectedUnitId) {
        document.getElementById('blockUnitSelect').value = String(state.ownerSelectedUnitId);
    }
    renderSelectedUnitTools();
});

document.getElementById('unitPhotoForm').addEventListener('submit', async event => {
    event.preventDefault();

    try {
        const accommodationId = selectedAccommodationId();
        const unitId = selectedUnitId();
        const source = new FormData(event.currentTarget);
        const file = source.get('file');
        if (!file || !file.size) throw new Error('Odaberi fotografiju sobe.');

        const body = new FormData();
        body.append('File', file, file.name);
        body.append('IsPrimary', source.get('isPrimary') === 'on' ? 'true' : 'false');

        await apiFetch(`/api/accommodations/${accommodationId}/units/${unitId}/photos`, {
            method: 'POST',
            body
        });

        event.currentTarget.reset();
        state.ownerSelectedUnitId = unitId;
        showMessage('Fotografija sobe je dodana.');
        await loadOwnerManagement(unitId);
    } catch (error) {
        showMessage(error.message, 'error');
    }
});

async function handleUnitPhotoAction(event) {
    const button = event.target.closest('button[data-owner-unit-photo-action]');
    if (!button) return;

    try {
        const accommodationId = selectedAccommodationId();
        const unitId = Number(button.dataset.unitId);
        const photoId = Number(button.dataset.photoId);
        const action = button.dataset.ownerUnitPhotoAction;

        if (action === 'primary') {
            await apiFetch(`/api/accommodations/${accommodationId}/units/${unitId}/photos/${photoId}/primary`, {
                method: 'POST'
            });
            showMessage('Primarna fotografija sobe je promijenjena.');
        } else {
            if (!confirm('Obrisati ovu fotografiju sobe?')) return;
            await apiFetch(`/api/accommodations/${accommodationId}/units/${unitId}/photos/${photoId}`, {
                method: 'DELETE'
            });
            showMessage('Fotografija sobe je obrisana.');
        }

        state.ownerSelectedUnitId = unitId;
        await loadOwnerManagement(unitId);
    } catch (error) {
        showMessage(error.message, 'error');
    }
}

document.getElementById('ownerUnits').addEventListener('click', handleUnitPhotoAction);
document.getElementById('ownerSelectedUnitPhotos').addEventListener('click', handleUnitPhotoAction);

document.getElementById('saveUnitAmenities').addEventListener('click', async () => {
    try {
        const accommodationId = selectedAccommodationId();
        const unitId = selectedUnitId();
        const amenityIds = checkedAmenityIds('ownerUnitAmenities');

        await apiFetch(`/api/accommodations/${accommodationId}/units/${unitId}/amenities`, {
            method: 'PUT',
            body: { amenityIds }
        });

        showMessage('Sadržaji sobe su spremljeni.');
        state.ownerSelectedUnitId = unitId;
        await loadOwnerManagement(unitId);
    } catch (error) {
        showMessage(error.message, 'error');
    }
});

// OWNER AVAILABILITY BLOCKS

document.getElementById('createAccommodationBlockForm').addEventListener('submit', async event => {
    event.preventDefault();
    const data = new FormData(event.currentTarget);

    try {
        const accommodationId = selectedAccommodationId();
        await apiFetch(`/api/accommodations/${accommodationId}/availability-blocks`, {
            method: 'POST',
            body: { startDate: data.get('startDate'), endDate: data.get('endDate') }
        });
        showMessage('Block smještaja je kreiran.');
        event.currentTarget.reset();
        await loadAccommodationBlocks();
    } catch (error) {
        showMessage(error.message, 'error');
    }
});

document.getElementById('createUnitBlockForm').addEventListener('submit', async event => {
    event.preventDefault();
    const data = new FormData(event.currentTarget);

    try {
        const accommodationId = selectedAccommodationId();
        const unitId = Number(data.get('unitId'));
        if (!unitId) throw new Error('Odaberi sobu.');

        await apiFetch(`/api/accommodations/${accommodationId}/units/${unitId}/availability-blocks`, {
            method: 'POST',
            body: { startDate: data.get('startDate'), endDate: data.get('endDate') }
        });
        showMessage('Block sobe je kreiran.');
        await loadUnitBlocks(unitId);
    } catch (error) {
        showMessage(error.message, 'error');
    }
});

document.getElementById('loadAccommodationBlocks').addEventListener('click', loadAccommodationBlocks);
document.getElementById('loadUnitBlocks').addEventListener('click', () => loadUnitBlocks());

async function loadAccommodationBlocks() {
    try {
        const accommodationId = selectedAccommodationId();
        const includePast = document.getElementById('includePastBlocks').checked;
        const blocks = await apiFetch(`/api/accommodations/${accommodationId}/availability-blocks?includePast=${includePast}`);
        state.lastBlockView = { type: 'accommodation' };
        renderBlocks(blocks, 'Smještaj');
    } catch (error) {
        showMessage(error.message, 'error');
    }
}

async function loadUnitBlocks(forcedUnitId = null) {
    try {
        const accommodationId = selectedAccommodationId();
        const unitId = forcedUnitId || selectedUnitId('blockUnitSelect');
        document.getElementById('blockUnitSelect').value = String(unitId);
        const includePast = document.getElementById('includePastBlocks').checked;
        const blocks = await apiFetch(`/api/accommodations/${accommodationId}/units/${unitId}/availability-blocks?includePast=${includePast}`);
        const unit = state.ownerManagement?.units.find(u => u.id === unitId);
        state.lastBlockView = { type: 'unit', unitId };
        renderBlocks(blocks, unit?.name || 'Soba');
    } catch (error) {
        showMessage(error.message, 'error');
    }
}

async function refreshCurrentBlocks() {
    if (!state.lastBlockView) {
        document.getElementById('blocksResult').innerHTML = '';
        return;
    }

    if (state.lastBlockView.type === 'unit') {
        await loadUnitBlocks(state.lastBlockView.unitId);
    } else {
        await loadAccommodationBlocks();
    }
}

function renderBlocks(blocks, targetName) {
    document.getElementById('blocksResult').innerHTML = blocks.length ? blocks.map(b => `
        <article class="card">
            <strong>${escapeHtml(targetName)}</strong>
            <div class="meta">${b.startDate} → ${b.endDate}</div>
            <div class="card-actions">
                <button data-block-action="update" data-block-id="${b.id}" data-start="${b.startDate}" data-end="${b.endDate}">Uredi</button>
                <button class="danger" data-block-action="delete" data-block-id="${b.id}">Obriši</button>
            </div>
        </article>
    `).join('') : '<p>Nema blockova.</p>';
}

document.getElementById('blocksResult').addEventListener('click', async event => {
    const button = event.target.closest('button[data-block-action]');
    if (!button) return;

    try {
        const blockId = Number(button.dataset.blockId);

        if (button.dataset.blockAction === 'update') {
            const startDate = prompt('Datum od (YYYY-MM-DD):', button.dataset.start);
            if (startDate == null) return;
            const endDate = prompt('Datum do (YYYY-MM-DD):', button.dataset.end);
            if (endDate == null) return;

            await apiFetch(`/api/availability-blocks/${blockId}`, {
                method: 'PUT',
                body: { startDate, endDate }
            });
            showMessage('Block je ažuriran.');
        } else {
            if (!confirm('Obrisati block?')) return;
            await apiFetch(`/api/availability-blocks/${blockId}`, { method: 'DELETE' });
            showMessage('Block je obrisan.');
        }

        await refreshCurrentBlocks();
    } catch (error) {
        showMessage(error.message, 'error');
    }
});

// OWNER RESERVATIONS

document.getElementById('loadOwnerReservations').addEventListener('click', loadOwnerReservations);

async function loadOwnerReservations() {
    try {
        const reservations = await apiFetch('/api/owner/reservations');
        document.getElementById('ownerReservations').innerHTML = reservations.map(r => `
            <article class="card">
                <strong>${escapeHtml(r.accommodationName)} / ${escapeHtml(r.unitName)}</strong>
                <div class="meta">${r.checkInDate} → ${r.checkOutDate} · ${r.guestCount} gostiju · ${r.totalPrice} € · ${reservationStatuses[r.status] || r.status}</div>
                ${r.status === 1 ? `<div class="card-actions"><button class="danger" data-owner-reservation-cancel="${r.id}">Owner otkaži</button></div>` : ''}
            </article>
        `).join('') || '<p>Nema rezervacija.</p>';
    } catch (error) {
        showMessage(error.message, 'error');
    }
}

document.getElementById('ownerReservations').addEventListener('click', async event => {
    const button = event.target.closest('[data-owner-reservation-cancel]');
    if (!button) return;
    if (!confirm('Owner otkazuje ovu rezervaciju?')) return;

    try {
        await apiFetch(`/api/owner/reservations/${button.dataset.ownerReservationCancel}/cancel`, { method: 'POST' });
        showMessage('Rezervacija je otkazana od strane Ownera.');
        await loadOwnerReservations();
    } catch (error) {
        showMessage(error.message, 'error');
    }
});

// ADMIN ACCOMMODATIONS

document.getElementById('loadPending').addEventListener('click', loadPending);
document.getElementById('loadAdminAccommodations').addEventListener('click', loadAdminAccommodations);

async function loadPending() {
    try {
        const items = await apiFetch('/api/admin/accommodations/pending');
        document.getElementById('pendingAccommodations').innerHTML = items.map(a => `
            <article class="card">
                <strong>${escapeHtml(a.name)}</strong>
                <div class="meta">${escapeHtml(a.city)}, ${escapeHtml(a.country)}</div>
                <p>${escapeHtml(a.description)}</p>
                <div class="card-actions">
                    <button data-admin-details-id="${a.id}">Prikaži detalje</button>
                    <button data-pending-action="approve" data-id="${a.id}">Approve</button>
                    <button data-pending-action="reject" data-id="${a.id}">Reject</button>
                </div>
                <div id="admin-details-${a.id}"></div>
            </article>
        `).join('') || '<p>Nema smještaja na čekanju.</p>';
    } catch (error) {
        showMessage(error.message, 'error');
    }
}

document.getElementById('pendingAccommodations').addEventListener('click', async event => {
    const button = event.target.closest('button[data-pending-action]');
    if (!button) return;
    const id = Number(button.dataset.id);

    try {
        if (button.dataset.pendingAction === 'reject') {
            const reason = prompt('Razlog odbijanja:', 'Nedostaju potrebni podaci.');
            if (reason == null) return;
            await apiFetch(`/api/admin/accommodations/${id}/reject`, { method: 'POST', body: { reason } });
        } else {
            await apiFetch(`/api/admin/accommodations/${id}/approve`, { method: 'POST' });
        }

        showMessage('Admin akcija je izvršena.');
        await loadPending();
        await loadAdminAccommodations();
    } catch (error) {
        showMessage(error.message, 'error');
    }
});

async function loadAdminAccommodations() {
    try {
        const items = await apiFetch('/api/admin/accommodations');
        state.adminAccommodations = items;

        document.getElementById('adminAccommodations').innerHTML = items.map(a => {
            let actions = '';
            if (a.approvalStatus === 2) {
                actions = `
                    <button data-admin-accommodation-action="approve" data-id="${a.id}">Approve</button>
                    <button data-admin-accommodation-action="reject" data-id="${a.id}">Reject</button>`;
            }
            if (a.approvalStatus === 3) {
                actions = `<button data-admin-accommodation-action="suspend" data-id="${a.id}">Suspend</button>`;
            }
            if (a.approvalStatus === 5) {
                actions = `<button data-admin-accommodation-action="unsuspend" data-id="${a.id}">Unsuspend</button>`;
            }

            return `
                <article class="card">
                    <strong>${escapeHtml(a.name)}</strong>
                    <div class="meta">
                        ${escapeHtml(a.city)}, ${escapeHtml(a.country)} · ${accommodationTypes[a.type] || a.type} ·
                        ${approvalStatuses[a.approvalStatus] || a.approvalStatus} · Active: ${a.isActive ? 'DA' : 'NE'}
                    </div>
                    <div class="card-actions">
                        <button data-admin-details-id="${a.id}">Prikaži detalje</button>
                        ${actions || '<span>Nema admin akcije za trenutni status.</span>'}
                    </div>
                    <div id="admin-details-${a.id}"></div>
                </article>`;
        }).join('') || '<p>Nema smještaja.</p>';
    } catch (error) {
        showMessage(error.message, 'error');
    }
}

document.getElementById('adminAccommodations').addEventListener('click', async event => {
    const button = event.target.closest('button[data-admin-accommodation-action]');
    if (!button) return;

    const id = Number(button.dataset.id);
    const action = button.dataset.adminAccommodationAction;

    try {
        if (action === 'reject') {
            const reason = prompt('Razlog odbijanja:', 'Nedostaju potrebni podaci.');
            if (reason == null) return;
            await apiFetch(`/api/admin/accommodations/${id}/reject`, { method: 'POST', body: { reason } });
        } else {
            await apiFetch(`/api/admin/accommodations/${id}/${action}`, { method: 'POST' });
        }

        showMessage(`Admin akcija "${action}" je izvršena.`);
        await loadAdminAccommodations();
        await loadPending();
    } catch (error) {
        showMessage(error.message, 'error');
    }
});

async function handleAdminAccommodationDetails(event) {
    const button = event.target.closest('button[data-admin-details-id]');
    if (!button) return;

    const id = Number(button.dataset.adminDetailsId);
    const target = document.getElementById(`admin-details-${id}`);
    if (!target) return;

    try {
        if (target.dataset.loaded === 'true') {
            target.innerHTML = '';
            target.dataset.loaded = 'false';
            button.textContent = 'Prikaži detalje';
            return;
        }

        const a = await apiFetch(`/api/admin/accommodations/${id}/details`);
        target.innerHTML = `
            <div class="card">
                <div class="meta">
                    ${escapeHtml(a.address)}, ${escapeHtml(a.city)}, ${escapeHtml(a.country)}<br>
                    Status: ${approvalStatuses[a.approvalStatus] || a.approvalStatus} · Active: ${a.isActive ? 'DA' : 'NE'}<br>
                    Sadržaji smještaja: ${a.amenityIds.map(x => amenityNames[x] || x).join(', ') || 'nema'}
                </div>
                <p>${escapeHtml(a.description)}</p>
                <h4>Sve fotografije smještaja</h4>
                ${renderPhotoGallery(a.photos || [], a.name)}
                <h4>${a.type === 1 ? 'Hotelske sobe' : 'Podaci apartmana'}</h4>
                ${(a.units || []).length ? a.units.map(u => `
                    <div class="card unit-card">
                        <strong>${escapeHtml(u.name)}</strong>
                        <div class="meta">
                            Kapacitet: ${u.capacity} · ${u.pricePerNight} €/noć ·
                            ${cancellationPolicies[u.cancellationPolicy] || u.cancellationPolicy} ·
                            Active: ${u.isActive ? 'DA' : 'NE'}<br>
                            Sadržaji sobe: ${u.amenityIds.map(x => amenityNames[x] || x).join(', ') || 'nema'}
                        </div>
                        ${renderPhotoGallery(u.photos || [], u.name)}
                    </div>
                `).join('') : '<div class="empty-box">Nema jedinica.</div>'}
            </div>`;
        target.dataset.loaded = 'true';
        button.textContent = 'Sakrij detalje';
    } catch (error) {
        showMessage(error.message, 'error');
    }
}

document.getElementById('pendingAccommodations').addEventListener('click', handleAdminAccommodationDetails);
document.getElementById('adminAccommodations').addEventListener('click', handleAdminAccommodationDetails);

// ADMIN ACCOUNTS

document.getElementById('loadAccounts').addEventListener('click', loadAccounts);

async function loadAccounts() {
    try {
        const accounts = await apiFetch('/api/admin/accounts');
        document.getElementById('adminAccounts').innerHTML = accounts.map(a => `
            <article class="card">
                <strong>${escapeHtml(a.firstName)} ${escapeHtml(a.lastName)}</strong>
                <div class="meta">${escapeHtml(a.email)} · ${roleNames[a.role] || a.role} · Active: ${a.isActive ? 'DA' : 'NE'}</div>
                <div class="card-actions">
                    ${a.isActive
                        ? `<button class="danger" data-account-action="deactivate" data-id="${a.id}">Deaktiviraj</button>`
                        : `<button data-account-action="activate" data-id="${a.id}">Aktiviraj</button>`}
                </div>
            </article>
        `).join('') || '<p>Nema korisnika.</p>';
    } catch (error) {
        showMessage(error.message, 'error');
    }
}

document.getElementById('adminAccounts').addEventListener('click', async event => {
    const button = event.target.closest('button[data-account-action]');
    if (!button) return;

    try {
        await apiFetch(`/api/admin/accounts/${button.dataset.id}/${button.dataset.accountAction}`, { method: 'POST' });
        showMessage('Status korisnika je promijenjen.');
        await loadAccounts();
    } catch (error) {
        showMessage(error.message, 'error');
    }
});

// ADMIN RESERVATIONS

document.getElementById('loadAdminReservations').addEventListener('click', loadAdminReservations);

async function loadAdminReservations() {
    try {
        const reservations = await apiFetch('/api/admin/reservations');
        document.getElementById('adminReservations').innerHTML = reservations.map(r => `
            <article class="card">
                <strong>${escapeHtml(r.accommodationName)} / ${escapeHtml(r.unitName)}</strong>
                <div class="meta">${r.checkInDate} → ${r.checkOutDate} · ${r.totalPrice} € · ${reservationStatuses[r.status] || r.status}</div>
                ${r.status === 1 ? `<div class="card-actions"><button class="danger" data-admin-reservation-cancel="${r.id}">Admin otkaži</button></div>` : ''}
            </article>
        `).join('') || '<p>Nema rezervacija.</p>';
    } catch (error) {
        showMessage(error.message, 'error');
    }
}

document.getElementById('adminReservations').addEventListener('click', async event => {
    const button = event.target.closest('[data-admin-reservation-cancel]');
    if (!button) return;
    if (!confirm('Admin otkazuje ovu rezervaciju?')) return;

    try {
        await apiFetch(`/api/admin/reservations/${button.dataset.adminReservationCancel}/cancel`, { method: 'POST' });
        showMessage('Rezervacija je otkazana od strane Admina.');
        await loadAdminReservations();
    } catch (error) {
        showMessage(error.message, 'error');
    }
});

// ADMIN REVIEWS

document.getElementById('loadAdminReviews').addEventListener('click', loadAdminReviews);

async function loadAdminReviews() {
    try {
        if (!state.adminAccommodations.length) {
            state.adminAccommodations = await apiFetch('/api/admin/accommodations');
        }

        const allReviews = [];
        for (const accommodation of state.adminAccommodations) {
            const data = await apiFetch(`/api/accommodations/${accommodation.id}/reviews`);
            data.reviews.forEach(review => allReviews.push({ ...review, accommodationName: accommodation.name }));
        }

        document.getElementById('adminReviews').innerHTML = allReviews.length ? allReviews.map(r => `
            <article class="card">
                <strong>${escapeHtml(r.accommodationName)} · ${escapeHtml(r.reviewerName)} · ${r.rating}/5</strong>
                <p>${escapeHtml(r.comment || '')}</p>
                <div class="card-actions">
                    <button class="danger" data-admin-review-delete="${r.id}">Obriši recenziju</button>
                </div>
            </article>
        `).join('') : '<p>Nema recenzija.</p>';
    } catch (error) {
        showMessage(error.message, 'error');
    }
}

document.getElementById('adminReviews').addEventListener('click', async event => {
    const button = event.target.closest('[data-admin-review-delete]');
    if (!button) return;
    if (!confirm('Obrisati ovu recenziju?')) return;

    try {
        await apiFetch(`/api/admin/reviews/${button.dataset.adminReviewDelete}`, { method: 'DELETE' });
        showMessage('Recenzija je obrisana od strane Admina.');
        await loadAdminReviews();
    } catch (error) {
        showMessage(error.message, 'error');
    }
});

// INITIALIZATION

setDefaultDates();
updateSessionUi();
loadAmenities();
