import { createSlice, type PayloadAction } from '@reduxjs/toolkit';

export interface AuthUser {
  id: string;
  email: string;
  roles: string[];
  permissions: string[];
}

export interface AuthState {
  accessToken: string | null;
  refreshToken: string | null;
  user: AuthUser | null;
}

const initialState: AuthState = {
  accessToken: null,
  refreshToken: null,
  user: null,
};

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    setCredentials(
      state,
      action: PayloadAction<{
        accessToken: string;
        refreshToken?: string;
        user: AuthUser;
      }>,
    ) {
      state.accessToken = action.payload.accessToken;
      state.refreshToken = action.payload.refreshToken ?? null;
      state.user = action.payload.user;
    },

    setAccessToken(state, action: PayloadAction<string>) {
      state.accessToken = action.payload;
    },

    setTokenPair(
      state,
      action: PayloadAction<{
        accessToken: string;
        refreshToken?: string;
      }>,
    ) {
      state.accessToken = action.payload.accessToken;
      state.refreshToken = action.payload.refreshToken ?? null;
    },

    logout(state) {
      state.accessToken = null;
      state.refreshToken = null;
      state.user = null;
    },
  },
});

export const { setCredentials, setAccessToken, setTokenPair, logout } = authSlice.actions;

export const authReducer = authSlice.reducer;

export const selectAuth = (state: { auth: AuthState }): AuthState => state.auth;

export const selectAccessToken = (state: { auth: AuthState }): string | null =>
  state.auth.accessToken;

export const selectIsAuthenticated = (state: { auth: AuthState }): boolean =>
  state.auth.accessToken !== null;

export const selectHasRole =
  (role: string) =>
  (state: { auth: AuthState }): boolean =>
    state.auth.user?.roles.includes(role) ?? false;
