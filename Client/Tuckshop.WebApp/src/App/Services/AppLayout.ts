import { observable, action, makeObservable } from 'mobx';
import { injectable } from 'inversify';

export enum ScreenSize {
    ExtraSmall = 1,
    Small = 2,
    Medium = 3,
    Large = 4,
    ExtraLarge = 5,
    Huge = 6
}

export interface IAppLayout {
    thinSideBar: boolean;
    sideBarExpanded: boolean;
    readonly currentScreenSize: ScreenSize;
    menuToggle(e: React.MouseEvent<HTMLDivElement, MouseEvent>): void;
    setup(): void;
    performLayout(): void;
}

/** Indicates whether the app header should float above the content when the page is scrolled */
const allowFloatingHeader = true;
const floatingHeaderClassName = "floating-header";

/**
 * Contains logic about the layout of the app. E.g. screensize, theme etc.
 * Use AppLayout.current for the singleton instance.
 */
@injectable()
export default class AppLayout implements IAppLayout {

    private floatingHeader = false;
    private contentPadding = -1;

    constructor() {
        this.onWindowResize = this.onWindowResize.bind(this);
        this.menuToggle = this.menuToggle.bind(this);

        window.addEventListener("resize", this.onWindowResize);

        window.addEventListener("click", (e) => this.sideBarExpanded = false);
        
        document.addEventListener("scroll", this.onScroll.bind(this));

        this.onWindowResize();

        makeObservable(this);
    }

    public get thinSideBar() {
        return (this._thinSideBar || this.currentScreenSize <= ScreenSize.ExtraLarge) && this.currentScreenSize > ScreenSize.Small;
    }
    public set thinSideBar(value: boolean) {
        this._thinSideBar = value;
        this.sideBarExpanded = false;
    }

    @observable
    public _thinSideBar = false;

    @observable
    public sideBarExpanded = false;

    @observable.ref
    public _currentScreenSize = ScreenSize.Medium;

    public get currentScreenSize() {
        return this._currentScreenSize;
    }
    private set currentScreenSize(value: ScreenSize) {
        if (value !== this._currentScreenSize) {
            this._currentScreenSize = value;

            this.thinSideBar = value <= ScreenSize.ExtraLarge;
        }
    }

    @action
    private onWindowResize() {
        
        if (window.innerWidth < 576) {
            this.currentScreenSize = ScreenSize.ExtraSmall;
        } else if (window.innerWidth < 768) {
            this.currentScreenSize = ScreenSize.Small;
        } else if (window.innerWidth < 992) {
            this.currentScreenSize = ScreenSize.Medium;
        } else if (window.innerWidth < 1200) {
            this.currentScreenSize = ScreenSize.Large;
        } else if (window.innerWidth < 1360) {
            this.currentScreenSize = ScreenSize.ExtraLarge;
        } else {
            this.currentScreenSize = ScreenSize.Huge;
        }

        this.performLayout();
    }

    public performLayout() {
        if (this.header) {

            const footerMargin = parseInt(window.getComputedStyle(this.footer!).marginTop);
            this.contentPanel!.style.minHeight = (window.innerHeight - this.footer!.clientHeight - footerMargin) + "px";
        }
    }

    private onScroll() {
        if (allowFloatingHeader && this.contentPanel && this.header) {

            if (this.currentScreenSize > ScreenSize.Large && window.scrollY > this.contentPadding) {
                if (!this.floatingHeader) {
                    
                    const headerHeight = this.header.clientHeight;
                    this.contentPanel.style.paddingTop = headerHeight + this.contentPadding + "px";
                    this.header.classList.add(floatingHeaderClassName);
                    this.header.style.width = this.contentPanel.clientWidth + "px";
                    this.floatingHeader = true;
                }
                
            } else if (this.floatingHeader) {
                this.contentPanel.style.paddingTop = "";
                this.header.classList.remove(floatingHeaderClassName);
                this.header.style.width = "";
                this.floatingHeader = false;
            }
        }
    }

    private header?: HTMLDivElement;
    private footer?: HTMLDivElement;
    private contentPanel?: HTMLDivElement;

    public setup() {
        this.header = document.getElementById("header-panel") as HTMLDivElement;
        this.footer = document.getElementById("footer-panel") as HTMLDivElement;
        this.contentPanel = document.getElementById("content-panel") as HTMLDivElement;
        this.contentPadding = parseInt(window.getComputedStyle(this.contentPanel).paddingTop);

        this.onWindowResize();
    }

    public menuToggle(e: React.MouseEvent<HTMLDivElement, MouseEvent>) {
        if (this.currentScreenSize <= ScreenSize.ExtraLarge) {
            this.sideBarExpanded = !this.sideBarExpanded;
        } else {
            this.thinSideBar = !this._thinSideBar;
        }
        e.stopPropagation();
    }

    public static isChildOf(element : Element, parent : Element) {
        while (true) {
            if (element === parent) {
                return true;
            }
            if (element.parentElement) {
                element = element.parentElement;
            } else {
                return false;
            }
        }
    }
}