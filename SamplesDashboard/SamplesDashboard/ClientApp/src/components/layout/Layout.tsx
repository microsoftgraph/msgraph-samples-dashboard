import * as React from 'react';
import { Container } from 'reactstrap';
import NavMenu from './NavMenu';

export default (props: { children?: React.ReactNode, setThemeMethod: (theme: string) => void }) => (
    <React.Fragment>
        <NavMenu setThemeMethod={props.setThemeMethod} />
        <Container>
            {props.children}
        </Container>
    </React.Fragment>
);
