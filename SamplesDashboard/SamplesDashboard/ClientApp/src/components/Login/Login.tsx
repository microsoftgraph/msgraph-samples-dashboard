// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useEffect, useState } from 'react';
import { Dropdown, Nav, NavDropdown, NavItem } from 'react-bootstrap';
import {
  AuthenticatedTemplate,
  UnauthenticatedTemplate,
  useMsal,
} from '@azure/msal-react';
import { IPublicClientApplication } from '@azure/msal-browser';
import {
  AuthProviderCallback,
  Client,
} from '@microsoft/microsoft-graph-client';
import { User } from 'microsoft-graph';

import AppUser from '../../types/AppUser';
import { useAppContext } from '../../AppContext';
import { authConfig } from '../../AuthConfig';

interface UserAvatarProps {
  user?: AppUser;
}

function UserAvatar(props: UserAvatarProps) {
  // If a user avatar is available, return an img tag with the pic
  return (
    <img
      src={props.user?.avatar || '/images/g-raph.png'}
      alt='user'
      className='rounded-circle align-self-center mr-2'
      style={{ width: '32px' }}
    ></img>
  );
}

async function getUser(
  msalInstance: IPublicClientApplication
): Promise<AppUser | undefined> {
  const graphClient = Client.init({
    authProvider: async (done: AuthProviderCallback) => {
      try {
        // First attempt a silent access token request
        const account = msalInstance.getActiveAccount();
        if (!account) {
          throw new Error('login_required');
        }

        // Get the access token silently
        // If the cache contains a non-expired token, this function
        // will just return the cached token. Otherwise, it will
        // make a request to the Azure OAuth endpoint to get a token
        const silentResult = await msalInstance.acquireTokenSilent({
          scopes: ['https://graph.microsoft.com/.default'],
          account: account,
        });

        done(null, silentResult.accessToken);
      } catch (err: any) {
        // If a silent request fails, it may be because the user needs
        // to login or grant consent to one or more of the requested scopes
        if (isInteractionRequired(err)) {
          const interactiveResult = await msalInstance.acquireTokenPopup({
            scopes: ['https://graph.microsoft.com/.default'],
          });

          done(null, interactiveResult.accessToken);
        } else {
          done(err, null);
        }
      }
    },
  });

  const user: User = await graphClient
    .api('/me')
    .select('displayName,givenName,mail,userPrincipalName')
    .get();

  const photoStream: ReadableStream = await graphClient
    .api('/me/photos/48x48/$value')
    .getStream();

  const response = new Response(photoStream);
  const photoBlob = await response.blob();
  const photoUrl = URL.createObjectURL(photoBlob);

  return {
    displayName: user.givenName || user.displayName || '',
    email: user.mail || user.userPrincipalName || '',
    avatar: photoUrl,
  };
}

// Check an error returned by MSAL to see
// if the error indicates an interactive auth prompt
// is needed
function isInteractionRequired(error: Error): boolean {
  if (!error.message || error.message.length <= 0) {
    return false;
  }

  return (
    error.message.indexOf('consent_required') > -1 ||
    error.message.indexOf('interaction_required') > -1 ||
    error.message.indexOf('login_required') > -1 ||
    error.message.indexOf('no_account_in_silent_request') > -1
  );
}

export default function Login() {
  const app = useAppContext();
  const msal = useMsal();

  const [user, setUser] = useState<AppUser>();

  const signIn = async () => {
    await msal.instance.loginPopup({
      scopes: [`${authConfig.clientId}/.default`],
      prompt: 'select_account',
    });

    // Get the user from Microsoft Graph
    const user = await getUser(msal.instance);

    setUser(user);
  };

  const signOut = async () => {
    await msal.instance.logoutPopup();
    msal.instance.setActiveAccount(null);
    setUser(undefined);
  };

  useEffect(() => {
    const checkUser = async () => {
      if (!user) {
        try {
          // Check if user is already signed in
          const account = msal.instance.getActiveAccount();
          if (account) {
            // Get the user from Microsoft Graph
            const user = await getUser(msal.instance);

            setUser(user);
          }
        } catch (err: any) {
          app.displayError!(err.message);
        }
      }
    };
    checkUser();
    // Empty dependency array on this effect because
    // it's a one-time check on load to see if the user
    // is already signed in (exists in MSAL cache)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return (
    <div>
      <AuthenticatedTemplate>
        <NavDropdown
          title={<UserAvatar user={user} />}
          id='user-dropdown'
          align='end'
        >
          <h5 className='dropdown-item-text mb-0'>{user?.displayName}</h5>
          <p className='dropdown-item-text text-muted mb-0'>{user?.email}</p>
          <Dropdown.Divider />
          <Dropdown.Item onClick={signOut}>Sign Out</Dropdown.Item>
        </NavDropdown>
      </AuthenticatedTemplate>
      <UnauthenticatedTemplate>
        <NavItem>
          <Nav.Link onClick={signIn}>Sign In</Nav.Link>
        </NavItem>
      </UnauthenticatedTemplate>
    </div>
  );
}
