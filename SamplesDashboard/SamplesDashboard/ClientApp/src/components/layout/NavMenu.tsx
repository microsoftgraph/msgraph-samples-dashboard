import * as React from 'react';
import { Link } from 'react-router-dom';
import { Collapse, Container, Dropdown, DropdownItem, DropdownMenu, DropdownToggle, Navbar, NavbarBrand, NavbarToggler, NavItem, NavLink } from 'reactstrap';
import { FontIcon, getTheme, Theme, ThemeContext } from '@fluentui/react';
import { LoginMenu } from '../api-authorization/LoginMenu';
import './NavMenu.css';

export default class NavMenu extends React.PureComponent<{ setThemeMethod: any }, { isOpen: boolean, isThemeDropdownOpen: boolean }> {

    public state = {
        isOpen: false,
        isThemeDropdownOpen: false,
    };

    public render() {
        return (
            <ThemeContext.Consumer>
                {(theme: Theme | undefined) => {
                    const darkMode = theme?.semanticColors.bodyBackground == '#171717';
                    return <header>
                        <Navbar className={`navbar-expand-sm navbar-toggleable-sm border-bottom box-shadow mb-3 ${darkMode ? 'navbar-dark' : 'navbar-light'}`}>
                            <Container>
                                <NavbarBrand tag={Link} to='/'>DevX Dashboard</NavbarBrand>
                                <NavbarToggler onClick={this.toggle} className='mr-2' />
                                <Collapse className='d-sm-inline-flex flex-sm-row-reverse' isOpen={this.state.isOpen} navbar>
                                    <ul className='navbar-nav flex-grow'>
                                        <Dropdown nav active isOpen={this.state.isThemeDropdownOpen} toggle={this.toggleThemeDropdown}>
                                            <DropdownToggle nav caret>Theme</DropdownToggle>
                                            <DropdownMenu>
                                                <DropdownItem onClick={() => this.props.setThemeMethod('light')}><FontIcon iconName='Sunny' /> Light</DropdownItem>
                                                <DropdownItem onClick={() => this.props.setThemeMethod('dark')}><FontIcon iconName='ClearNight' /> Dark</DropdownItem>
                                            </DropdownMenu>
                                        </Dropdown>
                                        <NavItem active>
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
                }}
            </ThemeContext.Consumer>
        );
    }

    private toggle = () => {
        this.setState({
            isOpen: !this.state.isOpen,

        });
    }

    private toggleThemeDropdown = () => {
        this.setState({
            isThemeDropdownOpen: !this.state.isThemeDropdownOpen,
        });
    }
}
