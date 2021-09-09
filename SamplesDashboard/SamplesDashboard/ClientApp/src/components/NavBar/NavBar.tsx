// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import {
  Container,
  Dropdown,
  Navbar,
  Nav,
  NavDropdown,
  NavItem,
} from 'react-bootstrap';
import { FontIcon } from '@fluentui/react';

import { useAppContext } from '../../AppContext';
import Login from '../Login/Login';
import './NavBar.css';

export default function NavBar() {
  const app = useAppContext();

  return (
    <Navbar
      expand='md'
      fixed='top'
      bg={app.theme}
      variant={app.theme === 'dark' ? 'dark' : 'light'}
    >
      <Container fluid>
        <Navbar.Brand href='/'>DevX Dashboard</Navbar.Brand>
        <Navbar.Toggle />
        <Navbar.Collapse>
          <Nav className='ms-auto align-items-center' navbar>
            <NavDropdown title='Theme' id='theme-dropdown'>
              <Dropdown.Item onClick={() => app.changeTheme!('light')}>
                <FontIcon iconName='Sunny' /> Light
              </Dropdown.Item>
              <Dropdown.Item onClick={() => app.changeTheme!('dark')}>
                <FontIcon iconName='ClearNight' /> Dark
              </Dropdown.Item>
            </NavDropdown>
            <NavItem>
              <Nav.Link
                href='https://github.com/microsoftgraph/msgraph-samples-dashboard/wiki/DevX-Dashboard-overview'
                target='_blank'
              >
                <FontIcon iconName='OpenInNewTab' className='link-icon' />
                Wiki
              </Nav.Link>
            </NavItem>
            <Login />
          </Nav>
        </Navbar.Collapse>
      </Container>
    </Navbar>
  );
}
