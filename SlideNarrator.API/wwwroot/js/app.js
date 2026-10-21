/* Logic giao diện SlideNarrator: Upload & Cấu hình → Kết quả & Trình phát → Danh sách dự án */
(() => {
    const LANGS = {
        vi: 'Tiếng Việt', en: 'Tiếng Anh', ja: 'Tiếng Nhật', ko: 'Tiếng Hàn',
        zh: 'Tiếng Trung', fr: 'Tiếng Pháp', de: 'Tiếng Đức'
    };

    const $ = (s) => document.querySelector(s);
    const esc = (s) => String(s ?? '').replace(/[&<>"']/g,
        (c) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));

    let slides = [];          // [{ id, kind: 'image'|'pdf'|'text', fileName, previewUrl, imageUrl, text }]
    let nextId = 1;
    let current = null;       // { project, sourceLanguage, targetLanguage, items: [...] }

    // ---------- Khởi tạo ----------
    function fillLanguageSelects() {
        for (const id of ['#sourceLanguage', '#targetLanguage']) {
            $(id).innerHTML = Object.entries(LANGS)
                .map(([code, name]) => `<option value="${code}">${name} (${code})</option>`).join('');
        }
        $('#sourceLanguage').value = 'en';
        $('#targetLanguage').value = 'vi';
    }

    function showAlert(boxSel, message, type = 'danger') {
        $(boxSel).innerHTML = message
            ? `<div class="alert alert-${type} alert-dismissible fade show" role="alert">${esc(message)}
                 <button type="button" class="btn-close" data-bs-dismiss="alert"></button></div>`
            : '';
    }

    const showTab = (id) => bootstrap.Tab.getOrCreateInstance($(id)).show();

    // ---------- Màn Upload & Cấu hình ----------
    async function addFiles(fileList) {
        const rejected = [];
        const accepted = [];
        for (const file of fileList) {
            const ext = file.name.split('.').pop().toLowerCase();
            if (['png', 'jpg', 'jpeg', 'pdf'].includes(ext)) accepted.push(file);
            else rejected.push(file.name);
        }

        if (!accepted.length) {
            return showAlert('#formAlert', `Chỉ nhận .pdf, .png, .jpg. Đã bỏ qua: ${rejected.join(', ')}`, 'warning');
        }

        showAlert('#formAlert', 'Đang tải file và đọc nội dung slide…', 'info');
        try {
            const result = await Api.uploadSlides(accepted);
            const pages = result?.pages ?? [];
            if (!pages.length) throw new Error('Backend không trả về trang nào từ file đã chọn.');

            pages.forEach((page) => {
                const isPdf = /pdf_icon/i.test(page.imageUrl ?? '');
                slides.push({
                    id: nextId++,
                    kind: isPdf ? 'pdf' : 'image',
                    fileName: page.sourceFileName || `Trang ${page.pageNumber}`,
                    // Ảnh đã được server lưu nên dùng luôn URL của server (bền hơn blob URL)
                    previewUrl: isPdf ? null : page.imageUrl,
                    imageUrl: isPdf ? '' : page.imageUrl,
                    text: (page.extractedText ?? '').trim()
                });
            });

            const emptyExtracted = pages.some((page) => !(page.extractedText ?? '').trim());
            const message = emptyExtracted
                ? 'Đã tải file. Một số trang chưa trích xuất được chữ; ảnh JPG/PNG hiện cần OCR hoặc nhập nội dung thủ công.'
                : '';
            showAlert('#formAlert', rejected.length
                ? `${message} Đã bỏ qua: ${rejected.join(', ')}`
                : message, 'warning');
            renderSlides();
        } catch (e) {
            showAlert('#formAlert', e.message);
        }
    }

    function addEmptySlide() {
        slides.push({ id: nextId++, kind: 'text', fileName: '', previewUrl: null, imageUrl: '', text: '' });
        renderSlides();
        const areas = document.querySelectorAll('#slideList textarea');
        areas[areas.length - 1]?.focus();
    }

    function thumbHtml(s) {
        if (s.kind === 'image') return `<img class="slide-thumb" src="${esc(s.previewUrl)}" alt="${esc(s.fileName)}" />`;
        if (s.kind === 'pdf') return `<div class="slide-thumb-placeholder"><span class="fs-3">📕</span>${esc(s.fileName)}</div>`;
        return `<div class="slide-thumb-placeholder"><span class="fs-3">✍️</span>Nhập tay</div>`;
    }

    function renderSlides() {
        $('#slideEmpty').classList.toggle('d-none', slides.length > 0);
        $('#slideList').innerHTML = slides.map((s, i) => `
            <div class="card slide-card" data-id="${s.id}">
                <div class="card-body">
                    <div class="d-flex flex-column flex-sm-row gap-3">
                        <div>${thumbHtml(s)}</div>
                        <div class="flex-grow-1">
                            <div class="d-flex justify-content-between align-items-center mb-1">
                                <span class="badge text-bg-secondary page-badge">Trang ${i + 1}</span>
                                <button type="button" class="btn btn-sm btn-outline-danger" data-action="remove" title="Xóa slide này">Xóa</button>
                            </div>
                            <textarea class="form-control" rows="3" data-action="text"
                                placeholder="Nhập văn bản thuyết minh của trang này (ngôn ngữ gốc)…">${esc(s.text)}</textarea>
                            <div class="small text-muted text-end mt-1"><span class="char-counter" data-counter></span></div>
                        </div>
                    </div>
                </div>
            </div>`).join('');
        slides.forEach(updateCounter);
    }

    // Không còn giới hạn độ dài: backend tự chia nhỏ văn bản khi dịch và đọc
    function updateCounter(s) {
        const el = document.querySelector(`.slide-card[data-id="${s.id}"] [data-counter]`);
        if (!el) return;
        el.textContent = `${s.text.length} ký tự`;
    }

    function setProgress(pct, text) {
        $('#progressWrap').classList.remove('d-none');
        const bar = $('#progressBar');
        bar.style.width = pct + '%';
        bar.textContent = pct + '%';
        $('#progressText').textContent = text || '';
    }

    async function onSubmit(ev) {
        ev.preventDefault();
        showAlert('#formAlert', '');

        const name = $('#projectName').value.trim();
        const sourceLanguage = $('#sourceLanguage').value;
        const targetLanguage = $('#targetLanguage').value;

        if (!name) return showAlert('#formAlert', 'Vui lòng nhập tên dự án.');
        if (!slides.length) return showAlert('#formAlert', 'Hãy thêm ít nhất một slide.');
        const empty = slides.map((s, i) => (s.text.trim() ? null : i + 1)).filter(Boolean);
        if (empty.length) return showAlert('#formAlert', `Trang ${empty.join(', ')} chưa có văn bản thuyết minh.`);

        const btn = $('#btnSubmit');
        btn.disabled = true;
        try {
            setProgress(0, 'Đang tạo dự án…');
            const project = await Api.createProject(name, sourceLanguage);

            const items = slides.map((s, i) => ({
                pageNumber: i + 1,
                previewUrl: s.kind === 'image' ? s.previewUrl : null,
                imageUrl: s.imageUrl,
                kind: s.kind,
                fileName: s.fileName,
                originalText: s.text.trim(),
                translatedText: null,
                audioUrl: null,
                error: null
            }));

            current = { project, sourceLanguage, targetLanguage, items };

            // Tạo lần lượt từng trang để không dồn dập dịch vụ TTS. Mỗi trang được backend lưu vào MySQL.
            for (let i = 0; i < items.length; i++) {
                setProgress(Math.round((i / items.length) * 100), `Đang dịch & tạo giọng đọc trang ${i + 1}/${items.length}…`);
                await generateForItem(items[i]);
            }
            setProgress(100, 'Hoàn tất.');

            renderResult();
            showTab('#tab-result');

            const failed = items.filter((x) => x.error).length;
            if (failed) showAlert('#formAlert', `${failed} trang tạo audio lỗi. Mở tab Kết quả để bấm “Tạo lại”.`, 'warning');
        } catch (e) {
            showAlert('#formAlert', e.message);
        } finally {
            btn.disabled = false;
            setTimeout(() => $('#progressWrap').classList.add('d-none'), 1500);
        }
    }

    async function generateForItem(item) {
        item.error = null;
        try {
            const r = await Api.generateAudio({
                projectId: current.project.id,
                projectName: current.project.name,
                pageNumber: item.pageNumber,
                text: item.originalText,
                imagePath: item.imageUrl,
                sourceLanguage: current.sourceLanguage,
                targetLanguage: current.targetLanguage || $('#targetLanguage').value
            });
            item.audioUrl = r.audioUrl;
            item.translatedText = r.translatedText ?? item.translatedText;
        } catch (e) {
            item.audioUrl = null;
            item.error = e.message;
        }
    }

    // ---------- Màn Kết quả & Trình phát ----------
    function resultCardHtml(it) {
        const thumb = it.previewUrl
            ? `<img class="slide-thumb" src="${esc(it.previewUrl)}" alt="Trang ${it.pageNumber}" />`
            : `<div class="slide-thumb-placeholder"><span class="fs-3">${it.kind === 'pdf' ? '📕' : '📝'}</span>Trang ${it.pageNumber}</div>`;

        const player = it.audioUrl
            ? `<audio controls preload="none" src="${esc(it.audioUrl)}"></audio>`
            : `<div class="alert alert-danger py-2 mb-0 d-flex justify-content-between align-items-center">
                   <span>${esc(it.error || 'Chưa có audio.')}</span>
                   <button type="button" class="btn btn-sm btn-outline-danger" data-action="retry" data-index="${it.pageNumber - 1}">Tạo lại</button>
               </div>`;

        return `
            <div class="card" data-page="${it.pageNumber}">
                <div class="card-body">
                    <div class="d-flex flex-column flex-sm-row gap-3">
                        <div>${thumb}</div>
                        <div class="flex-grow-1 vstack gap-2">
                            <span class="badge text-bg-secondary align-self-start">Trang ${it.pageNumber}</span>
                            <div>
                                <div class="small text-muted">Văn bản gốc (${esc(current.sourceLanguage)})</div>
                                <div class="result-text">${esc(it.originalText)}</div>
                            </div>
                            <div>
                                <div class="small text-muted">Bản dịch (${esc(current.targetLanguage)})</div>
                                <div class="result-text">${it.translatedText
                                    ? esc(it.translatedText)
                                    : '<span class="text-muted fst-italic">Chưa có bản dịch cho trang này.</span>'}</div>
                            </div>
                            ${player}
                        </div>
                    </div>
                </div>
            </div>`;
    }

    function renderResult() {
        const has = !!current;
        $('#resultEmpty').classList.toggle('d-none', has);
        $('#resultBox').classList.toggle('d-none', !has);
        if (!has) return;

        $('#resultTitle').textContent = current.project.name;
        $('#resultMeta').textContent =
            `${LANGS[current.sourceLanguage] ?? current.sourceLanguage} → ${LANGS[current.targetLanguage] ?? (current.targetLanguage || '?')} · ${current.items.length} trang`;
        $('#resultList').innerHTML = current.items.map(resultCardHtml).join('');
    }

    async function retry(index) {
        const item = current.items[index];
        const card = document.querySelector(`#resultList [data-page="${item.pageNumber}"]`);
        card?.querySelectorAll('button').forEach((b) => (b.disabled = true));
        await generateForItem(item);
        renderResult();
    }

    // ---------- Danh sách dự án ----------
    function fmtDate(iso) {
        if (!iso) return '';
        const d = new Date(/[zZ]|[+-]\d\d:\d\d$/.test(iso) ? iso : iso + 'Z');   // server lưu UTC
        return isNaN(d) ? iso : d.toLocaleString('vi-VN');
    }

    async function loadProjects() {
        showAlert('#projectsAlert', '');
        const body = $('#projectRows');
        body.innerHTML = '<tr><td colspan="5" class="text-center text-muted py-3">Đang tải…</td></tr>';
        try {
            const list = await Api.listProjects();
            body.innerHTML = list.length
                ? list.map((p) => `
                    <tr data-id="${p.id}">
                        <td>${esc(p.name)}</td>
                        <td>${esc(LANGS[p.sourceLanguage] ?? p.sourceLanguage)}</td>
                        <td class="text-end">${p.pageCount}</td>
                        <td>${esc(fmtDate(p.createdAt))}</td>
                        <td class="text-end text-nowrap">
                            <button class="btn btn-sm btn-outline-primary" data-action="open">Xem</button>
                            <button class="btn btn-sm btn-outline-danger" data-action="delete">Xóa</button>
                        </td>
                    </tr>`).join('')
                : '<tr><td colspan="5" class="text-center text-muted py-3">Chưa có dự án nào.</td></tr>';
        } catch (e) {
            body.innerHTML = '';
            showAlert('#projectsAlert', e.message);
        }
    }

    // Mở dự án: đọc trực tiếp từ MySQL qua GET /api/projects/{id}/view (không còn dựa vào localStorage)
    async function openProject(id) {
        const view = await Api.getProjectView(id);
        const items = (view.pages ?? []).map((p) => {
            const hasImage = /^\/uploads\//i.test(p.imageUrl ?? '');
            return {
                pageNumber: p.pageNumber,
                previewUrl: hasImage ? p.imageUrl : null,
                imageUrl: hasImage ? p.imageUrl : '',
                kind: hasImage ? 'image' : 'text',
                fileName: '',
                originalText: p.originalText ?? '',
                translatedText: p.translatedText || null,
                audioUrl: Api.toPlayableUrl(p.audioUrl),
                error: p.audioUrl ? null : 'Chưa có audio.'
            };
        });

        if (!items.length) {
            return showAlert('#projectsAlert', 'Dự án này chưa có trang nào được dịch và lưu.', 'info');
        }

        current = {
            project: { id: view.projectId, name: view.projectName || 'Dự án' },
            sourceLanguage: view.sourceLanguage || 'en',
            targetLanguage: view.language || '',
            items
        };
        renderResult();
        showTab('#tab-result');
    }

    async function onProjectAction(ev) {
        const btn = ev.target.closest('button[data-action]');
        if (!btn) return;
        const row = btn.closest('tr');
        const id = row.dataset.id;

        if (btn.dataset.action === 'open') {
            btn.disabled = true;
            try {
                await openProject(id);
            } catch (e) {
                showAlert('#projectsAlert', e.message);
            } finally {
                btn.disabled = false;
            }
        } else if (btn.dataset.action === 'delete') {
            if (!confirm(`Xóa dự án “${row.children[0].textContent}”? Hành động này không hoàn tác được.`)) return;
            try {
                await Api.deleteProject(id);
                if (current?.project.id === id) { current = null; renderResult(); }
                loadProjects();
            } catch (e) {
                showAlert('#projectsAlert', e.message);
            }
        }
    }

    // ---------- Gắn sự kiện ----------
    function bind() {
        const dz = $('#dropzone');
        const input = $('#fileInput');

        dz.addEventListener('click', () => input.click());
        dz.addEventListener('keydown', (e) => { if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); input.click(); } });
        input.addEventListener('change', () => { addFiles(input.files); input.value = ''; });
        ['dragenter', 'dragover'].forEach((t) => dz.addEventListener(t, (e) => { e.preventDefault(); dz.classList.add('dragover'); }));
        ['dragleave', 'drop'].forEach((t) => dz.addEventListener(t, (e) => { e.preventDefault(); dz.classList.remove('dragover'); }));
        dz.addEventListener('drop', (e) => addFiles(e.dataTransfer.files));

        $('#btnAddSlide').addEventListener('click', addEmptySlide);
        $('#createForm').addEventListener('submit', onSubmit);

        $('#slideList').addEventListener('input', (e) => {
            const area = e.target.closest('textarea[data-action="text"]');
            if (!area) return;
            const s = slides.find((x) => x.id === Number(area.closest('.slide-card').dataset.id));
            if (s) { s.text = area.value; updateCounter(s); }
        });
        $('#slideList').addEventListener('click', (e) => {
            const btn = e.target.closest('button[data-action="remove"]');
            if (!btn) return;
            const id = Number(btn.closest('.slide-card').dataset.id);
            slides = slides.filter((x) => x.id !== id);
            renderSlides();
        });

        $('#resultList').addEventListener('click', (e) => {
            const btn = e.target.closest('button[data-action="retry"]');
            if (btn) retry(Number(btn.dataset.index));
        });
        // Sự kiện "ended" không nổi bọt nên bắt ở pha capture để tự phát trang kế tiếp
        $('#resultList').addEventListener('ended', (e) => {
            if (!$('#autoNext').checked) return;
            const all = [...document.querySelectorAll('#resultList audio')];
            all[all.indexOf(e.target) + 1]?.play().catch(() => { /* trình duyệt chặn autoplay: bỏ qua */ });
        }, true);

        $('#tab-projects').addEventListener('shown.bs.tab', loadProjects);
        $('#btnReload').addEventListener('click', loadProjects);
        $('#projectRows').addEventListener('click', onProjectAction);
    }

    fillLanguageSelects();
    renderSlides();
    renderResult();
    bind();
})();
