import API from "../api/axios";

// Register new user
export const registerUser = async (data) => {
  const res = await API.post("/Auth/register", data);
  localStorage.setItem("token", res.data.token);
  localStorage.setItem("username", res.data.username); // save username
  return res.data;
};

// Login existing user
export const loginUser = async (data) => {
  const res = await API.post("/Auth/login", data);
  localStorage.setItem("token", res.data.token);
  localStorage.setItem("username", res.data.username); // save username
  return res.data;
};

// Logout user
export const logoutUser = async () => {
  try {
    await API.post("/Auth/revokeToken");
  } catch (err) {
    console.error("Logout failed:", err);
  } finally {
    localStorage.removeItem("token");
    localStorage.removeItem("username"); // clear username
    window.location.href = "/login";
  }
};
