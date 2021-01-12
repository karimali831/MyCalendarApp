import * as React from 'react';
import SideNav, { NavItem, NavIcon, NavText } from '@trendmicro/react-sidenav';
import { FaCalculator, FaExchangeAlt, FaSlidersH } from 'react-icons/fa'
import ClickOutside from 'react-click-outside'
import '@trendmicro/react-sidenav/dist/react-sidenav.css';
import { IDefaultConfig } from 'src/models/IDefaultConfig';

interface IOwnState {
    expanded: boolean,
    config: IDefaultConfig,
}

interface IOwnProps {
    pinSidebar: boolean,
    expanded?: boolean,
    config: IDefaultConfig,
    updateConfig: (config: IDefaultConfig) => void,
    initialState: () => void
}

export class SidebarMenu extends React.Component<IOwnProps, IOwnState> {
    constructor(props: IOwnProps) {
        super(props);

        this.state = {
            expanded: this.props.expanded !== undefined ? this.props.expanded : false,
            config: this.props.config
        };
    }

    public componentDidUpdate = (prevProps: IOwnProps, prevState: IOwnState) => {
        if (JSON.stringify(this.state.config) !== JSON.stringify(prevState.config)) {
            this.props.updateConfig(this.state.config);
        }
    }

    public render() {
        return (
            this.props.pinSidebar ?
            <div className="navik-header.sticky .navik-header-container">
                <ClickOutside
                    onClickOutside={() => this.onSetSidebarOpen(false)} >
                    <SideNav
                        onToggle={(expanded: boolean) => this.onSetSidebarOpen(expanded)}
                        expanded={this.state.expanded} >
                        <SideNav.Toggle />
                        <SideNav.Nav selected={true}>
                            <NavItem eventKey="reset" onSelect={() => this.props.initialState()}>
                                <NavIcon>
                                    <FaExchangeAlt />
                                </NavIcon>
                                <NavText>
                                    Default Configuration
                                </NavText>
                            </NavItem>
                            <NavItem eventKey="orderFeeFormula">
                                <NavIcon>
                                    <FaCalculator />
                                </NavIcon>
                                <NavText>
                                    Order Fee Formula
                                </NavText>
                                {this.props.config.orderFeeFormula.map((f, idx) => 
                                    <NavItem key={idx} eventKey={idx} style={{color: '#222'}} >
                                        <NavText>
                                            <div className="form-group">
                                                <label htmlFor={`orderValueMin-${idx}`}>Min Order Value</label>
                                                <input type="number" className="form-control" id={`orderValueMin-${idx}`} value={f.orderValueMin} onChange={this.inputChange} />
                                            </div>
                                            <div className="form-group">
                                                <label htmlFor={`orderValueMax-${idx}`} >Max Order Value</label>
                                                <input type="number" className="form-control" id={`orderValueMax-${idx}`} value={f.orderValueMax} onChange={this.inputChange} />
                               
                                            </div>
            
                                            <div className="form-group">
                                                <label htmlFor={`fee-${idx}`}>Fee</label>
                                                <input type="number" id={`fee-${idx}`} value={f.fee} className="form-control" onChange={this.inputChange} />
                                            </div>
                                            <div className="sidenav-divider" />
                                        </NavText>
                                    </NavItem>
                                )}
                            </NavItem>

                            <NavItem eventKey="fixedFees">
                                <NavIcon>
                                    <FaSlidersH />
                                </NavIcon>
                                <NavText>
                                    Fixed Fees
                                </NavText>
                                <NavItem key="serviceFee">
                                    <NavText>
                                        <div className="form-group">
                                            <label htmlFor="serviceFee">Service Fee <small>% of Order Value</small></label>
                                            <input type="number" id="serviceFee" value={this.props.config.serviceFee} className="form-control" onChange={this.inputChange} />               
                                        </div>  
                                    </NavText>
                                </NavItem>
                                <NavItem key="deliveryFeePerMile">
                                    <NavText>
                                        <div className="form-group">
                                            <label htmlFor="deliveryFeePerMile">Delivery Fee <small>per mile</small></label>
                                            <input type="number" id="deliveryFeePerMile" value={this.props.config.deliveryFeePerMile} className="form-control" onChange={this.inputChange} />              
                                        </div> 
                                    </NavText>
                                </NavItem>
                                <NavItem key="driverFee">
                                    <NavText>
                                        <div className="form-group">
                                            <label htmlFor="driverFee">Driver Fee</label>
                                            <input type="number" id="driverFee" value={this.props.config.driverFee} className="form-control" onChange={this.inputChange} />           
                                        </div>  
                                    </NavText>
                                </NavItem>
                            </NavItem>                                    
                        </SideNav.Nav>
                    </SideNav>
                </ClickOutside>
            </div>
            : null
        );
    }

    private inputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        if (e.target.id.includes("-")) {
            // order fee formula
            const idx = e.target.id.split("-")[1];
            const name = e.target.id.split("-")[0];

            const orderFeeFormula = [...this.state.config.orderFeeFormula]
            const formula = {
                ...this.state.config.orderFeeFormula[idx],
                [name]: e.target.value
            }

            orderFeeFormula[idx] = formula

            this.setState({
                config: {
                    ...this.state.config,
                    orderFeeFormula: orderFeeFormula
                },
            })

        }
        else{
            // fixed fee
            this.setState({
                config: {
                    ...this.state.config, 
                    [e.target.id]: e.target.value
                },
            })
        }
    }

    private onSetSidebarOpen = (value: boolean) => {
        this.setState({ expanded: value })
    }
}