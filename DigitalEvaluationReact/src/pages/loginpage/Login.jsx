import { useState } from "react";
import { useAuth } from "../../context/AuthContext";
import { useNavigate } from "react-router-dom";
import { GoogleLogin } from "@react-oauth/google";
import API from "../../api/axios";
import "./Login.css";

function Login() {
  const { login } = useAuth();
  const navigate = useNavigate();

  const [form, setForm] = useState({ email: "", password: "" });
  const [loading, setLoading] = useState(false);

  const handleChange = (e) => {
    setForm({ ...form, [e.target.name]: e.target.value });
  };

  const handleLogin = async (e) => {
    e.preventDefault();
    setLoading(true);
    try {
      await login(form);
      navigate("/dashboard");
    } catch (err) {
      alert(err.response?.data || "Login failed");
    } finally {
      setLoading(false);
    }
  };

  const handleGoogleSuccess = async (credentialResponse) => {
    try {
      const res = await API.post("/Auth/google-login", {
        token: credentialResponse.credential
      });
      localStorage.setItem("token", res.data.token);
      localStorage.setItem("username", res.data.username);
      navigate("/dashboard");
    } catch (err) {
      alert("Google login failed");
    }
  };

  return (
    <div className="login-container">
      {/* Brand Title OUTSIDE the box */}
      <h1 className="brand-title">Department of Evaluation</h1>

      <div className="login-box">
        <h2>Sign in</h2>
        <p className="login-subtitle">Access your account securely</p>

        <div className="google-btn">
          <GoogleLogin
            onSuccess={handleGoogleSuccess}
            onError={() => alert("Google Login Failed")}
          />
        </div>

        <div className="divider"><span>OR</span></div>

        <form onSubmit={handleLogin}>
          <input
            name="email"
            type="email"
            onChange={handleChange}
            placeholder="Email address"
            required
          />
          <input
            name="password"
            type="password"
            onChange={handleChange}
            placeholder="Password"
            required
          />
          <button type="submit" disabled={loading}>
            {loading ? "Signing in..." : "Continue"}
          </button>
        </form>

        <p className="login-footer">
          Don’t have an account?{" "}
          <span onClick={() => navigate("/register")}>Sign up</span>
        </p>
      </div>
    </div>
  );
}

export default Login;
