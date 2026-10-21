/* Lớp gọi API của Backend. Mọi đường dẫn endpoint gom về một chỗ để dễ đổi khi Backend hoàn thiện. */
const Api = (() => {
    // Endpoint production xử lý bản dịch và audio cho các slide.
    const AUDIO_ENDPOINT = '/api/Slides/process-presentation';

    async function request(url, options = {}) {
        let res;
        try {
            res = await fetch(url, options);
        } catch (e) {
            throw new Error('Không kết nối được tới máy chủ. Hãy kiểm tra API đã chạy chưa.');
        }

        if (res.status === 204) return null;

        const text = await res.text();
        let data = null;
        try { data = text ? JSON.parse(text) : null; } catch { /* không phải JSON */ }

        if (!res.ok) {
            const err = new Error(extractMessage(data, text, res.status));
            err.status = res.status;
            throw err;
        }
        return data;
    }

    function extractMessage(data, text, status) {
        if (data) {
            if (data.message) return data.message;
            if (data.error) return data.error;
            if (data.errors) {                                    // ValidationProblemDetails của ASP.NET
                return Object.values(data.errors).flat().join(' ');
            }
            if (data.title) return data.title;
        }
        return text && text.length < 200 ? text : `Lỗi máy chủ (HTTP ${status}).`;
    }

    const jsonPost = (url, body) => request(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(body)
    });

    /** Chuyển đường dẫn Backend trả về thành URL mà thẻ <audio> phát được. */
    function toPlayableUrl(raw) {
        if (!raw) return null;
        const basename = raw.split(/[\\/]/).pop();
        if (/OutputAudio/i.test(raw)) return '/audio/' + basename;   // đường dẫn file trên ổ đĩa server
        if (/^https?:\/\//i.test(raw) || raw.startsWith('/')) return raw;
        return '/audio/' + basename;
    }

    return {
        /** POST /api/projects */
        createProject: (name, sourceLanguage) =>
            jsonPost('/api/projects', { name, sourceLanguage }),

        /** GET /api/projects */
        listProjects: () => request('/api/projects'),

        /** DELETE /api/projects/{id} */
        deleteProject: (id) => request(`/api/projects/${id}`, { method: 'DELETE' }),

        /** POST /api/Slides/upload-slides */
        uploadSlides: (files) => {
            const formData = new FormData();
            for (const file of files) formData.append('files', file);
            return request('/api/Slides/upload-slides', {
                method: 'POST',
                body: formData
            });
        },

        /** Tạo bản dịch và thuyết minh cho một slide. */
        async generateAudio({ projectName, pageNumber, text, sourceLanguage, targetLanguage }) {
            const body = {
                projectName,
                sourceLanguage,
                targetLanguage,
                slides: [{
                    pageNumber,
                    narrativeText: text,
                    imagePath: ''
                }]
            };

            const data = await jsonPost(AUDIO_ENDPOINT, body);
            const result = data?.results?.[0];
            return {
                audioUrl: toPlayableUrl(result?.audioUrl),
                translatedText: result?.translatedText ?? null
            };
        },

        toPlayableUrl
    };
})();
