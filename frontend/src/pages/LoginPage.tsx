import { useState, type FormEvent } from "react";
import { Link, Navigate, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { ApiError } from "../api/client";

export function LoginPage() {
  const { user, loading, isAdmin, login } = useAuth();
  const navigate = useNavigate();
  const [email, setEmail] = useState("admin@admin.com");
  const [password, setPassword] = useState("Password123!");
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  if (!loading && user) {
    return <Navigate to={isAdmin ? "/admin/resources" : "/resources"} replace />;
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setSubmitting(true);
    try {
      await login(email, password);
      navigate("/resources");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Kunde inte logga in");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-gray-50 dark:bg-gray-950 px-4">
      <div className="w-full max-w-sm rounded-xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-6 shadow-sm">
        <h1 className="text-xl font-semibold text-gray-900 dark:text-gray-100 mb-1">Logga in</h1>
        <p className="text-sm text-gray-500 dark:text-gray-400 mb-6">Innovia Hub demo</p>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              E-post
            </label>
            <input
              type="email"
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="w-full rounded-md border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 px-3 py-2 text-base sm:text-sm text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-indigo-500"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              Lösenord
            </label>
            <input
              type="password"
              required
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="w-full rounded-md border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 px-3 py-2 text-base sm:text-sm text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-indigo-500"
            />
          </div>
          {error && <p className="text-sm text-red-600">{error}</p>}
          <button
            type="submit"
            disabled={submitting}
            className="w-full rounded-md bg-indigo-600 px-3 py-2 text-sm font-medium text-white hover:bg-indigo-500 disabled:opacity-50"
          >
            {submitting ? "Loggar in..." : "Logga in"}
          </button>
        </form>
        <p className="mt-4 text-center text-sm text-gray-500 dark:text-gray-400">
          Inget konto?{" "}
          <Link to="/register" className="font-medium text-indigo-600 hover:text-indigo-500">
            Registrera dig
          </Link>
        </p>
        <p className="mt-4 text-xs text-gray-400 dark:text-gray-500">
          Demo-admin: admin@admin.com / Password123!
        </p>
      </div>
    </div>
  );
}
