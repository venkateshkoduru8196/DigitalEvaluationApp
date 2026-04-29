import { createContext, useState, useEffect, useContext } from "react";
import API from "../api/axios";

export const AuthContext = createContext();

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);

  useEffect(() => {
    const storedUsername = localStorage.getItem("username");
    const token = localStorage.getItem("token");

    if (storedUsername && token) {
      setUser({ username: storedUsername });
    } else {
      setUser(null);
    }
  }, []);

  // Login
  const login = async (formData) => {
    const res = await API.post("/Auth/login", formData);

    localStorage.setItem("token", res.data.token);
    localStorage.setItem("username", res.data.username);

    setUser({ username: res.data.username });

    return res.data;
  };

  // Logout
  const logout = () => {
    localStorage.removeItem("token");
    localStorage.removeItem("username");

    setUser(null);
  };

  return (
    <AuthContext.Provider value={{ user, setUser, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = () => {
  return useContext(AuthContext);
};