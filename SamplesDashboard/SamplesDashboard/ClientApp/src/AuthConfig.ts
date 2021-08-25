// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

export const authConfig = {
  clientId: process.env.REACT_APP_CLIENT_ID || '',
  authority: `https://login.microsoftonline.com/${process.env.REACT_APP_TENANT_ID}`,
  redirectUri: process.env.REACT_APP_REDIRECT_URI,
};
