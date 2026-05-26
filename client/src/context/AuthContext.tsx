import { createContext, useContext, useState, useEffect, type ReactNode } from 'react';
import api from '../api/axios';

interface AuthUser {
  userId: string;
  email: string;
  role: string;
}

interface AuthContextType {
  user: AuthUser | null;
  token: string | null;
  login: (email: string, password: string) => Promise<string | null>;
  register: (email: string, password: string, role: string) => Promise<string | null>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setToken] = useState<string | null>(localStorage.getItem('token'));
  const [user, setUser] = useState<AuthUser | null>(null);

  useEffect(() => {
    if (token) {
      api.get('/auth/me').then((res) => {
        if (res.data.success) {
          setUser({ userId: res.data.userId, email: res.data.email, role: res.data.role });
        }
      }).catch(() => logout());
    }
  }, [token]);

  const login = async (email: string, password: string): Promise<string | null> => {
    try {
      const res = await api.post('/auth/login', { email, password });
      if (res.data.success) {
        localStorage.setItem('token', res.data.token);
        setToken(res.data.token);
        setUser({ userId: res.data.userId, email: res.data.email, role: res.data.role });
        return null;
      }
      return res.data.message;
    } catch (err: any) {
      return err.response?.data?.message || 'Login failed';
    }
  };

  const register = async (email: string, password: string, role: string): Promise<string | null> => {
    try {
      const res = await api.post('/auth/register', { email, password, role });
      if (res.data.success) {
        localStorage.setItem('token', res.data.token);
        setToken(res.data.token);
        setUser({ userId: res.data.userId, email: res.data.email, role: res.data.role });
        return null;
      }
      return res.data.message;
    } catch (err: any) {
      return err.response?.data?.message || 'Registration failed';
    }
  };

  const logout = () => {
    localStorage.removeItem('token');
    setToken(null);
    setUser(null);
  };

  return (
    <AuthContext.Provider value={{ user, token, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within AuthProvider');
  return ctx;
}
