import * as React from 'react';
import { Link } from 'react-router-dom';
import { Collapse, Container, Navbar, NavbarBrand, NavbarToggler, NavItem, NavLink } from 'reactstrap';
import { FontIcon } from '@fluentui/react';
import { LoginMenu } from '../api-authorization/LoginMenu';
import './NavMenu.css';

export default class NavMenu extends React.PureComponent<{}, { isOpen: boolean }> {
    public state = {
        isOpen: false,
    };

    public render() {
        return (
            <header>
                <Navbar className='navbar-expand-sm navbar-toggleable-sm border-bottom box-shadow mb-3' light>
                    <Container>
                        <NavbarBrand tag={Link} to='/'>DevX Dashboard</NavbarBrand>
                        <NavbarToggler onClick={this.toggle} className='mr-2' />
                        <Collapse className='d-sm-inline-flex flex-sm-row-reverse' isOpen={this.state.isOpen} navbar>
                            <ul className='navbar-nav flex-grow'>
                                <NavItem>
                                    <NavLink target='_blank' href='https://github.com/microsoftgraph/msgraph-samples-dashboard/wiki/DevX-Dashboard-overview'>
                                        <FontIcon iconName='OpenInNewTab' className='link-icon' />
                                        Wiki
                                    </NavLink>
                                </NavItem>
                                <LoginMenu>
                                </LoginMenu>
                            </ul>
                        </Collapse>
                    </Container>
                </Navbar>
            </header>
        );
    }

    private toggle = () => {
        this.setState({
            isOpen: !this.state.isOpen,

        });
    }
}
