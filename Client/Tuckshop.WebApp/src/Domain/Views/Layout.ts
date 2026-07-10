// Auto Generated
// The code below was generated using the data-code-name attributes in Layout.tsx.

export const Layout = [{ language: "jsx", code: 
`<p>
    Bootstrap allows you to layout content using the <code>row</code> and <code>col-</code> css classes. If you need elements to flow vertically
    this is easy to write (two column divs nested inside a row div). Creating a layout that flows horizontally is a lot more difficult.
</p>
<p>
    The <code>Neo.GridLayout</code> component makes this easy if you are happy that each cell is of equal width.
    Each direct child of <code>Neo.GridLayout</code> will be wrapped in a div with the bootstrap column class.
</p>
<p>
    You can specify how many columns to display for each screen size. Resize the browser window to see this work. If you don't specify any props,
    it will default to 2 columns above medium screen size, and 1 column below.
</p>
<p>
    <code>Neo.GridLayout</code>s can be nested. If you need to put multiple elements in a 'cell', wrap them in a nested grid layout, or just
    use a <code>div</code>.
</p>
<Neo.GridLayout withGaps md={2} xl={3} xxl={4} >
    <div className="box">One</div>
    <div className="box">Two</div>
    <div className="box">Three</div>
    <div className="box">Four</div>
    <Neo.GridLayout withGaps gutterSize={1} >
        <div className="box">Five</div>
        <div className="box">Six</div>
    </Neo.GridLayout>
</Neo.GridLayout>`}];

export const LayoutFlex = [{ language: "jsx", code: 
`<p>
    The <code>Neo.GridLayout</code> component also allows dynamic spacing of columns using flexbox. In the example below, each element will be spaced as far apart as possible.
    This is useful for toolbars where you have a left and right section.
</p>

<Neo.GridLayout justifyContent="between" alignItems="center" className="mt-3">
    <Neo.Button icon={Misc.Settings.icons.backIcon} isOutline>Back</Neo.Button>
    <div>Middle</div>
    <Neo.Button>Save</Neo.Button>
</Neo.GridLayout>

<div className="mt-5">
    <h5>Wrapping</h5>
    <p>
        To make the left and right sections wrap when the screen is small, add the bootstrap <code>flex-wrap</code> class.
    </p>

    <Neo.GridLayout justifyContent="between" className="flex-wrap" withGaps>
        <Neo.Button icon={Misc.Settings.icons.backIcon} isOutline>Back</Neo.Button>
        <div>
            <Neo.Button>Button 1</Neo.Button>
            <Neo.Button className="ms-4">Button 2</Neo.Button>
            <Neo.Button className="ms-4">Save</Neo.Button>
        </div>
    </Neo.GridLayout>
</div>`}];

export const LayoutCardInCard = [{ language: "jsx", code: 
`You should try not nest cards within other cards.

<Neo.Card title="Nested Card" className="mt-3">
    Styling for nested cards is slightly different to root cards in case nesting is unavoidable.
    
</Neo.Card>

<Neo.Card title="Shadows" className="mt-3 shadow">

    To add a shadow, you can use the <code>shadow</code> class. This can be applied to any component, and should be used on components that are children of other root components.
</Neo.Card>`}];

export const CardAccordion = [{ language: "jsx", code: 
`<p>
    If you need to control whether a card is collapsed or not from code, you can bind to the cards expanded state using the <code>isExpanded</code> prop.
    You can also use this to build an accordion where only a single card in a group is visible at a time.
</p>

<Neo.Card title="Section 1" isExpanded={new ValueSwitch(viewModel.meta.expandedCard, "Section 1")}>
    Content of section 1
</Neo.Card>
<Neo.Card title="Section 2" isExpanded={new ValueSwitch(viewModel.meta.expandedCard, "Section 2")}>
    Content of section 2
</Neo.Card>`}, { language: "javascript", title: "Property on ViewModel", code: `public expandedCard: string = "Section 1";`}, { language: "javascript", title: "ValueSwitch helper", code: `class ValueSwitch extends Model.ValueWrapper<boolean> {
    constructor(bind: Model.IPropertyInstance<string>, value: string) {
        super(() => bind.value === value, isSelected => bind.value = isSelected ? value : "");
    }
}`}];

export const LayoutFilledCard = [{ language: "jsx", code: 
`Card headers can be filled by adding the <code>card-header-filled</code> class. These cards also have their shadows restored using the <code>shadow</code> class.

<Neo.GridLayout className="mt-3" md={3}>

    <Neo.Card title="In progress" className="card-header-filled shadow">
        <strong className="d-flex mb-3 justify-content-center">20/75</strong>
        <Neo.ProgressBar progress={0.27} />
    </Neo.Card>

    <Neo.Card title="Completed" className="card-header-filled shadow">
        <strong className="d-flex mb-3 justify-content-center">45/75</strong>
        <Neo.ProgressBar progress={0.6} variant="success" />
    </Neo.Card>

    <Neo.Card title="Failed" className="card-header-filled shadow">
        <strong className="d-flex mb-3 justify-content-center">10/75</strong>
        <Neo.ProgressBar progress={0.13} variant="danger" />
    </Neo.Card>
</Neo.GridLayout>`}];