/* tslint:disable:max-classes-per-file */
import React from 'react';
import { observer } from 'mobx-react';
import { observable, makeObservable } from 'mobx';
import Scrollbar from 'react-custom-scrollbars';
import { AppService, Types } from '../Services/AppService';
import { IAppLayout, ScreenSize } from '../Services/AppLayout';
import { IAppMenuItem } from "../Services/RouteService";
import sidebarBackground from '../assets/img/mikes-mart.png';
import { Neo } from '@singularsystems/neo-react';

interface ISidebarProps {
    /** Shows a prefix of the menu item name if there is no icon */
    showPrefixes: boolean;

    /** Will only show the top level menu items if the sidebar is in it's narrow view. */
    singleLevelOnCollapse: boolean;
}

@observer
class Sidebar extends React.Component<ISidebarProps> {

    private routeService = AppService.get(Types.App.Services.RouteService);
    private appLayout = AppService.get(Types.App.Services.AppLayout);

    constructor(props: any) {
        super(props);
    }

    public render() {
        const layout = this.appLayout;
        const menuItemProvider = AppService.get(Types.Neo.Routing.MenuItemProvider);
        const menuItems = menuItemProvider.processRoutes(this.routeService.routes.menuRoutes, { collapseSingle: false, hideWhenEmpty: true });

        return (
            <div>
                <div className="menu-overlay"></div>
                <div
                    id="left-panel"
                    className="app-left-panel">

                    <div className="sidebar">
                        <div className="sidebar-header">
                            {/* Logo */}
                            <img src={sidebarBackground} alt="" />
                            {/* Menu toggle button */}
                            {layout.currentScreenSize > ScreenSize.Small &&
                                <span className={"menu-toggle-container"} onClick={layout.menuToggle}>
                                    {this.appLayout.thinSideBar && !this.appLayout.sideBarExpanded ? <Neo.Icon name="arrow_forward_ios" /> : <Neo.Icon name="arrow_back_ios_new" />}
                                </span>
                            }
                        </div>

                        <div className="sidebar-content">
                            {/* Full screen menu */}
                            {layout.currentScreenSize > ScreenSize.Small &&
                                <Scrollbar height="100%" hideTracksWhenNotNeeded>
                                    <Menu items={menuItems} level={1} appLayout={this.appLayout} sidebarProps={this.props} />
                                </Scrollbar>}

                            {/* Small screen menu */}
                            {layout.currentScreenSize <= ScreenSize.Small &&
                                <Menu items={menuItems} level={1} appLayout={this.appLayout} sidebarProps={this.props} />}
                        </div>
                    </div>
                </div>
            </div>
        );
    }
}

interface IMenuProps {
    appLayout: IAppLayout;
    items: IAppMenuItem[];
    level: number;
    sidebarProps: ISidebarProps;
}

@observer
class Menu extends React.Component<IMenuProps> {

    public render() {

        return (
            <ul className={"menu-level-" + this.props.level}>
                {this.props.items.map(item => <MenuItem key={item.path || item.name} item={item} level={this.props.level} appLayout={this.props.appLayout} sidebarProps={this.props.sidebarProps} />)}
            </ul>
        )
    }
}

interface IMenuItemProps {
    appLayout: IAppLayout;
    item: IAppMenuItem;
    level: number;
    sidebarProps: ISidebarProps;
}

@observer
class MenuItem extends React.Component<IMenuItemProps> {

    @observable.ref
    public isExpanded = true;

    constructor(props: IMenuItemProps) {
        super(props);

        if (props.item.children && props.item.expanded !== undefined) {
            this.isExpanded = props.item.expanded;
        }

        this.onExpanderClick = this.onExpanderClick.bind(this);

        makeObservable(this);
    }

    private onExpanderClick(e: React.MouseEvent<HTMLSpanElement, MouseEvent>) {
        if (this.props.item.children) {
            e.stopPropagation();
            e.preventDefault();

            const appLayout = this.props.appLayout;
            if (appLayout.thinSideBar && !appLayout.sideBarExpanded) {
                // Do nothing, to prevent header click from doing anything when the menu is collapsed
            } else {
                this.isExpanded = !this.isExpanded;
            }
        }
    }

    public render() {
        const item = this.props.item;
        const hasChildren = !!item.children;
        const isHeader = item.header ?? (!item.path && this.props.level === 1);
        const sidebarIsCollapsed = this.props.appLayout.thinSideBar && !this.props.appLayout.sideBarExpanded;
        const isExpanded = this.isExpanded && !(this.props.sidebarProps.singleLevelOnCollapse && sidebarIsCollapsed && this.props.level >= 2)

        let fakeIconText = "";
        let showIcon = !!item.icon;
        if (!isHeader && !item.icon && this.props.level <= 2) {
            if (this.props.sidebarProps.showPrefixes) {
                let matches = item.name.match(/\b(\w)/g);
                let acronym = matches?.join('');
                if (acronym) {
                    fakeIconText = acronym.substr(0, 2);
                }
            }
            showIcon = true;
        }

        let icon = !showIcon ? null : (<span className="sidebar-icon">{item.icon ? <Neo.Icon name={item.icon} fixedWidth /> : <span className="fake-icon">{fakeIconText}</span>}</span>);
        let itemContent = <>
            <div className="menu-item-content">
                {icon}
                <span className="menu-item-text">{item.name}</span>
            </div>
            {hasChildren &&
                <div className={"menu-expander " + (isExpanded ? "expanded" : "collapsed")} onClick={this.onExpanderClick}>
                    <Neo.Icon name={isExpanded ? "expand_more" : "chevron_right"} />
                </div>
            }
        </>

        return (
            <li>
                {!(isHeader && !item.children && this.props.appLayout.thinSideBar && !this.props.appLayout.sideBarExpanded) && // This condition hides section headers when menu is collapsed
                    <div className={"menu-item" + (hasChildren ? " has-children" : "") + (isExpanded ? " is-expanded" : "") + (isHeader ? " section-header" : "")}
                        data-tip={sidebarIsCollapsed ? item.name : null} data-tip-pos="right">

                        {item.path ?
                            item.component === undefined ?
                                <a href={item.path} target="_blank">{itemContent}</a> :
                                <Neo.Link isNav exact={item.exact || (hasChildren && isExpanded)} to={item.path}>
                                    {itemContent}
                                </Neo.Link> :
                            <span className="static-item" onClick={this.onExpanderClick}>
                                {itemContent}
                            </span>
                        }
                    </div>
                }

                {isExpanded && item.children && <Menu items={item.children} level={this.props.level + 1} appLayout={this.props.appLayout} sidebarProps={this.props.sidebarProps} />}
            </li>)
    }
}

export default Sidebar;