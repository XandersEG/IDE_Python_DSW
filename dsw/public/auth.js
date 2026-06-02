(function () {
    const TOKEN_KEY = "ide_token";
    const BASE_URL = "http://138.2.235.169/dsw/public";

    function getToken() {
        return localStorage.getItem(TOKEN_KEY);
    }

    function saveToken(token) {
        localStorage.setItem(TOKEN_KEY, token);
    }

    function clearToken() {
        localStorage.removeItem(TOKEN_KEY);
    }

    function parseJwtPayload(token) {
        if (!token) return null;
        const parts = token.split(".");
        if (parts.length !== 3) return null;
        try {
            const normalized = parts[1].replace(/-/g, "+").replace(/_/g, "/");
            const padded = normalized + "=".repeat((4 - (normalized.length % 4)) % 4);
            return JSON.parse(atob(padded));
        } catch (e) {
            return null;
        }
    }

    function requireAuth() {
        const token = getToken();
        const payload = parseJwtPayload(token);
        if (!token || !payload) {
            window.location.href = '/dsw/public/';
            return null;
        }
        if (payload.exp && Date.now() >= payload.exp * 1000) {
            window.location.href = '/dsw/public/';
            return null;
        }
        return { token, payload };
    }

    function attachLogout(linkId) {
        const element = document.getElementById(linkId);
        if (!element) return;
        element.addEventListener("click", function (event) {
            event.preventDefault();
            clearToken();
            window.location.href = '/dsw/public/';
        });
    }

    async function fetchJson(ruta, opciones = {}) {
        const token = getToken();
        const headers = {
            "Content-Type": "application/json",
            ...(token && { Authorization: `Bearer ${token}` })
        };

        let url = `${BASE_URL}${ruta}`;
            if (opciones.params) {
                url += `?${new URLSearchParams(opciones.params).toString()}`;
        }
        const response = await fetch(url, {
            method: opciones.method || "GET",
            headers,
            body: opciones.body ? JSON.stringify(opciones.body) : null,
        });
        
        /*const response = await fetch(`${BASE_URL}${ruta}`, {
            method: opciones.method || "GET",
            headers,
            body: opciones.body ? JSON.stringify(opciones.body) : null,
        });*/
        return response.json();
    }

    window.IDEAuth = {
        getToken,
        saveToken,
        clearToken,
        fetchJson,
        requireAuth,
        attachLogout,
        parseJwtPayload
    };
})();