/* Lớp gọi API của Backend. Mọi đường dẫn endpoint gom về một chỗ để dễ đổi khi Backend hoàn thiện. */
const Api = (() => {
    // Endpoint tạo audio: ưu tiên endpoint chuẩn theo đặc tả (Lead Backend sẽ làm),
    // nếu chưa có (404/405) thì tự động dùng endpoint demo hiện có.
    const AUDIO_ENDPOINTS = [
        '/api/slides/generate-audio',
        '/api/DemoAudio/generate-speech'
    ];
    let audioIdx = 0;

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
            if (data.message) return data.message;               // ErrorResponse của nhóm / DemoAudio
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

        /**
         * Tạo thuyết minh cho một slide. Gửi kèm cả tên trường kiểu đặc tả (camelCase)
         * lẫn tên trường của endpoint demo, trường thừa sẽ bị Backend bỏ qua.
         */
        async generateAudio({ projectId, projectName, pageNumber, text, sourceLanguage, targetLanguage }) {
            const body = {
                projectId, pageNumber, sourceLanguage, targetLanguage,
                text,
                projectName,
                textContent: text
            };

            let lastErr;
            for (let i = audioIdx; i < AUDIO_ENDPOINTS.length; i++) {
                try {
                    const data = await jsonPost(AUDIO_ENDPOINTS[i], body);
                    audioIdx = i;
                    return {
                        audioUrl: toPlayableUrl(data?.audioUrl ?? data?.filePath),
                        translatedText: data?.translatedText ?? null
                    };
                } catch (e) {
                    lastErr = e;
                    if (e.status === 404 || e.status === 405) continue;   // endpoint chưa tồn tại, thử endpoint kế
                    throw e;
                }
            }
            throw lastErr;
        },

        toPlayableUrl
    };
})();
