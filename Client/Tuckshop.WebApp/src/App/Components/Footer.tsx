import React from 'react';
import { observer } from 'mobx-react';
import SingularLogo from '../assets/img/singular-logo-dark.svg';

interface IFooterProps {
    
}

@observer
export default class Footer extends React.Component<IFooterProps> {

    constructor(props: IFooterProps) {
        super(props);
    }

    public render() {
        return (
            <div className="app-footer" id="footer-panel">
                Powered by <a href="https://www.singular.co.za" target="_blank"><img src={SingularLogo} alt="Singular" style={{ marginLeft: "8px", height: "24px" }} /></a>
            </div>
        );
    }
}