import axios from "axios";

const API = axios.create({
  baseURL: "https://app.genbasesoftware.com/api",
  withCredentials: true
});

// =============================
// ATTACH JWT TOKEN TO REQUESTS
// =============================
API.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem("token");

    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }

    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// =============================
// HANDLE TOKEN EXPIRY / REFRESH
// =============================
API.interceptors.response.use(
  (response) => response,

  async (error) => {
    const originalRequest = error.config;

    // Safety check
    if (!originalRequest) {
      return Promise.reject(error);
    }

    // If unauthorized, token expired, and not already retrying
    if (
      error.response?.status === 401 &&
      !originalRequest._retry &&
      !originalRequest.url.includes("/Auth/refreshToken")
    ) {
      originalRequest._retry = true;

      try {
        // Attempt refresh token
        const res = await axios.get(
          "https://app.genbasesoftware.com/api/Auth/refreshToken",
          {
            withCredentials: true,
          }
        );

        // Save new token
        localStorage.setItem("token", res.data.token);

        // Re-attach token
        originalRequest.headers.Authorization = `Bearer ${res.data.token}`;

        // Retry original request
        return API(originalRequest);

      } catch (refreshError) {
        console.error("Refresh failed:", refreshError);

        // Clear stored auth
        localStorage.removeItem("token");
        localStorage.removeItem("username");

        // Prevent redirect loop if already on login
        if (window.location.pathname !== "/login") {
          window.location.href = "/login";
        }

        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  }
);

export default API;