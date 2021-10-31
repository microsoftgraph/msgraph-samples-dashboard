// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useContext, createContext, useState } from 'react';

import AppError from './types/AppError';

type AppContext = {
  error?: AppError;
  displayError?: Function;
  clearError?: Function;
  theme: string;
  changeTheme?: Function;
};

const appContext = createContext<AppContext>({
  theme: 'light',
});

export function useAppContext(): AppContext {
  return useContext(appContext);
}

interface ProvideAppContextProps {
  children: React.ReactNode;
}

export default function ProvideAppContext({
  children,
}: ProvideAppContextProps) {
  const context = useProvideAppContext();

  // Provide context to all children components
  return <appContext.Provider value={context}>{children}</appContext.Provider>;
}

// Create the app context
function useProvideAppContext(): AppContext {
  const [error, setError] = useState<AppError>();

  // Set the error in the app's context
  // so any components that render it can do so
  const displayError = (message: string, debug?: string) => {
    setError({ message, debug });
  };

  // Clear the error
  const clearError = () => {
    setError(undefined);
  };

  // The user's selected theme: 'light' or 'dark'
  const [theme, setTheme] = useState<string>(
    localStorage.getItem('selectedTheme') || 'light'
  );

  const changeTheme = (theme: string) => {
    // Remember the user's selection for return visits
    localStorage.setItem('selectedTheme', theme);
    setTheme(theme);
  };

  return {
    error,
    displayError,
    clearError,
    theme,
    changeTheme,
  };
}
