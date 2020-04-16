import * as React from 'react';
import { Link } from 'react-router-dom';
import { Navbar, NavbarBrand, NavbarToggler } from 'reactstrap';
import './NavMenu.css';

export default class NavMenu extends React.PureComponent<{}, { isOpen: boolean }> {
    public state = {
        isOpen: false
    };

    public render() {
        return (
            <header>
                <Navbar className='navbar-expand-sm navbar-toggleable-sm border-bottom box-shadow mb-3' light>
                    <div>
                        <NavbarBrand tag={Link} to='/'>DevX Dashboard</NavbarBrand>
                        <NavbarToggler onClick={this.toggle} className='mr-2'/>
                    </div>
                </Navbar>
            </header>
        );
    }

    private toggle = () => {
        this.setState({
            isOpen: !this.state.isOpen
        });
    }
}
